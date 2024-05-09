using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactions.DragAndDrop;

namespace DeviceInterfaceManager.Behaviors;

public abstract class BaseDataGridDropHandler<T> : DropHandlerBase
{
    private const string RowDraggingUpStyleClass = "DraggingUp";
    private const string RowDraggingDownStyleClass = "DraggingDown";

    protected abstract T MakeCopy(T item);

    protected abstract bool Validate(DataGrid dg, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute);

    public override bool Validate(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        if (e.Source is Control c && sender is DataGrid dg)
        {
            bool valid = Validate(dg, e, sourceContext, targetContext, false);
            if (!valid)
            {
                return valid;
            }

            DataGridRow row = FindDataGridRowFromChildView(c);
            string direction = e.Data.Contains("direction") ? (string)e.Data.Get("direction")! : "down";
            ApplyDraggingStyleToRow(row, direction);
            ClearDraggingStyleFromAllRows(sender, row);
            return valid;
        }

        ClearDraggingStyleFromAllRows(sender);
        return false;
    }

    public override bool Execute(object? sender, DragEventArgs e, object? sourceContext, object? targetContext, object? state)
    {
        ClearDraggingStyleFromAllRows(sender);
        if (e.Source is Control && sender is DataGrid dg)
        {
            return Validate(dg, e, sourceContext, targetContext, true);
        }

        return false;
    }

    public override void Cancel(object? sender, RoutedEventArgs e)
    {
        base.Cancel(sender, e);
        // this is necessary to clear adorner borders when mouse leaves DataGrid
        // they would remain even after changing screens
        ClearDraggingStyleFromAllRows(sender);
    }

    protected bool RunDropAction(DataGrid dg, DragEventArgs e, bool bExecute, T sourceItem, T targetItem, ObservableCollection<T> items)
    {
        int sourceIndex = items.IndexOf(sourceItem);
        int targetIndex = items.IndexOf(targetItem);

        if (sourceIndex < 0 || targetIndex < 0)
        {
            return false;
        }

        switch (e.DragEffects)
        {
            case DragDropEffects.Copy:
            {
                if (!bExecute)
                {
                    return true;
                }

                T clone = MakeCopy(sourceItem);
                InsertItem(items, clone, targetIndex + 1);
                dg.SelectedIndex = targetIndex + 1;
                return true;
            }

            case DragDropEffects.Move:
            {
                if (!bExecute)
                {
                    return true;
                }

                MoveItem(items, sourceIndex, targetIndex);
                dg.SelectedIndex = targetIndex;
                return true;
            }

            case DragDropEffects.Link:
            {
                if (!bExecute)
                {
                    return true;
                }

                SwapItem(items, sourceIndex, targetIndex);
                dg.SelectedIndex = targetIndex;
                return true;
            }

            case DragDropEffects.None:
            default:
                return false;
        }
    }

    private static DataGridRow FindDataGridRowFromChildView(StyledElement sourceChild)
    {
        int maxDepth = 16;
        DataGridRow? row = null;
        StyledElement? current = sourceChild;
        while (maxDepth-- > 0 || row is null)
        {
            if (current is DataGridRow dataGridRowsPresenter)
            {
                row = dataGridRowsPresenter;
            }

            current = current?.Parent;
        }

        return row;
    }

    private static DataGridRowsPresenter? GetRowsPresenter(Visual v)
    {
        foreach (Visual cv in v.GetVisualChildren())
        {
            if (cv is DataGridRowsPresenter dataGridRowsPresenter)
            {
                return dataGridRowsPresenter;
            }

            if (GetRowsPresenter(cv) is { } dataGridRowsPresenter2)
            {
                return dataGridRowsPresenter2;
            }
        }

        return null;
    }

    private static void ClearDraggingStyleFromAllRows(object? sender, DataGridRow? exceptThis = null)
    {
        if (sender is not DataGrid dg)
        {
            return;
        }

        DataGridRowsPresenter? presenter = GetRowsPresenter(dg);
        if (presenter is null)
        {
            return;
        }

        foreach (Control? r in presenter.Children.Where(r => r != exceptThis))
        {
            r.Classes.Remove(RowDraggingUpStyleClass);
            r.Classes.Remove(RowDraggingDownStyleClass);
        }
    }

    private static void ApplyDraggingStyleToRow(StyledElement row, string direction)
    {
        switch (direction)
        {
            case "up":
            {
                row.Classes.Remove(RowDraggingDownStyleClass);
                if (row.Classes.Contains(RowDraggingUpStyleClass) == false)
                {
                    row.Classes.Add(RowDraggingUpStyleClass);
                }

                break;
            }

            case "down":
            {
                row.Classes.Remove(RowDraggingUpStyleClass);
                if (row.Classes.Contains(RowDraggingDownStyleClass) == false)
                {
                    row.Classes.Add(RowDraggingDownStyleClass);
                }

                break;
            }
        }
    }
}