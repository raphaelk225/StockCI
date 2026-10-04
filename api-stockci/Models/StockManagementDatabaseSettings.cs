using System;

namespace api_stockci.Models;

public class StockManagementDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string ProductsCollectionName { get; set; } = null!;

}
