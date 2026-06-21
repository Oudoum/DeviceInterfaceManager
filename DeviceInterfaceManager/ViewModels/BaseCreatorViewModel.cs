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
    public partial OutputCreator? SelectedOutputCreator { get; set; }

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
    public partial ObservableCollection<PreconditionModel>? Preconditions { get; set; }

    [ObservableProperty]
    public partial PreconditionModel? SelectedPrecondition { get; set; }

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