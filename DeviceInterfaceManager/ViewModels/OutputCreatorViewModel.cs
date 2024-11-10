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
        Unit = outputCreator.Unit;
        PmdgData = outputCreator.PmdgData;
        PmdgDataArrayIndex = outputCreator.PmdgDataArrayIndex;
        ModifiersCollection = new ObservableCollection<IModifier>(outputCreator.Modifiers ?? []);
        IsPadded = outputCreator.IsPadded;
        PaddingCharacter = outputCreator.PaddingCharacter;
        Digits = CreateDigits(outputCreator.DigitCount, outputCreator.DigitCheckedSum, outputCreator.DecimalPointCheckedSum);
        DigitCount = outputCreator.DigitCount;

        SearchPmdgData = PmdgData;
    }

#if DEBUG
    public OutputCreatorViewModel()
    {
        OutputsCollection =
        [
            1,
            2,
            3
        ];

        _outputCreator = new OutputCreator
        {
            IsActive = true,
            Preconditions = [new Precondition()],
            Description = "Description",
            OutputType = ProfileCreatorModel.Led,
            Outputs = OutputsCollection.ToArray()
        };
        Components =
        [
            new Component(1),
            new Component(2),
            new Component(3),
            new Component(4)
        ];
        Digits = CreateDigits(3, 1, 1);

        ModifiersCollection =
        [
            new Transformation(),
            new Comparison(),
            new Interpolation(),
            new Padding(),
            new Substring()
        ];
    }
#endif

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
        _outputCreator.PmdgData = PmdgData;
        _outputCreator.PmdgDataArrayIndex = PmdgDataArrayIndex;
        _outputCreator.Modifiers = ModifiersCollection.Count switch
        {
            > 0 => ModifiersCollection.ToArray(),
            0 => null,
            _ => _outputCreator.Modifiers
        };

        _outputCreator.IsPadded = IsPadded;
        _outputCreator.PaddingCharacter = PaddingCharacter;
        _outputCreator.DigitCount = DigitCount;
        SetCheckedSum();
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

        if (Data is not null)
        {
            if (Unit is not null)
            {
                return Data + " [" + Unit + "]";
            }

            return Data;
        }

        if (PmdgData is null)
        {
            return null;
        }

        if (PmdgDataArrayIndex is not null)
        {
            return PmdgData + " [" + PmdgDataArrayIndex + "]";
        }

        return PmdgData;
    }

    private void SetCheckedSum()
    {
        if (Digits.Count > 0)
        {
            foreach (DigitFormatting digit in Digits)
            {
                DigitCheckedSum = digit.GetDigitCheckedSum(DigitCheckedSum);
                DecimalPointCheckedSum = digit.GetDecimalPointCheckedSum(DecimalPointCheckedSum);
            }
        }

        _outputCreator.DigitCheckedSum = DigitCheckedSum;
        _outputCreator.DecimalPointCheckedSum = DecimalPointCheckedSum;
    }

    public string? Description { get; set; }

    [ObservableProperty]
    private string? _outputType;

    partial void OnOutputTypeChanged(string? value)
    {
        switch (value)
        {
            case ProfileCreatorModel.Led:
                Components = GetComponents(value);
                break;

            case ProfileCreatorModel.Dataline:
                Components = GetComponents(value);
                break;

            case ProfileCreatorModel.SevenSegment:
                Components = GetComponents(value);
                IsDisplay = true;
                IsPadded = false;
                return;
            
            case ProfileCreatorModel.Analog:
                Components = GetComponents(value);
                break;
        }

        IsDisplay = false;
        IsPadded = null;
        PaddingCharacterPair = null;
        DigitCount = null;
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
    private bool _isDisplay;

    public static string[] OutputTypes => [ProfileCreatorModel.Led, ProfileCreatorModel.Dataline, ProfileCreatorModel.SevenSegment, ProfileCreatorModel.Analog];

    [ObservableProperty]
    private IEnumerable<Component?>? _components;

    [ObservableProperty]
    private Component? _output;

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
    private ObservableCollection<int> _outputsCollection = [];

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
    private int? _position;

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
    private bool _isMsfsSimConnect;

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
        PmdgData = null;
    }

    [ObservableProperty]
    private bool _isPmdg;

    [ObservableProperty]
    private bool _isPmdg737;

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
    private bool _isPmdg777;

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
        PmdgData = null;
    }

    [ObservableProperty]
    private bool _isDim;

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
    private string? _data;

    partial void OnDataChanged(string? value)
    {
        if (value == string.Empty)
        {
            Data = null;
        }
    }

    [ObservableProperty]
    private string? _unit;

    partial void OnUnitChanged(string? value)
    {
        if (value == string.Empty)
        {
            Unit = null;
        }
    }

    [ObservableProperty]
    private string? _pmdgData;

    partial void OnPmdgDataChanged(string? value)
    {
        OnPropertyChanged(nameof(PmdgDataArrayIndices));
        PmdgDataArrayIndex = PmdgDataArrayIndices.FirstOrDefault();
    }

    [ObservableProperty]
    private string? _searchPmdgData;

    [ObservableProperty]
    private IEnumerable<string?>? _pmdgDataEnumerable;

    [ObservableProperty]
    private int? _pmdgDataArrayIndex;

    public int?[] PmdgDataArrayIndices => GetPmdgDataArrayIndices();

    private int?[] GetPmdgDataArrayIndices()
    {
        if (string.IsNullOrEmpty(PmdgData))
        {
            return [];
        }

        if (IsPmdg737 && typeof(B737.Data).GetField(PmdgData)?.GetCustomAttribute<MarshalAsAttribute>() is { } attribute1 && attribute1.Value != UnmanagedType.ByValTStr && attribute1.SizeConst is var size1)
        {
            return new int?[size1].Select((_, i) => i).Cast<int?>().ToArray();
        }

        if (IsPmdg777 && typeof(B777.Data).GetField(PmdgData)?.GetCustomAttribute<MarshalAsAttribute>() is { } attribute2 && attribute2.Value != UnmanagedType.ByValTStr && attribute2.SizeConst is var size2)
        {
            return new int?[size2].Select((_, i) => i).Cast<int?>().ToArray();
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

            case nameof(Substring):
                ModifiersCollection.Add(new Substring());
                break;
        }
    }

    [RelayCommand]
    private void RemoveModifier(IModifier modifier)
    {
        ModifiersCollection.Remove(modifier);
    }

    [ObservableProperty]
    private bool? _isPadded;

    public static Dictionary<string, char?> PaddingCharacters => new() { ["Zero"] = '0', ["Space"] = ' ' };

    public KeyValuePair<string, char?>? PaddingCharacterPair
    {
        get
        {
            if (PaddingCharacter is null)
            {
                return null;
            }

            return PaddingCharacters.First(s => s.Value == PaddingCharacter);
        }
        set
        {
            PaddingCharacter = value?.Value;
            OnPropertyChanged();
        }
    }

    [RelayCommand]
    private void ClearPaddingCharacter()
    {
        PaddingCharacterPair = null;
    }

    [ObservableProperty]
    private char? _paddingCharacter;

    public byte[] DigitCounts { get; } = [1, 2, 3, 4, 5, 6, 7, 8];

    [RelayCommand]
    private void ClearDigitCount()
    {
        DigitCount = null;
    }

    [ObservableProperty]
    private byte? _digitCount;

    partial void OnDigitCountChanged(byte? value)
    {
        if (value is null)
        {
            Digits.Clear();
            return;
        }

        if (Digits.Count > value)
        {
            for (int i = Digits.Count - 1; i >= value; i--)
            {
                Digits[i].IsDigitChecked = false;
                Digits[i].IsDecimalPointChecked = false;
                Digits.RemoveAt(i);
            }

            return;
        }

        for (int i = Digits.Count; i < value; i++)
        {
            Digits.Add(new DigitFormatting(i + 1));
        }
    }

    [ObservableProperty]
    private ObservableCollection<DigitFormatting> _digits;

    private static ObservableCollection<DigitFormatting> CreateDigits(byte? digitCount, byte? digitCheckedSum, byte? decimalPointCheckedSum)
    {
        ObservableCollection<DigitFormatting> digits = [];
        for (int i = 0; i < digitCount; i++)
        {
            DigitFormatting digitFormatting = new(i + 1, digitCheckedSum, decimalPointCheckedSum);
            digits.Add(digitFormatting);
        }

        return digits;
    }

    public byte? DigitCheckedSum { get; set; }

    public byte? DecimalPointCheckedSum { get; set; }

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