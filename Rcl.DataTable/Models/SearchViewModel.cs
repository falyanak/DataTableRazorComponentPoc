namespace Rcl.DataTable.Models;

public class SearchViewModel
{
    public bool IsVisible { get; set; } = false;
    public string? SearchUrl { get; set; }
    public string? EraseSearchUrl { get; set; }
    public string? SearchTerm { get; set; }
    public string Placeholder { get; set; } = "Rechercher...";
}