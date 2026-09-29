using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventoryERP.Web.Authorization;
using InventoryERP.Web.Data;
using InventoryERP.Web.Models;
using InventoryERP.Web.Models.ViewModels;

namespace InventoryERP.Web.Controllers;

[Authorize]
public class PurchasesController : Controller
{
    private readonly ApplicationDbContext _context;

    public PurchasesController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Purchases.View)]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .OrderByDescending(o => o.PurchaseDate)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(Policy = Permissions.Purchases.View)]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.PurchaseOrders
            .Include(o => o.Supplier)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    private async Task PopulateDropdowns()
    {
        ViewBag.Suppliers = new SelectList(await _context.Suppliers.OrderBy(s => s.Name).ToListAsync(), "Id", "Name");
        ViewBag.Products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.SKU, p.CostPrice, p.Unit })
            .ToListAsync();
    }

    [Authorize(Policy = Permissions.Purchases.Create)]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        var vm = new PurchaseFormViewModel
        {
            InvoiceNo = "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss")
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Purchases.Create)]
    public async Task<IActionResult> Create(PurchaseFormViewModel vm)
    {
        vm.Items = vm.Items?.Where(i => i.ProductId != 0 && i.Quantity > 0).ToList() ?? new List<PurchaseLineItem>();

        if (vm.Items.Count == 0)
            ModelState.AddModelError("", "Please add at least one product line item with quantity greater than zero.");

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(vm);
        }

        var order = new PurchaseOrder
        {
            InvoiceNo = string.IsNullOrWhiteSpace(vm.InvoiceNo) ? "PO-" + DateTime.Now.ToString("yyyyMMddHHmmss") : vm.InvoiceNo,
            SupplierId = vm.SupplierId,
            PurchaseDate = vm.PurchaseDate,
            Notes = vm.Notes,
            Status = OrderStatus.Completed,
            CreatedBy = User.Identity?.Name
        };

        decimal total = 0;
        foreach (var line in vm.Items)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null) continue;

            var subtotal = line.Quantity * line.UnitCost;
            total += subtotal;

            order.Items.Add(new PurchaseOrderItem
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                Subtotal = subtotal
            });

            product.Quantity += line.Quantity;
            product.CostPrice = line.UnitCost;

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.Purchase,
                QuantityChange = line.Quantity,
                BalanceAfter = product.Quantity,
                Reference = order.InvoiceNo,
                Notes = "Purchase order",
                CreatedBy = User.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            });
        }
        order.TotalAmount = total;

        _context.PurchaseOrders.Add(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Purchase order recorded and stock updated.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }
}
