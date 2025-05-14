using NLog;
using System.Linq;
 using NorthwindConsole.Model;
 using Microsoft.Extensions.Configuration;
 using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
 string path = Directory.GetCurrentDirectory() + "//nlog.config";
 
 // create instance of Logger
 var logger = LogManager.Setup().LoadConfigurationFromFile(path).GetCurrentClassLogger();
 
 logger.Info("Program started");
 
do
 {
   Console.WriteLine("1) Display categories");
   Console.WriteLine("2) Add category");
   Console.WriteLine("3) Display Category and related products");
   Console.WriteLine("4) Display all Categories and their related products");
   Console.WriteLine("5) Add product");
   Console.WriteLine("6) Edit product");
   Console.WriteLine("7) Display products (all/active/discontinued)");
   Console.WriteLine("8) Display specific product");
   Console.WriteLine("Enter to quit");
   string? choice = Console.ReadLine();
   Console.Clear();
   logger.Info("Option {choice} selected", choice);
 
   if (choice == "1")
   {
     // display categories

     var configuration = new ConfigurationBuilder()
             .AddJsonFile($"appsettings.json");
 
     var config = configuration.Build();
 
     var db = new DataContext();
     var query = db.Categories.OrderBy(p => p.CategoryName);
 
     Console.ForegroundColor = ConsoleColor.Green;
     Console.WriteLine($"{query.Count()} records returned");
     Console.ForegroundColor = ConsoleColor.Magenta;
     foreach (var item in query)
     {
       Console.WriteLine($"{item.CategoryName} - {item.Description}");
     }
     Console.ForegroundColor = ConsoleColor.White;
   }
   else if (choice == "2")
   {
     // Add category
    Category category = new();
     Console.WriteLine("Enter Category Name:");
     category.CategoryName = Console.ReadLine()!;
     Console.WriteLine("Enter the Category Description:");
     category.Description = Console.ReadLine();
     ValidationContext context = new ValidationContext(category, null, null);
     List<ValidationResult> results = new List<ValidationResult>();
 
     var isValid = Validator.TryValidateObject(category, context, results, true);
     if (isValid)
     {
       var db = new DataContext();
       // check for unique name
       if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
       {
         // generate validation error
         isValid = false;
         results.Add(new ValidationResult("Name exists", ["CategoryName"]));
       }
       else
       {
         logger.Info("Validation passed");
         // TODO: save category to db
       }
     }
     if (!isValid)
     {
       foreach (var result in results)
       {
         logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
       }
     }
   }
   else if (choice == "3")
   {
     var db = new DataContext();
     var query = db.Categories.OrderBy(p => p.CategoryId);
 
     Console.WriteLine("Select the category whose products you want to display:");
     Console.ForegroundColor = ConsoleColor.DarkRed;
     foreach (var item in query)
     {
       Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
     }
     Console.ForegroundColor = ConsoleColor.White;
     int id = int.Parse(Console.ReadLine()!);
     Console.Clear();
     logger.Info($"CategoryId {id} selected");
             Category category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id)!;
     Console.WriteLine($"{category.CategoryName} - {category.Description}");
         foreach (Product p in category.Products)
     {
       Console.WriteLine($"\t{p.ProductName}");
     }
   }
   else if (choice == "4")
   {
     var db = new DataContext();
     var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
     foreach (var item in query)
     {
       Console.WriteLine($"{item.CategoryName}");
       foreach (Product p in item.Products)
       {
         Console.WriteLine($"\t{p.ProductName}");
       }
     }
   }
  else if (choice == "5")
  {
      var db = new DataContext();

      Product product = new();
      Console.WriteLine("Enter Product Name:");
      product.ProductName = Console.ReadLine()!;
      Console.WriteLine("Enter Supplier ID (or leave blank):");
      string supplierInput = Console.ReadLine()!;
      if (int.TryParse(supplierInput, out int supplierId) && db.Suppliers.Any(s => s.SupplierId == supplierId))
        {
          product.SupplierId = supplierId;
        }
      else
        {
          Console.WriteLine("Invalid or missing Supplier ID. Setting to null.");
          product.SupplierId = null;
        }
      Console.WriteLine("Enter Category ID:");
      product.CategoryId = int.Parse(Console.ReadLine()!);
      Console.WriteLine("Enter Quantity Per Unit:");
      product.QuantityPerUnit = Console.ReadLine();
      Console.WriteLine("Enter Unit Price:");
      product.UnitPrice = decimal.Parse(Console.ReadLine()!);
      Console.WriteLine("Enter Units In Stock:");
      product.UnitsInStock = short.Parse(Console.ReadLine()!);
      Console.WriteLine("Enter Units On Order:");
      product.UnitsOnOrder = short.Parse(Console.ReadLine()!);
      Console.WriteLine("Enter Reorder Level:");
      product.ReorderLevel = short.Parse(Console.ReadLine()!);
      Console.WriteLine("Is Discontinued? (true/false):");
      product.Discontinued = bool.Parse(Console.ReadLine()!);

      db.Products.Add(product);
      db.SaveChanges();
      logger.Info("Product added: {name}", product.ProductName);
  }
  else if (choice == "6")
  {
      var db = new DataContext();
      Console.WriteLine("Enter Product ID to edit:");
      int id = int.Parse(Console.ReadLine()!);
      var product = db.Products.FirstOrDefault(p => p.ProductId == id);
      if (product != null)
      {
          Console.WriteLine($"Editing {product.ProductName}");
          Console.Write("New name: ");
          product.ProductName = Console.ReadLine()!;
          Console.Write("Discontinued (true/false): ");
          product.Discontinued = bool.Parse(Console.ReadLine()!);
          db.SaveChanges();
          logger.Info("Product updated: {name}", product.ProductName);
      }
  }
  else if (choice == "7")
  {
    var db = new DataContext();
    Console.WriteLine("1) All\n2) Active\n3) Discontinued");
    string filter = Console.ReadLine()!;
    var products = db.Products.AsQueryable();
    if (filter == "2")
        products = products.Where(p => !p.Discontinued);
    else if (filter == "3")
        products = products.Where(p => p.Discontinued);

    foreach (var p in products)
    {
        var status = p.Discontinued ? "[DISCONTINUED]" : "";
        Console.WriteLine($"{p.ProductName} {status}");
    }
  }
  else if (choice == "8")
{
    var db = new DataContext();
    Console.WriteLine("Enter Product ID:");
    int id = int.Parse(Console.ReadLine()!);
    var p = db.Products.FirstOrDefault(p => p.ProductId == id);
    if (p != null)
    {
        Console.WriteLine($"ID: {p.ProductId}");
        Console.WriteLine($"Name: {p.ProductName}");
        Console.WriteLine($"Supplier ID: {p.SupplierId}");
        Console.WriteLine($"Category ID: {p.CategoryId}");
        Console.WriteLine($"Quantity Per Unit: {p.QuantityPerUnit}");
        Console.WriteLine($"Price: {p.UnitPrice}");
        Console.WriteLine($"Stock: {p.UnitsInStock}");
        Console.WriteLine($"Order: {p.UnitsOnOrder}");
        Console.WriteLine($"Reorder: {p.ReorderLevel}");
        Console.WriteLine($"Discontinued: {p.Discontinued}");
    }
}
   else if (String.IsNullOrEmpty(choice))
   {
     break;
   }
   Console.WriteLine();
 } while (true);
 
 logger.Info("Program ended");