using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.Models.Devices;

namespace DeviceInterfaceManager.ViewModels;

public abstract partial class BaseCreatorViewModel : ObservableObject
{
    protected readonly IInputOutputDevice InputOutputDevice;

    protected BaseCreatorViewModel(IInputOutputDevice inputOutputDevice, IReadOnlyCollection<OutputCreator> outputCreators, IEnumerable<IPrecondition>? preconditions)
    {
        InputOutputDevice = inputOutputDevice;
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
        InputOutputDevice = new DeviceSerialBase();
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

    public static char[] Operators => ['=', '≠', '<', '>', '≤', '≥'];
    
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