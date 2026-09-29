-- Seed script batch 3: meters/chain/engine/small parts -> InventoryERP (ZErpDb)
-- Safe to re-run: uses NOT EXISTS guards, won't duplicate rows.
USE ZErpDb;
GO

BEGIN TRANSACTION;

-- 1. Categories -----------------------------------------------------------
INSERT INTO Categories (Name, Description)
SELECT v.Name, v.Description
FROM (VALUES
    ('Chain & Transmission', 'Chain covers, clutch wire/cover, springs'),
    ('Engine Parts',         'Hub, magnet cover, cam kits'),
    ('Small Parts & Accessories', 'Misc small fittings, locks, tools')
) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM Categories c WHERE c.Name = v.Name);

-- 2. Products ---------------------------------------------------------------
INSERT INTO Products
    (Name, SKU, CategoryId, SupplierId, Description, CostPrice, SellingPrice, Quantity, ReorderLevel, Unit, ImageUrl, IsActive, CreatedAt)
SELECT p.Name, p.SKU, c.Id, NULL, NULL, p.CostPrice, p.CostPrice, p.Quantity, 5, p.Unit, NULL, 1, SYSUTCDATETIME()
FROM (VALUES
    -- Name                                   SKU        CategoryName                    Qty  CostPrice  Unit
    ('125cc Meter Rider',                  'MTR-015', 'Meters & Lights',             3,  1000.00, 'pcs'),
    ('Back Light Cover 70cc',              'MTR-016', 'Meters & Lights',            12,    75.00, 'pcs'),
    ('Back Light 125cc',                   'MTR-017', 'Meters & Lights',             6,    95.00, 'pcs'),
    ('Head Light Focus',                   'MTR-018', 'Meters & Lights',             6,   100.00, 'pcs'),
    ('Back Light Bracket 70cc Old',        'MTR-019', 'Meters & Lights',             6,   150.00, 'pcs'),
    ('Back Light 70cc New',                'MTR-020', 'Meters & Lights',             6,   150.00, 'pcs'),

    ('Chain Cover Plastic 70cc',           'CHN-001', 'Chain & Transmission',        3,   150.00, 'pcs'),
    ('Chain Cover Steel 70cc',             'CHN-002', 'Chain & Transmission',        3,   550.00, 'pcs'),
    ('Chain Cover 70cc Original',          'CHN-003', 'Chain & Transmission',        3,   800.00, 'pcs'),
    ('Chain Cover 125cc Normal',           'CHN-004', 'Chain & Transmission',        3,   800.00, 'pcs'),
    ('Chain Cover 125cc Original',         'CHN-005', 'Chain & Transmission',        3,   950.00, 'pcs'),
    ('Clutch Wire',                        'CHN-006', 'Chain & Transmission',      100,    15.00, 'pcs'),
    ('Clutch Cover 70cc',                  'CHN-007', 'Chain & Transmission',        3,   200.00, 'pcs'),
    ('Clutch Box Spring',                  'CHN-008', 'Chain & Transmission',        3,    70.00, 'pcs'),
    ('Baksa Spring',                       'CHN-009', 'Chain & Transmission',        3,    60.00, 'pcs'),

    ('Grany Hub 70cc',                     'ENG-001', 'Engine Parts',                6,   250.00, 'pcs'),
    ('Magnet Tapa (Cover)',                'ENG-002', 'Engine Parts',                3,   500.00, 'pcs'),
    ('Cam Kit 70cc',                       'ENG-003', 'Engine Parts',                2,   520.00, 'pcs'),
    ('Cam Kit 70cc Ching',                 'ENG-004', 'Engine Parts',                2,   560.00, 'pcs'),

    ('Plug Holder',                        'ACC-001', 'Small Parts & Accessories',  12,    30.00, 'pcs'),
    ('Petrol Filter',                      'ACC-002', 'Small Parts & Accessories',  12,    15.00, 'pcs'),
    ('Makhi',                              'ACC-003', 'Small Parts & Accessories',  24,    10.00, 'pcs'),
    ('Domchi 70cc',                        'ACC-004', 'Small Parts & Accessories',  12,    20.00, 'pcs'),
    ('Domchi 125cc',                       'ACC-005', 'Small Parts & Accessories',  12,    35.00, 'pcs'),
    ('Tapa Lock',                          'ACC-006', 'Small Parts & Accessories',  10,    60.00, 'pcs'),
    ('Drum Rubber',                        'ACC-007', 'Small Parts & Accessories',   9,    30.00, 'pcs'),
    ('Grany Boot',                         'ACC-008', 'Small Parts & Accessories',   6,    75.00, 'pcs'),
    ('Seal Kit 70cc',                      'ACC-009', 'Small Parts & Accessories',   3,    80.00, 'pcs'),
    ('Hammer',                             'ACC-010', 'Small Parts & Accessories',   3,    55.00, 'pcs')
) AS p(Name, SKU, CategoryName, Quantity, CostPrice, Unit)
JOIN Categories c ON c.Name = p.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM Products ex WHERE ex.SKU = p.SKU);

-- 3. Opening stock ledger entries -------------------------------------------
INSERT INTO StockTransactions (ProductId, Type, QuantityChange, BalanceAfter, Reference, Notes, CreatedBy, CreatedAt)
SELECT pr.Id, 2, pr.Quantity, pr.Quantity, 'Opening Stock', 'Seeded from handwritten stock register (batch 3)', 'Admin', SYSUTCDATETIME()
FROM Products pr
WHERE (pr.SKU IN ('MTR-015','MTR-016','MTR-017','MTR-018','MTR-019','MTR-020')
    OR pr.SKU LIKE 'CHN-%' OR pr.SKU LIKE 'ENG-%' OR pr.SKU LIKE 'ACC-%')
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
