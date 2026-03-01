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

    public IActionResult Index(int page = 1, string? sort = null, int pageSize = 10)
    {
        var state = cache.Get<DataTableState<Guid>>(StateCacheKey) ?? new DataTableState<Guid>();

        if (state.PageSize != pageSize)
        {
            state.PageSize = pageSize;
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

        var state = cache.Get<DataTableState<Guid>>(StateCacheKey) ?? new DataTableState<Guid>();
        state.SelectedId = id;
        cache.Set(StateCacheKey, state);

        product.LastConsulted = DateTime.Now;
        return View(product);
    }

    // --- LOGIQUE DE RENDU PARTIEL ---
    private IActionResult BuildTableResult(DataTableState<Guid> state)
    {
        var data = GetCachedData(); 
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
            ]
        };

        vm.ProcessData(state.PageSize);

        // DÉTECTION HTMX : Si l'en-tête HX-Request est présent
        // On renvoie uniquement le fichier .razor sans le Layout global.
        if (Request.Headers.ContainsKey("HX-Request"))
        {
            // Note : Assure-toi que le nom du fichier correspond (souvent DataTable.razor)
            return PartialView("_TablePartial", vm); 
        }

        // Sinon (chargement initial), on renvoie la vue complète avec le Layout.
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