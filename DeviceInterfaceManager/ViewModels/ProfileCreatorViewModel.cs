using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
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

    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private ProfileCreatorModel? _profileCreatorModel;

    public ProfileCreatorViewModel(ObservableCollection<IInputOutputDevice> inputOutputDevices, IDialogService dialogService)
    {
        _inputOutputDevices = inputOutputDevices;
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
                    Output = new Component(1)
                }
            ]
        };
        AddInput();
        AddOutput();
    }
#endif

    private string? _previousProfileName;

    public string? ProfileName
    {
        get => ProfileCreatorModel?.ProfileName;
        set
        {
            if (ProfileCreatorModel is null)
            {
                return;
            }

            _previousProfileName = ProfileCreatorModel.ProfileName;
            ProfileCreatorModel.ProfileName = value;
            OnPropertyChanged();
        }
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileAsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ChangeProfileNameCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddInputCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddOutputCommand))]
    private IInputOutputDevice? _inputOutputDevice;

    partial void OnInputOutputDeviceChanged(IInputOutputDevice? value)
    {
        ProfileCreatorModel = new ProfileCreatorModel();
    }

    [ObservableProperty]
    private string? _errorText;

    private string OldFilePath => Path.Combine("Profiles", "Creator", _previousProfileName + ".json");
    private string NewFilePath => Path.Combine("Profiles", "Creator", ProfileName + ".json");

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
            InputOutputDevice = inputOutputDevice;
            await ChangeProfileNameAsync();
            //Add logic for converting
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
                    SuggestedStartLocation = new DesktopDialogStorageFolder(Path.Combine(Environment.CurrentDirectory, Path.GetDirectoryName(NewFilePath) ?? string.Empty)),
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

                InputOutputDevice = _inputOutputDevices.FirstOrDefault(device => device.DeviceName == profileCreatorModel.DeviceName);
                if (InputOutputDevice is null)
                {
                    ErrorText = profileCreatorModel.ProfileName + " could not be loaded, because no controller for this profile was found";
                    return;
                }

                if (await OverwriteCheck())
                {
                    ProfileCreatorModel = profileCreatorModel;
                    _previousProfileName = ProfileName;

                    ErrorText = ProfileName + " successfully loaded";
                }
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }
    }

    private async Task<bool> OverwriteCheck()
    {
        if (ProfileCreatorModel is null)
        {
            return false;
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
            ErrorText = ProfileName + " successfully saved";
        }
        catch (Exception e)
        {
            ErrorText = e.Message;
        }
    }

    //Button 4
    [RelayCommand(CanExecute = nameof(CanEditProfile))]
    private async Task SaveProfileAsAsync()
    {
        string? profileName = await RenameProfileAsync();
        if (!string.IsNullOrEmpty(profileName))
        {
            _previousProfileName = ProfileName;
            ProfileName = profileName;
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
        string? profileName = await RenameProfileAsync();
        if (!string.IsNullOrEmpty(profileName))
        {
            ProfileName = profileName;

            if (!string.IsNullOrEmpty(_previousProfileName))
            {
                try
                {
                    File.Move(OldFilePath, NewFilePath);
                    string text = await File.ReadAllTextAsync(NewFilePath);
                    text = text.Replace(_previousProfileName, ProfileName);
                    await File.WriteAllTextAsync(NewFilePath, text);
                    ErrorText = _previousProfileName + " successfully renamed to " + ProfileName;
                }
                catch (Exception e)
                {
                    ErrorText = e.Message;
                }
            }

            _previousProfileName = ProfileName;
        }
    }

    //Button 6
    [ObservableProperty]
    private bool? _isSortedAscending;

    [RelayCommand]
    private void SortInputOutput()
    {
        if (ProfileCreatorModel is null)
        {
            return;
        }

        IsSortedAscending ??= false;
        var sortedInputList = ProfileCreatorModel.InputCreators.ToList();
        sortedInputList.Sort((x, y) => IsSortedAscending.Value ? Comparer<Component?>.Default.Compare(y.Input, x.Input) : Comparer<Component?>.Default.Compare(x.Input, y.Input));
        for (int i = 0; i < sortedInputList.Count; i++)
        {
            ProfileCreatorModel.InputCreators.Move(ProfileCreatorModel.InputCreators.IndexOf(sortedInputList[i]), i);
        }

        var sortedOutputList = ProfileCreatorModel.OutputCreators.ToList();
        sortedOutputList.Sort((x, y) => IsSortedAscending.Value ? Comparer<Component?>.Default.Compare(y.Output, x.Output) : Comparer<Component?>.Default.Compare(x.Output, y.Output));
        for (int i = 0; i < sortedOutputList.Count; i++)
        {
            ProfileCreatorModel.OutputCreators.Move(ProfileCreatorModel.OutputCreators.IndexOf(sortedOutputList[i]), i);
        }

        IsSortedAscending = !IsSortedAscending;
        ErrorText = ProfileName + " successfully sorted";
    }

    //Button 7
    [RelayCommand]
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
                                if (!await ShowConfirmationDialogAsync("input rows"))
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
                                if (!await ShowConfirmationDialogAsync("output rows"))
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
    private void DuplicateInputOutputCreatorRow(object creator)
    {
        ActivateDeactivateAllOrCloneInputOutputCreator(null, creator);
    }

    [RelayCommand]
    private void ActivateAllInputOutputCreator(object creator)
    {
        ActivateDeactivateAllOrCloneInputOutputCreator(true, creator);
    }

    [RelayCommand]
    private void DeactivateAllInputOutputCreator(object creator)
    {
        ActivateDeactivateAllOrCloneInputOutputCreator(false, creator);
    }

    private void ActivateDeactivateAllOrCloneInputOutputCreator(bool? isActive, object creator)
    {
        if (ProfileCreatorModel is null || creator is not IList selectedItems)
        {
            return;
        }

        foreach (object? item in selectedItems.Cast<object>().ToList())
        {
            switch (item)
            {
                case InputCreator input when isActive is null:
                    ProfileCreatorModel.InputCreators.Add(input.Clone());
                    continue;

                case InputCreator input:
                    input.IsActive = isActive.Value;
                    break;

                case OutputCreator output when isActive is null:
                    ProfileCreatorModel.OutputCreators.Add(output.Clone());
                    continue;

                case OutputCreator output:
                    output.IsActive = isActive.Value;
                    break;
            }
        }
    }

    public string[] EventDataTypePreSelections { get; set; } = [ProfileCreatorModel.MsfsSimConnect, ProfileCreatorModel.Pmdg737];

    public string EventDataTypePreSelection { get; set; } = ProfileCreatorModel.Pmdg737;

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
    [NotifyPropertyChangedFor(nameof(IsStartedText))]
    [NotifyPropertyChangedFor(nameof(IsStatedIcon))]
    private bool _isStarted;

    public string IsStartedText => IsStarted ? "Stop" : "Start";

    public Geometry? IsStatedIcon => (Geometry?)Application.Current!.FindResource(IsStarted ? "Stop" : "Play");

    [RelayCommand]
    private async Task StartProfilesAsync()
    {
        // switch (Driver)
        // {
        //     case ProfileCreatorModel.FdsUsb when !IsStarted:
        //         // await Profiles.Instance.StartAsync(ProfileCreatorModel, deviceList.FirstOrDefault(k => k.SerialNumber == Device.Value));
        //         break;
        //
        //     default:
        //         // Profiles.Instance.Stop();
        //         break;
        // }
        await Task.Delay(TimeSpan.Zero);

        IsStarted = !IsStarted;
        if (!IsStarted)
        {
            ErrorText = ProfileCreatorModel?.ProfileName + " stopped";
            return;
        }

        ErrorText = ProfileCreatorModel?.ProfileName + " started";
    }

    // public void DragOver(IDropInfo dropInfo)
    // {
    //     if ((dropInfo.Data is not InputCreator || dropInfo.TargetItem is not InputCreator) && (dropInfo.Data is not OutputCreator || dropInfo.TargetItem is not OutputCreator))
    //     {
    //         return;
    //     }
    //
    //     dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
    //     dropInfo.Effects = System.Windows.DragDropEffects.Move;
    // }
    //
    // public void Drop(IDropInfo dropInfo)
    // {
    //     if (dropInfo.Data is InputCreator)
    //     {
    //         InputCreator item = (InputCreator)dropInfo.Data;
    //         _ = ProfileCreatorModel.InputCreator.Remove(item);
    //         if (dropInfo.InsertIndex >= 0 && dropInfo.InsertIndex <= ProfileCreatorModel.InputCreator.Count)
    //         {
    //             ProfileCreatorModel.InputCreator.Insert(dropInfo.InsertIndex, item);
    //             return;
    //         }
    //         ProfileCreatorModel.InputCreator.Add(item);
    //     }
    //     else if (dropInfo.Data is OutputCreator item)
    //     {
    //         _ = ProfileCreatorModel.OutputCreator.Remove(item);
    //         if (dropInfo.InsertIndex >= 0 && dropInfo.InsertIndex <= ProfileCreatorModel.OutputCreator.Count)
    //         {
    //             ProfileCreatorModel.OutputCreator.Insert(dropInfo.InsertIndex, item);
    //             return;
    //         }
    //         ProfileCreatorModel.OutputCreator.Add(item);
    //     }
    // }
}