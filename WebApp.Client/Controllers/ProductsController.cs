using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using WebApp.Client.Models;
using Rcl.DataTable.Models;

namespace WebApp.Client.Controllers;

public class ProductsController(IMemoryCache cache) : Controller
{
    private const string DataCacheKey = "Products_Data_List";
    private const string StateCacheKey = "Products_Table_State";

    // 1. Source de vérité : on récupère ou on initialise le million de produits en cache
    private List<Product> GetCachedData()
    {
        return cache.GetOrCreate(DataCacheKey, entry =>
        {
            // Le cache expire après 60 min d'inactivité pour libérer la RAM si inutilisé
            entry.SlidingExpiration = TimeSpan.FromMinutes(60);
            return GenerateMillion();
        }) ?? new List<Product>();
    }

    public IActionResult Index(int page = 1, string? sort = null, int pageSize = 10)
    {
        var state = cache.Get<DataTableState<Guid>>(StateCacheKey) ?? new DataTableState<Guid>();

        // Si la taille change, on revient par sécurité à la page 1
        if (state.PageSize != pageSize)
        {
            state.PageSize = pageSize;
            state.PageIndex = 1;
        }
        else
        {
            state.PageIndex = page;
        }


        // 2. GESTION DU TRI
        if (sort != null)
        {
            state.IsAscending = (state.SortColumn == sort) ? !state.IsAscending : true;
            state.SortColumn = sort;
        }

        // 3. PERSISTANCE
        cache.Set(StateCacheKey, state);

        return BuildTableResult(state);
    }

    [HttpDelete]
   [HttpPost] // On passe en HttpPost car les formulaires HTML standards ne supportent pas DELETE
public IActionResult Delete(Guid id)
{
    var data = GetCachedData();
    var item = data.FirstOrDefault(p => p.Id == id);

    if (item != null)
    {
        data.Remove(item);
        cache.Set(DataCacheKey, data);
    }

    // Après suppression, on redirige vers l'index pour voir la liste à jour
    return RedirectToAction(nameof(Index));
}

  public IActionResult Details(Guid id)
{
    var data = GetCachedData();
    var product = data.FirstOrDefault(p => p.Id == id);
    if (product == null) return NotFound();

    // 1. ON MÉMORISE l'ID sélectionné dans le cache AVANT d'aller aux détails
    var state = cache.Get<DataTableState<Guid>>(StateCacheKey) ?? new DataTableState<Guid>();
    state.SelectedId = id;
    cache.Set(StateCacheKey, state);

    product.LastConsulted = DateTime.Now;
    return View(product);
}

    public IActionResult GetSpecification(Guid id)
    {
        var data = GetCachedData();
        var product = data.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();


        return Content($"Spécifications techniques du produit {product.Name} (ID: {product.Id})");
    }

    public IActionResult GetDescription(Guid id)
    {
        var data = GetCachedData();
        var product = data.FirstOrDefault(p => p.Id == id);
        if (product == null) return NotFound();


        return Content($"Description du produit {product.Name} (ID: {product.Id})");
    }


    // Méthode pivot : Délègue le tri et la pagination au ViewModel agnostique
    private IActionResult BuildTableResult(DataTableState<Guid> state)
    {
        var data = GetCachedData(); // On donne la référence de la liste complète (1M) du cache
        var vm = new DataTableViewModel<Product, Guid>
        {
            TableId = "prod-grid",
            Items = data,
            CurrentPage = state.PageIndex,
            PageSize = state.PageSize,     // CRUCIAL : On injecte la taille de page du cache
            SelectedId = state.SelectedId,
            SortColumn = state.SortColumn,
            IsAsc = state.IsAscending,
            KeySelector = p => p.Id,
            HasActionButton=true,
            TotalItems=data.Count,

            Columns = [
            ("N°", "Id"),
            ("Référence", "Description"),
            ("Nom", "Name"),
            ("Prix", "Price"),
            ("Consulté", "LastConsulted")
            ]
        };

        // LE VIEWMODEL TRAVAILLE EN AUTONOMIE SUR LES DONNÉES DU CACHE
        // Il va trier, calculer le nouveau TotalPages (ex: 1M / 50 = 20 000)
        // et extraire uniquement les lignes de la page courante.
        vm.ProcessData(state.PageSize);

        // if (Request.Headers["HX-Request"] == "true")
        //     return PartialView("_TablePartial", vm);

        return View("Index", vm);
    }

    private List<Product> GenerateMillion() => Enumerable.Range(1, 1000000).Select(i => new Product
    {
        Id = Guid.NewGuid(),
        Description = $"PRD-{i:D7}",
        Name = $"Produit Haute Performance {i}",
        Price = i * 0.45m
    }).ToList();
}