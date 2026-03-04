using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Client.Models;
using Rcl.DataTable.Models;

namespace WebApp.Client.Controllers;

public class ProductsController(IMemoryCache cache) : Controller
{
    private const string DataCacheKey = "Products_Data_List";
    private const string StateCacheKey = "Products_Table_State";

    private List<Product> GetCachedData()
    {
        return cache.GetOrCreate(DataCacheKey, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(60);
            return GenerateMillion();
        }) ?? new List<Product>();
    }

    public IActionResult Index(int page = 1, string? sort = null, int pageSize = 10, string? searchTerm = null)
    {
        var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey) ?? new DataTableState<Product, Guid>();

        // 1. Détection du changement de recherche
        // On ne recalcule la liste filtrée que si le terme a réellement changé
        if (state.SearchTerm != searchTerm)
        {
            state.SearchTerm = searchTerm;
            state.PageIndex = 1;

            var allData = GetCachedData();

            // On stocke la liste filtrée (ou complète) UNE SEULE FOIS dans le state
            state.FilteredItems = state.IsFiltered
                ? allData.Where(p => p.Name.Contains(searchTerm!, StringComparison.OrdinalIgnoreCase)
                                  || p.Description.Contains(searchTerm!, StringComparison.OrdinalIgnoreCase)
                ).ToList()
                : allData;
        }

        // 2. Mise à jour des paramètres de navigation
        state.PageIndex = page;
        state.PageSize = pageSize;

        if (sort != null)
        {
            state.IsAscending = (state.SortColumn == sort) ? !state.IsAscending : true;
            state.SortColumn = sort;
        }

        cache.Set(StateCacheKey, state);

        // On passe un état "prêt à l'emploi" au constructeur de vue
        return BuildTableResult(state);
    }

    private IActionResult BuildTableResult(DataTableState<Product, Guid> state)
    {
        // Plus besoin de filtrer ici ! On utilise ce qui est dans le state.
        // Si FilteredItems est vide et qu'on n'est pas filtré, on prend la source brute par sécurité.
        var data = (state.FilteredItems.Any() || state.IsFiltered)
                   ? state.FilteredItems
                   : GetCachedData();

        var vm = new DataTableViewModel<Product, Guid>
        {
            TableId = "prod-grid",
            Items = data, // ProcessData fera le Skip/Take sur cette liste
            CurrentPage = state.PageIndex,
            PageSize = state.PageSize,
            SelectedId = state.SelectedId,
            SortColumn = state.SortColumn,
            IsAsc = state.IsAscending,
            KeySelector = p => p.Id,
            HasActionButton = true,
            TotalItems = data.Count, // Toujours le million d'origine
            FilteredCount = data.Count, // Nombre après filtrage
            Columns = [
                ("N°", "Id"),
            ("Référence", "Description"),
            ("Nom", "Name"),
            ("Prix", "Price"),
            ("Consulté", "LastConsulted")
            ],
            Search = new SearchViewModel
            {
                IsVisible = true,
                SearchTerm = state.SearchTerm,
                SearchUrl = Url.Action("Index", "Products"),
                EraseSearchUrl = Url.Action("EraseSearchFilter", "Products"),
                Placeholder = "Rechercher un produit..."
            }
        };

        vm.ProcessData(state.PageSize);

        return Request.Headers.ContainsKey("HX-Request")
             ? PartialView("_TablePartial", vm)
             : View("Index", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        // 1. Mise à jour de la source globale (Le million)
        var allData = GetCachedData();
        var item = allData.FirstOrDefault(p => p.Id == id);

        if (item != null)
        {
            allData.Remove(item);
            cache.Set(DataCacheKey, allData);

            // 2. Mise à jour de la vue filtrée dans le State
            var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey);
            if (state != null)
            {
                // On retire l'objet de la liste filtrée si elle le contient
                var filteredItem = state.FilteredItems.FirstOrDefault(p => p.Id == id);
                if (filteredItem != null)
                {
                    state.FilteredItems.Remove(filteredItem);

                    // Si la page actuelle devient vide après suppression (ex: dernier item de la page 5)
                    // On peut optionnellement reculer d'une page
                    if (!state.FilteredItems.Any(p => true) && state.PageIndex > 1)
                        state.PageIndex--;

                    cache.Set(StateCacheKey, state);
                }
            }

            // On prépare l'objet pour le JS
            var counts = new
            {
                total = allData.Count.ToString("N0", new System.Globalization.CultureInfo("fr-FR")),
                filtered = state?.FilteredItems.Count ?? 0,
                isFiltered = state?.IsFiltered ?? false
            };

            // HX-Trigger : Le serveur "crie" l'événement 'productUpdated'
            Response.Headers.Add("HX-Trigger", System.Text.Json.JsonSerializer.Serialize(new
            {
                productUpdated = counts
            }));
        }

        // 3. Rendu fluide via HTMX
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            var currentState = cache.Get<DataTableState<Product, Guid>>(StateCacheKey);
            return BuildTableResult(currentState!);
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(Guid id)
    {
        var data = GetCachedData();
        var product = data.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey) ?? new DataTableState<Product, Guid>();
        state.SelectedId = id;
        cache.Set(StateCacheKey, state);

        product.LastConsulted = DateTime.Now;
        return View(product);
    }

    public IActionResult EraseSearchFilter()
    {
        var state = cache.Get<DataTableState<Product, Guid>>(StateCacheKey);
        if (state != null)
        {
            state.SearchTerm = null;
            state.FilteredItems = [];
            state.PageIndex = 1; // Optionnel : revenir à la première page
            cache.Set(StateCacheKey, state);
        }
        return RedirectToAction(nameof(Index));
    }

    private List<Product> GenerateMillion() => Enumerable.Range(1, 1000000).Select(i => new Product
    {
        Id = Guid.NewGuid(),
        Description = $"PRD-{i:D7}",
        Name = $"Produit Haute Performance {i}",
        Price = i * 0.45m
    }).ToList();

    public IActionResult GetDescription(Guid id)
    {
        var data = GetCachedData();
        var product = data.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        return Content($"Description du produit :\n{product.Name} - {product.Description}\nPrix : {product.Price:C}");
    }

    public IActionResult GetSpecification(Guid id)
    {
        var data = GetCachedData();
        var product = data.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();

        return Content("Spécifications techniques du produit :\n- Poids : 1kg\n- Dimensions : 10x20x30cm\n- Couleur : Rouge\n- Garantie : 2 ans");
    }
}