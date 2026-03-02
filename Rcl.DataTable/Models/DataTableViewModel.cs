namespace Rcl.DataTable.Models;

public class DataTableViewModel<TItem, TKey>
{
    public string TableId { get; set; } = "dt-default";
    public IEnumerable<TItem> Items { get; set; } = [];
    public List<(string Display, string Prop)> Columns { get; set; } = [];
    public Func<TItem, TKey> KeySelector { get; set; } = default!;

    /// <summary>
    /// Extrait la clé unique d'un élément de la liste
    /// </summary>
    public TKey GetKey(TItem item)
    {
        return KeySelector(item);
    }

    public TKey? SelectedId { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }

    public string? SortColumn { get; set; }
    public bool IsAsc { get; set; }

    public int PageSize { get; set; } = 10;
    public List<int> PageSizeOptions { get; set; } = [10, 20, 50, 100];

    public SearchViewModel Search { get; set; } = new(); 

    // Important : Ton générateur d'URL doit inclure le SearchTerm pour que 
    // la pagination et le tri conservent le filtre actif.
    public string GetFullUrl(string baseUrl) {
        return $"{baseUrl}?searchTerm={Search.SearchTerm}&sort={SortColumn}&isAsc={IsAsc}&page={CurrentPage}";
    }

 public bool HasActionButton { get; set; }

    // Pattern pour l'URL, ex: "/Products/Details/{0}"
    public string DetailUrlPattern { get; set; } = string.Empty;
    public string DeleteUrlPattern { get; set; } = string.Empty; // ex: "/Products/Delete/{0}"

    public string GetPageUrl(int page)
    {
        // On propage systématiquement la taille de page, le tri et l'ordre
        return $"/Products/Index?page={page}&pageSize={PageSize}&sort={SortColumn}";
    }

    public string GetSortUrl(string prop) => $"/Products?sort={prop}";
    public string GetFormattedValue(TItem item, string propName)
    {
        var prop = typeof(TItem).GetProperty(propName);
        var value = prop?.GetValue(item);

        if (value == null) return "-";

        return value switch
        {
            Guid g => g.ToString().Substring(0, 8) + "...", // Affiche un ID court plus élégant
            DateTime dt => dt.ToString("dd/MM/yyyy"),
            decimal d => d.ToString("C2", new System.Globalization.CultureInfo("fr-FR")),
            _ => value.ToString() ?? ""
        };
    }

    public string GetInfoPagePositionFormattedValue()
    {
        var culture = System.Globalization.CultureInfo.GetCultureInfo("fr-FR");
        var infoPage =  $"Page {CurrentPage.ToString("N0", culture)} / {TotalPages.ToString("N0", culture)}";
        return infoPage;    
    }

    public void ProcessData(int pageSize)
    {
        if (Items == null) return;

        // 1. On met à jour la propriété interne pour que GetPageUrl utilise la bonne valeur
        this.PageSize = pageSize;

        // 2. Tri (agnostique par réflexion)
        // TODO: Optimiser ce tri pour ne pas faire de réflexion à chaque fois (ex: cache des propriétés)
        if (!string.IsNullOrEmpty(SortColumn))
        {
            var prop = typeof(TItem).GetProperty(SortColumn);
            if (prop != null)
            {
                Items = IsAsc
                    ? Items.OrderBy(x => prop.GetValue(x, null)).ToList()
                    : Items.OrderByDescending(x => prop.GetValue(x, null)).ToList();
            }
        }

        // 3. RECALCUL CRITIQUE DU NOMBRE DE PAGES
        this.TotalPages = (int)Math.Ceiling((double)Items.Count() / pageSize);

        // 4. Pagination
        Items = Items.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();
    }
}