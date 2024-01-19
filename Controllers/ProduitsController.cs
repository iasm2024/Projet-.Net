using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanierMVC.Data;
using PanierMVC.Models;
using System.IO;//ADDED
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
///ADDDED
namespace PanierMVC.Controllers
{
    public class ProduitsController : Controller
    {
        private readonly PanierMVCContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ProduitsController> _logger;
        private readonly IMemoryCache _memoryCache;

        [BindProperty]
        public IFormFile image { get; set; }

        [BindProperty]
        public Produit Produit { get; set; } = default!;
        [BindProperty]
        public SearchModel SearchModel { get; set; } = default!;

        public ProduitsController(PanierMVCContext context, IWebHostEnvironment webHostEnvironment, ILogger<ProduitsController> logger,IMemoryCache _memoryCache)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
           this._memoryCache = _memoryCache;
        }



        public async Task<IActionResult> Index([Bind("MinPriceString", "MaxPriceString", "Category")] SearchModel searchmodel)
        {
            try
            {
                // Retrieve data
                if (!_memoryCache.TryGetValue("key", out List<Produit> produits))
                {
                    produits = await _context.Produit.ToListAsync();

                    // Cache the data
                    _memoryCache.Set("key", produits, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
                    });
                    _logger.LogInformation("/Produits: Produits loaded from the database.");

                    // Log database access
                    _logger.LogInformation("/Produits: Homepage displayed with the list of products.");
                }
                else
                {
                    // Log cache hit
                    _logger.LogInformation("/Produits: Homepage displayed with the list of products from the cache.");
                }

                // Apply search filters if provided
                if (searchmodel != null)
                {
                    if (!string.IsNullOrEmpty(searchmodel.Category) && searchmodel.Category != "All")
                    {
                        _logger.LogInformation($"/Produits: Homepage displayed with the list of products with category .'{searchmodel.Category} ");
                        produits = produits.Where(p => p.type == searchmodel.Category).ToList();
                    }

                    decimal minPrice;
                    decimal maxPrice;

                    if (!string.IsNullOrEmpty(searchmodel.MinPriceString) && !string.IsNullOrEmpty(searchmodel.MinPriceString))
                    {

                        if (decimal.TryParse(searchmodel.MinPriceString, out minPrice) &&
                            decimal.TryParse(searchmodel.MaxPriceString, out maxPrice))
                        {
                            _logger.LogInformation($"/Produits: Homepage displayed with the list of products with min price  .'{searchmodel.MinPriceString} ");

                            produits = produits.Where(p =>
                            {
                                if (decimal.TryParse(p.prix, out decimal price))
                                {
                                    return (price >= minPrice && price <= maxPrice);
                                }
                                return false; // Handle parsing failure
                            }).ToList();
                        }
                    }
                }

                // If there are no filtered products, display all products
           
                return View(produits);
            }
            catch (Exception e)
            {
                _logger.LogError("/Produits: Error occurred while loading the index page");
                return NotFound();
            }
        }  
        
        public async Task<IActionResult> Index2([Bind("MinPriceString", "MaxPriceString", "Category")] SearchModel searchmodel)
        {
            try
            {
                // Retrieve data
                if (!_memoryCache.TryGetValue("key", out List<Produit> produits))
                {
                    produits = await _context.Produit.ToListAsync();

                    // Cache the data
                    _memoryCache.Set("key", produits, new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // Cache for 10 minutes
                    });
                    _logger.LogInformation("/Produits: Produits loaded from the database.");

                    // Log database access
                    _logger.LogInformation("/Produits: Homepage displayed with the list of products.");
                }
                else
                {
                    // Log cache hit
                    _logger.LogInformation("/Produits: Homepage displayed with the list of products from the cache.");
                }

                // Apply search filters if provided
                if (searchmodel != null)
                {
                    if (!string.IsNullOrEmpty(searchmodel.Category) && searchmodel.Category != "All")
                    {
                        _logger.LogInformation($"/Produits: Homepage displayed with the list of products with category .'{searchmodel.Category} ");
                        produits = produits.Where(p => p.type == searchmodel.Category).ToList();
                    }

                    decimal minPrice;
                    decimal maxPrice;

                    if (!string.IsNullOrEmpty(searchmodel.MinPriceString) && !string.IsNullOrEmpty(searchmodel.MinPriceString))
                    {

                        if (decimal.TryParse(searchmodel.MinPriceString, out minPrice) &&
                            decimal.TryParse(searchmodel.MaxPriceString, out maxPrice))
                        {
                            _logger.LogInformation($"/Produits: Homepage displayed with the list of products with min price  .'{searchmodel.MinPriceString} ");

                            produits = produits.Where(p =>
                            {
                                if (decimal.TryParse(p.prix, out decimal price))
                                {
                                    return (price >= minPrice && price <= maxPrice);
                                }
                                return false; // Handle parsing failure
                            }).ToList();
                        }
                    }
                }

                // If there are no filtered products, display all products
           
                return View(produits);
            }
            catch (Exception e)
            {
                _logger.LogError("/Produits: Error occurred while loading the index page");
                return NotFound();
            }
        }
        public  IActionResult Panier()
        {
            return View();
        }
        public IActionResult RefreshCache()
        {
            // Effacez le cache pour forcer une nouvelle récupération des données lors de la prochaine demande
            _memoryCache.Remove("key");
            _logger.LogInformation($"rafraichir les donnees pour un ajout");
            return RedirectToAction("Index");
        }

        // GET: Produits/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Produit == null)
            {
                _logger.LogError($"/Details :Base de données est introuvable ");
                return NotFound();
            }

            var produit = await _context.Produit
                .FirstOrDefaultAsync(m => m.id == id);
            if (produit == null)
            {
                _logger.LogError($"/Details :Produit avec id '{id}' est introuvable ");
                return NotFound();
            }
            _logger.LogInformation($"/Details :Page Détails du produit '{id}' est bien affichée");
            return View(produit);
        }

        // GET: Produits/Create
        public IActionResult Create()
        {
            _logger.LogInformation("/Create :Formulaire de l'ajout du produit est bien affiché");
            return View();
        }


        // POST: Produits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,nom,type,prix,image")] Produit produit)
        {
            try
            {
                if (ModelState.IsValid)
                {

                    //image
                    if (image != null && image.Length > 0)
                    {
                        // Generate a unique filename using a timestamp
                        var fileName = DateTime.Now.Ticks + Path.GetExtension(image.FileName);

                        var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        // Ensure the uploads folder exists
                        Directory.CreateDirectory(uploadsFolder);

                        // Save the file to the server
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(fileStream);
                        }

                        // Save the file path in your database
                        produit.image = "/images/" + fileName; // Update the path as per your project structure
                    }


                    _context.Add(produit);
                    _logger.LogInformation("Nouveau Produit ajouté");
                    RefreshCache();
                    _logger.LogInformation("Cache memory refreshed");


                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index2));
                }
              
                return View(produit);
            }

            catch(Exception e)
            {
                _logger.LogError("Echec d'ajout du produit");
                return Create();
            }

        }

        // GET: Produits/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Produit == null)
            {
                _logger.LogError("Echéc de modification");
                return NotFound();
            }

            var produit = await _context.Produit.FindAsync(id);
            if (produit == null)
            {
                return NotFound();
            }
            return View(produit);
        }

        // POST: Produits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,nom,type,prix,image")] Produit produit)
        {
            if (id != produit.id)
            {
                _logger.LogError("l'id du produit est incorrecte ");
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(produit);
                    _logger.LogInformation($"Produit '{id}' bien modifié ");
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProduitExists(produit.id))
                    {
                        _logger.LogInformation($"Produit avec '{id}' est introuvqble");
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(produit);
        }

        // GET: Produits/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Produit == null)
            {
                return NotFound();
            }

            var produit = await _context.Produit
                .FirstOrDefaultAsync(m => m.id == id);
            if (produit == null)
            {
                return NotFound();
            }

            return View(produit);
        }

        // POST: Produits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Produit == null)
            {
                return Problem("Entity set 'PanierMVCContext.Produit'  is null.");
            }
            var produit = await _context.Produit.FindAsync(id);
            if (produit != null)
            {
                _context.Produit.Remove(produit);

                _logger.LogInformation($"Produit '{id}' est bien supprimé ");

            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProduitExists(int id)
        {
            return (_context.Produit?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
