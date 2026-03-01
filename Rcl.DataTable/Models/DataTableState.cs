
namespace Rcl.DataTable.Models;

public class DataTableState<TKey>
{
    public TKey? SelectedId { get; set; }
    public string? SortColumn { get; set; }
    public bool IsAscending { get; set; } = true;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 15;
}