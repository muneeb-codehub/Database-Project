using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1.DataAccess
{
    /// <summary>
    /// Data Access Layer for Shopping Cart operations
    /// </summary>
    public class CartDAL
    {
        private readonly string connectionString = Connection.connect;

        /// <summary>
        /// Add product to cart
        /// </summary>
        public bool AddToCart(int userId, int productId, int quantity)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();

                // Get cart ID for user
                int cartId = GetCartIdForUser(userId, con);

                // Insert cart item
                string query = @"
                    INSERT INTO cart_items (cart_item_id, cart_id, product_id, quantity)
                    VALUES (cart_items_seq.NEXTVAL, :cart_id, :product_id, :quantity)";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("cart_id", OracleDbType.Int32).Value = cartId;
                    cmd.Parameters.Add("product_id", OracleDbType.Int32).Value = productId;
                    cmd.Parameters.Add("quantity", OracleDbType.Int32).Value = quantity;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Get cart items for a user
        /// </summary>
        public DataTable GetCartItems(int userId)
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT ci.cart_item_id, p.product_id, p.name AS product_name, ci.quantity, p.price
                    FROM cart_items ci
                    JOIN products p ON ci.product_id = p.product_id
                    JOIN cart c ON ci.cart_id = c.cart_id
                    WHERE c.user_id = :user_id";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        public bool RemoveFromCart(int cartItemId)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "DELETE FROM cart_items WHERE cart_item_id = :cart_item_id";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("cart_item_id", OracleDbType.Int32).Value = cartItemId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Clear entire cart
        /// </summary>
        public bool ClearCart(int userId)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    DELETE FROM cart_items 
                    WHERE cart_id IN (SELECT cart_id FROM cart WHERE user_id = :user_id)";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        public bool UpdateCartItemQuantity(int cartItemId, int newQuantity)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "UPDATE cart_items SET quantity = :quantity WHERE cart_item_id = :cart_item_id";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("quantity", OracleDbType.Int32).Value = newQuantity;
                    cmd.Parameters.Add("cart_item_id", OracleDbType.Int32).Value = cartItemId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Get or create cart ID for user
        /// </summary>
        private int GetCartIdForUser(int userId, OracleConnection con)
        {
            string query = "SELECT cart_id FROM cart WHERE user_id = :user_id";
            using (OracleCommand cmd = new OracleCommand(query, con))
            {
                cmd.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
                object result = cmd.ExecuteScalar();

                if (result != null)
                    return Convert.ToInt32(result);

                // Create new cart if doesn't exist
                string insertQuery = "INSERT INTO cart (cart_id, user_id) VALUES (cart_seq.NEXTVAL, :user_id) RETURNING cart_id INTO :cart_id";
                using (OracleCommand insertCmd = new OracleCommand(insertQuery, con))
                {
                    insertCmd.Parameters.Add("user_id", OracleDbType.Int32).Value = userId;
                    OracleParameter cartIdParam = new OracleParameter("cart_id", OracleDbType.Int32);
                    cartIdParam.Direction = ParameterDirection.Output;
                    insertCmd.Parameters.Add(cartIdParam);
                    insertCmd.ExecuteNonQuery();
                    return Convert.ToInt32(cartIdParam.Value.ToString());
                }
            }
        }
    }
}
