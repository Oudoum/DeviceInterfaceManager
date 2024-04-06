using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;
using DeviceInterfaceManager.Models.SimConnect.MSFS.PMDG.SDK;

namespace DeviceInterfaceManager.ViewModels;

public partial class OutputCreatorViewModel : BaseCreatorViewModel, IOutputCreator
{
    private readonly IOutputCreator _outputCreator;

    public OutputCreatorViewModel(IInputOutputDevice inputOutputDevice, IOutputCreator outputCreator, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
        : base(inputOutputDevice, outputCreators, preconditions)
    {
        _outputCreator = outputCreator;
        OutputType = outputCreator.OutputType;
        Output = outputCreator.Output;
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
            Output = new Component(1),
        };
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
        _outputCreator.DecimalPointCheckedSum = DigitCheckedSum;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComparisonEnabled))]
    private string? _outputType;

    partial void OnOutputTypeChanged(string? value)
    {
        switch (value)
        {
            case ProfileCreatorModel.Led:
                Components = InputOutputDevice.Led.Components;
                break;

            case ProfileCreatorModel.Dataline:
                Components = InputOutputDevice.Dataline.Components;
                break;

            case ProfileCreatorModel.SevenSegment:
                Components = InputOutputDevice.SevenSegment.Components;
                IsDisplay = true;

                IsPadded = false;
                IsInverted = false;
                SwitchDataType();
                return;
        }

        IsDisplay = false;
        IsPadded = null;
        PaddingCharacterPair = null;
        DigitCount = null;
        SubstringStart = null;
        SubstringEnd = null;
    }

    [ObservableProperty]
    private bool _isDisplay;

    public static string[] OutputTypes => [ProfileCreatorModel.Led, ProfileCreatorModel.Dataline, ProfileCreatorModel.SevenSegment];

    [ObservableProperty]
    private IEnumerable<Component?>? _components;

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComparisonEnabled))]
    private string? _dataType;

    partial void OnDataTypeChanged(string? value)
    {
        switch (value)
        {
            case ProfileCreatorModel.MsfsSimConnect:
                SearchPmdgData = null;
                PmdgData = null;
                break;

            case ProfileCreatorModel.Pmdg737:
                Data = null;
                Unit = null;
                break;
        }

        SwitchDataType();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsComparisonEnabled))]
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

    public static Func<string?, CancellationToken, Task<IEnumerable<object>>?> AsyncPopulator => (input, token) => input != null ? SearchPmdgDataAsync(input) : null;

    private static async Task<IEnumerable<object>> SearchPmdgDataAsync(string input)
    {
        return await Task.Run(() =>
        {
            return string.IsNullOrEmpty(input)
                ? typeof(B737.Data).GetFields().Select(field => field.Name).Take(typeof(B737.Data).GetFields().Length - 1)
                : typeof(B737.Data).GetFields().Select(field => field.Name).Where(name => name.Contains(input, StringComparison.OrdinalIgnoreCase)).Take(typeof(B737.Data).GetFields().Length - 1);
        });
    }

    [ObservableProperty]
    private int? _pmdgDataArrayIndex;

    public int?[] PmdgDataArrayIndices => (string.IsNullOrEmpty(PmdgData)
        ? Array.Empty<int?>()
        : typeof(B737.Data).GetField(PmdgData)?.GetCustomAttribute<MarshalAsAttribute>() is { } attribute && attribute.Value != UnmanagedType.ByValTStr && attribute.SizeConst is var size
            ? new int?[size].Select((_, i) => i).Cast<int?>().ToArray()
            : null) ?? Array.Empty<int?>();

    public bool IsComparisonEnabled => (!string.IsNullOrEmpty(PmdgData) && (typeof(B737.Data).GetField(PmdgData)?.FieldType == typeof(bool) || typeof(B737.Data).GetField(PmdgData)?.FieldType == typeof(bool[]))
                                                                        && DataType == ProfileCreatorModel.Pmdg737 && !IsDisplay)
                                       || (DataType != ProfileCreatorModel.Pmdg737 && !IsDisplay);

    [ObservableProperty]
    private char? _operator;

    private string? _comparisonValue;

    public string? ComparisonValue
    {
        get => string.IsNullOrEmpty(PmdgData)
               || typeof(B737.Data).GetField(PmdgData)?.FieldType != typeof(bool)
               || typeof(B737.Data).GetField(PmdgData)?.FieldType != typeof(bool[])
               || DataType == ProfileCreatorModel.MsfsSimConnect
            ? _comparisonValue
            : null;
        set => SetProperty(ref _comparisonValue, value);
    }

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

    [ObservableProperty]
    private char? _paddingCharacter;

    public byte[] DigitCounts { get; } = [1, 2, 3, 4, 5, 6, 7, 8];

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
                await InputOutputDevice.SetSevenSegmentAsync(position, isEnabled ? "8" : " " );
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