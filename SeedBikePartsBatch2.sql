-- Seed script batch 2: ignition/kits/seats/levers/bearings/body/meters -> InventoryERP (ZErpDb)
-- Safe to re-run: uses NOT EXISTS guards, won't duplicate rows.
USE ZErpDb;
GO

BEGIN TRANSACTION;

-- 1. Categories -----------------------------------------------------------
INSERT INTO Categories (Name, Description)
SELECT v.Name, v.Description
FROM (VALUES
    ('Ignition & Coils',  'Timing chain kits, ignition coils'),
    ('Repair Kits',       'Half/full repair kits'),
    ('Seats & Frame',     'Seat foam, steel rods, locks'),
    ('Levers & Stands',   'Levers, gear levers, side/main stands'),
    ('Bearings',          'Wheel and crown bearings'),
    ('Body Parts',        'Mud guards, caps, mirrors, wall seals'),
    ('Meters & Lights',   'Speedometers, head/back lights, indicators')
) AS v(Name, Description)
WHERE NOT EXISTS (SELECT 1 FROM Categories c WHERE c.Name = v.Name);

-- 2. Products ---------------------------------------------------------------
INSERT INTO Products
    (Name, SKU, CategoryId, SupplierId, Description, CostPrice, SellingPrice, Quantity, ReorderLevel, Unit, ImageUrl, IsActive, CreatedAt)
SELECT p.Name, p.SKU, c.Id, NULL, NULL, p.CostPrice, p.CostPrice, p.Quantity, 5, p.Unit, NULL, 1, SYSUTCDATETIME()
FROM (VALUES
    -- Name                                   SKU        CategoryName          Qty  CostPrice  Unit
    ('Timing Chain Kit 82cc',              'IGN-001', 'Ignition & Coils',  6,   250.00, 'pcs'),
    ('Timing Chain Kit 84cc',              'IGN-002', 'Ignition & Coils',  6,   250.00, 'pcs'),
    ('Coil Guchga 70cc Future',            'IGN-003', 'Ignition & Coils',  6,  1200.00, 'pcs'),
    ('Coil Guchga 70cc Ching',             'IGN-004', 'Ignition & Coils',  6,  1200.00, 'pcs'),
    ('Coil 125cc Future',                  'IGN-005', 'Ignition & Coils',  6,   700.00, 'pcs'),
    ('Gutlka Coil',                        'IGN-006', 'Ignition & Coils',  6,   280.00, 'pcs'),
    ('Gutle Coil Ching',                   'IGN-007', 'Ignition & Coils',  6,   280.00, 'pcs'),

    ('Half Kit 70cc Vendor',               'KIT-001', 'Repair Kits',      20,    90.00, 'pcs'),
    ('Full Kit 70cc Vendor',               'KIT-002', 'Repair Kits',      10,   200.00, 'pcs'),
    ('Full Kit 125cc Vendor',              'KIT-003', 'Repair Kits',      10,   200.00, 'pcs'),
    ('Half Kit 125cc Vendor',              'KIT-004', 'Repair Kits',      10,    95.00, 'pcs'),

    ('Seat Foam Pipe 70cc Old',            'SEA-001', 'Seats & Frame',     2,   350.00, 'pcs'),
    ('Seat Foam Pipe 70cc New',            'SEA-002', 'Seats & Frame',     2,   350.00, 'pcs'),
    ('Seat Foam Pipe China 70cc',          'SEA-003', 'Seats & Frame',     2,   350.00, 'pcs'),
    ('Steel Rod Seat Frame 70cc',          'SEA-004', 'Seats & Frame',     2,   500.00, 'pcs'),
    ('Steel Rod Seat Frame Ching',         'SEA-005', 'Seats & Frame',     2,   500.00, 'pcs'),
    ('Lock 70cc Old',                      'SEA-006', 'Seats & Frame',    16,   250.00, 'pcs'),
    ('Lock 70cc New',                      'SEA-007', 'Seats & Frame',    16,   260.00, 'pcs'),
    ('Steel Rod Seat Frame 70cc New',      'SEA-008', 'Seats & Frame',     2,   500.00, 'pcs'),

    ('Lever Plastic Set',                  'LEV-001', 'Levers & Stands',  12,    75.00, 'pcs'),
    ('Steel Lever 70cc Set',               'LEV-002', 'Levers & Stands',  12,   140.00, 'pcs'),
    ('Lever Set 125cc',                    'LEV-003', 'Levers & Stands',  12,   140.00, 'pcs'),
    ('Gear Lever 70cc',                    'LEV-004', 'Levers & Stands',   6,   135.00, 'pcs'),
    ('Gear Lever 125cc',                   'LEV-005', 'Levers & Stands',   6,   180.00, 'pcs'),
    ('Side Stand 70cc',                    'LEV-006', 'Levers & Stands',   6,   170.00, 'pcs'),
    ('Side Stand 125cc',                   'LEV-007', 'Levers & Stands',   6,   200.00, 'pcs'),
    ('Main Stand 70cc',                    'LEV-008', 'Levers & Stands',   3,   450.00, 'pcs'),
    ('Main Stand 125cc',                   'LEV-009', 'Levers & Stands',   3,   800.00, 'pcs'),

    ('Bearing 6300',                       'BRG-001', 'Bearings',         10,    60.00, 'pcs'),
    ('Bearing 6301',                       'BRG-002', 'Bearings',         10,    65.00, 'pcs'),
    ('Bearing 6203',                       'BRG-003', 'Bearings',         10,    70.00, 'pcs'),
    ('Bearing 6000',                       'BRG-004', 'Bearings',         10,    56.00, 'pcs'),
    ('Bearing 6001',                       'BRG-005', 'Bearings',         10,    60.00, 'pcs'),
    ('Bearing 6202',                       'BRG-006', 'Bearings',         10,   100.00, 'pcs'),
    ('Bearing 6302',                       'BRG-007', 'Bearings',         10,    65.00, 'pcs'),
    ('Bearing Crown 6304',                 'BRG-008', 'Bearings',         10,   250.00, 'pcs'),

    ('Wall Seal China/Japani',             'BDY-001', 'Body Parts',       40,    55.00, 'pcs'),
    ('Mud Guard 70cc Plastic',             'BDY-002', 'Body Parts',        2,   350.00, 'pcs'),
    ('Mud Guard 70cc Vendor',              'BDY-003', 'Body Parts',        3,  1500.00, 'pcs'),
    ('Mud Guard 125cc Vendor',             'BDY-004', 'Body Parts',        2,  2250.00, 'pcs'),
    ('Cap/Dhakan Ching',                   'BDY-005', 'Body Parts',        6,   250.00, 'pcs'),
    ('Mirror',                             'BDY-006', 'Body Parts',        4,   250.00, 'pcs'),

    ('Meter Body 70cc Normal',             'MTR-001', 'Meters & Lights',   6,   110.00, 'pcs'),
    ('Meter Body 70cc Rider',              'MTR-002', 'Meters & Lights',   6,   200.00, 'pcs'),
    ('Meter Body 125cc Rider',             'MTR-003', 'Meters & Lights',   6,   300.00, 'pcs'),
    ('Chota Kit 70cc Head Light',          'MTR-004', 'Meters & Lights',  12,   350.00, 'pcs'),
    ('Back Light 70cc',                    'MTR-005', 'Meters & Lights',  12,   120.00, 'pcs'),
    ('Head Light 125cc',                   'MTR-006', 'Meters & Lights',   6,   350.00, 'pcs'),
    ('Indicator 70cc New',                 'MTR-007', 'Meters & Lights',  12,    90.00, 'pcs'),
    ('Indicator 70cc Omega',               'MTR-008', 'Meters & Lights',  12,   130.00, 'pcs'),
    ('Indicator 70cc Old',                 'MTR-009', 'Meters & Lights',  12,   100.00, 'pcs'),
    ('Indicator 125cc Old',                'MTR-010', 'Meters & Lights',  12,   100.00, 'pcs'),
    ('Back Dati 125cc',                    'MTR-011', 'Meters & Lights',  12,   100.00, 'pcs'),
    ('Head Light Bulb 70cc',               'MTR-012', 'Meters & Lights',   2,   280.00, 'box'),
    ('Head Light Holder',                  'MTR-013', 'Meters & Lights',  12,    25.00, 'pcs'),
    ('Meter Full 70cc Rider',              'MTR-014', 'Meters & Lights',   3,   500.00, 'pcs')
) AS p(Name, SKU, CategoryName, Quantity, CostPrice, Unit)
JOIN Categories c ON c.Name = p.CategoryName
WHERE NOT EXISTS (SELECT 1 FROM Products ex WHERE ex.SKU = p.SKU);

-- 3. Opening stock ledger entries -------------------------------------------
INSERT INTO StockTransactions (ProductId, Type, QuantityChange, BalanceAfter, Reference, Notes, CreatedBy, CreatedAt)
SELECT pr.Id, 2, pr.Quantity, pr.Quantity, 'Opening Stock', 'Seeded from handwritten stock register (batch 2)', 'Admin', SYSUTCDATETIME()
FROM Products pr
WHERE pr.SKU LIKE 'IGN-%' OR pr.SKU LIKE 'KIT-%' OR pr.SKU LIKE 'SEA-%'
   OR pr.SKU LIKE 'LEV-%' OR pr.SKU LIKE 'BRG-%' OR pr.SKU LIKE 'BDY-%' OR pr.SKU LIKE 'MTR-%'
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
