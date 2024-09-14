using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;
using DeviceInterfaceManager.Models.Modifiers;
using DeviceInterfaceManager.ViewModels;

namespace DeviceInterfaceManager.Behaviors;

public class ItemsControlDropHandler : DropHandlerBase
{
    private bool Validate<T>(ItemsControl itemsControl, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute) where T : IModifier
    {
        if (sourceContext is not T sourceItem
            || targetContext is not OutputCreatorViewModel vm
            || itemsControl.GetVisualAt(e.GetPosition(itemsControl)) is not Control { DataContext: T targetItem })
        {
            return false;
        }

        var items = vm.ModifiersCollection;
        int sourceIndex = items.IndexOf(sourceItem);
        int targetIndex = items.IndexOf(targetItem);

        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        switch (e.DragEffects)
        {
            case DragDropEffects.Move:
            {
                if (bExecute)
                {
                    MoveItem(items, sourceIndex, targetIndex);
                }

                return true;
            }

            case DragDropEffects.Link:
            {
                if (bExecute)
                {
                    SwapItem(items, sourceIndex, targetIndex);
                }

                return true;
            }

            default:
                return false;
        }
    }

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is ItemsControl itemsControl)
        {
            return Validate<IModifier>(itemsControl, e, sourceContext, targetContext, false);
        }

        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control && sender is ItemsControl itemsControl)
        {
            return Validate<IModifier>(itemsControl, e, sourceContext, targetContext, true);
        }

        return false;
    }
}