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
using DeviceInterfaceManager.Models.FlightSim.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputCreatorViewModel : BaseCreatorViewModel, IOutputCreator
{
    private readonly IOutputCreator _outputCreator;

    public OutputCreatorViewModel(IInputOutputDevice inputOutputDevice, IOutputCreator outputCreator, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
        : base(inputOutputDevice, outputCreators, preconditions)
    {
        _outputCreator = outputCreator;
        OutputType = outputCreator.OutputType;
        Components = GetComponents(OutputType);
        Output = Components.FirstOrDefault(x => x?.Position == outputCreator.Output?.Position);
        DataType = outputCreator.DataType;
        PmdgData = outputCreator.PmdgData;
        PmdgDataArrayIndex = outputCreator.PmdgDataArrayIndex;
        Operator = outputCreator.Operator;
        ComparisonValue = outputCreator.ComparisonValue;
        TrueValue = outputCreator.TrueValue;
        FalseValue = outputCreator.FalseValue;
        Data = outputCreator.Data;
        Unit = outputCreator.Unit;
        IsInverted = outputCreator.IsInverted;
        NumericFormat = outputCreator.NumericFormat;
        IsPadded = outputCreator.IsPadded;
        PaddingCharacter = outputCreator.PaddingCharacter;
        Digits = CreateDigits(outputCreator.DigitCount, outputCreator.DigitCheckedSum, outputCreator.DecimalPointCheckedSum);
        DigitCount = outputCreator.DigitCount;
        SubstringStart = outputCreator.SubstringStart;
        SubstringEnd = outputCreator.SubstringEnd;

        SearchPmdgData = PmdgData;
    }

#if DEBUG
    public OutputCreatorViewModel()
    {
        _outputCreator = new OutputCreator
        {
            IsActive = true,
            Preconditions = [new Precondition()],
            Description = "Description",
            OutputType = ProfileCreatorModel.Led,
            Output = new Component(1)
        };
        Components = new List<Component>();
        Digits = CreateDigits(3, 1, 1);
    }
#endif

    public override Precondition[]? Copy()
    {
        _outputCreator.OutputType = OutputType;
        _outputCreator.Output = Output;
        _outputCreator.DataType = DataType;
        _outputCreator.PmdgData = PmdgData;
        _outputCreator.PmdgDataArrayIndex = PmdgDataArrayIndex;
        _outputCreator.Operator = Operator;
        _outputCreator.ComparisonValue = ComparisonValue;
        _outputCreator.TrueValue = TrueValue;
        _outputCreator.FalseValue = FalseValue;
        _outputCreator.Data = Data;
        _outputCreator.Unit = Unit;
        _outputCreator.IsInverted = IsInverted;
        _outputCreator.NumericFormat = NumericFormat;
        _outputCreator.IsPadded = IsPadded;
        _outputCreator.PaddingCharacter = PaddingCharacter;
        _outputCreator.DigitCount = DigitCount;
        SetCheckedSum();
        _outputCreator.SubstringStart = SubstringStart;
        _outputCreator.SubstringEnd = SubstringEnd;
        return base.Copy();
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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvertedEnabled))]
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
                IsInverted = false;
                SwitchDataType();
                return;
        }

        IsDisplay = false;
        NumericFormat = null;
        IsPadded = null;
        PaddingCharacterPair = null;
        DigitCount = null;
        SubstringStart = null;
        SubstringEnd = null;
    }

    private IEnumerable<Component?> GetComponents(string? value)
    {
        return value switch
        {
            ProfileCreatorModel.Led => InputOutputDevice.Led.Components,
            ProfileCreatorModel.Dataline => InputOutputDevice.Dataline.Components,
            ProfileCreatorModel.SevenSegment => InputOutputDevice.SevenSegment.Components,
            _ => Components
        };
    }

    [ObservableProperty]
    private bool _isDisplay;

    public static string[] OutputTypes => [ProfileCreatorModel.Led, ProfileCreatorModel.Dataline, ProfileCreatorModel.SevenSegment];

    [ObservableProperty]
    private IEnumerable<Component?> _components;

    [ObservableProperty]
    private Component? _output;

    partial void OnOutputChanged(Component? value)
    {
        switch (OutputType)
        {
            case ProfileCreatorModel.Led:
                Task.Run(() => InputOutputDevice.Led.PerformOperationOnAllComponents(async i => await InputOutputDevice.SetLedAsync(i, false)));
                break;

            case ProfileCreatorModel.Dataline:
                Task.Run(() => InputOutputDevice.Dataline.PerformOperationOnAllComponents(async i => await InputOutputDevice.SetDatalineAsync(i, false)));
                break;

            case ProfileCreatorModel.SevenSegment:
                Task.Run(() => InputOutputDevice.SevenSegment.PerformOperationOnAllComponents(async i => await InputOutputDevice.SetSevenSegmentAsync(i, " ")));
                break;
        }
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
            }

            _dataType = value;
            SwitchDataType();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvertedEnabled))]
    private bool _isMsfsSimConnect;

    partial void OnIsMsfsSimConnectChanged(bool value)
    {
        if (!value)
        {
            return;
        }

        DataType = ProfileCreatorModel.MsfsSimConnect;
        IsPmdg737 = false;
        IsPmdg777 = false;
        IsPmdg = false;
        SearchPmdgData = null;
        PmdgData = null;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvertedEnabled))]
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
        IsPmdg = true;
        Data = null;
        Unit = null;
        PmdgData = null;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsInvertedEnabled))]
    private string? _pmdgData;

    partial void OnPmdgDataChanged(string? value)
    {
        OnPropertyChanged(nameof(PmdgDataArrayIndices));
        PmdgDataArrayIndex = PmdgDataArrayIndices.FirstOrDefault();
        SwitchDataType();
    }

    private void SwitchDataType()
    {
        Operator = null;
        ComparisonValue = null;
        TrueValue = null;
        FalseValue = null;
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

    public bool IsInvertedEnabled => GetIsInvertedEnabled();

    private bool GetIsInvertedEnabled()
    {
        if (IsDisplay)
        {
            return false;
        }
        
        if (!IsPmdg)
        {
            return true;
        }
        
        if (string.IsNullOrEmpty(PmdgData))
        {
            return false;
        }

        if (IsPmdg737 && typeof(B737.Data).GetField(PmdgData)?.FieldType == typeof(bool) || typeof(B737.Data).GetField(PmdgData)?.FieldType == typeof(bool[]))
        {
            return true;
        }
        
        if (IsPmdg777 && typeof(B777.Data).GetField(PmdgData)?.FieldType == typeof(bool) || typeof(B777.Data).GetField(PmdgData)?.FieldType == typeof(bool[]))
        {
            return true;
        }

        return false;
    }
    
    [ObservableProperty]
    private char? _operator;

    [RelayCommand]
    private void ClearOperator()
    {
        Operator = null;
    }

    [ObservableProperty]
    private string? _comparisonValue;

    [ObservableProperty]
    private double? _trueValue;

    [ObservableProperty]
    private double? _falseValue;

    [ObservableProperty]
    private string? _data;

    [ObservableProperty]
    private string? _unit;

    [ObservableProperty]
    private bool _isInverted;

    [ObservableProperty]
    private string? _numericFormat;

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

    [ObservableProperty]
    private byte? _substringStart;

    [ObservableProperty]
    private byte? _substringEnd;

    private async Task SetOutputPosition(string? position, bool isEnabled)
    {
        switch (OutputType)
        {
            case ProfileCreatorModel.Led:
                await InputOutputDevice.SetLedAsync(position, isEnabled);
                break;

            case ProfileCreatorModel.Dataline:
                await InputOutputDevice.SetDatalineAsync(position, isEnabled);
                break;

            case ProfileCreatorModel.SevenSegment:
                await InputOutputDevice.SetSevenSegmentAsync(position, isEnabled ? "8" : " ");
                break;
        }
    }

    [RelayCommand]
    private async Task PointerEnteredComboBox(string? position)
    {
        await SetOutputPosition(position, true);
    }

    [RelayCommand]
    private async Task PointerExitedComboBox(string? position)
    {
        await SetOutputPosition(position, false);
    }
}