using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace DeviceInterfaceManager.Models;

public partial class PreconditionModel : Precondition
{
    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private bool _hasError;

    public PreconditionModel(OutputCreator outputCreator)
    {
        IsActive = true;
        Operator = '=';
        ReferenceId = outputCreator.Id;
        Description = outputCreator.Description;
    }

    public PreconditionModel(IPrecondition precondition, IEnumerable<OutputCreator> outputCreators)
    {
        IsActive = precondition.IsActive;
        Operator = precondition.Operator;
        ComparisonValue = precondition.ComparisonValue;
        IsOrOperator = precondition.IsOrOperator;

        OutputCreator? matchingOutputCreator = outputCreators.FirstOrDefault(oc => oc.Id == precondition.ReferenceId);
        if (matchingOutputCreator is not null)
        {
            Description = matchingOutputCreator.Description;
            ReferenceId = precondition.ReferenceId;
            return;
        }

        HasError = true;
    }
}