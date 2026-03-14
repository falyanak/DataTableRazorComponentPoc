namespace Rcl.DataTable.Models;

public class ColumnDefinition
{
    public string Display { get; set; } = string.Empty;
    public string Prop { get; set; } = string.Empty;
    public bool IsSecondary { get; set; } // Si true -> va dans le volet sur mobile
    public string ResponsiveClass { get; set; } = string.Empty; // ex: d-none d-md-table-cell
}