using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;

namespace WindowsFormsApp1.DataAccess
{
    /// <summary>
    /// Data Access Layer for Order operations using stored procedures
    /// </summary>
    public class OrderDAL
    {
        private readonly string connectionString = Connection.connect;

        /// <summary>
        /// Process checkout using stored procedure
        /// </summary>
        public int ProcessCheckout(int userId, string paymentMethod, DataTable cartItems)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                using (OracleCommand cmd = new OracleCommand("sp_ProcessCheckout", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_user_id", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("p_payment_method", OracleDbType.Varchar2).Value = paymentMethod;
                    
                    OracleParameter orderIdParam = new OracleParameter("p_order_id", OracleDbType.Int32);
                    orderIdParam.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(orderIdParam);

                    cmd.ExecuteNonQuery();
                    
                    return Convert.ToInt32(orderIdParam.Value.ToString());
                }
            }
        }

        /// <summary>
        /// Get order history for a user
        /// </summary>
        public DataTable GetOrderHistory(int userId)
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT o.order_id, o.total_price, o.status, o.created_at,
                           (SELECT MAX(payment_method) FROM payments WHERE order_id = o.order_id) AS payment_method
                    FROM orders o
                    WHERE o.user_id = :user_id
                    ORDER BY o.created_at DESC";

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
        /// Get all pending orders
        /// </summary>
        public DataTable GetPendingOrders()
        {
            DataTable dt = new DataTable();
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = @"
                    SELECT o.order_id, o.user_id, o.total_price, o.status, o.created_at
                    FROM orders o
                    WHERE o.status IN ('Processing', 'Pending')
                    ORDER BY o.created_at DESC";

                using (OracleCommand cmd = new OracleCommand(query, con))
                using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                {
                    adapter.Fill(dt);
                }
            }
            return dt;
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public bool UpdateOrderStatus(int orderId, string newStatus)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "UPDATE orders SET status = :status WHERE order_id = :order_id";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("status", OracleDbType.Varchar2).Value = newStatus;
                    cmd.Parameters.Add("order_id", OracleDbType.Int32).Value = orderId;
                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Assign order to delivery personnel using stored procedure
        /// </summary>
        public bool AssignOrderToDelivery(int orderId, int deliveryUserId)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                using (OracleCommand cmd = new OracleCommand("sp_AssignDelivery", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add("p_order_id", OracleDbType.Int32).Value = orderId;
                    cmd.Parameters.Add("p_delivery_user_id", OracleDbType.Int32).Value = deliveryUserId;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }
    }
}
