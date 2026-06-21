using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG;
using DeviceInterfaceManager.Models.Modifiers;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputCreatorViewModel : BaseCreatorViewModel, IOutputCreator
{
    private readonly IOutputCreator _outputCreator;

    public OutputCreatorViewModel(IDeviceService deviceService, IOutputCreator outputCreator, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
        : base(deviceService, outputCreators, preconditions)
    {
        _outputCreator = outputCreator;
        Description = outputCreator.Description;
        OutputType = outputCreator.OutputType;
        Components = GetComponents(OutputType);
        if (outputCreator.Outputs?.Length > 1)
        {
            OutputsCollection = new ObservableCollection<int>(outputCreator.Outputs);
            Position = 0;
        }

        if (outputCreator.Outputs?.Length > 0)
        {
            Output = Components?.FirstOrDefault(x => x?.Position == outputCreator.Outputs[^1]);
        }

        DataType = outputCreator.DataType;
        Data = outputCreator.Data;
        SearchPmdgData = Data;
        Unit = outputCreator.Unit;
        ModifiersCollection = new ObservableCollection<IModifier>(outputCreator.Modifiers ?? []); 
        Display = outputCreator.Display;
    }

    public override Precondition[]? Copy()
    {
        _outputCreator.Description = GetDescription();
        _outputCreator.OutputType = OutputType;
        _outputCreator.Outputs = OutputsCollection.Count switch
        {
            > 0 => OutputsCollection.ToArray(),
            0 => GetOutputs(),
            _ => _outputCreator.Outputs
        };
        _outputCreator.DataType = DataType;
        _outputCreator.Data = Data;
        _outputCreator.Unit = Unit;
        _outputCreator.Modifiers = ModifiersCollection.Count switch
        {
            > 0 => ModifiersCollection.ToArray(),
            0 => null,
            _ => _outputCreator.Modifiers
        }; 
        
        _outputCreator.Display = Display;
        return base.Copy();
    }

    private int[]? GetOutputs()
    {
        if (Output is null)
        {
            return null;
        }

        return [Output.Position];
    }

    private string? GetDescription()
    {
        if (!string.IsNullOrEmpty(Description))
        {
            return Description;
        }

        if (Data is null)
        {
            return Data;
        }

        if (Unit is not null)
        {
            return Data + " [" + Unit + "]";
        }

        return Data;
    }

    public string? Description { get; set; }

    [ObservableProperty]
    public partial string? OutputType { get; set; }

    partial void OnOutputTypeChanged(string? value)
    {
        switch (value)
        {
            case ProfileCreatorModel.Led:
            case ProfileCreatorModel.Dataline:
                Components = GetComponents(value);
                break;

            case ProfileCreatorModel.SevenSegment:
                Components = GetComponents(value);
                IsDisplay = true;
                Display = new Display();
                return;

            case ProfileCreatorModel.Analog:
                Components = GetComponents(value);
                break;
        }

        IsDisplay = false;
        Display = null;
    }

    private IEnumerable<Component?>? GetComponents(string? value)
    {
        return value switch
        {
            ProfileCreatorModel.Led => DeviceService.Outputs?.Led.Components,
            ProfileCreatorModel.Dataline => DeviceService.Outputs?.Dataline.Components,
            ProfileCreatorModel.SevenSegment => DeviceService.Outputs?.SevenSegment.Components,
            ProfileCreatorModel.Analog => DeviceService.Outputs?.Analog.Components,
            _ => Components
        };
    }

    [ObservableProperty]
    public partial bool IsDisplay { get; set; }

    public static string[] OutputTypes => [ProfileCreatorModel.Led, ProfileCreatorModel.Dataline, ProfileCreatorModel.SevenSegment, ProfileCreatorModel.Analog];

    [ObservableProperty]
    public partial IEnumerable<Component?>? Components { get; set; }

    [ObservableProperty]
    public partial Component? Output { get; set; }

    partial void OnOutputChanged(Component? value)
    {
        if (Position is not null)
        {
            Position = value?.Position;
        }

        switch (OutputType)
        {
            case ProfileCreatorModel.Led:
                Task.Run(() => DeviceService.Outputs?.Led.PerformOperationOnAllComponents(async i => await DeviceService.SetLedAsync(i, false)));
                break;

            case ProfileCreatorModel.Dataline:
                Task.Run(() => DeviceService.Outputs?.Dataline.PerformOperationOnAllComponents(async i => await DeviceService.SetDatalineAsync(i, false)));
                break;

            case ProfileCreatorModel.SevenSegment:
                Task.Run(() => DeviceService.Outputs?.SevenSegment.PerformOperationOnAllComponents(async i => await DeviceService.SetSevenSegmentAsync(i, " ")));
                break;
            
            case ProfileCreatorModel.Analog:
                Task.Run(() => DeviceService.Outputs?.Analog.PerformOperationOnAllComponents(async i => await DeviceService.SetAnalogAsync(i, 0)));
                break;
        }
    }

    public int[]? Outputs { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<int> OutputsCollection { get; set; } = [];

    [RelayCommand]
    private void RemoveOutputs(IList list)
    {
        List<int> outputs = [..list.Cast<int>()];
        foreach (int output in outputs)
        {
            OutputsCollection.Remove(output);
        }

        if (OutputsCollection.Count > 0)
        {
            Position = OutputsCollection[^1];
        }
    }

    [ObservableProperty]
    public partial int? Position { get; set; }

    [RelayCommand]
    private void AddOutput()
    {
        if (Output is null || OutputsCollection.Contains(Output.Position))
        {
            return;
        }

        OutputsCollection.Add(Output.Position);
        OutputsCollection = new ObservableCollection<int>(OutputsCollection.Order());
        Position = Output.Position;
    }

    private string? _dataType;

    public string? DataType
    {
        get => _dataType;
        set
        {
            IsMsfsSimConnect = value switch
            {
                ProfileCreatorModel.MsfsSimConnect => true,
                ProfileCreatorModel.Pmdg737 => false,
                ProfileCreatorModel.Pmdg777 => false,
                ProfileCreatorModel.Dim => false,
                _ => IsMsfsSimConnect
            };

            switch (value)
            {
                case ProfileCreatorModel.MsfsSimConnect:
                    IsMsfsSimConnect = true;
                    break;

                case ProfileCreatorModel.Pmdg737:
                    IsPmdg737 = true;
                    break;

                case ProfileCreatorModel.Pmdg777:
                    IsPmdg777 = true;
                    break;

                case ProfileCreatorModel.Dim:
                    IsDim = true;
                    break;
            }

            _dataType = value;
        }
    }

    [ObservableProperty]
    public partial bool IsMsfsSimConnect { get; set; }

    partial void OnIsMsfsSimConnectChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        DataType = ProfileCreatorModel.MsfsSimConnect;
        OnMsfsSimConnectChanged();
        IsDim = false;
    }

    private void OnMsfsSimConnectChanged()
    {
        IsPmdg737 = false;
        IsPmdg777 = false;
        IsPmdg = false;
        SearchPmdgData = null;
        Data = null;
        Unit = null;
    }

    [ObservableProperty]
    public partial bool IsPmdg { get; set; }

    [ObservableProperty]
    public partial bool IsPmdg737 { get; set; }

    partial void OnIsPmdg737Changed(bool value)
    {
        if (!value)
        {
            return;
        }

        DataType = ProfileCreatorModel.Pmdg737;
        OnPmdgChanged();
        PmdgDataEnumerable = typeof(B737.Data).GetFields().Select(field => field.Name);
    }

    [ObservableProperty]
    public partial bool IsPmdg777 { get; set; }

    partial void OnIsPmdg777Changed(bool value)
    {
        if (!value)
        {
            return;
        }

        DataType = ProfileCreatorModel.Pmdg777;
        OnPmdgChanged();
        PmdgDataEnumerable = typeof(B777.Data).GetFields().Select(field => field.Name);
    }

    private void OnPmdgChanged()
    {
        IsMsfsSimConnect = false;
        IsDim = false;
        IsPmdg = true;
        Data = null;
        Unit = null;
        SearchPmdgData = null;
    }

    [ObservableProperty]
    public partial bool IsDim { get; set; }

    partial void OnIsDimChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        DataType = ProfileCreatorModel.Dim;
        IsMsfsSimConnect = false;
        OnMsfsSimConnectChanged();
        Data = null;
        Unit = null;
    }


    [ObservableProperty]
    public partial string? Data { get; set; }

    partial void OnDataChanged(string? value)
    {
        if (value == string.Empty)
        {
            Data = null;
        }

        if (!(DataType == ProfileCreatorModel.Pmdg737 | DataType == ProfileCreatorModel.Pmdg747 | DataType == ProfileCreatorModel.Pmdg777))
        {
            return;
        }

        OnPropertyChanged(nameof(PmdgDataArrayIndices));
        Unit = PmdgDataArrayIndices.FirstOrDefault();
    }

    [ObservableProperty]
    public partial string? Unit { get; set; }

    partial void OnUnitChanged(string? value)
    {
        if (value == string.Empty)
        {
            Unit = null;
        }
    }

    [ObservableProperty]
    public partial string? SearchPmdgData { get; set; }

    [ObservableProperty]
    public partial IEnumerable<string?>? PmdgDataEnumerable { get; set; }

    public string?[] PmdgDataArrayIndices => GetPmdgDataArrayIndices();

    private string?[] GetPmdgDataArrayIndices()
    {
        if (string.IsNullOrEmpty(Data))
        {
            return [];
        }

        if (IsPmdg737 && typeof(B737.Data).GetField(Data)?.GetCustomAttribute<MarshalAsAttribute>() is { } attribute1 && attribute1.Value != UnmanagedType.ByValTStr && attribute1.SizeConst is var size1)
        {
            return Enumerable.Range(0, size1).Select(i => i.ToString()).ToArray();
        }

        if (IsPmdg777 && typeof(B777.Data).GetField(Data)?.GetCustomAttribute<MarshalAsAttribute>() is { } attribute2 && attribute2.Value != UnmanagedType.ByValTStr && attribute2.SizeConst is var size2)
        {
            return Enumerable.Range(0, size2).Select(i => i.ToString()).ToArray();
        }

        return [];
    }

    public IModifier[]? Modifiers { get; set; }

    public ObservableCollection<IModifier> ModifiersCollection { get; set; }

    [RelayCommand]
    private void AddModifier(string type)
    {
        switch (type)
        {
            case nameof(Transformation):
                ModifiersCollection.Add(new Transformation());
                break;

            case nameof(Comparison):
                ModifiersCollection.Add(new Comparison());
                break;

            case nameof(Interpolation):
                ModifiersCollection.Add(new Interpolation());
                break;

            case nameof(Padding):
                ModifiersCollection.Add(new Padding());
                break;

            case nameof(Inserting):
                ModifiersCollection.Add(new Inserting());
                break;

            case nameof(Substring):
                ModifiersCollection.Add(new Substring());
                break;

            case nameof(Blinking):
                ModifiersCollection.Add(new Blinking());
                break;
        }
    }

    [RelayCommand]
    private void RemoveModifier(IModifier modifier)
    {
        ModifiersCollection.Remove(modifier);
    }

    [ObservableProperty]
    public partial Display? Display { get; set; }

    private async Task SetOutputPosition(int position, bool isEnabled)
    {
        switch (OutputType)
        {
            case ProfileCreatorModel.Led:
                await DeviceService.SetLedAsync(position, isEnabled);
                break;

            case ProfileCreatorModel.Dataline:
                await DeviceService.SetDatalineAsync(position, isEnabled);
                break;

            case ProfileCreatorModel.SevenSegment:
                await DeviceService.SetSevenSegmentAsync(position, isEnabled ? "8" : " ");
                break;
            
            case ProfileCreatorModel.Analog:
                await DeviceService.SetAnalogAsync(position, isEnabled ? 100 : 0);
                break;
        }
    }

    [RelayCommand]
    private async Task PointerEnteredComboBox(string? position)
    {
        await SetOutputPosition(Convert.ToInt32(position), true);
    }

    [RelayCommand]
    private async Task PointerExitedComboBox(string? position)
    {
        await SetOutputPosition(Convert.ToInt32(position), false);
    }
}