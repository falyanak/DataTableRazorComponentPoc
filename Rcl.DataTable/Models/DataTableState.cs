
using System.Net.Http.Headers;

namespace Rcl.DataTable.Models;

public class DataTableState<TItem, TKey>
{
    public TKey? SelectedId { get; set; }
    public string? SortColumn { get; set; }
    public bool IsAscending { get; set; } = true;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;   // TODO : rendre dynamique
    public string? SearchTerm { get; set; }
    public bool IsFiltered => !string.IsNullOrWhiteSpace(SearchTerm);
    public List<TItem> FilteredItems { get; set; } = [];
}