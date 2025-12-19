using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services;
using DeviceInterfaceManager.Services.Devices;
using DeviceInterfaceManager.ViewModels.Dialogs;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using Microsoft.Extensions.Logging;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace DeviceInterfaceManager.ViewModels;

public partial class ProfileCreatorViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly ObservableCollection<IDeviceService> _inputOutputDevices;
    private readonly SimConnectClientService _simConnectClientService;
    private readonly PmdgHelperService _pmdgHelperService;
    private readonly IDialogService _dialogService;
    
    public ProfileCreatorViewModel(ILogger<ProfileCreatorModel> logger, ObservableCollection<IDeviceService> inputOutputDevices, SimConnectClientService simConnectClientService, PmdgHelperService pmdgHelperService, IDialogService dialogService)
    {
        _logger = logger;
        _inputOutputDevices = inputOutputDevices;
        _simConnectClientService = simConnectClientService;
        _pmdgHelperService = pmdgHelperService;
        _dialogService = dialogService;
    }

    [ObservableProperty]
    private ProfileCreatorModel? _profileCreatorModel;

    private string? _previousProfileName;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeDeviceCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileAsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeProfileNameCommand))]
    [NotifyCanExecuteChangedFor(nameof(SortInputOutputCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddInputCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddOutputCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartProfilesCommand))]
    private string? _profileName;

    [ObservableProperty]
    private IDeviceService? _inputOutputDevice;

    [ObservableProperty]
    private bool _infoBarIsOpen;

    [ObservableProperty]
    private string? _infoBarMessage;

    [ObservableProperty]
    private InfoBarSeverity _infoBarSeverity;

    private void SetInfoBar(string? message, InfoBarSeverity severity)
    {
        InfoBarMessage = message;
        InfoBarSeverity = severity;
        InfoBarIsOpen = true;
    }

    private string OldFilePath => Path.Combine(App.ProfilesPath, _previousProfileName + ".json");
    private string NewFilePath => Path.Combine(App.ProfilesPath, ProfileName + ".json");

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    //Button 0
    [RelayCommand]
    private async Task CreateProfileCommand()
    {
        await ChangeDeviceAsync(true);
    }

    //Button 1
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task ChangeDeviceAsync(bool changeProfileName)
    {
        AskComboBoxDialogModel dialogModel = _dialogService.CreateViewModel<AskComboBoxDialogModel>();
        dialogModel.Title = "Please select your device:";
        dialogModel.Text = InputOutputDevice?.DeviceName;
        dialogModel.ObservableCollection = _inputOutputDevices;
        dialogModel.SelectedItem = InputOutputDevice;

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = dialogModel,
            Title = "Device",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        IDeviceService? inputOutputDevice = dialogModel.SelectedItem;
        if (result == ContentDialogResult.Primary && inputOutputDevice is not null)
        {
            if (changeProfileName)
            {
                _previousProfileName = null;
                string? profileName = await NameProfileAsync();
                if (profileName is not null)
                {
                    ProfileCreatorModel = new ProfileCreatorModel();
                    _previousProfileName = profileName;
                    ProfileName = profileName;
                    InputOutputDevice = inputOutputDevice;
                    ProfileCreatorModel.DeviceName = InputOutputDevice.DeviceName;
                    return;
                }

                _previousProfileName = ProfileName;
                if (ProfileCreatorModel is null)
                {
                    InputOutputDevice = inputOutputDevice;
                }
            }
        }
    }

    //Button 2
    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        IDialogStorageFile? dialogStorageFile = await _dialogService.ShowOpenFileDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new OpenFileDialogSettings
            {
                Title = "Select a profile",
                AllowMultiple = false,
                SuggestedStartLocation = new DesktopDialogStorageFolder(App.ProfilesPath),
                Filters = [new FileFilter("Json", "*json")]
            });

        if (dialogStorageFile is not null)
        {
            Stream stream = await dialogStorageFile.OpenReadAsync();
            using StreamReader streamReader = new(stream);
            string result = await streamReader.ReadToEndAsync();
            if (!string.IsNullOrEmpty(result))
            {
                await TryAssignDeviceWithProfileAsync(result, Path.GetFileNameWithoutExtension(dialogStorageFile.Name));
            }
        }
    }

    private async Task TryAssignDeviceWithProfileAsync(string result, string fileName)
    {
        ProfileCreatorModel? profileCreatorModel;
        try
        {
            profileCreatorModel = JsonSerializer.Deserialize<ProfileCreatorModel>(result);
        }
        catch (Exception e)
        {
            SetInfoBar(e.Message, InfoBarSeverity.Error);
            return;
        }

        if (profileCreatorModel is null)
        {
            SetInfoBar("Profile could not be loaded.", InfoBarSeverity.Error);
            return;
        }
        
        IDeviceService? inputOutputDevice = _inputOutputDevices.FirstOrDefault(device => device.DeviceName == profileCreatorModel.DeviceName);

        if (inputOutputDevice is null)
        {
            switch (_inputOutputDevices.Count)
            {
                case 1:
                    inputOutputDevice = _inputOutputDevices[0];
                    break;

                case > 1:
                    await ChangeDeviceAsync(false);
                    break;
            }

            TaskDialogStandardResult dialogResult = await _dialogService.ShowTaskDialogAsync(
                Ioc.Default.GetService<MainWindowViewModel>()!,
                new TaskDialogSettings
                {
                    Header = "Profile not for device",
                    Content = "Are you sure you want to load this profile? All mapped positions will be removed!",
                    Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton]
                });

            if (!CheckRemap(dialogResult, profileCreatorModel))
            {
                return;
            }
        }

        if (await OverwriteCheck())
        {
            ProfileCreatorModel = profileCreatorModel;
            ProfileName = fileName;
            _previousProfileName = ProfileName;

            if (inputOutputDevice is not null)
            {
                InputOutputDevice = inputOutputDevice;
            }

            SetInfoBar(_previousProfileName + " successfully loaded.", InfoBarSeverity.Success);
        }
            
        profileCreatorModel.DeviceName = InputOutputDevice?.DeviceName;
    }

    private bool CheckRemap(TaskDialogStandardResult result, ProfileCreatorModel profileCreatorModel)
    {
        switch (result)
        {
            case TaskDialogStandardResult.No:
                break;

            case TaskDialogStandardResult.Yes:
                foreach (InputCreator inputCreator in profileCreatorModel.InputCreators)
                {
                    inputCreator.Input = null;
                }

                foreach (OutputCreator outputCreator in profileCreatorModel.OutputCreators)
                {
                    outputCreator.Outputs = null;
                }

                return true;
        }

        return false;
    }

    private async Task<bool> OverwriteCheck()
    {
        if (ProfileCreatorModel?.DeviceName is null)
        {
            return true;
        }

        if (ProfileCreatorModel.InputCreators.Count == 0 && ProfileCreatorModel.OutputCreators.Count == 0)
        {
            return true;
        }

        TaskDialogStandardResult result = await _dialogService.ShowTaskDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new TaskDialogSettings
            {
                Header = "Overwrite",
                Content = $"Are you sure you want to overwrite the profile for {ProfileCreatorModel.DeviceName}?",
                Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton]
            });

        return result == TaskDialogStandardResult.Yes;
    }

    private bool CanEditProfile()
    {
        return ProfileCreatorModel is not null;
    }

    //Button 3
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task SaveProfile()
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(NewFilePath) ?? string.Empty);
            await File.WriteAllTextAsync(NewFilePath, JsonSerializer.Serialize(ProfileCreatorModel, _serializerOptions));

            SetInfoBar(ProfileName + " successfully saved.", InfoBarSeverity.Success);
        }
        catch (Exception e)
        {
            SetInfoBar(e.Message, InfoBarSeverity.Error);
        }
    }

    //Button 4
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task SaveProfileAsAsync()
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        string? profileName = await NameProfileAsync();
        if (profileName is not null)
        {
            _previousProfileName = ProfileName;
            ProfileName = profileName;
            await SaveProfile();
        }
    }

    private async Task<string?> NameProfileAsync()
    {
        AskTextBoxDialogModel dialogModel = _dialogService.CreateViewModel<AskTextBoxDialogModel>();
        dialogModel.Title = "Please enter your profile name:";
        dialogModel.Text = _previousProfileName;

        ContentDialogResult result;
        ContentDialogSettings contentDialogSettings = new()
        {
            Content = dialogModel,
            Title = "Profile name",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        };

        do
        {
            result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, contentDialogSettings);
        } while (result == ContentDialogResult.Primary && dialogModel.CheckForErrors());

        return result == ContentDialogResult.Primary ? dialogModel.Text : null;
    }

    //Button 5
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task ChangeProfileNameAsync()
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        string? profileName = await NameProfileAsync();
        if (!string.IsNullOrEmpty(profileName))
        {
            ProfileName = profileName;

            if (!string.IsNullOrEmpty(_previousProfileName))
            {
                try
                {
                    File.Move(OldFilePath, NewFilePath);
                    string text = await File.ReadAllTextAsync(NewFilePath);
                    text = text.Replace(_previousProfileName, profileName);
                    await File.WriteAllTextAsync(NewFilePath, text);

                    SetInfoBar($"{_previousProfileName} successfully renamed to {profileName}.", InfoBarSeverity.Success);
                }
                catch (Exception e)
                {
                    SetInfoBar(e.Message, InfoBarSeverity.Error);
                }
            }

            _previousProfileName = profileName;
        }
    }

    //Button 6
    [ObservableProperty]
    private bool? _isSortedAscending;

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private void SortInputOutput()
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        var sortedInputList = ProfileCreatorModel.InputCreators.ToList();
        var sortedOutputList = ProfileCreatorModel.OutputCreators.ToList();

        switch (IsSortedAscending)
        {
            case false or null:
                IsSortedAscending = true;
                sortedInputList.Sort((x, y) => Comparer<int?>.Default.Compare(y.Input, x.Input));
                sortedOutputList.Sort((x, y) => Comparer<int?>.Default.Compare(y.Outputs?[0], x.Outputs?[0]));
                break;

            case true:
                IsSortedAscending = false;
                sortedInputList.Sort((x, y) => Comparer<int?>.Default.Compare(x.Input, y.Input));
                sortedOutputList.Sort((x, y) => Comparer<int?>.Default.Compare(x.Outputs?[0], y.Outputs?[0]));
                break;
        }

        ProfileCreatorModel.InputCreators = new ObservableCollection<InputCreator>(sortedInputList);
        ProfileCreatorModel.OutputCreators = new ObservableCollection<OutputCreator>(sortedOutputList);

        SetInfoBar(ProfileName + " successfully sorted.", InfoBarSeverity.Success);
    }

    //Button 7
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task ClearProfileAsync()
    {
        TaskDialogStandardResult result = await _dialogService.ShowTaskDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new TaskDialogSettings
            {
                Header = "Clear",
                Content = "Are you sure you want to clear all inputs and outputs?",
                Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton]
            });

        if (result == TaskDialogStandardResult.Yes)
        {
            ProfileCreatorModel?.InputCreators.Clear();
            ProfileCreatorModel?.OutputCreators.Clear();
        }
    }

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private void AddInput()
    {
        ProfileCreatorModel?.InputCreators.Add(new InputCreator());
    }

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private void AddOutput()
    {
        ProfileCreatorModel?.OutputCreators.Add(new OutputCreator());
    }

    private async Task<bool> ShowConfirmationDialogAsync(string rowType)
    {
        TaskDialogStandardResult result = await _dialogService.ShowTaskDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new TaskDialogSettings
            {
                Header = "Delete",
                Content = $"Are you sure you want to delete the selected {rowType}?",
                Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton]
            });
        return result == TaskDialogStandardResult.Yes;
    }

    [RelayCommand]
    private async Task DeleteInputOutputCreatorRowAsync(object creator)
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        switch (creator)
        {
            case InputCreator when !await ShowConfirmationDialogAsync("input row"):
                return;

            case InputCreator inputCreator:
                _ = ProfileCreatorModel.InputCreators.Remove(inputCreator);
                return;

            case OutputCreator when !await ShowConfirmationDialogAsync("output row"):
                return;

            case OutputCreator outputCreator:
                _ = ProfileCreatorModel.OutputCreators.Remove(outputCreator);
                return;

            case IList selectedItems:
            {
                bool isShown = false;
                IList<InputCreator> clonedInputCreators = [];
                IList<OutputCreator> clonedOutputCreators = [];

                foreach (object? item in selectedItems.Cast<object>().ToList())
                {
                    switch (item)
                    {
                        case InputCreator input:
                        {
                            if (!isShown)
                            {
                                string rowType = "input rows";
                                if (selectedItems.Count() == 1)
                                {
                                    rowType = "input row";
                                }

                                if (!await ShowConfirmationDialogAsync(rowType))
                                {
                                    return;
                                }

                                isShown = true;
                            }

                            clonedInputCreators.Add(input);
                            break;
                        }

                        case OutputCreator output:
                        {
                            if (!isShown)
                            {
                                string rowType = "output rows";
                                if (selectedItems.Count() == 1)
                                {
                                    rowType = "output row";
                                }

                                if (!await ShowConfirmationDialogAsync(rowType))
                                {
                                    return;
                                }

                                isShown = true;
                            }

                            clonedOutputCreators.Add(output);
                            break;
                        }
                    }
                }

                if (clonedInputCreators.Count > 0)
                {
                    foreach (InputCreator inputCreator in clonedInputCreators)
                    {
                        ProfileCreatorModel.InputCreators.Remove(inputCreator);
                    }
                }

                if (clonedOutputCreators.Count > 0)
                {
                    foreach (OutputCreator outputCreator in clonedOutputCreators)
                    {
                        ProfileCreatorModel.OutputCreators.Remove(outputCreator);
                    }
                }

                break;
            }
        }
    }

    [RelayCommand]
    private void DuplicateInputOutputCreatorRow(IList inputOutputCreators)
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        IList<InputCreator> clonedInputCreators = [];
        IList<OutputCreator> clonedOutputCreators = [];

        int lastIndex = -1;

        foreach (object inputOutputCreator in inputOutputCreators)
        {
            switch (inputOutputCreator)
            {
                case InputCreator inputCreator:
                    clonedInputCreators.Add((InputCreator)inputCreator.Clone());
                    lastIndex = ProfileCreatorModel.InputCreators.IndexOf(inputCreator);
                    break;

                case OutputCreator outputCreator:
                    clonedOutputCreators.Add((OutputCreator)outputCreator.Clone());
                    lastIndex = ProfileCreatorModel.OutputCreators.IndexOf(outputCreator);
                    break;
            }
        }

        foreach (InputCreator inputCreator in clonedInputCreators)
        {
            lastIndex++;
            ProfileCreatorModel.InputCreators.Insert(lastIndex, inputCreator);
        }

        foreach (OutputCreator outputCreator in clonedOutputCreators)
        {
            lastIndex++;
            ProfileCreatorModel.OutputCreators.Insert(lastIndex, outputCreator);
        }
    }

    [RelayCommand]
    private static void ActivateInputOutputCreator(IList list)
    {
        ToggleInputOutputCreator(list, true);
    }

    [RelayCommand]
    private static void DeactivateInputOutputCreator(IList list)
    {
        ToggleInputOutputCreator(list, false);
    }

    private static void ToggleInputOutputCreator(IEnumerable list, bool isActive)
    {
        foreach (IActive item in list)
        {
            item.IsActive = isActive;
        }
    }

    public static string[] EventDataTypePreSelections => [ProfileCreatorModel.MsfsSimConnect, ProfileCreatorModel.Pmdg737, ProfileCreatorModel.Pmdg777];

    public string EventDataTypePreSelection { get; set; } = ProfileCreatorModel.MsfsSimConnect;

    [RelayCommand]
    private async Task EditInput(InputCreator inputCreator)
    {
        if (ProfileCreatorModel is null || InputOutputDevice is null)
        {
            return;
        }

        inputCreator.InputType ??= ProfileCreatorModel.Switch;
        inputCreator.EventType ??= EventDataTypePreSelection;

        InputCreatorViewModel inputCreatorViewModel = new(
            InputOutputDevice,
            inputCreator,
            ProfileCreatorModel.OutputCreators,
            inputCreator.Preconditions);

        ContentDialogResult inputResult = await _dialogService.ShowContentDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new ContentDialogSettings
            {
                Content = inputCreatorViewModel,
                Title = "Input Creator",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                FullSizeDesired = true
            });

        if (inputResult == ContentDialogResult.Primary)
        {
            inputCreator.Preconditions = inputCreatorViewModel.Copy();
        }
    }

    [RelayCommand]
    private async Task EditOutput(OutputCreator outputCreator)
    {
        if (ProfileCreatorModel is null || InputOutputDevice is null)
        {
            return;
        }

        outputCreator.OutputType ??= ProfileCreatorModel.Led;
        outputCreator.DataType ??= EventDataTypePreSelection;

        OutputCreatorViewModel outputCreatorViewModel = new(
            InputOutputDevice,
            outputCreator,
            ProfileCreatorModel.OutputCreators,
            outputCreator.Preconditions);

        ContentDialogResult outputResult = await _dialogService.ShowContentDialogAsync(
            Ioc.Default.GetService<MainWindowViewModel>()!,
            new ContentDialogSettings
            {
                Content = outputCreatorViewModel,
                Title = "Output Creator",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                FullSizeDesired = true
            });

        await InputOutputDevice.ResetAllOutputsAsync();

        if (outputResult == ContentDialogResult.Primary)
        {
            outputCreator.Preconditions = outputCreatorViewModel.Copy();
        }
    }

    [ObservableProperty]
    private bool _isStarted;

    private ProfileService? _profile;

    [RelayCommand(CanExecute = nameof(CanEditProfile), IncludeCancelCommand = true)]
    private async Task StartProfilesAsync(CancellationToken token)
    {
        if (ProfileCreatorModel is null || InputOutputDevice is null)
        {
            return;
        }

        IsStarted = !IsStarted;
        if (!IsStarted)
        {
            _simConnectClientService.Disconnect();
            if (_profile is not null)
            {
                await _profile.DisposeAsync();
            }

            SetInfoBar(ProfileName + " stopped.", InfoBarSeverity.Informational);
            return;
        }

        await _simConnectClientService.ConnectAsync(token);

        if (!token.IsCancellationRequested)
        {
            _profile = new ProfileService(_simConnectClientService, _pmdgHelperService, ProfileCreatorModel, InputOutputDevice);
            SetInfoBar(ProfileName + " started.", InfoBarSeverity.Informational);
            return;
        }

        IsStarted = !IsStarted;
    }
}