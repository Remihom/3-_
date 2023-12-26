using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

// Інтерфейс для продукту
public interface IProduct
{
    int Id { get; set; }
    string Name { get; set; }
}

// Клас представляє клієнта
public class Customer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public List<Order> Orders { get; set; }
}

// Клас представляє замовлення
public class Order
{
    [Key]
    public int OrderId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    public Customer Customer { get; set; }

    public List<Product> Products { get; set; }
}

// Клас представляє продукт
public class Product : IProduct
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public int OrderId { get; set; }

    public Order Order { get; set; }
}

// Клас базового продукту
public class ProductBase : IProduct
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    [Required]
    public decimal Price { get; set; }

    [MaxLength(50)]
    public string Brand { get; set; }
}

// Клас для одягового продукту, успадкований від базового продукту
public class ClothingProduct : ProductBase
{
    [MaxLength(20)]
    public string ClothingSize { get; set; }

    [MaxLength(20)]
    public string Color { get; set; }
}

// Клас контексту бази даних
public class OnlineStoreContext : DbContext
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Налаштування відносин між таблицями у базі даних
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Order)
            .WithMany(o => o.Products)
            .HasForeignKey(p => p.OrderId);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(c => c.Orders)
            .HasForeignKey(o => o.CustomerId);

        // Введення тестових даних
        modelBuilder.Entity<Customer>().HasData(new Customer { Id = 1, Name = "Микола Цибуля", Email = "mikola.cibylya@gmail.com" });
        modelBuilder.Entity<Order>().HasData(new Order { OrderId = 1, CustomerId = 1 });
        modelBuilder.Entity<Product>().HasData(new Product { Id = 1, Name = "Running Shoes", OrderId = 1 });
    }

    // Метод для додавання продукту
    public void AddProduct(Product product)
    {
        Products.Add(product);
        SaveChanges();
    }

    // Метод для оновлення продукту
    public void UpdateProduct(Product product)
    {
        Products.Update(product);
        SaveChanges();
    }

    // Метод для видалення продукту
    public void DeleteProduct(Product product)
    {
        Products.Remove(product);
        SaveChanges();
    }
}

// Головний клас програми
public class Program
{
    public static void Main(string[] args)
    {

        
            using (var context = new OnlineStoreContext())
            {
                // Додаємо  дані до таблиці Products
                var product1 = new Product { Name = "Running Shoes", Price = 49.99, OrderId = 111 };
                var product2 = new Product { Name = "Stylish T-Shirt", Price = 19.99, OrderId = 222 };
                var product3 = new Product { Name = "Trainers", Price = 39.99, OrderId = 333 };
                var product4 = new Product { Name = "Running Shoes", Price = 59.99, OrderId = 111 };
                var product5 = new Product { Name = "Sports Shoes", Price = 29.99, OrderId = 222 };
                var product6 = new Product { Name = "Running Socks", Price = 9.99, OrderId = 333 };

                context.Products.AddRange(product1, product2, product3, product4, product5, product6);
                context.SaveChanges();

                // Додаємо  дані до таблиці Orders
                var order1 = new Order { OrderId = 1, OrderDate = DateTime.Now, CustomerId = 11 };
                var order2 = new Order { OrderId = 2, OrderDate = DateTime.Now, CustomerId = 22 };
                var order3 = new Order { OrderId = 2, OrderDate = DateTime.Now, CustomerId = 33 };
                context.Orders.AddRange(order1, order2, order3);
                context.SaveChanges();

                // Додаємо  дані до таблиці Customers
                var customer1 = new Customer { CustomerId = 11, FirstName = "Ivan", LastName = "Gruzd" };
                var customer2 = new Customer { CustomerId = 22, FirstName = "Maria", LastName = "Petrenko" };
                var customer3 = new Customer { CustomerId = 33, FirstName = "Mark", LastName = "Parnui" };
                context.Customers.AddRange(customer1, customer2, customer3);
                context.SaveChanges();

            using (var context = new OnlineStoreContext())
        {
                // Приклади використання LINQ
                var unionResult = context.Products
                    .Where(p => p.Name.Contains("Shoes"))
                    .Union(context.Products.Where(p => p.Name.Contains("Stylish")));

                var exceptResult = context.Products
                    .Where(p => p.Name.Contains("Shoes"))
                    .Except(context.Products.Where(p => p.Name.Contains("Running")));

                var intersectResult = context.Products
                    .Where(p => p.Name.Contains("Shoes"))
                    .Intersect(context.Products.Where(p => p.Name.Contains("Running")));

                var joinResult = context.Orders
                    .Join(context.Products, o => o.OrderId, p => p.OrderId, (o, p) => new { Order = o, Product = p });

                var distinctResult = context.Products.Select(p => p.Name).Distinct();

                var groupByResult = context.Products.GroupBy(p => p.OrderId);

                var maxPrice = context.Products.Max(p => p.Price);
                var minPrice = context.Products.Min(p => p.Price);
                var averagePrice = context.Products.Average(p => p.Price);

                var maxSum = context.Order.Max(p => p.Sum);


                var productWithHighestTotalSum = context.Orders
                    .Join(context.Products, o => o.OrderId, p => p.OrderId, (o, p) => new { Order = o, Product = p });
                    .GroupBy(p => p.Name)
                    .OrderByDescending(g => g.Sum(p => p.Price * p.QuantitySold))
                   

                // Завантаження даних (Eager Loading)
                var customer = context.Customers
                    .Include(c => c.Orders)
                    .ThenInclude(o => o.Products)
                    .FirstOrDefault();

                // Завантаження даних (Explicit Loading)
                var explicitCustomer = context.Customers.FirstOrDefault();
                context.Entry(explicitCustomer).Collection(c => c.Orders).Load();
                foreach (var order in explicitCustomer.Orders)
                {
                    context.Entry(order).Collection(o => o.Products).Load();
                }

                // Завантаження даних (Lazy Loading)

                // Завантаження невідслідковуваних даних
                var untrackedProducts = context.Products.AsNoTracking().ToList();

                // Збереження змінених та нових даних
                var productToUpdate = context.Products.Find(1);
                productToUpdate.Name = "Jeans";
                context.SaveChanges();

                var newProduct = new Product { Name = "Skirt", OrderId = 2 };
                context.Products.Add(newProduct);
                context.SaveChanges();

                var productToDelete = context.Products.Find(2);
                context.Products.Remove(productToDelete);
                context.SaveChanges();

                // Виклик збережених процедур та функцій
                context.Database.ExecuteSqlRaw("EXEC dbo.YourStoredProcedure @parameter", new SqlParameter("@parameter", "value"));

                var result = context.Products.FromSqlRaw("SELECT * FROM dbo.YourFunction(@parameter)", new SqlParameter("@parameter", "value")).ToList();
        }
    }
}