using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices.interfaceIT.ENET;
using DeviceInterfaceManager.Views;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using HanumanInstitute.MvvmDialogs.FileSystem;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using IDialogService = HanumanInstitute.MvvmDialogs.IDialogService;

namespace DeviceInterfaceManager.ViewModels;

public partial class ProfileCreatorViewModel(ObservableCollection<DeviceItem> deviceItems, IDialogService dialogService) : ObservableObject
{
    private readonly IDialogService _dialogService = dialogService;
    
    private ObservableCollection<DeviceItem> _deviceItems = deviceItems;
    
    [ObservableProperty]
    private ProfileCreatorModel? _profileCreatorModel;
    
    public List<ProfileCreatorModel> ProfileCreatorModels { get; set; } = [];
    
    public string? PreviousProfileName { get; private set; }
    
    public string? ProfileName
    {
        get => ProfileCreatorModel?.ProfileName;
        set
        {
            PreviousProfileName = ProfileCreatorModel?.ProfileName;
            ProfileCreatorModel.ProfileName = value;
            OnPropertyChanged();
            UpdateButtons();
        }
    }
    
    private void UpdateButtons()
    {
        SaveProfileCommand.NotifyCanExecuteChanged();
        SaveProfileAsCommand.NotifyCanExecuteChanged();
        SortInputOutputCommand.NotifyCanExecuteChanged();
        ClearProfileCommand.NotifyCanExecuteChanged();
    }
    
    public string? Driver
    {
        get => ProfileCreatorModel?.Driver;
        set
        {
            if (ProfileCreatorModel?.Driver == value)
            {
                return;
            }

            ProfileCreatorModel = new ProfileCreatorModel
            {
                Driver = value
            };
            ProfileCreatorModels.Clear();
            DeviceCollection.Clear();
            Device = new KeyValuePair<string, string>();

            SetupDriver(value);

            OnPropertyChanged();
            UpdateButtons();
        }
    }
    
    [ObservableProperty]
    private string _portName = "COM3";
    
    private void SetupDriver(string driver)
    {
        switch (driver)
        {
            case ProfileCreatorModel.FdsUsb:
                // foreach (var iitdevice in deviceList)
                // {
                //     CreateDevice(iitdevice.BoardName, iitdevice.SerialNumber);
                // }
                break;

            case ProfileCreatorModel.FdsEnet:
                //Async RelayCommand
                break;

            case ProfileCreatorModel.Arduino:
                CreateDevice("Arduino Mega 2560", PortName);
                break;

            case ProfileCreatorModel.CPflightUsb:
                // CreateDevice(Devices.CPflight.Device.MCP.DeviceName, PortName);
                break;
        }
    }
    
    // [RelayCommand]
    // private async Task<InterfaceITEthernetInfo> GetInterfaceITEthernetBoardInfoAsync()
    // {
    //     InterfaceITEthernetDiscovery? discovery = await InterfaceITEthernet.ReceiveControllerDiscoveryDataAsync();
    //     InterfaceITEthernetInfo interfaceITEthernetInfo = null;
    //     if (discovery is not null)
    //     {
    //         CreateDevice(discovery.Value.Name, discovery.Value.IPAddress);
    //         InterfaceITEthernet interfaceITEthernet = new(discovery.Value.IPAddress);
    //         CancellationTokenSource tokenSource = new();
    //         InterfaceITEthernet.ConnectionStatus connectionStatus = await interfaceITEthernet.InterfaceITEthernetConnectionAsync(tokenSource.Token);
    //         if (connectionStatus == InterfaceITEthernet.ConnectionStatus.Connected)
    //         {
    //             interfaceITEthernetInfo = await interfaceITEthernet.GetInterfaceITEthernetDataAsync((ledNumber, direction) => { }, tokenSource.Token);
    //             FullDevice = interfaceITEthernet;
    //             ErrorText = $"{Driver} => {discovery.Value.Name} was found and has been added to the devices!";
    //             return interfaceITEthernetInfo;
    //         }
    //     }
    //     ErrorText = $"{Driver} => no devices were found!";
    //     return interfaceITEthernetInfo;
    // }

    private void CreateDevice(string name, string serial)
    {
        KeyValuePair<string, string>  device = new(name, serial);
        if (!DeviceCollection.Contains(device))
        {
            DeviceCollection.Add(device);
        }
    }

    public static string[] Drivers => ProfileCreatorModel.Drivers;


    private KeyValuePair<string, string> _device;
    public KeyValuePair<string, string> Device
    {
        get => _device;
        set
        {
            if (value.Key is null)
            {
                _device = value;
                OnPropertyChanged(nameof(Device));
                return;
            }

            // if (string.IsNullOrEmpty(ProfileCreatorModel.DeviceName) || _result.Value && !ProfileCreatorModels.Any(s => s.DeviceName == Device.Key))
            // {
            //     ProfileCreatorModels.Add(ProfileCreatorModel);
            //     SetDevice(value, ProfileCreatorModel);
            // }
            else if (!ProfileCreatorModels.Any(s => s.DeviceName == value.Key))
            {
                ProfileCreatorModel profileCreatorModel = new()
                {
                    ProfileName = ProfileCreatorModel.ProfileName,
                    Driver = ProfileCreatorModel.Driver,
                };
                SetDevice(value, profileCreatorModel);
                ProfileCreatorModels.Add(profileCreatorModel);
            }
            // else if (_result.Value && ProfileCreatorModels.Any(s => s.DeviceName == value.Key))
            // {
            //     if (OverrideDevice() == MessageDialogResult.Affirmative)
            //     {
            //         int index = ProfileCreatorModels.FindIndex(s => s.DeviceName == value.Key);
            //         if (index != -1)
            //         {
            //             ProfileCreatorModels[index] = ProfileCreatorModel;
            //         }
            //     }
            // }
            ProfileCreatorModel = ProfileCreatorModels.Where(s => s.DeviceName == value.Key).FirstOrDefault();
            ProfileCreatorModel.DeviceName = value.Key;
            ProfileName = ProfileCreatorModel.ProfileName;
            Driver = ProfileCreatorModel.Driver;
            _device = value;
            OnPropertyChanged(nameof(Device));
        }
    }

    // private MessageDialogResult OverrideDevice()
    // {
    //     return dialogCoordinator.ShowModalMessageExternal(this, "Override", $"Are you sure you want to override the profile for {ProfileCreatorModel.DeviceName}?",
    //     MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
    //     {
    //         AnimateHide = false,
    //         AnimateShow = false,
    //         AffirmativeButtonText = "Yes",
    //         NegativeButtonText = "No"
    //     });
    // }

    private void SetDevice(KeyValuePair<string,string> device, ProfileCreatorModel profileCreatorModel)
    {
        switch (Driver)
        {
            case ProfileCreatorModel.FdsUsb:
                // FullDevice = deviceList.Where(s => s.SerialNumber == device.Value).FirstOrDefault();
                break;

            case ProfileCreatorModel.FdsEnet:
                break;

            case ProfileCreatorModel.Arduino:
                // FullDevice = ProfileCreatorArduino.IOStartStop;
                break;

            case ProfileCreatorModel.CPflightUsb:
                // if (device.Key == Devices.CPflight.Device.MCP.DeviceName)
                // {
                //     FullDevice = Devices.CPflight.Device.MCP;
                // }
                break;
        }
        profileCreatorModel.DeviceName = device.Key;
    }

    public ObservableCollection<KeyValuePair<string, string>> DeviceCollection { get; set; } = [];

    // private readonly ObservableCollection<InterfaceIT_BoardInfo.Device> deviceList;

    private void DeviceList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        Driver = null;
    }

    public object? FullDevice;

    [ObservableProperty]
    private string? _errorText;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileCommand))]
    [NotifyCanExecuteChangedFor(nameof(SaveProfileAsCommand))]
    [NotifyCanExecuteChangedFor(nameof(SortInputOutputCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearProfileCommand))]
    private string _profileNameButtonContent = "Ok";

    private string OldFilePath => Path.Combine("Profiles", "Creator", PreviousProfileName + ".json");
    private string NewFilePath => Path.Combine("Profiles", "Creator", ProfileName + ".json");

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
            Stream? stream = await dialogStorageFile.OpenReadAsync();
            using StreamReader streamReader = new(stream);
            result = await streamReader.ReadToEndAsync();
        }

        if (!string.IsNullOrEmpty(result))
        {
            try
            {
                ProfileCreatorModel profileCreatorModel = JsonSerializer.Deserialize<ProfileCreatorModel>(result) ?? throw new InvalidOperationException();

                if (deviceItems.Any(device => device.InputOutputDevice.DeviceName == profileCreatorModel.DeviceName))
                {
                    Driver = profileCreatorModel.Driver;
                    ProfileCreatorModel = profileCreatorModel;
                    PreviousProfileName = ProfileName;
                    foreach (var item in DeviceCollection)
                    {
                        if (item.Key != profileCreatorModel?.DeviceName)
                        {
                            continue;
                        }

                        Device = item;
                        break;
                    }

                    ProfileNameButtonContent = "Edit";
                    ErrorText = ProfileName + " successfully loaded";
                    return;
                }

                ErrorText = ProfileName + "could not be loaded, because no controller for this profile was found";
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }
    }

    [RelayCommand]
    private void ProfileNameSave()
    {
        switch (ProfileNameButtonContent)
        {
            case "Ok" when !string.IsNullOrEmpty(ProfileName):
            {
                RenameProfile();
                ProfileNameButtonContent = "Edit";
                break;
            }

            case "Edit":
                ProfileNameButtonContent = "Ok";
                break;
        }
    }

    private void RenameProfile()
    {
        if (string.IsNullOrEmpty(PreviousProfileName))
        {
            return;
        }

        try
        {
            File.Move(OldFilePath, NewFilePath);
            string text = File.ReadAllText(NewFilePath);
            text = text.Replace(PreviousProfileName, ProfileName);
            File.WriteAllText(NewFilePath, text);
            ErrorText = PreviousProfileName + " successfully renamed to " + ProfileName;
        }
        catch (Exception e)
        {
            ErrorText = e.Message;
        }
    }
    
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    [RelayCommand]
    private void SaveProfile()
    {
        try
        {
            _ = Directory.CreateDirectory(Path.GetDirectoryName(NewFilePath) ?? string.Empty);
            File.WriteAllText(NewFilePath, JsonSerializer.Serialize(ProfileCreatorModel, _serializerOptions));
            ErrorText = ProfileName + " successfully saved";
        }
        catch (Exception e)
        {
            ErrorText = e.Message;
        }
    }

    [RelayCommand]
    private async Task SaveProfileAsAsync()
    {
        AskTextBoxViewModel viewModel = _dialogService.CreateViewModel<AskTextBoxViewModel>();
        viewModel.Title = "Please enter your profile name.";
        viewModel.Text = PreviousProfileName;
        
        ContentDialogResult result = await _dialogService.ShowContentDialogAsync(App.MainWindowViewModel, new ContentDialogSettings
        {
            Content = viewModel,
            Title = "Profile name",
            PrimaryButtonText = "OK",
            SecondaryButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary
        });

        string? text = viewModel.Text;
        if (result == ContentDialogResult.Primary && !string.IsNullOrEmpty(text))
        {
            ProfileName = text;
            SaveProfile();
            ProfileNameButtonContent = "Edit";
        }
    }

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
        var sortedInputList = ProfileCreatorModel.InputCreator.ToList();
        sortedInputList.Sort((x, y) => IsSortedAscending.Value ? Comparer<KeyValuePair<string, string>?>.Default.Compare(y.Input, x.Input) : Comparer<KeyValuePair<string, string>?>.Default.Compare(x.Input, y.Input));
        for (int i = 0; i < sortedInputList.Count; i++)
        {
            ProfileCreatorModel.InputCreator.Move(ProfileCreatorModel.InputCreator.IndexOf(sortedInputList[i]), i);
        }

        var sortedOutputList = ProfileCreatorModel.OutputCreator.ToList();
        sortedOutputList.Sort((x, y) => IsSortedAscending.Value ? Comparer<KeyValuePair<string, string>?>.Default.Compare(y.Output, x.Output) : Comparer<KeyValuePair<string, string>?>.Default.Compare(x.Output, y.Output));
        for (int i = 0; i < sortedOutputList.Count; i++)
        {
            ProfileCreatorModel.OutputCreator.Move(ProfileCreatorModel.OutputCreator.IndexOf(sortedOutputList[i]), i);
        }
        IsSortedAscending = !IsSortedAscending;
        ErrorText = ProfileName + " successfully sorted";
    }

    [RelayCommand]
    private async Task ClearProfileAsync()
    {
        // if (ProfileCreatorModel is not null && MessageDialogResult.Affirmative == await dialogCoordinator
        //     .ShowMessageAsync(this, "Clear", "Are you sure you want to clear all inputs and outputs?",
        //     MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
        //     {
        //         AnimateHide = false,
        //         AnimateShow = false,
        //         AffirmativeButtonText = "Yes",
        //         NegativeButtonText = "No"
        //     }))
        // {
        //     ProfileCreatorModel.InputCreator.Clear();
        //     ProfileCreatorModel.OutputCreator.Clear();
        // }
    }

    [RelayCommand]
    private void AddInput()
    {
        ProfileCreatorModel?.InputCreator.Add(new InputCreator() { Id = Guid.NewGuid(), IsActive = true });
    }

    [RelayCommand]
    private void AddOutput()
    {
        ProfileCreatorModel?.OutputCreator.Add(new OutputCreator() { Id = Guid.NewGuid(), IsActive = true });
    }

    private async Task<bool> ShowConfirmationDialogAsync(string rowType)
    {
        // MessageDialogResult result = await dialogCoordinator.ShowMessageAsync(this, "Delete", $"Are you sure you want to delete the selected {rowType}?",
        //     MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
        //     {
        //         AnimateHide = false,
        //         AnimateShow = false,
        //         AffirmativeButtonText = "Yes",
        //         NegativeButtonText = "No"
        //     });
        // return _result == MessageDialogResult.Affirmative;
        return false;
    }

    [RelayCommand]
    private async Task DeleteInputOutputCreatorRowAsync(object creator)
    {
        switch (creator)
        {
            case InputCreator when !await ShowConfirmationDialogAsync("input row"):
                return;

            case InputCreator inputCreator:
                _ = ProfileCreatorModel.InputCreator.Remove(inputCreator);
                break;

            case OutputCreator when !await ShowConfirmationDialogAsync("output row"):
                return;

            case OutputCreator outputCreator:
                _ = ProfileCreatorModel.OutputCreator.Remove(outputCreator);
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
                            _ = ProfileCreatorModel.InputCreator.Remove(input);
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
                            _ = ProfileCreatorModel.OutputCreator.Remove(output);
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
        if (creator is not IList selectedItems)
        {
            return;
        }

        foreach (object? item in selectedItems.Cast<object>().ToList())
        {
            switch (item)
            {
                case InputCreator input when isActive is null:
                    ProfileCreatorModel.InputCreator.Add(input.Clone());
                    continue;

                case InputCreator input:
                    input.IsActive = isActive.Value;
                    break;

                case OutputCreator output when isActive is null:
                    ProfileCreatorModel.OutputCreator.Add(output.Clone());
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
    private void EditInputOutput(object inputOutputCreator)
    {
        // NavigationService navigationService = new();
        // switch (inputOutputCreator)
        // {
        //     case InputCreator inputCreator:
        //         inputCreator.InputType ??= ProfileCreatorModel.Switch;
        //         inputCreator.EventType ??= EventDataTypePreSelection;
        //
        //         navigationService.NavigateToInputCreator(
        //             inputCreator,
        //             ProfileCreatorModel.OutputCreator,
        //             FullDevice);
        //         break;
        //
        //     case OutputCreator outputCreator:
        //         outputCreator.OutputType ??= ProfileCreatorModel.Led;
        //         outputCreator.DataType ??= EventDataTypePreSelection;
        //
        //         navigationService.NavigateToOutputCreator(
        //             outputCreator,
        //             ProfileCreatorModel.OutputCreator,
        //             FullDevice);
        //         break;
        // }
    }

    [ObservableProperty]
    private bool _isStarted;

    [RelayCommand]
    private async Task StartProfilesAsync()
    {
        switch (Driver)
        {
            case ProfileCreatorModel.FdsUsb when !IsStarted:
                // await Profiles.Instance.StartAsync(ProfileCreatorModel, deviceList.FirstOrDefault(k => k.SerialNumber == Device.Value));
                break;

            case ProfileCreatorModel.Arduino when !IsStarted:
                // await Profiles.Instance.StartAsync(ProfileCreatorModel, PortName);
                break;

            default:
                // Profiles.Instance.Stop();
                break;
        }

        IsStarted = !IsStarted;
        if (!IsStarted)
        { 
            ErrorText = ProfileCreatorModel.ProfileName + " stopped";
            return;
        }
        ErrorText = ProfileCreatorModel.ProfileName + " started";
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