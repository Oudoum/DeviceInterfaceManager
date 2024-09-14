using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using DeviceInterfaceManager.Models;
using DeviceInterfaceManager.ViewModels;

namespace DeviceInterfaceManager.Behaviors;

public sealed class InputCreatorDataGridDropHandler : BaseDataGridDropHandler<InputCreator>
{
    protected override InputCreator MakeCopy(InputCreator item)
    {
        return item;
    }

    protected override bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not InputCreator sourceItem
            || targetContext is not ProfileCreatorViewModel vm
            || vm.ProfileCreatorModel is null
            || dg.GetVisualAt(e.GetPosition(dg)) is not Control { DataContext: InputCreator targetItem })
        {
            return false;
        }

        var items = vm.ProfileCreatorModel.InputCreators;
        return RunDropAction(dg, e, bExecute, sourceItem, targetItem, items);
    }
}