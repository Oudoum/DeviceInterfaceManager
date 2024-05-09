using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.ViewModels;

namespace DeviceInterfaceManager.Behaviors;

public class OutputCreatorDataGridDropHandler : BaseDataGridDropHandler<OutputCreator>
{
    protected override OutputCreator MakeCopy(OutputCreator item)
    {
        return item;
    }

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not OutputCreator sourceItem
            || targetContext is not ProfileCreatorViewModel vm
            || vm.ProfileCreatorModel is null
            || dg.GetVisualAt(e.GetPosition(dg)) is not Control { DataContext: OutputCreator targetItem })
        {
            return false;
        }

        var items = vm.ProfileCreatorModel.OutputCreators;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);

    }
}