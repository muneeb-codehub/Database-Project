using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1.DataAccess
{
    /// <summary>
    /// Data Access Layer for Product operations using stored procedures
    /// </summary>
    public class ProductDAL
    {
        private readonly string connectionString = Connection.connect;

        /// <summary>
        /// Add product using stored procedure
        /// </summary>
        public bool AddProduct(int productId, string name, string description, decimal price, int stockQuantity, string categoryName)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                using (OracleCommand cmd = new OracleCommand("sp_AddProduct", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_product_id", OracleDbType.Int32).Value = productId;
                    cmd.Parameters.Add("p_name", OracleDbType.Varchar2).Value = name;
                    cmd.Parameters.Add("p_description", OracleDbType.Varchar2).Value = description;
                    cmd.Parameters.Add("p_price", OracleDbType.Decimal).Value = price;
                    cmd.Parameters.Add("p_stock_quantity", OracleDbType.Int32).Value = stockQuantity;
                    cmd.Parameters.Add("p_category_name", OracleDbType.Varchar2).Value = categoryName;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Update product inventory using stored procedure
        /// </summary>
        public bool UpdateInventory(int productId, int newStockQuantity)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                using (OracleCommand cmd = new OracleCommand("sp_UpdateInventory", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_product_id", OracleDbType.Int32).Value = productId;
                    cmd.Parameters.Add("p_stock_quantity", OracleDbType.Int32).Value = newStockQuantity;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Delete product
        /// </summary>
        public bool DeleteProduct(int productId)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM products WHERE product_id = :product_id";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("product_id", OracleDbType.Int32).Value = productId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Search products by name and category
        /// </summary>
        public DataTable SearchProducts(string productName, string categoryName)
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT p.product_id, p.name AS PRODUCT_NAME, p.price, p.stock_quantity, c.name AS CATEGORY_NAME
                    FROM products p
                    INNER JOIN categories c ON p.category_id = c.category_id
                    WHERE LOWER(p.name) LIKE :productName 
                    AND LOWER(c.name) = :categoryName";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("productName", OracleDbType.Varchar2).Value = $"%{productName.ToLower()}%";
                    cmd.Parameters.Add("categoryName", OracleDbType.Varchar2).Value = categoryName.ToLower();

                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        public DataTable GetAllProducts()
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT p.product_id, p.name, p.description, p.price, p.stock_quantity, c.name AS category_name
                    FROM products p
                    INNER JOIN categories c ON p.category_id = c.category_id";

                using (OracleCommand cmd = new OracleCommand(query, con))
                using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Check if product exists
        /// </summary>
        public bool ProductExists(int productId)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM products WHERE product_id = :product_id";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("product_id", OracleDbType.Int32).Value = productId;
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
