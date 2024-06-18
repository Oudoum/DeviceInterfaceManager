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
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim;
using DeviceInterfaceManager.Models.FlightSim.MSFS;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace DeviceInterfaceManager.ViewModels;

public partial class ProfileCreatorViewModel : ObservableObject
{
    private readonly ObservableCollection<IInputOutputDevice> _inputOutputDevices;

    private readonly SimConnectClient _simConnectClient;

    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ProfileCreatorModel? _profileCreatorModel;

    public ProfileCreatorViewModel(ObservableCollection<IInputOutputDevice> inputOutputDevices, SimConnectClient simConnectClient, IDialogService dialogService)
    {
        _inputOutputDevices = inputOutputDevices;
        _simConnectClient = simConnectClient;
        _dialogService = dialogService;
    }

#if DEBUG
    public ProfileCreatorViewModel()
    {
        ObservableCollection<IInputOutputDevice> inputOutputDevices =
        [
            new DeviceSerialBase()
        ];
        _inputOutputDevices = inputOutputDevices;
        _dialogService = Ioc.Default.GetService<IDialogService>()!;
        ProfileCreatorModel = new ProfileCreatorModel
        {
            InputCreators =
            [
                new InputCreator
                {
                    IsActive = true,
                    Preconditions = [new Precondition()],
                    Description = "Description",
                    InputType = ProfileCreatorModel.Switch,
                    Input = new Component(1)
                }
            ],

            OutputCreators =
            [
                new OutputCreator
                {
                    IsActive = true,
                    Preconditions = [new Precondition()],
                    Description = "Description",
                    OutputType = ProfileCreatorModel.Led,
                    Output = new Component(1),
                    FlightSimValue = 1234,
                    OutputValue = 4321
                }
            ]
        };
        AddInput();
        AddOutput();
        _simConnectClient = new SimConnectClient();
    }
#endif

    private string? _previousProfileName;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileAsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeProfileNameCommand))]
    [NotifyCanExecuteChangedFor(nameof(SortInputOutputCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddInputCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddOutputCommand))]
    [NotifyCanExecuteChangedFor(nameof(StartProfilesCommand))]
    private IInputOutputDevice? _inputOutputDevice;

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
    private string NewFilePath => Path.Combine(App.ProfilesPath, ProfileCreatorModel?.ProfileName + ".json");

    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    //Button 1
    [RelayCommand]
    private async Task ChangeDeviceAsync()
    {
        AskComboBoxViewModel viewModel = _dialogService.CreateViewModel<AskComboBoxViewModel>();
        viewModel.Title = "Please select your device:";
        viewModel.Text = InputOutputDevice?.DeviceName;
        viewModel.ObservableCollection = _inputOutputDevices;
        viewModel.SelectedItem = InputOutputDevice;

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = viewModel,
            Title = "Device",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        IInputOutputDevice? inputOutputDevice = viewModel.SelectedItem;
        if (result == ContentDialogResult.Primary && inputOutputDevice is not null)
        {
            ProfileCreatorModel ??= new ProfileCreatorModel();
            InputOutputDevice = inputOutputDevice;
            ProfileCreatorModel.DeviceName = InputOutputDevice.DeviceName;
            await ChangeProfileNameAsync();
        }
    }

    //Button 2
    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        IDialogStorageFile? dialogStorageFile = await _dialogService
            .ShowOpenFileDialogAsync(
                Ioc.Default.GetService<MainWindowViewModel>()!,
                new OpenFileDialogSettings
                {
                    Title = "Select a profile",
                    AllowMultiple = false,
                    SuggestedStartLocation = new DesktopDialogStorageFolder(App.ProfilesPath),
                    Filters = [new FileFilter("Json", "*json")]
                });

        string result = string.Empty;
        if (dialogStorageFile is not null)
        {
            Stream stream = await dialogStorageFile.OpenReadAsync();
            using StreamReader streamReader = new(stream);
            result = await streamReader.ReadToEndAsync();
        }

        if (!string.IsNullOrEmpty(result))
        {
            try
            {
                ProfileCreatorModel profileCreatorModel = JsonSerializer.Deserialize<ProfileCreatorModel>(result) ?? throw new InvalidOperationException();

                IInputOutputDevice? inputOutputDevice = _inputOutputDevices.FirstOrDefault(device => device.DeviceName == profileCreatorModel.DeviceName);
                switch (inputOutputDevice)
                {
                    case null when InputOutputDevice is null:
                        SetInfoBar(profileCreatorModel.ProfileName + " could not be loaded, because no controller for this profile was found and no controller is selected.", InfoBarSeverity.Warning);
                        return;

                    case null when InputOutputDevice is not null:
                    {
                        TaskDialogStandardResult dialogResult = await _dialogService.ShowTaskDialogAsync(
                            Ioc.Default.GetService<MainWindowViewModel>()!,
                            new TaskDialogSettings
                            {
                                Header = "Profile not for devices",
                                Content = "Do you want to keep the mapped positions of this profile with available positions on your device?",
                                Buttons = [TaskDialogButton.YesButton, TaskDialogButton.NoButton, TaskDialogButton.CancelButton]
                            });

                        if (!CheckRemap(dialogResult, profileCreatorModel))
                        {
                            return;
                        }

                        profileCreatorModel.DeviceName = InputOutputDevice?.DeviceName;
                        break;
                    }
                }

                if (await OverwriteCheck())
                {
                    ProfileCreatorModel = profileCreatorModel;
                    _previousProfileName = ProfileCreatorModel.ProfileName;
                    
                    if (inputOutputDevice is not null)
                    {
                        InputOutputDevice = inputOutputDevice;
                    }
                    
                    SetInfoBar(_previousProfileName + " successfully loaded.", InfoBarSeverity.Success);
                }
            }
            catch (Exception e)
            {
                SetInfoBar(e.Message, InfoBarSeverity.Error);
            }
        }
    }

    private bool CheckRemap(TaskDialogStandardResult result, ProfileCreatorModel profileCreatorModel)
    {
        switch (result)
        {
            case TaskDialogStandardResult.Cancel:
                return false;

            case TaskDialogStandardResult.No:
                foreach (InputCreator inputCreator in profileCreatorModel.InputCreators)
                {
                    inputCreator.Input = null;
                }

                foreach (OutputCreator outputCreator in profileCreatorModel.OutputCreators)
                {
                    outputCreator.Output = null;
                }
                return true;
            
            case TaskDialogStandardResult.Yes:
                if (InputOutputDevice is null)
                {
                    return false;
                }
                
                foreach (InputCreator inputCreator in profileCreatorModel.InputCreators)
                {
                    if (inputCreator.Input is null)
                    {
                        continue;
                    }
                    
                    if (inputCreator.InputType == ProfileCreatorModel.Switch && InputOutputDevice.Switch.Components.All(x => x.Position != inputCreator.Input.Position))
                    {
                        inputCreator.Input = null;
                    }
                }

                foreach (OutputCreator outputCreator in profileCreatorModel.OutputCreators)
                {
                    if (outputCreator.Output is null)
                    {
                        continue;
                    }
                    
                    switch (outputCreator.OutputType)
                    {
                        case ProfileCreatorModel.Led when InputOutputDevice.Led.Components.All(x => x.Position != outputCreator.Output.Position):
                        case ProfileCreatorModel.Dataline when InputOutputDevice.Dataline.Components.All(x => x.Position != outputCreator.Output.Position):
                        case ProfileCreatorModel.SevenSegment when InputOutputDevice.SevenSegment.Components.All(x => x.Position != outputCreator.Output.Position):
                            outputCreator.Output = null;
                            break;
                    }
                }
                return true;

            case TaskDialogStandardResult.None:
                break;

            case TaskDialogStandardResult.OK:
                break;

            case TaskDialogStandardResult.Retry:
                break;

            case TaskDialogStandardResult.Close:
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
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
            
            SetInfoBar(ProfileCreatorModel?.ProfileName + " successfully saved.", InfoBarSeverity.Success);
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
        
        string? profileName = await RenameProfileAsync();
        if (!string.IsNullOrEmpty(profileName))
        {
            _previousProfileName = ProfileCreatorModel.ProfileName;
            ProfileCreatorModel.ProfileName = profileName;
            await SaveProfile();
        }
    }

    private async Task<string?> RenameProfileAsync()
    {
        AskTextBoxViewModel viewModel = _dialogService.CreateViewModel<AskTextBoxViewModel>();
        viewModel.Title = "Please enter your profile name:";
        viewModel.Text = _previousProfileName;

        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = viewModel,
            Title = "Profile name",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        string? profileName = viewModel.Text;

        if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(profileName))
        {
            return profileName;
        }

        return null;
    }

    //Button 5
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task ChangeProfileNameAsync()
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }
        
        string? profileName = await RenameProfileAsync();
        if (!string.IsNullOrEmpty(profileName))
        {
            ProfileCreatorModel.ProfileName = profileName;

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
                sortedInputList.Sort((x, y) => Comparer<int?>.Default.Compare(y.Input?.Position, x.Input?.Position));
                sortedOutputList.Sort((x, y) => Comparer<int?>.Default.Compare(y.Output?.Position, x.Output?.Position));
                break;

            case true:
                IsSortedAscending = false;
                sortedInputList.Sort((x, y) => Comparer<int?>.Default.Compare(x.Input?.Position, y.Input?.Position));
                sortedOutputList.Sort((x, y) => Comparer<int?>.Default.Compare(x.Output?.Position, y.Output?.Position));
                break;
        }
        
        ProfileCreatorModel.InputCreators = new ObservableCollection<InputCreator>(sortedInputList);
        ProfileCreatorModel.OutputCreators = new ObservableCollection<OutputCreator>(sortedOutputList);
        
        SetInfoBar(ProfileCreatorModel.ProfileName + " successfully sorted.", InfoBarSeverity.Success);
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
        ProfileCreatorModel?.InputCreators.Add(new InputCreator { Id = Guid.NewGuid(), IsActive = true });
    }

    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private void AddOutput()
    {
        ProfileCreatorModel?.OutputCreators.Add(new OutputCreator { Id = Guid.NewGuid(), IsActive = true });
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
                break;

            case OutputCreator when !await ShowConfirmationDialogAsync("output row"):
                return;

            case OutputCreator outputCreator:
                _ = ProfileCreatorModel.OutputCreators.Remove(outputCreator);
                break;

            case IList selectedItems:
            {
                bool isShown = false;
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
                            }

                            isShown = true;
                            _ = ProfileCreatorModel.InputCreators.Remove(input);
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
                            }

                            isShown = true;
                            _ = ProfileCreatorModel.OutputCreators.Remove(output);
                            break;
                        }
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
        
        foreach (object inputOutputCreator in inputOutputCreators)
        {
            switch (inputOutputCreator)
            {
                case InputCreator inputCreator:
                    ProfileCreatorModel.InputCreators.Add(inputCreator.Clone());
                    break;

                case OutputCreator outputCreator:
                    ProfileCreatorModel.OutputCreators.Add(outputCreator.Clone());
                    break;
            }
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
                DefaultButton = ContentDialogButton.Primary
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
                DefaultButton = ContentDialogButton.Primary
            });

        if (outputResult == ContentDialogResult.Primary)
        {
            outputCreator.Preconditions = outputCreatorViewModel.Copy();
        }
    }

    [ObservableProperty]
    private bool _isStarted;

    partial void OnIsStartedChanged(bool value)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            desktop.MainWindow.Topmost = value;
        }
    }

    private Profile? _profile;

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
            _simConnectClient.Disconnect();
            if (_profile is not null)
            {
                await _profile.DisposeAsync();
            }

            SetInfoBar(ProfileCreatorModel?.ProfileName + " stopped.", InfoBarSeverity.Informational);
            return;
        }

        await _simConnectClient.ConnectAsync(token);

        if (!token.IsCancellationRequested && _simConnectClient.SimConnect is not null)
        {
            _profile = new Profile(_simConnectClient, ProfileCreatorModel, InputOutputDevice);
            SetInfoBar(ProfileCreatorModel?.ProfileName + " started.", InfoBarSeverity.Informational);
            return;
        }

        IsStarted = !IsStarted;
    }
}