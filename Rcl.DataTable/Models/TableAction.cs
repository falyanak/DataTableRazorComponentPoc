namespace Rcl.DataTable.Models;

public class TableAction
{
    public string Icon { get; set; } = "";
    public string Title { get; set; } = ""; // C'est le texte du bouton
    public string Url { get; set; } = "";
    public string ColorClass { get; set; } = "text-primary";
    public bool IsDelete { get; set; } = false;
    
    // Helper pour générer les attributs HTMX de confirmation si c'est une suppression
    public string GetHtmxConfirm() => IsDelete ? "confirm('Êtes-vous sûr ?')" : "";
}