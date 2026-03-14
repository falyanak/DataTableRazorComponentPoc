using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Client.Models;
using Rcl.DataTable.Models;

namespace WebApp.Client.Controllers;

public class ProductsController(IMemoryCache cache) : Controller
{
    private const string DataCacheKey = "Products_Data_List";
    private const string StateCacheKey = "Products_Table_State";

    public IActionResult Index(int page = 1, string? sort = null, int pageSize = 10, string? searchTerm = null)
    {
        var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey) ?? new DataTableState<Product, Guid>();

        // 1. Filtrage (si searchTerm change ou est présent)
        if (state.SearchTerm != searchTerm)
        {
            state.SearchTerm = searchTerm;
            state.PageIndex = 1;
            state.ExpandedRows.Clear();

            var allData = GetCachedData();
            state.FilteredItems = !string.IsNullOrEmpty(searchTerm)
                ? allData.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                                  || p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList()
                : allData;
        }

        // 2. Navigation & Tri
        state.PageIndex = page;
        state.PageSize = pageSize > 0 ? pageSize : 10;

        if (!string.IsNullOrEmpty(sort))
        {
            state.IsAscending = (state.SortColumn == sort) ? !state.IsAscending : true;
            state.SortColumn = sort;
            
            // On trie ici les données filtrées
            var listToSort = (state.FilteredItems.Any() || !string.IsNullOrEmpty(searchTerm)) ? state.FilteredItems : GetCachedData();
            state.FilteredItems = state.IsAscending 
                ? listToSort.OrderBy(p => p.GetType().GetProperty(sort)?.GetValue(p)).ToList()
                : listToSort.OrderByDescending(p => p.GetType().GetProperty(sort)?.GetValue(p)).ToList();
        }

        cache.Set(StateCacheKey, state);
        return BuildTableResult(state);
    }

    [HttpPost]
    [ValidateAntiForgeryToken] // Important pour ton composant DataTableActions
    public IActionResult Delete(Guid id)
    {
        var allData = GetCachedData();
        var item = allData.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            allData.Remove(item);
            cache.Set(DataCacheKey, allData);
            
            // Mettre à jour la liste filtrée dans le state
            var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey);
            if (state != null) state.FilteredItems.RemoveAll(x => x.Id == id);
        }

        // HTMX demande le rafraîchissement de la table
        var currentState = cache.Get<DataTableState<Product, Guid>>(StateCacheKey) ?? new DataTableState<Product, Guid>();
        return BuildTableResult(currentState);
    }

    [HttpGet]
    public IActionResult ToggleRow(Guid id, int page, string? sort, bool isAsc, string? searchTerm)
    {
        var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey);
        if (state != null)
        {
            var rowId = id.ToString();
            if (state.ExpandedRows.Contains(rowId)) state.ExpandedRows.Remove(rowId);
            else state.ExpandedRows.Add(rowId);

            state.PageIndex = page;
            state.IsAscending = isAsc;
            state.SortColumn = sort;
            state.SearchTerm = searchTerm;

            cache.Set(StateCacheKey, state);
            return BuildTableResult(state);
        }
        return BadRequest();
    }

    private IActionResult BuildTableResult(DataTableState<Product, Guid> state)
    {
        var data = (state.FilteredItems.Any() || !string.IsNullOrEmpty(state.SearchTerm))
                   ? state.FilteredItems
                   : GetCachedData();

        var vm = new DataTableViewModel<Product, Guid>
        {
            TableId = "dt-prod-grid", // FIX: Doit correspondre au hx-target des sous-composants
            Items = data,
            CurrentPage = state.PageIndex,
            PageSize = state.PageSize,
            SortColumn = state.SortColumn,
            IsAsc = state.IsAscending,
            KeySelector = p => p.Id,
            TotalItems = data.Count,
            ExpandedRows = state.ExpandedRows,
            
            Columns = new List<ColumnDefinition>
            {
                new() { Display = "Nom", Prop = "Name", IsSecondary = false },
                new() { Display = "Référence", Prop = "Description", IsSecondary = true, ResponsiveClass = "d-none d-md-table-cell" },
                new() { Display = "Prix", Prop = "Price", IsSecondary = true, ResponsiveClass = "d-none d-lg-table-cell" },
                new() { Display = "Consulté", Prop = "LastConsulted", IsSecondary = true, ResponsiveClass = "d-none d-xl-table-cell" }
            },
            Search = new SearchViewModel
            {
                SearchTerm = state.SearchTerm,
                SearchUrl = "/Products/Index" // Le sous-composant utilise hx-get sur cette URL
            }
        };

        vm.ProcessData(state.PageSize);

        // Sélection de la vue partielle ou complète
        return Request.Headers.ContainsKey("HX-Request")
             ? PartialView("_TablePartial", vm)
             : View("Index", vm);
    }

    private List<Product> GetCachedData() => cache.GetOrCreate(DataCacheKey, entry => GenerateMillion())!;

    private List<Product> GenerateMillion() => Enumerable.Range(1, 100).Select(i => new Product
    {
        Id = Guid.NewGuid(),
        Description = $"PRD-{i:D7}",
        Name = $"Produit {i}",
        Price = i * 1.5m,
        LastConsulted = DateTime.Now.AddMinutes(-i)
    }).ToList();
}