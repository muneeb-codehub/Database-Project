using System;
using System.Data;
using WindowsFormsApp1.DataAccess;

namespace WindowsFormsApp1.BusinessLogic
{
    /// <summary>
    /// Business Logic Layer for Order operations
    /// </summary>
    public class OrderService
    {
        private readonly OrderDAL orderDAL;

        public OrderService()
        {
            orderDAL = new OrderDAL();
        }

        /// <summary>
        /// Process checkout and create order
        /// </summary>
        public int ProcessCheckout(int userId, string paymentMethod, DataTable cartItems)
        {
            // Business validation
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            if (string.IsNullOrWhiteSpace(paymentMethod))
                throw new ArgumentException("Payment method is required.");

            if (cartItems == null || cartItems.Rows.Count == 0)
                throw new ArgumentException("Cart is empty.");

            return orderDAL.ProcessCheckout(userId, paymentMethod, cartItems);
        }

        /// <summary>
        /// Get order history for a user
        /// </summary>
        public DataTable GetOrderHistory(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            return orderDAL.GetOrderHistory(userId);
        }

        /// <summary>
        /// Get all pending orders for cashier
        /// </summary>
        public DataTable GetPendingOrders()
        {
            return orderDAL.GetPendingOrders();
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public bool UpdateOrderStatus(int orderId, string newStatus)
        {
            if (orderId <= 0)
                throw new ArgumentException("Invalid order ID.");

            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.");

            return orderDAL.UpdateOrderStatus(orderId, newStatus);
        }

        /// <summary>
        /// Assign order to delivery personnel
        /// </summary>
        public bool AssignOrderToDelivery(int orderId, int deliveryUserId)
        {
            if (orderId <= 0 || deliveryUserId <= 0)
                throw new ArgumentException("Invalid order or delivery user ID.");

            return orderDAL.AssignOrderToDelivery(orderId, deliveryUserId);
        }
    }
}
