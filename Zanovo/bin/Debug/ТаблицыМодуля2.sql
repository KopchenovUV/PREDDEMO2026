-- ============================================
-- База данных для молочного комбината
-- На основе документов: Производство, Спецификация, Цены, Заказ покупателя
-- ============================================

USE [gas159]
GO

-- 1. Продукция
CREATE TABLE Product (
    Code NVARCHAR(50) PRIMARY KEY,          -- НФ-00000006 и т.д.
    Name NVARCHAR(200) NOT NULL,            -- Сметана классическая 15% 540г.
    Unit NVARCHAR(20) NOT NULL,             -- шт, кг
    Price DECIMAL(18,2) NOT NULL            -- цена продажи
);
GO

-- 2. Материалы (сырьё)
CREATE TABLE Material (
    Code NVARCHAR(50) PRIMARY KEY,          -- НФ-00000004, НФ-00000005
    Name NVARCHAR(200) NOT NULL,            -- Молоко нормализованное, Закваска...
    Unit NVARCHAR(20) NOT NULL,             -- кг, л, шт
    Cost DECIMAL(18,2) NOT NULL             -- закупочная цена
);
GO

-- 3. Спецификация (нормы расхода материалов на продукцию)
CREATE TABLE ProductMaterial (
    ProductCode NVARCHAR(50) NOT NULL,
    MaterialCode NVARCHAR(50) NOT NULL,
    Quantity DECIMAL(18,4) NOT NULL,        -- норма расхода (0.9 кг молока на 1 шт сметаны)
    CONSTRAINT PK_ProductMaterial PRIMARY KEY (ProductCode, MaterialCode),
    CONSTRAINT FK_PM_Product FOREIGN KEY (ProductCode) REFERENCES Product(Code),
    CONSTRAINT FK_PM_Material FOREIGN KEY (MaterialCode) REFERENCES Material(Code)
);
GO

-- 4. Заказчики
CREATE TABLE Customer (
    ID_Customer INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,            -- ООО "Ассоль"
    ContactInfo NVARCHAR(500) NULL          -- телефон, email, адрес
);
GO

-- 5. Заказы
CREATE TABLE [Order] (
    ID_Order INT IDENTITY(1,1) PRIMARY KEY,
    OrderNumber NVARCHAR(50) NOT NULL,      -- "Заказ покупателя № 2"
    OrderDate DATE NOT NULL,
    ID_Customer INT NOT NULL,
    CONSTRAINT FK_Order_Customer FOREIGN KEY (ID_Customer) REFERENCES Customer(ID_Customer)
);
GO

-- 6. Состав заказа
CREATE TABLE OrderItem (
    ID_OrderItem INT IDENTITY(1,1) PRIMARY KEY,
    ID_Order INT NOT NULL,
    ProductCode NVARCHAR(50) NOT NULL,
    Quantity INT NOT NULL,                  -- количество продукции в заказе
    PriceAtOrder DECIMAL(18,2) NOT NULL,    -- цена на момент заказа (из Цены.xlsx)
    CONSTRAINT FK_OI_Order FOREIGN KEY (ID_Order) REFERENCES [Order](ID_Order),
    CONSTRAINT FK_OI_Product FOREIGN KEY (ProductCode) REFERENCES Product(Code)
);
GO