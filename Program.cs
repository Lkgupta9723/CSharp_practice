using System;
using System.Collections.Generic;

// Interface IDiscountable
public interface IDiscountable
{
    bool IsDiscountable { get; }
    decimal ApplyDiscount(decimal percentage);
}

// Abstract Product class implementing IDiscountable
public abstract class Product : IDiscountable
{
    public string ProductId { get; }
    public string Name { get; }
    public string Description { get; protected set; }
    public decimal Price { get; protected set; }
    public int InventoryCount { get; protected set; }
    public bool IsDiscountable { get; protected set; }

    protected Product(string productId, string name, decimal price, int inventoryCount, bool isDiscountable, string description = "No description")
    {
        if (string.IsNullOrWhiteSpace(productId))
            throw new ArgumentException("Product ID cannot be empty.");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.");
        if (price <= 0)
            throw new ArgumentException("Price must be positive.");
        if (inventoryCount < 0)
            throw new ArgumentException("Inventory cannot be negative.");

        ProductId = productId;
        Name = name;
        Price = price;
        InventoryCount = inventoryCount;
        IsDiscountable = isDiscountable;
        Description = description;
    }

    public abstract decimal CalculateTax();

    public virtual string GetDetails()
    {
        return $"{Name} ({ProductId}) - {Description}, Price: {Price:C}, In Stock: {InventoryCount}";
    }

    public decimal ApplyDiscount(decimal percentage)
    {
        if (!IsDiscountable)
            throw new InvalidOperationException($"{Name} is not discountable.");

        if (percentage < 0 || percentage > 100)
            throw new ArgumentException("Percentage must be between 0 and 100.");

        decimal discountAmount = (Price * percentage) / 100;
        Price -= discountAmount;
        return Price;
    }

    public void DecreaseInventory(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        if (quantity > InventoryCount)
            throw new InvalidOperationException($"Product '{Name}' ({ProductId}) is out of stock.");

        InventoryCount -= quantity;
    }

    public void IncreaseInventory(int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.");

        InventoryCount += quantity;
    }
}

// Derived Product Classes
public class ElectronicProduct : Product
{
    public int WarrantyPeriod { get; set; } // months
    public double Weight { get; set; } // kg

    public ElectronicProduct(string productId, string name, decimal price, int inventoryCount)
        : base(productId, name, price, inventoryCount, true, "Electronic Product")
    {
    }

    public override decimal CalculateTax()
    {
        return Price * 0.0825m; // 8.25% tax
    }

    public override string GetDetails()
    {
        return base.GetDetails() + $", Warranty: {WarrantyPeriod} months, Weight: {Weight} kg";
    }
}

public class BookProduct : Product
{
    public string Author { get; set; } = "Unknown";
    public string ISBN { get; set; } = "Unknown";
    public double Weight { get; set; }

    public BookProduct(string productId, string name, decimal price, int inventoryCount)
        : base(productId, name, price, inventoryCount, true, "Book Product")
    {
    }

    public override decimal CalculateTax()
    {
        return Price * 0.06m; // 6% tax
    }

    public override string GetDetails()
    {
        return base.GetDetails() + $", Author: {Author}, ISBN: {ISBN}, Weight: {Weight} kg";
    }
}

public class ClothingProduct : Product
{
    public string Size { get; set; } = "Unknown";
    public string Color { get; set; } = "Unknown";
    public double Weight { get; set; }

    public ClothingProduct(string productId, string name, decimal price, int inventoryCount)
        : base(productId, name, price, inventoryCount, false, "Clothing Product")
    {
    }

    public override decimal CalculateTax()
    {
        return Price * 0.075m; // 7.5% tax
    }

    public override string GetDetails()
    {
        return base.GetDetails() + $", Size: {Size}, Color: {Color}, Weight: {Weight} kg";
    }
}

// ShoppingCart class
public class ShoppingCart
{
    private Dictionary<Product, int> items = new();

    public void AddProduct(Product product, int quantity)
    {
        if (product.InventoryCount < quantity)
            throw new InvalidOperationException($"Product '{product.Name}' ({product.ProductId}) is out of stock.");

        if (items.ContainsKey(product))
            items[product] += quantity;
        else
            items[product] = quantity;

        product.DecreaseInventory(quantity);
    }

    public void RemoveProduct(Product product)
    {
        if (items.ContainsKey(product))
        {
            product.IncreaseInventory(items[product]);
            items.Remove(product);
        }
    }

    public void UpdateQuantity(Product product, int newQuantity)
    {
        if (!items.ContainsKey(product))
            throw new InvalidOperationException("Product not in cart.");

        int currentQuantity = items[product];

        if (newQuantity > currentQuantity)
        {
            int diff = newQuantity - currentQuantity;
            product.DecreaseInventory(diff);
        }
        else if (newQuantity < currentQuantity)
        {
            int diff = currentQuantity - newQuantity;
            product.IncreaseInventory(diff);
        }

        items[product] = newQuantity;
    }

    public decimal CalculateSubtotal()
    {
        decimal subtotal = 0;
        foreach (var item in items)
        {
            subtotal += item.Key.Price * item.Value;
        }
        return subtotal;
    }

    public decimal CalculateTax()
    {
        decimal tax = 0;
        foreach (var item in items)
        {
            tax += item.Key.CalculateTax() * item.Value;
        }
        return tax;
    }

    public decimal CalculateTotal()
    {
        return CalculateSubtotal() + CalculateTax();
    }

    public Dictionary<Product, int> GetItems()
    {
        return items;
    }
}

// Customer class
public enum MembershipLevel
{
    Regular,
    Premium,
    VIP
}

public class Customer
{
    public string Name { get; }
    public string Email { get; }
    public string Address { get; set; } = "Unknown";
    public MembershipLevel MembershipLevel { get; set; }
    public ShoppingCart ShoppingCart { get; }

    public Customer(string name, string email)
    {
        Name = name ?? throw new ArgumentException("Name cannot be null.");
        Email = email ?? throw new ArgumentException("Email cannot be null.");
        ShoppingCart = new ShoppingCart();
    }

    public decimal CalculateLoyaltyDiscount()
    {
        decimal discountPercentage = 0;
        switch (MembershipLevel)
        {
            case MembershipLevel.Premium:
                discountPercentage = 5;
                break;
            case MembershipLevel.VIP:
                discountPercentage = 10;
                break;
        }
        return ShoppingCart.CalculateSubtotal() * discountPercentage / 100;
    }
}

// Order class
public enum ShippingMethod
{
    Standard,
    Express,
    NextDay
}

public class Order
{
    private static int orderCounter = 1;

    public string OrderId { get; }
    public Customer Customer { get; }
    public DateTime OrderDate { get; }
    public ShippingMethod ShippingMethod { get; set; }
    public Dictionary<Product, int> Items { get; }

    public Order(Customer customer, string orderId = "")
    {
        Customer = customer;
        OrderId = string.IsNullOrEmpty(orderId) ? $"ORD-{DateTime.Now.Year}-{orderCounter++:D3}" : orderId;
        OrderDate = DateTime.Now;
        Items = new Dictionary<Product, int>(customer.ShoppingCart.GetItems());
    }

    public decimal CalculateShippingCost()
    {
        double totalWeight = 0;
        foreach (var item in Items)
        {
            dynamic prod = item.Key;
            totalWeight += prod.Weight * item.Value;
        }

        decimal cost = ShippingMethod switch
        {
            ShippingMethod.Standard => (decimal)totalWeight * 1m,
            ShippingMethod.Express => (decimal)totalWeight * 3m,
            ShippingMethod.NextDay => (decimal)totalWeight * 5m,
            _ => 0
        };

        return cost < 5 ? 5 : cost;
    }

    public void ProcessOrder()
    {
        // Normally here you'd handle payment, etc.
        Console.WriteLine($"Processing order {OrderId} for {Customer.Name}");
    }

    public string GenerateReceipt()
    {
        decimal subtotal = 0, tax = 0;
        string receipt = "====== ORDER RECEIPT ======\n";
        receipt += $"Order ID: {OrderId}\n";
        receipt += $"Date: {OrderDate}\n";
        receipt += $"Customer: {Customer.Name}\n";
        receipt += $"Email: {Customer.Email}\n";
        receipt += $"Address: {Customer.Address}\n";
        receipt += $"Membership: {Customer.MembershipLevel}\n";
        receipt += "Items:\n";

        foreach (var item in Items)
        {
            Product prod = item.Key;
            int qty = item.Value;
            decimal itemSubtotal = prod.Price * qty;
            decimal itemTax = prod.CalculateTax() * qty;
            subtotal += itemSubtotal;
            tax += itemTax;

            receipt += $"{prod.Name} ({prod.ProductId})\n";
            receipt += $"Original Price: {prod.Price + prod.Price * 0.1m:C}\n";
            receipt += $"Discount: {(prod.IsDiscountable ? $"{prod.Price * 0.1m:C} (10%)" : "$0.00 (0%)")}\n";
            receipt += $"Quantity: {qty}\n";
            receipt += $"Subtotal: {itemSubtotal:C}\n";
            receipt += $"Tax: {itemTax:C}\n";

            if (prod is BookProduct book)
            {
                receipt += $"Details: Author: {book.Author}, ISBN: {book.ISBN}\n";
            }
            else if (prod is ClothingProduct cloth)
            {
                receipt += $"Details: Size: {cloth.Size}, Color: {cloth.Color}\n";
            }
        }

        decimal loyaltyDiscount = Customer.CalculateLoyaltyDiscount();
        decimal shipping = CalculateShippingCost();
        decimal total = subtotal + tax + shipping - loyaltyDiscount;

        receipt += "Order Summary:\n";
        receipt += $"Subtotal: {subtotal:C}\n";
        receipt += $"Tax: {tax:C}\n";
        receipt += $"Loyalty Discount: {loyaltyDiscount:C} ({(Customer.MembershipLevel == MembershipLevel.Premium ? "5%" : Customer.MembershipLevel == MembershipLevel.VIP ? "10%" : "0%")} {Customer.MembershipLevel} membership)\n";
        receipt += $"Shipping: {shipping:C} ({ShippingMethod})\n";
        receipt += $"Total: {total:C}\n";
        receipt += "Thank you for shopping with us!\n";
        receipt += "============================\n";

        return receipt;
    }
}

// Program class
public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Create products
            ElectronicProduct laptop = new ElectronicProduct("E001", "Gaming Laptop", 1200.00m, 10)
            {
                WarrantyPeriod = 24,
                Weight = 2.5
            };

            BookProduct book = new BookProduct("B001", "C# Programming Guide", 45.99m, 50)
            {
                Author = "John Smith",
                ISBN = "978-3-16-148410-0",
                Weight = 0.8
            };

            ClothingProduct shirt = new ClothingProduct("C001", "Casual Shirt", 29.99m, 100)
            {
                Size = "L",
                Color = "Blue",
                Weight = 0.3
            };

            // Create customer
            Customer customer = new Customer("Alice Johnson", "alice@example.com")
            {
                Address = "123 Main St, Anytown, USA",
                MembershipLevel = MembershipLevel.Premium
            };

            // Add products to shopping cart
            customer.ShoppingCart.AddProduct(laptop, 1);
            customer.ShoppingCart.AddProduct(book, 2);
            customer.ShoppingCart.AddProduct(shirt, 3);

            // Apply discounts
            laptop.ApplyDiscount(10.0m);
            book.ApplyDiscount(5.0m);

            // Calculate loyalty discount
            decimal loyaltyDiscount = customer.CalculateLoyaltyDiscount();

            // Create order
            Order order = new Order(customer, "ORD-2023-001");
            order.ShippingMethod = ShippingMethod.Express;
            order.ProcessOrder();

            // Generate receipt
            string receipt = order.GenerateReceipt();
            Console.WriteLine(receipt);

            // Try to order an out-of-stock product
            try
            {
                ElectronicProduct smartphone = new ElectronicProduct("E002", "Smartphone", 800.00m, 0);
                customer.ShoppingCart.AddProduct(smartphone, 1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
