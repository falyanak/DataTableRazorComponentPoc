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

        state.PageSize = pageSize;

        // Mise à jour de l'état avec le nouveau terme de recherche
        // Si le terme change, on repasse généralement à la page 1
        // true si searchTerm est différent de l'actuel, ou si c'est la première fois qu'on le définit
        if (state.SearchTerm != searchTerm)
        {
            state.SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm;
            state.PageIndex = 1;
        }
        else
        {
            state.PageIndex = page;
        }

        if (sort != null)
        {
            state.IsAscending = (state.SortColumn == sort) ? !state.IsAscending : true;
            state.SortColumn = sort;
        }

        cache.Set(StateCacheKey, state);

        return BuildTableResult(state);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(Guid id)
    {
        var data = GetCachedData();
        var item = data.FirstOrDefault(p => p.Id == id);

        if (item != null)
        {
            data.Remove(item);
            cache.Set(DataCacheKey, data);
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

    // --- LOGIQUE DE RENDU PARTIEL ---
    private IActionResult BuildTableResult(DataTableState<Product, Guid> state)
    {
        List<Product> data;

        // on détecte une navigation avec même terme de recherche filtré et des résultats déjà calculés
        if (state.IsFiltered && state.FilteredItems.Any())
        {
            data = state.FilteredItems;
        }
        else
        {
            // Sinon, on procède au filtrage classique à partir de la source complète
            data = GetCachedData();

            // --- LOGIQUE DE FILTRAGE AVANT PAGINATION ---
            if (!string.IsNullOrWhiteSpace(state.SearchTerm))
            {
                var term = state.SearchTerm.ToLower();
                data = data.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    p.Description.ToLower().Contains(term)
                ).ToList();

                state.FilteredItems = data;
            }
            else
            {
                state.FilteredItems = [];
            }
        }

        var vm = new DataTableViewModel<Product, Guid>
        {
            TableId = "prod-grid",
            Items = data,
            CurrentPage = state.PageIndex,
            PageSize = state.PageSize,
            SelectedId = state.SelectedId,
            SortColumn = state.SortColumn,
            IsAsc = state.IsAscending,
            KeySelector = p => p.Id,
            HasActionButton = true,
            TotalItems = data.Count,
            Columns = [
                ("N°", "Id"),
                ("Référence", "Description"),
                ("Nom", "Name"),
                ("Prix", "Price"),
                ("Consulté", "LastConsulted")
            ],
            // On configure le sous-modèle de recherche
            Search = new SearchViewModel
            {
                IsVisible = true,
                SearchTerm = state.SearchTerm,
                SearchUrl = Url.Action("Index", "Products"), // L'Index gère tout maintenant
                EraseSearchUrl = Url.Action("EraseSearchFilter", "Products"),
                Placeholder = "Rechercher un produit..."
            }
        };

        vm.ProcessData(state.PageSize);

        // DÉTECTION HTMX : Si l'en-tête HX-Request est présent
        // On renvoie uniquement le fichier .razor sans le Layout global.
        // Sinon (chargement initial), on renvoie la vue complète avec le Layout.
        return Request.Headers.ContainsKey("HX-Request")
         ? PartialView("_TablePartial", vm)
         : View("Index", vm);
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