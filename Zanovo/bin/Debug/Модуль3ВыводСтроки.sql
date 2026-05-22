SELECT 
    o.OrderNumber AS 'Номер заказа',
    c.Name AS 'Заказчик',
    o.OrderDate AS 'Дата заказа',
    SUM(oi.Quantity * oi.PriceAtOrder) AS 'Общая стоимость заказа'
FROM [Order] o
JOIN Customer c ON o.ID_Customer = c.ID_Customer
JOIN OrderItem oi ON o.ID_Order = oi.ID_Order
WHERE o.OrderNumber = 'Заказ покупателя № 2'
GROUP BY o.OrderNumber, c.Name, o.OrderDate;