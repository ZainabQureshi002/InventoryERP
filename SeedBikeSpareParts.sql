-- Seed script: bike spare parts stock register -> InventoryERP (ZErpDb)
-- Safe to re-run: uses NOT EXISTS guards, won't duplicate rows.
USE ZErpDb;
GO

BEGIN TRANSACTION;

-- 1. Categories -----------------------------------------------------------
INSERT INTO Categories (Name, Description)
SELECT v.Name, v.Description
FROM (VALUES
    ('Filters',       'Bike ATV/oil filters'),
    ('Spark Plugs',   'Bike spark plugs'),
    ('Brake Shoes',   'Bike brake shoes'),
    ('Clutch Plates', 'Clutch plates and pressure plates'),
    ('Cables',        'Bike cable sets (clutch/brake/accelerator)'),
    ('Tubes',         'Bike tyre tubes')
) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM Categories c WHERE c.Name = v.Name);

-- 2. Products ---------------------------------------------------------------
INSERT INTO Products
    (Name, SKU, CategoryId, SupplierId, Description, CostPrice, SellingPrice, Quantity, ReorderLevel, Unit, ImageUrl, IsActive, CreatedAt)
SELECT p.Name, p.SKU, c.Id, NULL, NULL, p.CostPrice, p.CostPrice, p.Quantity, 5, 'pcs', NULL, 1, SYSUTCDATETIME()
FROM (VALUES
    -- Name                              SKU        CategoryName      Qty  CostPrice
    ('ATV Filter Normal',            'FIL-001', 'Filters',       24,    25.00),
    ('ATV Filter New Euro Normal',   'FIL-002', 'Filters',       24,    40.00),
    ('ATV Filter New Special',       'FIL-003', 'Filters',       24,    85.00),
    ('ATV Filter Old Special',       'FIL-004', 'Filters',       24,    75.00),

    ('Spark Plug 70cc Crown',        'SPK-001', 'Spark Plugs',   10,    88.00),
    ('Spark Plug 125cc Crown',       'SPK-002', 'Spark Plugs',   10,   108.00),
    ('Spark Plug 70cc Ching',        'SPK-003', 'Spark Plugs',   10,    60.00),
    ('Spark Plug 125cc Advance',     'SPK-004', 'Spark Plugs',   10,    90.00),
    ('Spark Plug Gold Sparko 125cc', 'SPK-005', 'Spark Plugs',    3,  1333.00),
    ('Spark Plug Gold Sparko 70cc',  'SPK-006', 'Spark Plugs',    3,  1100.00),

    ('Brake Shoe MB',                'BRK-001', 'Brake Shoes',   25,   250.00),
    ('Brake Shoe AD',                'BRK-002', 'Brake Shoes',   25,   200.00),
    ('Brake Shoe 125cc Front',       'BRK-003', 'Brake Shoes',   12,   450.00),

    ('Clutch Plate 70cc',            'CLT-001', 'Clutch Plates', 12,   135.00),
    ('Clutch Pressure Plate 70cc',   'CLT-002', 'Clutch Plates', 12,   130.00),
    ('Clutch Plate 125cc',           'CLT-003', 'Clutch Plates',  4,   280.00),
    ('Pressure Plate 125cc',         'CLT-004', 'Clutch Plates',  4,   240.00),

    ('Cable Set 70cc - All',         'CBL-001', 'Cables',        20,   150.00),
    ('Cable Set 70cc - Cables',      'CBL-002', 'Cables',        20,   150.00),
    ('Cable Set 70cc - {4}',         'CBL-003', 'Cables',        20,   150.00),
    ('Cable Set 125cc - Item 1',     'CBL-004', 'Cables',        10,   300.00),
    ('Cable Set 125cc - All',        'CBL-005', 'Cables',        10,   300.00),
    ('Cable Set 125cc - {5}',        'CBL-006', 'Cables',        10,   300.00),
    ('Cable Set 125cc - Cables',     'CBL-007', 'Cables',        10,   300.00),
    ('Cable Set 125cc - Item 2',     'CBL-008', 'Cables',        10,   300.00),

    ('Tube 70cc',                    'TUB-001', 'Tubes',          6,   335.00),
    ('Tube 125cc Front',             'TUB-002', 'Tubes',          3,   460.00),
    ('Tube 126cc Back',              'TUB-003', 'Tubes',          3,   490.00)
) AS p(Name, SKU, CategoryName, Quantity, CostPrice)
JOIN Categories c ON c.Name = p.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM Products ex WHERE ex.SKU = p.SKU);

-- 3. Opening stock ledger entries (keeps Stock Ledger screen consistent) ----
INSERT INTO StockTransactions (ProductId, Type, QuantityChange, BalanceAfter, Reference, Notes, CreatedBy, CreatedAt)
SELECT pr.Id, 2, pr.Quantity, pr.Quantity, 'Opening Stock', 'Seeded from handwritten stock register', 'Admin', SYSUTCDATETIME()
FROM Products pr
WHERE pr.SKU IN (
    'FIL-001','FIL-002','FIL-003','FIL-004',
    'SPK-001','SPK-002','SPK-003','SPK-004','SPK-005','SPK-006',
    'BRK-001','BRK-002','BRK-003',
    'CLT-001','CLT-002','CLT-003','CLT-004',
    'CBL-001','CBL-002','CBL-003','CBL-004','CBL-005','CBL-006','CBL-007','CBL-008',
    'TUB-001','TUB-002','TUB-003'
)
AND NOT EXISTS (
    SELECT 1 FROM StockTransactions st
    WHERE st.ProductId = pr.Id AND st.Reference = 'Opening Stock'
);

COMMIT TRANSACTION;

-- Quick verification
SELECT c.Name AS Category, COUNT(*) AS Products, SUM(p.Quantity) AS TotalQty, SUM(p.Quantity * p.CostPrice) AS StockValue
FROM Products p JOIN Categories c ON c.Id = p.CategoryId
GROUP BY c.Name
ORDER BY c.Name;
GO
