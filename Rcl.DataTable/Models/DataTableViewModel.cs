using System.Globalization;
using System.Reflection;

namespace Rcl.DataTable.Models;

public class DataTableViewModel<TItem, TKey>
{
    // --- PROPRIÉTÉS DE BASE ---
    public string TableId { get; set; } = "dt-default";
    public IEnumerable<TItem> Items { get; set; } = [];
    public List<ColumnDefinition> Columns { get; set; } = [];
    public Func<TItem, TKey> KeySelector { get; set; } = default!;

    // --- ÉTAT DE LA NAVIGATION ---
    public TKey? SelectedId { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalItems { get; set; } // Total absolu (ex: 1,000,000)
    public int FilteredCount { get; set; } // Total après filtrage
    public string? SortColumn { get; set; }
    public bool IsAsc { get; set; } = true;
    public int PageSize { get; set; } = 10;
    public List<int> PageSizeOptions { get; set; } = [10, 20, 50, 100];
    
    // --- COMPOSANTS ---
    public SearchViewModel Search { get; set; } = new();
    public List<string> ExpandedRows { get; set; } = new();
    public bool HasActionButton { get; set; }

    // --- MÉTHODES DE CLÉ ---
    public TKey GetKey(TItem item) => KeySelector(item);

    // --- GÉNÉRATION D'URLS (HTMX) ---
    public string GetToggleUrl(string rowId)
    {
        // On passe tout l'état actuel pour que le toggle ne réinitialise pas la vue
        return $"/Products/ToggleRow/{rowId}?page={CurrentPage}&pageSize={PageSize}&searchTerm={Uri.EscapeDataString(Search.SearchTerm ?? "")}&sort={SortColumn}&isAsc={IsAsc}";
    }

   public string BaseUrl { get; set; } = "/Products/Index"; // Valeur par défaut ou injectée

public string GetPageUrl(int page)
{
    // On concatène l'URL du contrôleur avec la QueryString
    return $"{BaseUrl}?page={page}&pageSize={PageSize}&searchTerm={Uri.EscapeDataString(Search.SearchTerm ?? "")}&sort={SortColumn}&isAsc={IsAsc}";
}
    public string GetSortUrl(string column)
    {
        // Si on clique sur la même colonne, on inverse le tri, sinon on passe en ASC
        bool nextAsc = (SortColumn == column) ? !IsAsc : true;
        return $"?page=1&pageSize={PageSize}&searchTerm={Uri.EscapeDataString(Search.SearchTerm ?? "")}&sort={column}&isAsc={nextAsc.ToString().ToLower()}";
    }

    // --- FORMATTAGE ---
    public string GetFormattedValue(TItem item, string propName)
    {
        PropertyInfo? prop = typeof(TItem).GetProperty(propName);
        object? value = prop?.GetValue(item);

        if (value == null) return "-";

        return value switch
        {
            Guid g => g.ToString().Substring(0, 8).ToUpper(),
            DateTime dt => dt.ToString("dd/MM/yyyy HH:mm"),
            decimal d => d.ToString("C2", CultureInfo.GetCultureInfo("fr-FR")),
            double db => db.ToString("N2", CultureInfo.GetCultureInfo("fr-FR")),
            _ => value.ToString() ?? ""
        };
    }

    public string GetInfoPagePositionFormattedValue()
    {
        CultureInfo culture = CultureInfo.GetCultureInfo("fr-FR");
        string info = $"Page {CurrentPage.ToString("N0", culture)} / {TotalPages.ToString("N0", culture)}";
        return info;
    }

    // --- MOTEUR DE TRAITEMENT ---
    public void ProcessData(int pageSize)
    {
        if (Items == null) return;

        this.PageSize = pageSize;
        IQueryable<TItem> query = Items.AsQueryable();

        // 1. Tri par réflexion
        if (!string.IsNullOrEmpty(SortColumn))
        {
            PropertyInfo? prop = typeof(TItem).GetProperty(SortColumn);
            if (prop != null)
            {
                Items = IsAsc
                    ? Items.OrderBy(x => prop.GetValue(x, null)).ToList()
                    : Items.OrderByDescending(x => prop.GetValue(x, null)).ToList();
            }
        }

        // 2. Calcul du nombre de pages basé sur la liste filtrée reçue
        this.TotalPages = (int)Math.Ceiling((double)Items.Count() / pageSize);

        // 3. Sécurité sur la page actuelle
        if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;
        if (CurrentPage < 1) CurrentPage = 1;

        // 4. Pagination (Skip/Take)
        Items = Items.Skip((CurrentPage - 1) * pageSize).Take(pageSize).ToList();
    }
}