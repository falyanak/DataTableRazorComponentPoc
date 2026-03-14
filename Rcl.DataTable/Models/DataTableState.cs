public class DataTableState<TItem, TKey>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public bool IsAscending { get; set; } = true;
    public string? SearchTerm { get; set; }
    public TKey? SelectedId { get; set; }
    public List<TItem> FilteredItems { get; set; } = new();
    
    // NOUVEAU : Mémorise les lignes ouvertes (IDs)
    public List<string> ExpandedRows { get; set; } = new();

    public bool IsFiltered => !string.IsNullOrEmpty(SearchTerm);
}