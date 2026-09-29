namespace InventoryERP.Web.Models;

public enum OrderStatus
{
    Pending = 0,
    Completed = 1,
    Cancelled = 2
}

public enum StockTransactionType
{
    Purchase = 0,
    Sale = 1,
    AdjustmentAdd = 2,
    AdjustmentRemove = 3,
    SaleCancelled = 4,
    PurchaseCancelled = 5
}
