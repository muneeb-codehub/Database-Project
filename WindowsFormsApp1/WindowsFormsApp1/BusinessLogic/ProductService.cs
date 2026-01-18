using System;
using System.Data;
using WindowsFormsApp1.DataAccess;

namespace WindowsFormsApp1.BusinessLogic
{
    /// <summary>
    /// Business Logic Layer for Product operations
    /// </summary>
    public class ProductService
    {
        private readonly ProductDAL productDAL;

        public ProductService()
        {
            productDAL = new ProductDAL();
        }

        /// <summary>
        /// Add a new product with validation
        /// </summary>
        public bool AddProduct(int productId, string name, string description, decimal price, int stockQuantity, string categoryName)
        {
            // Business validation
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name cannot be empty.");

            if (price <= 0)
                throw new ArgumentException("Price must be greater than zero.");

            if (stockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.");

            return productDAL.AddProduct(productId, name, description, price, stockQuantity, categoryName);
        }

        /// <summary>
        /// Update product inventory
        /// </summary>
        public bool UpdateInventory(int productId, int newStockQuantity)
        {
            if (newStockQuantity < 0)
                throw new ArgumentException("Stock quantity cannot be negative.");

            return productDAL.UpdateInventory(productId, newStockQuantity);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        public bool DeleteProduct(int productId)
        {
            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            return productDAL.DeleteProduct(productId);
        }

        /// <summary>
        /// Search products by name and category
        /// </summary>
        public DataTable SearchProducts(string productName, string categoryName)
        {
            return productDAL.SearchProducts(productName, categoryName);
        }

        /// <summary>
        /// Get all products
        /// </summary>
        public DataTable GetAllProducts()
        {
            return productDAL.GetAllProducts();
        }

        /// <summary>
        /// Check if product exists
        /// </summary>
        public bool ProductExists(int productId)
        {
            return productDAL.ProductExists(productId);
        }
    }
}
