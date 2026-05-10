-- ============================================
-- ECommerceDB - Tables + Seed Data
-- ============================================

-- TABLES

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id           INT IDENTITY(1,1) PRIMARY KEY,
        Username     NVARCHAR(100)  NOT NULL,
        Email        NVARCHAR(255)  NOT NULL UNIQUE,
        PasswordHash NVARCHAR(500)  NOT NULL,
        CreatedAt    DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Users created.';
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
BEGIN
    CREATE TABLE Products (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        Name        NVARCHAR(200)  NOT NULL,
        Description NVARCHAR(1000) NOT NULL DEFAULT '',
        Price       DECIMAL(18,2)  NOT NULL,
        Stock       INT            NOT NULL DEFAULT 0,
        Category    NVARCHAR(100)  NOT NULL DEFAULT '',
        Brand       NVARCHAR(100)  NOT NULL DEFAULT '',
        ImageUrl    NVARCHAR(500)  NULL,
        CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Products created.';
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
BEGIN
    CREATE TABLE Orders (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        UserId          INT            NOT NULL REFERENCES Users(Id),
        TotalPrice      DECIMAL(18,2)  NOT NULL,
        ShippingAddress NVARCHAR(500)  NOT NULL,
        City            NVARCHAR(100)  NOT NULL,
        PostalCode      NVARCHAR(20)   NOT NULL,
        Status          NVARCHAR(50)   NOT NULL DEFAULT 'Pending',
        CreatedAt       DATETIME2      NOT NULL DEFAULT GETUTCDATE()
    );
    PRINT 'Table Orders created.';
END

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderItems' AND xtype='U')
BEGIN
    CREATE TABLE OrderItems (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        OrderId     INT            NOT NULL REFERENCES Orders(Id),
        ProductId   INT            NOT NULL REFERENCES Products(Id),
        ProductName NVARCHAR(200)  NOT NULL,
        UnitPrice   DECIMAL(18,2)  NOT NULL,
        Quantity    INT            NOT NULL
    );
    PRINT 'Table OrderItems created.';
END

-- SEED DATA

IF NOT EXISTS (SELECT 1 FROM Products)
BEGIN
    INSERT INTO Products (Name, Description, Price, Stock, Category, Brand, ImageUrl) VALUES
    ('Dell XPS 15 Laptop',
     'High-performance 15.6" laptop with Intel Core i7, 16GB RAM, 512GB SSD, OLED display. Perfect for professionals and developers.',
     5999.99, 15, 'Laptops', 'Dell',
     'https://images.unsplash.com/photo-1593642632559-0c6d3fc62b89?w=400'),

    ('Apple MacBook Pro 14"',
     'Apple M3 Pro chip, 18GB unified memory, 512GB SSD. Industry-leading performance and battery life.',
     9499.99, 10, 'Laptops', 'Apple',
     'https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=400'),

    ('Lenovo ThinkPad X1 Carbon',
     'Ultra-lightweight business laptop, 14" IPS display, Intel Core i5, 16GB RAM, 256GB SSD. Built for mobility.',
     4799.99, 20, 'Laptops', 'Lenovo',
     'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400'),

    ('HP LaserJet Pro M404dn',
     'Monochrome laser printer, 38 ppm, automatic duplex printing, Ethernet connectivity. Ideal for small offices.',
     1299.99, 25, 'Printers', 'HP',
     'https://images.unsplash.com/photo-1612815154858-60aa4c59eaa6?w=400'),

    ('Canon PIXMA TR8620',
     'All-in-one inkjet printer with fax, scan, copy. Wireless connectivity, 4800 dpi print resolution.',
     699.99, 30, 'Printers', 'Canon',
     'https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400'),

    ('Samsung 27" 4K Monitor',
     '27" UHD 4K IPS panel, 60Hz refresh rate, HDR10, USB-C connectivity. Crystal-clear image quality.',
     1799.99, 18, 'Monitors', 'Samsung',
     'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400'),

    ('LG 34" UltraWide Monitor',
     '34" curved IPS display, 3440x1440 resolution, 100Hz, HDR400. Immersive ultrawide experience.',
     2499.99, 12, 'Monitors', 'LG',
     'https://images.unsplash.com/photo-1593640408182-31c228b4eb4e?w=400'),

    ('Logitech MX Master 3S Mouse',
     'Advanced wireless mouse, 8000 DPI sensor, MagSpeed scroll wheel, Bluetooth & USB receiver. Ergonomic design.',
     349.99, 50, 'Peripherals', 'Logitech',
     'https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400'),

    ('Keychron K2 Mechanical Keyboard',
     'Compact 75% wireless mechanical keyboard, RGB backlight, Bluetooth 5.1, compatible with Windows & Mac.',
     449.99, 35, 'Peripherals', 'Keychron',
     'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400'),

    ('Cisco 24-Port Gigabit Switch',
     'Managed 24-port Gigabit Ethernet switch with 4 SFP uplinks. VLAN support, PoE capable, rack-mountable.',
     1899.99, 8, 'Networking', 'Cisco',
     'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400'),

    ('Samsung 1TB SSD (870 EVO)',
     'SATA 2.5" internal SSD, 560 MB/s read speed, 530 MB/s write speed. Reliable storage upgrade for any PC.',
     399.99, 60, 'Storage', 'Samsung',
     'https://images.unsplash.com/photo-1597872200969-2b65d56bd16b?w=400'),

    ('WD My Passport 2TB External HDD',
     'Portable external hard drive, USB 3.0, password protection with hardware encryption. Available in multiple colors.',
     279.99, 45, 'Storage', 'Western Digital',
     'https://images.unsplash.com/photo-1531492746076-161ca9bcad58?w=400');

    PRINT 'Seed data inserted: 12 products.';
END
ELSE
BEGIN
    PRINT 'Products already exist, skipping seed.';
END

PRINT 'ECommerceDB setup complete.';


-- ============================================
-- NEW: ProductSpecifications table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductSpecifications' AND xtype='U')
BEGIN
    CREATE TABLE ProductSpecifications (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        ProductId   INT            NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
        SpecKey     NVARCHAR(100)  NOT NULL,
        SpecValue   NVARCHAR(500)  NOT NULL
    );
    PRINT 'Table ProductSpecifications created.';
END

-- ============================================
-- NEW: Reviews table
-- ============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
BEGIN
    CREATE TABLE Reviews (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        ProductId   INT            NOT NULL REFERENCES Products(Id) ON DELETE CASCADE,
        UserId      INT            NOT NULL REFERENCES Users(Id),
        Rating      INT            NOT NULL CHECK (Rating BETWEEN 1 AND 5),
        Title       NVARCHAR(200)  NOT NULL,
        Body        NVARCHAR(2000) NOT NULL DEFAULT '',
        CreatedAt   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_Review_UserProduct UNIQUE (UserId, ProductId)
    );
    PRINT 'Table Reviews created.';
END

-- ============================================
-- SEED: Specifications for existing products
-- ============================================
IF NOT EXISTS (SELECT 1 FROM ProductSpecifications)
BEGIN
    -- Dell XPS 15 (Id=1)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (1,'Processor','Intel Core i7-13700H'),
    (1,'RAM','16GB DDR5'),
    (1,'Storage','512GB NVMe SSD'),
    (1,'Display','15.6" OLED 3.5K 120Hz'),
    (1,'GPU','NVIDIA RTX 4060 8GB'),
    (1,'Battery','86Wh, up to 12h'),
    (1,'Weight','1.86 kg'),
    (1,'OS','Windows 11 Home');

    -- Apple MacBook Pro 14" (Id=2)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (2,'Processor','Apple M3 Pro'),
    (2,'RAM','18GB Unified Memory'),
    (2,'Storage','512GB SSD'),
    (2,'Display','14.2" Liquid Retina XDR'),
    (2,'GPU','M3 Pro 18-core GPU'),
    (2,'Battery','70Wh, up to 18h'),
    (2,'Weight','1.61 kg'),
    (2,'OS','macOS Sonoma');

    -- Lenovo ThinkPad X1 Carbon (Id=3)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (3,'Processor','Intel Core i5-1335U'),
    (3,'RAM','16GB LPDDR5'),
    (3,'Storage','256GB NVMe SSD'),
    (3,'Display','14" IPS 1920x1200'),
    (3,'GPU','Intel Iris Xe Graphics'),
    (3,'Battery','57Wh, up to 15h'),
    (3,'Weight','1.12 kg'),
    (3,'OS','Windows 11 Pro');

    -- HP LaserJet Pro M404dn (Id=4)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (4,'Print Speed','38 ppm'),
    (4,'Resolution','1200 x 1200 dpi'),
    (4,'Duplex','Automatic'),
    (4,'Connectivity','Ethernet, USB 2.0'),
    (4,'Paper Capacity','350 sheets'),
    (4,'Monthly Duty Cycle','80,000 pages'),
    (4,'Dimensions','36.2 x 36.4 x 22.6 cm');

    -- Canon PIXMA TR8620 (Id=5)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (5,'Print Speed','15 ipm color'),
    (5,'Resolution','4800 x 1200 dpi'),
    (5,'Functions','Print, Scan, Copy, Fax'),
    (5,'Connectivity','Wi-Fi, USB, Bluetooth'),
    (5,'Paper Capacity','200 sheets'),
    (5,'Scanner Resolution','1200 x 2400 dpi'),
    (5,'Ink System','5 individual ink tanks');

    -- Samsung 27" 4K Monitor (Id=6)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (6,'Panel','IPS'),
    (6,'Resolution','3840 x 2160 (4K UHD)'),
    (6,'Refresh Rate','60Hz'),
    (6,'Response Time','5ms GTG'),
    (6,'HDR','HDR10'),
    (6,'Connectivity','HDMI 2.0, DisplayPort 1.2, USB-C'),
    (6,'Brightness','350 nits');

    -- LG 34" UltraWide (Id=7)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (7,'Panel','IPS Curved'),
    (7,'Resolution','3440 x 1440 (UWQHD)'),
    (7,'Refresh Rate','100Hz'),
    (7,'Response Time','5ms GTG'),
    (7,'HDR','HDR400'),
    (7,'Connectivity','HDMI 2.0 x2, DisplayPort 1.4'),
    (7,'Brightness','400 nits');

    -- Logitech MX Master 3S (Id=8)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (8,'Sensor','Darkfield 8000 DPI'),
    (8,'Connectivity','Bluetooth, USB Receiver'),
    (8,'Buttons','7 programmable'),
    (8,'Battery','500mAh, up to 70 days'),
    (8,'Compatibility','Windows, macOS, Linux'),
    (8,'Weight','141g');

    -- Keychron K2 (Id=9)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (9,'Layout','75% (84 keys)'),
    (9,'Switch Options','Gateron Red/Blue/Brown'),
    (9,'Connectivity','Bluetooth 5.1, USB-C'),
    (9,'Backlight','RGB'),
    (9,'Battery','4000mAh'),
    (9,'Compatibility','Windows, macOS');

    -- Cisco Switch (Id=10)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (10,'Ports','24x Gigabit Ethernet + 4x SFP'),
    (10,'Switching Capacity','56 Gbps'),
    (10,'Management','Web UI, CLI, SNMP'),
    (10,'PoE','Yes, PoE+'),
    (10,'VLANs','Up to 4094'),
    (10,'Mounting','1U Rack-mountable');

    -- Samsung SSD (Id=11)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (11,'Capacity','1TB'),
    (11,'Interface','SATA III 6Gb/s'),
    (11,'Form Factor','2.5"'),
    (11,'Read Speed','560 MB/s'),
    (11,'Write Speed','530 MB/s'),
    (11,'NAND Type','Samsung V-NAND MLC'),
    (11,'Warranty','5 years');

    -- WD Passport (Id=12)
    INSERT INTO ProductSpecifications (ProductId, SpecKey, SpecValue) VALUES
    (12,'Capacity','2TB'),
    (12,'Interface','USB 3.0'),
    (12,'Form Factor','2.5" Portable'),
    (12,'Dimensions','110 x 82 x 15 mm'),
    (12,'Weight','130g'),
    (12,'Security','256-bit AES Hardware Encryption'),
    (12,'Warranty','3 years');

    PRINT 'Specifications seed inserted.';
END
