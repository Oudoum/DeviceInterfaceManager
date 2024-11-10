using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Services.Devices;

namespace DeviceInterfaceManager.ViewModels;

public abstract partial class BaseCreatorViewModel : ObservableObject
{
    protected readonly IDeviceService DeviceService;

    protected BaseCreatorViewModel(IDeviceService deviceService, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
    {
        DeviceService = deviceService;
        OutputCreators = outputCreators;
        if (preconditions is null)
        {
            return;
        }

        Preconditions = new ObservableCollection<PreconditionModel>(preconditions.Select(precondition => new PreconditionModel(precondition, outputCreators)));
        SelectedPrecondition = Preconditions.FirstOrDefault();
        SelectedOutputCreator = OutputCreators.FirstOrDefault(oc => SelectedPrecondition is not null && oc.Id == SelectedPrecondition.ReferenceId);
    }

#if DEBUG
    protected BaseCreatorViewModel()
    {
        DeviceService = new DeviceSerialService();
        OutputCreators =
        [
            new OutputCreator
            {
                IsActive = true,
                Preconditions = [new Precondition()],
                Description = "Description 1",
                OutputType = ProfileCreatorModel.Led,
                Outputs = [1, 2, 3]
            },
            new OutputCreator
            {
                IsActive = true,
                Preconditions = [new Precondition()],
                Description = "Description 2",
                OutputType = ProfileCreatorModel.Led,
                Outputs = [1, 2, 3]
            }
        ];
    }
#endif

    public virtual Precondition[]? Copy()
    {
        if (Preconditions is null || Preconditions.Count == 0)
        {
            return null;
        }

        List<Precondition> preconditions = [];
        preconditions.AddRange(Preconditions);
        return preconditions.ToArray();
    }

    public IEnumerable<OutputCreator> OutputCreators { get; set; }

    [ObservableProperty]
    private OutputCreator? _selectedOutputCreator;

    partial void OnSelectedOutputCreatorChanged(OutputCreator? value)
    {
        if (SelectedPrecondition is null || value is null)
        {
            return;
        }

        SelectedPrecondition.HasError = false;
        SelectedPrecondition.ReferenceId = value.Id;
        SelectedPrecondition.Description = value.Description;
    }

    [ObservableProperty]
    private ObservableCollection<PreconditionModel>? _preconditions;

    [ObservableProperty]
    private PreconditionModel? _selectedPrecondition;

    partial void OnSelectedPreconditionChanged(PreconditionModel? value)
    {
        SelectedOutputCreator = OutputCreators.FirstOrDefault(x => value is not null && x.Id == value.ReferenceId);
    }

    public static char[] Operators => Models.Modifiers.Comparison.Operators;

    [RelayCommand]
    private void AddPrecondition()
    {
        if (!OutputCreators.Any())
        {
            return;
        }

        Preconditions ??= [];
        SelectedOutputCreator ??= OutputCreators.First();
        PreconditionModel preconditionModel = new(SelectedOutputCreator);
        Preconditions.Add(preconditionModel);
        SelectedPrecondition = preconditionModel;
    }

    [RelayCommand]
    private void RemovePrecondition()
    {
        if (SelectedPrecondition is null || Preconditions is null)
        {
            return;
        }

        Preconditions.Remove(SelectedPrecondition);
        if (Preconditions.Count == 0)
        {
            SelectedPrecondition = null;
            Preconditions = null;
            return;
        }

        SelectedPrecondition = Preconditions[^1];
    }

    [RelayCommand]
    private void ClearPreconditions()
    {
        if (Preconditions is null)
        {
            return;
        }

        Preconditions.Clear();
        SelectedPrecondition = null;
        Preconditions = null;
    }

    [RelayCommand]
    private void ChangeLogicalOperator(string logicalOperator)
    {
        if (SelectedPrecondition is not null)
        {
            SelectedPrecondition.IsOrOperator = logicalOperator switch
            {
                "AND" => false,
                "OR" => true,
                _ => SelectedPrecondition.IsOrOperator
            };
        }
    }
}