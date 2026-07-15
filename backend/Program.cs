using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Force application to run on http://localhost:5000 for standard local development
builder.WebHost.UseUrls("http://localhost:5000");

// Configure CORS to allow our decoupled static frontend browser files to query the REST endpoints
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowFrontend");

// Serve frontend static files
app.UseDefaultFiles();
app.UseStaticFiles();

// ==========================================================================
// CENTRAL CONFIGURATION & CONSTANTS FOR SQL DATABASE TABLES
// ==========================================================================
const string CONN_STRING_NAME = "ZadHoldingDB_Azure";
const string TBL_CUSTOMER = "[Customer Master]";   // Actual Customer table name
const string TBL_MATERIAL = "[Material Master]";   // Actual Material table name
const string TBL_SALESPERSON = "[Salesperson Master]"; // Actual Salesperson table name

// ==========================================================================
// IN-MEMORY MOCK DATA STORE (Fallback if Azure SQL is not yet configured)
// ==========================================================================
var mockCustomers = new List<CustomerDto>
{
    new() { Code = "C41699", Name = "Thamam Trading" },
    new() { Code = "C10042", Name = "Jawad & Sons" },
    new() { Code = "C90481", Name = "Qatar Cooperative Society" },
    new() { Code = "C30489", Name = "Al Meera Consumer Goods" },
    new() { Code = "C49021", Name = "Doha Food Distributors" },
    new() { Code = "C12849", Name = "Gulf Hypermarket" },
    new() { Code = "C69104", Name = "Oasis Logistics" }
};

var mockMaterials = new List<MaterialDto>
{
    new() { No = 1, Description = "FLOUR NO 1 (BLUE)", Code = "5001", Packing = "1*50KG", DefaultPrice = 140.00m },
    new() { No = 2, Description = "FLOUR NO 1 (BLACK)", Code = "5002", Packing = "1*50KG", DefaultPrice = 138.00m },
    new() { No = 3, Description = "FLOUR NO 1 (RED)", Code = "5003", Packing = "1*50KG", DefaultPrice = 140.00m },
    new() { No = 4, Description = "FLOUR NO 2 (PURPLE)", Code = "5004", Packing = "1*50KG", DefaultPrice = 140.00m },
    new() { No = 5, Description = "FLOUR NO 2 (GREEN)", Code = "5005", Packing = "1*50KG", DefaultPrice = 138.00m },
    new() { No = 6, Description = "FLOUR NO 3 (BROWN)", Code = "5006", Packing = "1*50KG", DefaultPrice = 140.00m },
    new() { No = 7, Description = "FLOUR NO 1- MALABAR PRT", Code = "5015", Packing = "1*50KG", DefaultPrice = 125.00m },
    new() { No = 8, Description = "FLOUR NO 1 -PIZZA FLR", Code = "5008", Packing = "1*50KG", DefaultPrice = 155.00m },
    new() { No = 9, Description = "FINE BRAN 1*10kg", Code = "312", Packing = "1*10kg", DefaultPrice = 0.00m },
    new() { No = 10, Description = "FINE BRAN 1*30kg", Code = "335", Packing = "1*30kg", DefaultPrice = 0.00m },
    new() { No = 11, Description = "saudi suger", Code = "1272", Packing = "1*50kg", DefaultPrice = 0.00m },
    new() { No = 12, Description = "QFM SUGAR", Code = "1251", Packing = "1*50KG", DefaultPrice = 0.00m },
    new() { No = 13, Description = "ZAIN CORN OIL", Code = "1306", Packing = "6*1.8ltr", DefaultPrice = 0.00m },
    new() { No = 14, Description = "AL RAI SUNFLOWER OIL", Code = "1302", Packing = "6*1.8ltr", DefaultPrice = 0.00m },
    new() { No = 15, Description = "ZAIN SUNFLOWER OIL", Code = "1305", Packing = "6*1.8ltr", DefaultPrice = 0.00m },
    new() { No = 16, Description = "ZAIN CORN OIL 5ltr", Code = "1356", Packing = "4*5ltr", DefaultPrice = 0.00m },
    new() { No = 17, Description = "ZAIN SUNFLOWER OIL 5 ltr", Code = "1354", Packing = "4*5ltr", DefaultPrice = 0.00m },
    new() { No = 18, Description = "ZAIN PALM OIL", Code = "1418", Packing = "18LTR", DefaultPrice = 0.00m },
    new() { No = 19, Description = "AL RAI PALM OIL", Code = "1385", Packing = "18LTR", DefaultPrice = 0.00m },
    new() { No = 20, Description = "YARA GHEE", Code = "1373", Packing = "16LTR", DefaultPrice = 0.00m },
    new() { No = 21, Description = "ZAIN BAKER FLOUR", Code = "5010", Packing = "1*50KG", DefaultPrice = 110.00m },
    new() { No = 22, Description = "ZAIN FLOUR NO 1(BLACK)", Code = "5011", Packing = "1*50KG", DefaultPrice = 110.00m },
    new() { No = 23, Description = "ZAIN FLOUR NO 2(GREEN)", Code = "5012", Packing = "1*50KG", DefaultPrice = 110.00m },
    new() { No = 24, Description = "CHAKKI ATTA 50kg", Code = "1108", Packing = "1*50KG", DefaultPrice = 130.00m },
    new() { No = 25, Description = "SEMOLINA ROUGH", Code = "303", Packing = "1*25KG", DefaultPrice = 80.00m },
    new() { No = 26, Description = "SEMOLINA SOFT", Code = "304", Packing = "1*25KG", DefaultPrice = 80.00m },
    new() { No = 27, Description = "DAWI SUNFLOWER", Code = "1423", Packing = "5LTR", DefaultPrice = 0.00m },
    new() { No = 28, Description = "CHAKKI ATTA 10kg", Code = "1118", Packing = "1*10KG", DefaultPrice = 28.50m }
};

// Initial pre-loaded order representing the handwritten example in the PDF
var mockOrders = new List<OrderDto>
{
    new()
    {
        Id = "ord-100302-0001",
        MemoNumber = "100302-0001",
        Date = "2026-05-20",
        CustomerName = "Thamam Trading",
        CustomerCode = "C41699",
        PaymentMode = "CREDIT",
        SalesPerson = "Jawed Akthar",
        SalesPNCode = "100302",
        TotalQty = 10,
        TotalFoc = 0,
        TotalAmount = 1030.00m,
        IsCreditVerified = true,
        Items = new List<OrderItemDto>
        {
            new()
            {
                No = 18,
                Description = "ZAIN PALM OIL",
                Code = "1418",
                Packing = "18LTR",
                Qty = 10,
                Foc = 0,
                UnitPrice = 103.00m,
                TotalPrice = 1030.00m,
                Remarks = "Handwritten code change: 1425"
            }
        }
    }
};

// Helper method to verify database connectivity
string? GetConnectionString()
{
    var connStr = app.Configuration.GetConnectionString(CONN_STRING_NAME);
    if (string.IsNullOrEmpty(connStr))
    {
        // Check if there is an environment variable or default fallback
        return null;
    }
    return connStr;
}

bool IsSqlConnected()
{
    var connStr = GetConnectionString();
    if (connStr == null) return false;
    
    try
    {
        using var conn = new SqlConnection(connStr);
        conn.Open();
        return true;
    }
    catch
    {
        return false;
    }
}

// ==========================================================================
// REST API ENDPOINTS
// ==========================================================================

// 0. GET /api/salespersons?query=...
// Fetches salesperson master list based on search term
app.MapGet("/api/salespersons", async (HttpContext context) =>
{
    string? query = context.Request.Query["query"].ToString()?.Trim()?.ToLower();
    
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            var sqlSalespersons = new List<SalespersonDto>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = $"SELECT [ERP EMP CODE] as SalesPNCode, SalespersonName, UserType, [Supervisor ERP EMP CODE] as SupervisorCode FROM {TBL_SALESPERSON}";
                if (!string.IsNullOrEmpty(query))
                {
                    sql += " WHERE LOWER(SalespersonName) LIKE @query OR LOWER([ERP EMP CODE]) LIKE @query";
                }
                sql += " ORDER BY SalespersonName";

                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(query))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    sqlSalespersons.Add(new SalespersonDto
                    {
                        Code = reader["SalesPNCode"]?.ToString() ?? "",
                        Name = reader["SalespersonName"]?.ToString() ?? "",
                        UserType = reader["UserType"]?.ToString() ?? "",
                        SupervisorCode = reader["SupervisorCode"]?.ToString()
                    });
                }
            }
            return Results.Ok(sqlSalespersons);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Salesperson query failed: {ex.Message}.");
        }
    }
    
    // Mock Fallback
    var mockSalespersons = new List<SalespersonDto>
    {
        new() { Code = "100302", Name = "Jawed Akthar" },
        new() { Code = "5003", Name = "Default Sales Office" }
    };

    if (string.IsNullOrEmpty(query))
    {
        return Results.Ok(mockSalespersons);
    }
    
    var matches = mockSalespersons.Where(s => s.Name.ToLower().Contains(query) || s.Code.ToLower().Contains(query)).ToList();
    return Results.Ok(matches);
});

// 0.1 POST /api/login
// Validates username and password
app.MapPost("/api/login", async (LoginRequestDto req) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = $"SELECT [ERP EMP CODE] as SalesPNCode, SalespersonName, UserType, [Supervisor ERP EMP CODE] as SupervisorCode FROM {TBL_SALESPERSON} WHERE [ERP EMP CODE] = @code AND Password = @pass";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@code", req.EmpCode);
                cmd.Parameters.AddWithValue("@pass", req.Password);
                
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var dto = new SalespersonDto
                    {
                        Code = reader["SalesPNCode"]?.ToString() ?? "",
                        Name = reader["SalespersonName"]?.ToString() ?? "",
                        UserType = reader["UserType"]?.ToString() ?? "Salesman",
                        SupervisorCode = reader["SupervisorCode"]?.ToString()
                    };
                    await reader.CloseAsync();
                    
                    try
                    {
                        string routeSql = "SELECT TOP 1 Route FROM CustomerSalespersonMapping WHERE SalesPNCode = @code AND Route IS NOT NULL AND Route != 'nan'";
                        using var routeCmd = new SqlCommand(routeSql, conn);
                        routeCmd.Parameters.AddWithValue("@code", dto.Code);
                        using var routeReader = await routeCmd.ExecuteReaderAsync();
                        if (await routeReader.ReadAsync())
                        {
                            dto.Route = routeReader.GetString(0);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Ignore missing Route column on Azure
                        Console.WriteLine($"Could not fetch Route: {ex.Message}");
                    }

                    return Results.Ok(dto);
                }
            }
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Login query failed: {ex.Message}.");
            return Results.StatusCode(500);
        }
    }
    
    // Mock Fallback
    if (req.EmpCode == "100302" && req.Password == "password") {
        return Results.Ok(new SalespersonDto { Code = "100302", Name = "Jawed Akthar", UserType = "Salesman" });
    }
    return Results.Unauthorized();
});

// 0.2 POST /api/change-password
// Changes the user's password after verifying the current one
app.MapPost("/api/change-password", async (ChangePasswordDto req) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                
                // First verify the current password
                string verifySql = $"SELECT COUNT(1) FROM {TBL_SALESPERSON} WHERE [ERP EMP CODE] = @code AND Password = @currentPass";
                using var verifyCmd = new SqlCommand(verifySql, conn);
                verifyCmd.Parameters.AddWithValue("@code", req.EmpCode);
                verifyCmd.Parameters.AddWithValue("@currentPass", req.CurrentPassword);
                
                int count = Convert.ToInt32(await verifyCmd.ExecuteScalarAsync());
                if (count == 0)
                {
                    return Results.Unauthorized();
                }
                
                // Then update the password
                string updateSql = $"UPDATE {TBL_SALESPERSON} SET Password = @newPass WHERE [ERP EMP CODE] = @code";
                using var updateCmd = new SqlCommand(updateSql, conn);
                updateCmd.Parameters.AddWithValue("@code", req.EmpCode);
                updateCmd.Parameters.AddWithValue("@newPass", req.NewPassword);
                
                int rowsAffected = await updateCmd.ExecuteNonQueryAsync();
                if (rowsAffected > 0)
                {
                    return Results.Ok();
                }
                return Results.StatusCode(500);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Change Password query failed: {ex.Message}.");
            return Results.StatusCode(500);
        }
    }
    
    // Mock Fallback
    if (req.EmpCode == "100302" && req.CurrentPassword == "password") {
        return Results.Ok();
    }
    return Results.Unauthorized();
});

// 1. GET /api/customers?query=...
// Fetches customer master list based on search term
app.MapGet("/api/customers", async (HttpContext context) =>
{
    string? query = context.Request.Query["query"].ToString()?.Trim()?.ToLower();
    string? spCode = context.Request.Query["salesPersonCode"].ToString()?.Trim();
    
    // Check if SQL database is configured and connected
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            var sqlCustomers = new List<CustomerDto>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                // Parameterized SQL query on your Customer table
                string sql = "";
                if (!string.IsNullOrEmpty(spCode))
                {
                    sql = $"SELECT C.SAPCode as CustomerCode, C.NAME1 as CustomerName, C.PriceType FROM {TBL_CUSTOMER} C INNER JOIN CustomerSalespersonMapping M ON C.SAPCode = M.CustomerCode WHERE M.SalesPNCode = @spCode";
                    if (!string.IsNullOrEmpty(query))
                    {
                        sql += " AND (LOWER(C.NAME1) LIKE @query OR LOWER(C.SAPCode) LIKE @query)";
                    }
                }
                else 
                {
                    sql = $"SELECT SAPCode as CustomerCode, NAME1 as CustomerName, PriceType FROM {TBL_CUSTOMER}";
                    if (!string.IsNullOrEmpty(query))
                    {
                        sql += " WHERE LOWER(NAME1) LIKE @query OR LOWER(SAPCode) LIKE @query";
                    }
                }
                sql += " ORDER BY NAME1";

                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(spCode))
                {
                    cmd.Parameters.AddWithValue("@spCode", spCode);
                }
                if (!string.IsNullOrEmpty(query))
                {
                    cmd.Parameters.AddWithValue("@query", $"%{query}%");
                }

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    sqlCustomers.Add(new CustomerDto
                    {
                        Code = reader["CustomerCode"]?.ToString() ?? "",
                        Name = reader["CustomerName"]?.ToString() ?? "",
                        PriceType = reader["PriceType"]?.ToString() ?? "REGULAR"
                    });
                }
            }
            return Results.Ok(sqlCustomers);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Customer query failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Mock Fallback
    if (string.IsNullOrEmpty(query))
    {
        return Results.Ok(mockCustomers);
    }
    
    var matches = mockCustomers.Where(c => c.Name.ToLower().Contains(query) || c.Code.ToLower().Contains(query)).ToList();
    return Results.Ok(matches);
});

// 2. GET /api/materials
// Loads the dynamic material catalog

app.MapGet("/api/foc-eligibility/{customerCode}", async (string customerCode) =>
{
    var allowedMaterials = new System.Collections.Generic.List<string>();
    try
    {
        using (var conn = new Microsoft.Data.SqlClient.SqlConnection(GetConnectionString()))
        {
            await conn.OpenAsync();
            string sql = @"
                SELECT e.ErpCode
                FROM FocEligibility e
                INNER JOIN FocCustomerCategory c ON e.FocCategory = c.FocCategory
                WHERE c.CustomerCode = @code AND e.IsAllowed = 1
            ";
            using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@code", customerCode);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        allowedMaterials.Add(reader.GetString(0));
                    }
                }
            }
        }
        return Results.Ok(allowedMaterials);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});


app.MapGet("/api/materials", async () =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            var sqlMaterials = new List<MaterialDto>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                var pricesDict = new Dictionary<string, Dictionary<string, MaterialPriceDto>>();
                try
                {
                    string priceSql = "SELECT ERP_CODE, PriceType, Price, UOM FROM PriceMaster WHERE EffectiveTo IS NULL";
                    using (var cmdPrices = new SqlCommand(priceSql, conn))
                    using (var readerPrices = await cmdPrices.ExecuteReaderAsync())
                    {
                        while (await readerPrices.ReadAsync())
                        {
                            string code = readerPrices["ERP_CODE"]?.ToString() ?? "";
                            string pType = readerPrices["PriceType"]?.ToString() ?? "REGULAR";
                            decimal pPrice = readerPrices["Price"] != DBNull.Value ? Convert.ToDecimal(readerPrices["Price"]) : 0m;
                            string uom = readerPrices["UOM"]?.ToString() ?? "EA";
                            if (!pricesDict.ContainsKey(code))
                            {
                                pricesDict[code] = new Dictionary<string, MaterialPriceDto>();
                            }
                            pricesDict[code][pType] = new MaterialPriceDto { Price = pPrice, UOM = uom };
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not fetch PriceMaster: {ex.Message}");
                }

                // Querying your Material table
                // Using ROW_NUMBER for ItemNo since it doesn't exist explicitly
                string sql = $@"SELECT 
                                    ROW_NUMBER() OVER (ORDER BY Material) as ItemNo, 
                                    Material_Description as Description, 
                                    Material as MaterialCode, 
                                    Base_Unit_of_Measure as Packing, 
                                    Price as DefaultPrice,
                                    [Material Group] as MaterialGroup,
                                    [Sales Group] as SalesGroup
                                FROM {TBL_MATERIAL} 
                                ORDER BY Material";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var matDto = new MaterialDto
                    {
                        No = Convert.ToInt32(reader["ItemNo"]),
                        Description = reader["Description"]?.ToString() ?? "",
                        Code = reader["MaterialCode"]?.ToString() ?? "",
                        Packing = reader["Packing"]?.ToString() ?? "",
                        DefaultPrice = reader["DefaultPrice"] != DBNull.Value ? Convert.ToDecimal(reader["DefaultPrice"]) : 0.00m,
                        Group = reader["MaterialGroup"]?.ToString() ?? "",
                        SalesGroup = reader["SalesGroup"]?.ToString() ?? ""
                    };
                    
                    if (pricesDict.TryGetValue(matDto.Code, out var pDict))
                    {
                        matDto.Prices = pDict;
                    }
                    
                    sqlMaterials.Add(matDto);
                }
            }
            return Results.Ok(sqlMaterials);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Material query failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    return Results.Ok(mockMaterials);
});

// 3. GET /api/orders
// Returns all booked orders
app.MapGet("/api/orders", async (HttpContext context) =>
{
    string? salesPersonCode = context.Request.Query["salesPersonCode"];
    string? userType = context.Request.Query["userType"];
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            var sqlOrders = new List<OrderDto>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                // Fetch OrderHeaders from database
                string sql = "SELECT o.OrderID, o.OrderNumber, o.OrderDate, o.CustomerName, o.CustomerCode, o.PaymentMode, o.SalesPerson, o.SalesPNCode, o.TotalQty, o.TotalFOC, o.TotalAmount, o.IsCreditVerified, o.Approver, o.Status, o.RejectReason, o.RequiredDate, o.ReferenceNumber, a.SalespersonName as ApproverName FROM OrderHeader o LEFT JOIN [Salesperson Master] a ON o.Approver = a.[ERP EMP CODE]";
                if (!string.IsNullOrEmpty(salesPersonCode))
                {
                    if (userType.Equals("Supervisor", StringComparison.OrdinalIgnoreCase))
                        sql += " WHERE o.SalesPNCode = @spCode OR o.Approver = @spCode OR o.SalesPNCode IN (SELECT [ERP EMP CODE] FROM [Salesperson Master] WHERE [Supervisor ERP EMP CODE] = @spCode)";
                    else if (userType.Equals("Manager", StringComparison.OrdinalIgnoreCase))
                        sql += " WHERE o.SalesPNCode = @spCode OR o.Approver = @spCode OR o.SalesPNCode IN (SELECT [ERP EMP CODE] FROM [Salesperson Master] WHERE [Manager ERP EMP CODE] = @spCode)";
                    else
                        sql += " WHERE o.SalesPNCode = @spCode";
                }
                sql += " ORDER BY o.OrderID DESC";
                using var cmd = new SqlCommand(sql, conn);
                if (!string.IsNullOrEmpty(salesPersonCode))
                {
                    cmd.Parameters.AddWithValue("@spCode", salesPersonCode);
                }
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    sqlOrders.Add(new OrderDto
                    {
                        Id = reader["OrderID"].ToString() ?? "",
                        MemoNumber = reader["OrderNumber"].ToString() ?? "",
                        Date = Convert.ToDateTime(reader["OrderDate"]).ToString("yyyy-MM-dd"),
                        CustomerName = reader["CustomerName"].ToString() ?? "",
                        CustomerCode = reader["CustomerCode"].ToString() ?? "",
                        PaymentMode = reader["PaymentMode"].ToString() ?? "",
                        SalesPerson = reader["SalesPerson"].ToString() ?? "",
                        SalesPNCode = reader["SalesPNCode"].ToString() ?? "",
                        TotalQty = Convert.ToInt32(reader["TotalQty"]),
                        TotalFoc = Convert.ToInt32(reader["TotalFOC"]),
                        TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                        IsCreditVerified = Convert.ToBoolean(reader["IsCreditVerified"]),
                        Approver = reader["Approver"]?.ToString(),
                        ApproverName = reader["ApproverName"]?.ToString(),
                        Status = reader["Status"]?.ToString() ?? "Pending",
                        RejectReason = reader["RejectReason"]?.ToString(),
                        RequiredDate = reader["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(reader["RequiredDate"]).ToString("yyyy-MM-dd") : "",
                        ReferenceNumber = reader["ReferenceNumber"]?.ToString() ?? ""
                    });
                }
            }
            return Results.Ok(sqlOrders);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Orders query failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    return Results.Ok(mockOrders);
});

// 4. GET /api/orders/{id}
// Fetches detailed information for a single order
app.MapGet("/api/orders/{id}", async (string id) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr) && int.TryParse(id, out int numericId))
    {
        try
        {
            OrderDto? order = null;
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                
                // Fetch Header
                string sqlHeader = "SELECT o.OrderID, o.OrderNumber, o.OrderDate, o.CustomerName, o.CustomerCode, o.PaymentMode, o.SalesPerson, o.SalesPNCode, o.TotalQty, o.TotalFOC, o.TotalAmount, o.IsCreditVerified, o.Approver, o.Status, o.RejectReason, o.RequiredDate, o.ReferenceNumber, a.SalespersonName as ApproverName FROM OrderHeader o LEFT JOIN [Salesperson Master] a ON o.Approver = a.[ERP EMP CODE] WHERE o.OrderID = @id";
                using (var cmdHeader = new SqlCommand(sqlHeader, conn))
                {
                    cmdHeader.Parameters.AddWithValue("@id", numericId);
                    using var reader = await cmdHeader.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        order = new OrderDto
                        {
                            Id = reader["OrderID"].ToString() ?? "",
                            MemoNumber = reader["OrderNumber"].ToString() ?? "",
                            Date = Convert.ToDateTime(reader["OrderDate"]).ToString("yyyy-MM-dd"),
                            CustomerName = reader["CustomerName"].ToString() ?? "",
                            CustomerCode = reader["CustomerCode"].ToString() ?? "",
                            PaymentMode = reader["PaymentMode"].ToString() ?? "",
                            SalesPerson = reader["SalesPerson"].ToString() ?? "",
                            SalesPNCode = reader["SalesPNCode"].ToString() ?? "",
                            TotalQty = Convert.ToInt32(reader["TotalQty"]),
                            TotalFoc = Convert.ToInt32(reader["TotalFOC"]),
                            TotalAmount = Convert.ToDecimal(reader["TotalAmount"]),
                            IsCreditVerified = Convert.ToBoolean(reader["IsCreditVerified"]),
                            Approver = reader["Approver"]?.ToString(),
                            ApproverName = reader["ApproverName"]?.ToString(),
                            Status = reader["Status"]?.ToString() ?? "Pending",
                            RejectReason = reader["RejectReason"]?.ToString(),
                            RequiredDate = reader["RequiredDate"] != DBNull.Value ? Convert.ToDateTime(reader["RequiredDate"]).ToString("yyyy-MM-dd") : "",
                            ReferenceNumber = reader["ReferenceNumber"]?.ToString() ?? "",
                            Items = new List<OrderItemDto>()
                        };
                    }
                }

                // Fetch details if header exists
                if (order != null)
                {
                    string sqlDetails = "SELECT ItemNo, MaterialCode, MaterialDescription, Packing, OrderedQty, FOCQty, UnitPrice, LineTotal, Remarks FROM OrderDetails WHERE OrderID = @id ORDER BY ItemNo";
                    using var cmdDetails = new SqlCommand(sqlDetails, conn);
                    cmdDetails.Parameters.AddWithValue("@id", numericId);
                    using var readerDetails = await cmdDetails.ExecuteReaderAsync();
                    while (await readerDetails.ReadAsync())
                    {
                        order.Items.Add(new OrderItemDto
                        {
                            No = Convert.ToInt32(readerDetails["ItemNo"]),
                            Code = readerDetails["MaterialCode"].ToString() ?? "",
                            Description = readerDetails["MaterialDescription"].ToString() ?? "",
                            Packing = readerDetails["Packing"].ToString() ?? "",
                            Qty = Convert.ToInt32(readerDetails["OrderedQty"]),
                            Foc = Convert.ToInt32(readerDetails["FOCQty"]),
                            UnitPrice = Convert.ToDecimal(readerDetails["UnitPrice"]),
                            TotalPrice = Convert.ToDecimal(readerDetails["LineTotal"]),
                            Remarks = readerDetails["Remarks"].ToString() ?? ""
                        });
                    }
                    return Results.Ok(order);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Single Order query failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    var match = mockOrders.FirstOrDefault(o => o.Id == id);
    return match != null ? Results.Ok(match) : Results.NotFound($"Order with ID {id} not found.");
});

// 5. POST /api/orders
// Book a new order memo
app.MapPost("/api/orders", async (OrderDto newOrder) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                
                // Set custom properties
                int memoSeq = 1000 + new Random().Next(100);
                string memoNum = $"{newOrder.SalesPNCode}-{DateTime.Now:yyMMdd}-{memoSeq}";
                
                using var trans = conn.BeginTransaction();
                try
                {
                    // 1. Check and insert Customer Master
                    string sqlCust = @"
                        IF NOT EXISTS (SELECT 1 FROM [Customer Master] WHERE [SAPCode] = @code)
                        BEGIN
                            INSERT INTO [Customer Master] ([SAPCode], [NAME1], [SALES_OFFICE]) 
                            VALUES (@code, @name, @office)
                        END";
                    using (var cmdCust = new SqlCommand(sqlCust, conn, trans))
                    {
                        cmdCust.Parameters.AddWithValue("@code", newOrder.CustomerCode);
                        cmdCust.Parameters.AddWithValue("@name", newOrder.CustomerName);
                        cmdCust.Parameters.AddWithValue("@office", newOrder.SalesPNCode);
                        await cmdCust.ExecuteNonQueryAsync();
                    }

                    // 2. Check and insert Salesperson Master
                    string sqlSales = @"
                        IF NOT EXISTS (SELECT 1 FROM [Salesperson Master] WHERE [ERP EMP CODE] = @pn)
                        BEGIN
                            INSERT INTO [Salesperson Master] ([ERP EMP CODE], [SalespersonName], [Password], [UserType]) 
                            VALUES (@pn, @sales, 'password', 'Salesman')
                        END";
                    using (var cmdSales = new SqlCommand(sqlSales, conn, trans))
                    {
                        cmdSales.Parameters.AddWithValue("@pn", newOrder.SalesPNCode);
                        cmdSales.Parameters.AddWithValue("@sales", newOrder.SalesPerson ?? "Default");
                        await cmdSales.ExecuteNonQueryAsync();
                    }

                    // Fetch Approver
                    string? approverCode = null;
                    string sqlApprover = "SELECT [Supervisor ERP EMP CODE] FROM [Salesperson Master] WHERE [ERP EMP CODE] = @pn";
                    using (var cmdApp = new SqlCommand(sqlApprover, conn, trans))
                    {
                        cmdApp.Parameters.AddWithValue("@pn", newOrder.SalesPNCode);
                        var result = await cmdApp.ExecuteScalarAsync();
                        if (result != null && result != DBNull.Value)
                            approverCode = result.ToString();
                    }

                    // 3. Insert Header
                    string sqlHeader = @"
                        INSERT INTO OrderHeader (OrderNumber, OrderDate, CustomerName, CustomerCode, PaymentMode, SalesPerson, SalesPNCode, TotalQty, TotalFOC, TotalAmount, IsCreditVerified, CreatedTime, Approver, Status, RequiredDate, ReferenceNumber)
                        OUTPUT INSERTED.OrderID
                        VALUES (@num, @date, @custName, @custCode, @pay, @sales, @pn, @qty, @foc, @amt, @verified, GETDATE(), @approver, 'Pending', @reqDate, @refNum)";
                    
                    int newOrderId = 0;
                    using (var cmd = new SqlCommand(sqlHeader, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@num", memoNum);
                        cmd.Parameters.AddWithValue("@date", DateTime.Parse(newOrder.Date));
                        cmd.Parameters.AddWithValue("@custName", newOrder.CustomerName);
                        cmd.Parameters.AddWithValue("@custCode", newOrder.CustomerCode);
                        cmd.Parameters.AddWithValue("@pay", newOrder.PaymentMode);
                        cmd.Parameters.AddWithValue("@sales", newOrder.SalesPerson);
                        cmd.Parameters.AddWithValue("@pn", newOrder.SalesPNCode);
                        cmd.Parameters.AddWithValue("@qty", newOrder.TotalQty);
                        cmd.Parameters.AddWithValue("@foc", newOrder.TotalFoc);
                        cmd.Parameters.AddWithValue("@amt", newOrder.TotalAmount);
                        cmd.Parameters.AddWithValue("@verified", newOrder.PaymentMode == "CREDIT");
                        cmd.Parameters.AddWithValue("@approver", string.IsNullOrEmpty(approverCode) ? DBNull.Value : approverCode);
                        cmd.Parameters.AddWithValue("@reqDate", string.IsNullOrEmpty(newOrder.RequiredDate) ? DBNull.Value : DateTime.Parse(newOrder.RequiredDate));
                        cmd.Parameters.AddWithValue("@refNum", string.IsNullOrEmpty(newOrder.ReferenceNumber) ? DBNull.Value : newOrder.ReferenceNumber);

                        newOrderId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                    }

                    // Insert Details
                    foreach (var item in newOrder.Items)
                    {
                        string sqlDetail = @"
                            INSERT INTO OrderDetails (OrderID, ItemNo, MaterialCode, MaterialDescription, Packing, OrderedQty, FOCQty, UnitPrice, LineTotal, Remarks)
                            VALUES (@orderId, @no, @code, @desc, @pack, @qty, @foc, @price, @total, @remarks)";
                        
                        using var cmdDetail = new SqlCommand(sqlDetail, conn, trans);
                        cmdDetail.Parameters.AddWithValue("@orderId", newOrderId);
                        cmdDetail.Parameters.AddWithValue("@no", item.No);
                        cmdDetail.Parameters.AddWithValue("@code", item.Code);
                        cmdDetail.Parameters.AddWithValue("@desc", item.Description);
                        cmdDetail.Parameters.AddWithValue("@pack", item.Packing);
                        cmdDetail.Parameters.AddWithValue("@qty", item.Qty);
                        cmdDetail.Parameters.AddWithValue("@foc", item.Foc);
                        cmdDetail.Parameters.AddWithValue("@price", item.UnitPrice);
                        cmdDetail.Parameters.AddWithValue("@total", item.Qty * item.UnitPrice);
                        cmdDetail.Parameters.AddWithValue("@remarks", item.Remarks ?? "");

                        await cmdDetail.ExecuteNonQueryAsync();
                    }

                    await trans.CommitAsync();
                    
                    newOrder.Id = newOrderId.ToString();
                    newOrder.MemoNumber = memoNum;
                    newOrder.IsCreditVerified = newOrder.PaymentMode == "CREDIT";
                    return Results.Created($"/api/orders/{newOrderId}", newOrder);
                }
                catch
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Insert Order failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    newOrder.Id = $"ord-{DateTime.Now.Ticks}";
    int mockSeq = mockOrders.Count + 1000 + new Random().Next(10);
    newOrder.MemoNumber = $"{newOrder.SalesPNCode}-{mockSeq}";
    newOrder.IsCreditVerified = newOrder.PaymentMode == "CREDIT";
    mockOrders.Add(newOrder);
    return Results.Created($"/api/orders/{newOrder.Id}", newOrder);
});

// 6. PUT /api/orders/{id}
// Update an existing order memo
app.MapPut("/api/orders/{id}", async (string id, OrderDto updatedOrder) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr) && int.TryParse(id, out int numericId))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                using var trans = conn.BeginTransaction();
                try
                {
                    // Check if order is already approved
                    string checkStatusSql = "SELECT Status FROM OrderHeader WHERE OrderID = @id";
                    using (var cmdCheck = new SqlCommand(checkStatusSql, conn, trans))
                    {
                        cmdCheck.Parameters.AddWithValue("@id", numericId);
                        var status = await cmdCheck.ExecuteScalarAsync();
                        if (status != null && status.ToString() == "Approved")
                        {
                            await trans.RollbackAsync();
                            return Results.BadRequest("Order is already approved and cannot be edited.");
                        }
                    }

                    // 1. Check and insert Customer Master
                    string sqlCust = @"
                        IF NOT EXISTS (SELECT 1 FROM [Customer Master] WHERE [SAPCode] = @code)
                        BEGIN
                            INSERT INTO [Customer Master] ([SAPCode], [NAME1], [SALES_OFFICE]) 
                            VALUES (@code, @name, @office)
                        END";
                    using (var cmdCust = new SqlCommand(sqlCust, conn, trans))
                    {
                        cmdCust.Parameters.AddWithValue("@code", updatedOrder.CustomerCode);
                        cmdCust.Parameters.AddWithValue("@name", updatedOrder.CustomerName);
                        cmdCust.Parameters.AddWithValue("@office", updatedOrder.SalesPNCode);
                        await cmdCust.ExecuteNonQueryAsync();
                    }

                    // 2. Check and insert Salesperson Master
                    string sqlSales = @"
                        IF NOT EXISTS (SELECT 1 FROM [Salesperson Master] WHERE [ERP EMP CODE] = @pn)
                        BEGIN
                            INSERT INTO [Salesperson Master] ([ERP EMP CODE], [SalespersonName], [Password], [UserType]) 
                            VALUES (@pn, @sales, 'password', 'Salesman')
                        END";
                    using (var cmdSales = new SqlCommand(sqlSales, conn, trans))
                    {
                        cmdSales.Parameters.AddWithValue("@pn", updatedOrder.SalesPNCode);
                        cmdSales.Parameters.AddWithValue("@sales", updatedOrder.SalesPerson ?? "Default");
                        await cmdSales.ExecuteNonQueryAsync();
                    }

                    // 3. Update Header
                    string sqlHeader = @"
                        UPDATE OrderHeader 
                        SET OrderDate = @date, CustomerName = @custName, CustomerCode = @custCode, 
                            PaymentMode = @pay, SalesPerson = @sales, SalesPNCode = @pn, 
                            TotalQty = @qty, TotalFOC = @foc, TotalAmount = @amt, 
                            IsCreditVerified = @verified, ModifiedTime = GETDATE(),
                            Status = CASE WHEN Status = 'Rejected' THEN 'Pending' ELSE Status END,
                            RejectReason = CASE WHEN Status = 'Rejected' THEN NULL ELSE RejectReason END,
                            RequiredDate = @reqDate, ReferenceNumber = @refNum
                        WHERE OrderID = @id";
                    
                    using (var cmd = new SqlCommand(sqlHeader, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@id", numericId);
                        cmd.Parameters.AddWithValue("@date", DateTime.Parse(updatedOrder.Date));
                        cmd.Parameters.AddWithValue("@custName", updatedOrder.CustomerName);
                        cmd.Parameters.AddWithValue("@custCode", updatedOrder.CustomerCode);
                        cmd.Parameters.AddWithValue("@pay", updatedOrder.PaymentMode);
                        cmd.Parameters.AddWithValue("@sales", updatedOrder.SalesPerson ?? "Jawed Akthar");
                        cmd.Parameters.AddWithValue("@pn", updatedOrder.SalesPNCode ?? "100302");
                        cmd.Parameters.AddWithValue("@qty", updatedOrder.TotalQty);
                        cmd.Parameters.AddWithValue("@foc", updatedOrder.TotalFoc);
                        cmd.Parameters.AddWithValue("@amt", updatedOrder.TotalAmount);
                        cmd.Parameters.AddWithValue("@verified", updatedOrder.PaymentMode == "CREDIT");
                        cmd.Parameters.AddWithValue("@reqDate", string.IsNullOrEmpty(updatedOrder.RequiredDate) ? DBNull.Value : DateTime.Parse(updatedOrder.RequiredDate));
                        cmd.Parameters.AddWithValue("@refNum", string.IsNullOrEmpty(updatedOrder.ReferenceNumber) ? DBNull.Value : updatedOrder.ReferenceNumber);

                        int rows = await cmd.ExecuteNonQueryAsync();
                        if (rows == 0) return Results.NotFound($"Order with ID {id} not found in database.");
                    }

                    // Clear old details
                    string sqlDeleteDetails = "DELETE FROM OrderDetails WHERE OrderID = @id";
                    using (var cmdDelete = new SqlCommand(sqlDeleteDetails, conn, trans))
                    {
                        cmdDelete.Parameters.AddWithValue("@id", numericId);
                        await cmdDelete.ExecuteNonQueryAsync();
                    }

                    // Insert new details
                    foreach (var item in updatedOrder.Items)
                    {
                        string sqlDetail = @"
                            INSERT INTO OrderDetails (OrderID, ItemNo, MaterialCode, MaterialDescription, Packing, OrderedQty, FOCQty, UnitPrice, LineTotal, Remarks)
                            VALUES (@orderId, @no, @code, @desc, @pack, @qty, @foc, @price, @total, @remarks)";
                        
                        using var cmdDetail = new SqlCommand(sqlDetail, conn, trans);
                        cmdDetail.Parameters.AddWithValue("@orderId", numericId);
                        cmdDetail.Parameters.AddWithValue("@no", item.No);
                        cmdDetail.Parameters.AddWithValue("@code", item.Code);
                        cmdDetail.Parameters.AddWithValue("@desc", item.Description);
                        cmdDetail.Parameters.AddWithValue("@pack", item.Packing);
                        cmdDetail.Parameters.AddWithValue("@qty", item.Qty);
                        cmdDetail.Parameters.AddWithValue("@foc", item.Foc);
                        cmdDetail.Parameters.AddWithValue("@price", item.UnitPrice);
                        cmdDetail.Parameters.AddWithValue("@total", item.Qty * item.UnitPrice);
                        cmdDetail.Parameters.AddWithValue("@remarks", item.Remarks ?? "");

                        await cmdDetail.ExecuteNonQueryAsync();
                    }

                    await trans.CommitAsync();
                    updatedOrder.Id = id;
                    updatedOrder.IsCreditVerified = updatedOrder.PaymentMode == "CREDIT";
                    return Results.Ok(updatedOrder);
                }
                catch
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Update Order failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    var index = mockOrders.FindIndex(o => o.Id == id);
    if (index == -1) return Results.NotFound($"Order with ID {id} not found.");
    
    updatedOrder.Id = id;
    updatedOrder.MemoNumber = mockOrders[index].MemoNumber; // Retain original memo #
    updatedOrder.IsCreditVerified = updatedOrder.PaymentMode == "CREDIT";
    mockOrders[index] = updatedOrder;
    
    return Results.Ok(updatedOrder);
});

// 6.1. PUT /api/orders/{id}/approve
// Approve an order memo
app.MapPut("/api/orders/{id}/approve", async (string id, ApproveOrderRequestDto req) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr) && int.TryParse(id, out int numericId))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = "UPDATE OrderHeader SET Status = 'Approved', RejectReason = @reason, Approver = @approver WHERE OrderID = @id AND (Approver = @approver OR SalesPNCode IN (SELECT [ERP EMP CODE] FROM [Salesperson Master] WHERE [Manager ERP EMP CODE] = @approver OR [Supervisor ERP EMP CODE] = @approver))";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", numericId);
                cmd.Parameters.AddWithValue("@approver", req.ApproverCode);
                cmd.Parameters.AddWithValue("@reason", (object?)req.Reason ?? DBNull.Value);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.BadRequest("Order not found, or you are not authorized to approve this order.");
                
                return Results.Ok();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Approve Order failed: {ex.Message}");
            return Results.StatusCode(500);
        }
    }
    return Results.BadRequest("Invalid Order ID or Configuration.");
});

// 6.2. PUT /api/orders/{id}/reject
// Reject an order memo
app.MapPut("/api/orders/{id}/reject", async (string id, RejectOrderRequestDto req) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr) && int.TryParse(id, out int numericId))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = "UPDATE OrderHeader SET Status = 'Rejected', RejectReason = @reason, Approver = @approver WHERE OrderID = @id AND (Approver = @approver OR SalesPNCode IN (SELECT [ERP EMP CODE] FROM [Salesperson Master] WHERE [Manager ERP EMP CODE] = @approver OR [Supervisor ERP EMP CODE] = @approver))";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", numericId);
                cmd.Parameters.AddWithValue("@approver", req.ApproverCode);
                cmd.Parameters.AddWithValue("@reason", req.Reason);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0) return Results.BadRequest("Order not found, or you are not authorized to reject this order.");
                
                return Results.Ok();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Reject Order failed: {ex.Message}");
            return Results.StatusCode(500);
        }
    }
    return Results.BadRequest("Invalid Order ID or Configuration.");
});

// 7. DELETE /api/orders/{id}
// Delete an order memo
app.MapDelete("/api/orders/{id}", async (string id) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr) && int.TryParse(id, out int numericId))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                
                // Cascade delete via FK in OrderDetails removes both header & details
                string sql = "DELETE FROM OrderHeader WHERE OrderID = @id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", numericId);
                
                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows > 0)
                {
                    return Results.Ok(new { message = $"Order with ID {id} deleted successfully." });
                }
                return Results.NotFound($"Order with ID {id} not found in database.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Delete Order failed: {ex.Message}. Falling back to memory store.");
        }
    }

    // Fallback
    var order = mockOrders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound($"Order with ID {id} not found.");
    
    mockOrders.Remove(order);
    return Results.Ok(new { message = $"Order with ID {id} deleted successfully from mock store." });
});

// 8. GET /api/mappings
// Fetches all customer-salesperson mappings
app.MapGet("/api/mappings", async () =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            var sqlMappings = new List<MappingDto>();
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = $@"
                    SELECT M.MappingID, M.CustomerCode, C.NAME1 as CustomerName, M.SalesPNCode, S.SalespersonName 
                    FROM CustomerSalespersonMapping M
                    INNER JOIN {TBL_CUSTOMER} C ON M.CustomerCode = C.SAPCode
                    LEFT JOIN {TBL_SALESPERSON} S ON M.SalesPNCode = S.[ERP EMP CODE]
                    ORDER BY M.MappingID DESC
                ";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    sqlMappings.Add(new MappingDto
                    {
                        MappingID = Convert.ToInt32(reader["MappingID"]),
                        CustomerCode = reader["CustomerCode"]?.ToString() ?? "",
                        CustomerName = reader["CustomerName"]?.ToString() ?? "Unknown Customer",
                        SalesPNCode = reader["SalesPNCode"]?.ToString() ?? "",
                        SalespersonName = reader["SalespersonName"]?.ToString() ?? "Unknown Salesperson"
                    });
                }
            }
            return Results.Ok(sqlMappings);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Mappings query failed: {ex.Message}");
            return Results.Problem("Database error occurred while fetching mappings.");
        }
    }
    return Results.Ok(new List<MappingDto>()); // Mock fallback empty list
});

// 9. POST /api/mappings
// Creates a new customer-salesperson mapping
app.MapPost("/api/mappings", async (MappingRequestDto req) =>
{
    if (string.IsNullOrEmpty(req.CustomerCode) || string.IsNullOrEmpty(req.SalesPNCode))
        return Results.BadRequest("CustomerCode and SalesPNCode are required.");

    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                
                // Check if mapping already exists
                string checkSql = "SELECT COUNT(*) FROM CustomerSalespersonMapping WHERE CustomerCode = @c AND SalesPNCode = @s";
                using (var checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@c", req.CustomerCode);
                    checkCmd.Parameters.AddWithValue("@s", req.SalesPNCode);
                    int count = (int)(await checkCmd.ExecuteScalarAsync() ?? 0);
                    if (count > 0) return Results.BadRequest("Mapping already exists.");
                }

                // Check restrict multiple routes
                string restrictSql = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = 'RestrictMultipleRoutes'";
                using (var restrictCmd = new SqlCommand(restrictSql, conn))
                {
                    var restrictVal = await restrictCmd.ExecuteScalarAsync();
                    if (restrictVal?.ToString()?.ToLower() == "true")
                    {
                        string anyCheckSql = "SELECT COUNT(*) FROM CustomerSalespersonMapping WHERE CustomerCode = @c AND SalesPNCode != @s";
                        using (var anyCheckCmd = new SqlCommand(anyCheckSql, conn))
                        {
                            anyCheckCmd.Parameters.AddWithValue("@c", req.CustomerCode);
                            anyCheckCmd.Parameters.AddWithValue("@s", req.SalesPNCode);
                            int anyCount = (int)(await anyCheckCmd.ExecuteScalarAsync() ?? 0);
                            if (anyCount > 0) return Results.BadRequest("Customer is already assigned to a different route/salesperson. Multiple assignments are restricted.");
                        }
                    }
                }

                string insertSql = "INSERT INTO CustomerSalespersonMapping (CustomerCode, SalesPNCode) OUTPUT INSERTED.MappingID VALUES (@c, @s)";
                using var cmd = new SqlCommand(insertSql, conn);
                cmd.Parameters.AddWithValue("@c", req.CustomerCode);
                cmd.Parameters.AddWithValue("@s", req.SalesPNCode);
                int newId = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                
                return Results.Ok(new { MappingID = newId, Message = "Mapping created successfully." });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Insert Mapping failed: {ex.Message}");
            return Results.Problem("Database error occurred while saving mapping.");
        }
    }
    return Results.Ok(new { MappingID = 999, Message = "Mock mapping created." });
});

// 10. DELETE /api/mappings/{id}
// Deletes a customer-salesperson mapping
app.MapDelete("/api/mappings/{id}", async (int id) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = "DELETE FROM CustomerSalespersonMapping WHERE MappingID = @id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);
                int rows = await cmd.ExecuteNonQueryAsync();
                
                if (rows > 0) return Results.Ok(new { Message = "Mapping deleted successfully." });
                return Results.NotFound("Mapping not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SQL Delete Mapping failed: {ex.Message}");
            return Results.Problem("Database error occurred while deleting mapping.");
        }
    }
    return Results.Ok(new { Message = "Mock mapping deleted." });
});

// Startup logs to show which mode is running
var serverStatus = IsSqlConnected() ? "Azure SQL Connected" : "Running in Mock Memory Mode (Azure SQL Connection String not active)";
Console.WriteLine("==========================================================================");
Console.WriteLine($" QFI Sales Customer Order Memo API - Status: {serverStatus}");
Console.WriteLine(" Listening on: http://localhost:5000");
Console.WriteLine("==========================================================================");

app.MapGet("/api/settings", async () =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string sql = "SELECT SettingValue FROM SystemSettings WHERE SettingKey = 'RestrictMultipleRoutes'";
                using var cmd = new SqlCommand(sql, conn);
                var val = await cmd.ExecuteScalarAsync();
                bool restrict = (val?.ToString()?.ToLower() == "true");
                return Results.Ok(new { restrictMultipleRoutes = restrict });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error reading settings: " + ex.Message);
        }
    }
    return Results.Ok(new { restrictMultipleRoutes = false });
});

app.MapPut("/api/settings", async (UpdateSettingsRequest req) =>
{
    var connStr = GetConnectionString();
    if (!string.IsNullOrEmpty(connStr))
    {
        try
        {
            using (var conn = new SqlConnection(connStr))
            {
                await conn.OpenAsync();
                string val = req.RestrictMultipleRoutes ? "true" : "false";
                string sql = @"
                    IF EXISTS (SELECT 1 FROM SystemSettings WHERE SettingKey = 'RestrictMultipleRoutes')
                        UPDATE SystemSettings SET SettingValue = @val WHERE SettingKey = 'RestrictMultipleRoutes'
                    ELSE
                        INSERT INTO SystemSettings (SettingKey, SettingValue) VALUES ('RestrictMultipleRoutes', @val)
                ";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@val", val);
                await cmd.ExecuteNonQueryAsync();
                return Results.Ok(new { Message = "Settings updated successfully" });
            }
        }
        catch (Exception ex)
        {
            return Results.StatusCode(500);
        }
    }
    return Results.StatusCode(500);
});


// ==========================================
// PRICE MAPPING ENDPOINTS
// ==========================================
app.MapGet("/api/prices", async () =>
{
    var prices = new List<object>();
    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(GetConnectionString()))
    {
        await conn.OpenAsync();
        string sql = @"
            SELECT p.PriceID, p.ERP_CODE, m.Material_Description as ItemName, p.PriceType, p.Price, p.UOM, 
                   CONVERT(varchar, p.EffectiveFrom, 23) as EffectiveFrom, 
                   CONVERT(varchar, p.EffectiveTo, 23) as EffectiveTo 
            FROM PriceMaster p 
            LEFT JOIN [Material Master] m ON CAST(p.ERP_CODE AS VARCHAR(100)) = CAST(m.Material AS VARCHAR(100)) 
            WHERE p.EffectiveTo IS NULL 
            ORDER BY p.ERP_CODE";
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(sql, conn))
        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                prices.Add(new {
                    Id = reader.GetInt32(0),
                    ErpCode = reader.GetString(1),
                    ItemName = reader.IsDBNull(2) ? "Unknown" : reader.GetString(2),
                    PriceType = reader.IsDBNull(3) ? "" : reader.GetString(3),
                    Price = reader.GetDecimal(4),
                    Uom = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    EffectiveFrom = reader.IsDBNull(6) ? null : reader.GetString(6),
                    EffectiveTo = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
        }
    }
    return Results.Ok(prices);
});

app.MapPut("/api/prices/{id}", async (int id, System.Text.Json.JsonElement payload) =>
{
    decimal newPrice = payload.GetProperty("price").GetDecimal();
    string effectiveFromStr = payload.GetProperty("effectiveFrom").GetString();
    string effectiveToStr = payload.TryGetProperty("effectiveTo", out var t) && t.ValueKind != System.Text.Json.JsonValueKind.Null ? t.GetString() : null;
    
    using (var conn = new Microsoft.Data.SqlClient.SqlConnection(GetConnectionString()))
    {
        await conn.OpenAsync();
        
        // 1. Get old record
        string getOldSql = "SELECT ERP_CODE, PriceType, UOM, EffectiveFrom FROM PriceMaster WHERE PriceID = @Id";
        string erpCode = "", priceType = "", uom = "";
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(getOldSql, conn))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    erpCode = reader.GetString(0);
                    priceType = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    uom = reader.IsDBNull(2) ? "" : reader.GetString(2);
                }
                else 
                {
                    return Results.NotFound("Price not found");
                }
            }
        }

        using (var transaction = conn.BeginTransaction())
        {
            try 
            {
                // 2. Close old record
                string updateOld = "UPDATE PriceMaster SET EffectiveTo = DATEADD(day, -1, @NewFrom) WHERE PriceID = @Id";
                using (var cmdUpdate = new Microsoft.Data.SqlClient.SqlCommand(updateOld, conn, transaction))
                {
                    cmdUpdate.Parameters.AddWithValue("@NewFrom", effectiveFromStr);
                    cmdUpdate.Parameters.AddWithValue("@Id", id);
                    await cmdUpdate.ExecuteNonQueryAsync();
                }

                // 3. Insert new record
                string insertNew = @"INSERT INTO PriceMaster (ERP_CODE, PriceType, Price, UOM, EffectiveFrom, EffectiveTo) 
                                     VALUES (@Erp, @Type, @Price, @Uom, @From, @To)";
                using (var cmdInsert = new Microsoft.Data.SqlClient.SqlCommand(insertNew, conn, transaction))
                {
                    cmdInsert.Parameters.AddWithValue("@Erp", erpCode);
                    cmdInsert.Parameters.AddWithValue("@Type", priceType);
                    cmdInsert.Parameters.AddWithValue("@Price", newPrice);
                    cmdInsert.Parameters.AddWithValue("@Uom", uom);
                    cmdInsert.Parameters.AddWithValue("@From", effectiveFromStr);
                    if (string.IsNullOrEmpty(effectiveToStr))
                        cmdInsert.Parameters.AddWithValue("@To", DBNull.Value);
                    else
                        cmdInsert.Parameters.AddWithValue("@To", effectiveToStr);
                    await cmdInsert.ExecuteNonQueryAsync();
                }
                
                transaction.Commit();
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return Results.Problem(ex.Message);
            }
        }
    }
    return Results.Ok();
});

app.MapPost("/api/prices/upload", async (Microsoft.AspNetCore.Http.HttpRequest request) =>
{
    if (!request.HasFormContentType) return Results.BadRequest("Expected form data.");
    var form = await request.ReadFormAsync();
    var file = form.Files.GetFile("file");
    if (file == null || file.Length == 0) return Results.BadRequest("No file uploaded.");
    
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "temp_upload.xlsx");
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    // Execute python script
    var process = new System.Diagnostics.Process();
    process.StartInfo.FileName = "python";
    process.StartInfo.Arguments = $"import_prices.py \"{filePath}\"";
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.UseShellExecute = false;
    process.Start();
    string output = process.StandardOutput.ReadToEnd();
    string err = process.StandardError.ReadToEnd();
    process.WaitForExit();
    
    if (process.ExitCode != 0)
    {
        return Results.Problem($"Failed to import: {err}");
    }
    
    return Results.Ok(new { message = "Import successful", output = output });
});

app.Run();


// ==========================================================================
// DTO DEFINITIONS FOR SERIALIZATION
// ==========================================================================
public class CustomerDto
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string PriceType { get; set; } = "REGULAR";
}

public class SalespersonDto
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string UserType { get; set; } = "";
    public string? SupervisorCode { get; set; }
    public string Route { get; set; } = "";
}

public class LoginRequestDto
{
    public string EmpCode { get; set; } = "";
    public string Password { get; set; } = "";
}

public class MaterialPriceDto
{
    public decimal Price { get; set; }
    public string UOM { get; set; } = "";
}

public class MaterialDto
{
    public int No { get; set; }
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string Packing { get; set; } = "";
    public decimal DefaultPrice { get; set; }
    public string Group { get; set; } = "";
    public string SalesGroup { get; set; } = "";
    public Dictionary<string, MaterialPriceDto> Prices { get; set; } = new();
}

public class OrderDto
{
    public string Id { get; set; } = "";
    public string MemoNumber { get; set; } = "";
    public string Date { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerCode { get; set; } = "";
    public string PaymentMode { get; set; } = "CREDIT";
    public string? SalesPerson { get; set; }
    public string? SalesPNCode { get; set; }
    public int TotalQty { get; set; }
    public int TotalFoc { get; set; }
    public decimal TotalAmount { get; set; }
    public bool IsCreditVerified { get; set; }
    public string? Approver { get; set; }
    public string? ApproverName { get; set; }
    public string Status { get; set; } = "Pending";
    public string? RejectReason { get; set; }
    public string RequiredDate { get; set; } = "";
    public string ReferenceNumber { get; set; } = "";
    public List<OrderItemDto> Items { get; set; } = new();
}

public class ApproveOrderRequestDto
{
    public string ApproverCode { get; set; } = "";
    public string? Reason { get; set; }
}

public class RejectOrderRequestDto
{
    public string ApproverCode { get; set; } = "";
    public string Reason { get; set; } = "";
}

public class UpdateSettingsRequest
{
    public bool RestrictMultipleRoutes { get; set; }
}

public class OrderItemDto
{
    public int No { get; set; }
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string Packing { get; set; } = "";
    public int Qty { get; set; }
    public int Foc { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string Remarks { get; set; } = "";
}

public class MappingDto
{
    public int MappingID { get; set; }
    public string CustomerCode { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string SalesPNCode { get; set; } = "";
    public string SalespersonName { get; set; } = "";
}

public class MappingRequestDto
{
    public string CustomerCode { get; set; } = "";
    public string SalesPNCode { get; set; } = "";
}

public class ChangePasswordDto
{
    public string EmpCode { get; set; } = "";
    public string CurrentPassword { get; set; } = "";
    public string NewPassword { get; set; } = "";
}
