namespace Rcl.DataTable.Models;

public class TableAction
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public string ColorClass { get; set; } = "text-primary";
    public bool IsDelete { get; set; } = false;
}