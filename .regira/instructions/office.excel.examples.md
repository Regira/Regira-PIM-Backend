# Office.Excel — Example: Order Report Export

> Context: A sales dashboard lets managers download monthly order reports as Excel files, and import product price lists uploaded by suppliers.

## Export orders to Excel

```csharp
public IMemoryFile ExportOrders(IEnumerable<Order> orders)
{
    IExcelManager<OrderRow> excel = new Regira.Office.Excel.MiniExcel.ExcelManager<OrderRow>();

    var sheet = new ExcelSheet<OrderRow>
    {
        Name = "Orders",
        Data = orders.Select(o => new OrderRow
        {
            OrderId      = o.Id,
            CustomerName = o.CustomerName,
            Total        = o.Total,
            Date         = o.OrderDate.ToString("yyyy-MM-dd"),
            Status       = o.Status.ToString()
        })
    };

    return excel.Create([sheet]);
}

public class OrderRow
{
    public int     OrderId      { get; set; }
    public string? CustomerName { get; set; }
    public decimal Total        { get; set; }
    public string? Date         { get; set; }
    public string? Status       { get; set; }
}
```

## Import supplier price list

```csharp
public IEnumerable<PriceUpdate> ImportPriceList(byte[] excelBytes)
{
    IExcelManager<PriceUpdate> excel = new Regira.Office.Excel.MiniExcel.ExcelManager<PriceUpdate>();
    var file   = excelBytes.ToBinaryFile("prices.xlsx");
    var sheets = excel.Read(file);
    return sheets.FirstOrDefault()?.Data ?? [];
}

public class PriceUpdate
{
    public string?  Sku   { get; set; }
    public decimal  Price { get; set; }
}
```

## Controller action

```csharp
[HttpGet("orders/export")]
public IActionResult DownloadOrderReport()
{
    var orders = _orderService.List();
    var file   = _reportService.ExportOrders(orders);
    return this.File(file);
}
```
