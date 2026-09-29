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
public class SalesController : Controller
{
    private readonly ApplicationDbContext _context;

    public SalesController(ApplicationDbContext context) => _context = context;

    [Authorize(Policy = Permissions.Sales.View)]
    public async Task<IActionResult> Index()
    {
        var orders = await _context.SalesOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.SaleDate)
            .ToListAsync();
        return View(orders);
    }

    [Authorize(Policy = Permissions.Sales.View)]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    private async Task PopulateDropdowns()
    {
        ViewBag.Customers = new SelectList(await _context.Customers.OrderBy(c => c.Name).ToListAsync(), "Id", "Name");
        ViewBag.Products = await _context.Products.Where(p => p.IsActive).OrderBy(p => p.Name)
            .Select(p => new { p.Id, p.Name, p.SKU, p.SellingPrice, p.Quantity, p.Unit })
            .ToListAsync();
    }

    [Authorize(Policy = Permissions.Sales.Create)]
    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        var vm = new SaleFormViewModel
        {
            InvoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss")
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = Permissions.Sales.Create)]
    public async Task<IActionResult> Create(SaleFormViewModel vm)
    {
        vm.Items = vm.Items?.Where(i => i.ProductId != 0 && i.Quantity > 0).ToList() ?? new List<SaleLineItem>();

        if (vm.Items.Count == 0)
            ModelState.AddModelError("", "Please add at least one product line item with quantity greater than zero.");

        var products = new Dictionary<int, Product>();
        foreach (var line in vm.Items)
        {
            var product = await _context.Products.FindAsync(line.ProductId);
            if (product == null)
            {
                ModelState.AddModelError("", "One of the selected products no longer exists.");
                continue;
            }
            products[line.ProductId] = product;
            if (line.Quantity > product.Quantity)
                ModelState.AddModelError("", $"Insufficient stock for {product.Name}. Available: {product.Quantity}, requested: {line.Quantity}.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(vm);
        }

        var order = new SalesOrder
        {
            InvoiceNo = string.IsNullOrWhiteSpace(vm.InvoiceNo) ? "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss") : vm.InvoiceNo,
            CustomerId = vm.CustomerId,
            SaleDate = vm.SaleDate,
            Notes = vm.Notes,
            Status = OrderStatus.Completed,
            CreatedBy = User.Identity?.Name
        };

        decimal total = 0;
        foreach (var line in vm.Items)
        {
            var product = products[line.ProductId];
            var subtotal = line.Quantity * line.UnitPrice;
            total += subtotal;

            order.Items.Add(new SalesOrderItem
            {
                ProductId = line.ProductId,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Subtotal = subtotal
            });

            product.Quantity -= line.Quantity;

            _context.StockTransactions.Add(new StockTransaction
            {
                ProductId = product.Id,
                Type = StockTransactionType.Sale,
                QuantityChange = -line.Quantity,
                BalanceAfter = product.Quantity,
                Reference = order.InvoiceNo,
                Notes = "Sales order",
                CreatedBy = User.Identity?.Name,
                CreatedAt = DateTime.UtcNow
            });
        }
        order.TotalAmount = total;

        _context.SalesOrders.Add(order);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Sale recorded and stock updated.";
        return RedirectToAction(nameof(Details), new { id = order.Id });
    }
}
