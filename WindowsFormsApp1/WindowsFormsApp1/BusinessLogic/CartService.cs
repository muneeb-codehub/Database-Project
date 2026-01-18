using System;
using System.Data;
using WindowsFormsApp1.DataAccess;

namespace WindowsFormsApp1.BusinessLogic
{
    /// <summary>
    /// Business Logic Layer for Shopping Cart operations
    /// </summary>
    public class CartService
    {
        private readonly CartDAL cartDAL;

        public CartService()
        {
            cartDAL = new CartDAL();
        }

        /// <summary>
        /// Add product to cart with validation
        /// </summary>
        public bool AddToCart(int userId, int productId, int quantity)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            if (productId <= 0)
                throw new ArgumentException("Invalid product ID.");

            if (quantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            return cartDAL.AddToCart(userId, productId, quantity);
        }

        /// <summary>
        /// Get cart items for a user
        /// </summary>
        public DataTable GetCartItems(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            return cartDAL.GetCartItems(userId);
        }

        /// <summary>
        /// Remove item from cart
        /// </summary>
        public bool RemoveFromCart(int cartItemId)
        {
            if (cartItemId <= 0)
                throw new ArgumentException("Invalid cart item ID.");

            return cartDAL.RemoveFromCart(cartItemId);
        }

        /// <summary>
        /// Clear entire cart for user
        /// </summary>
        public bool ClearCart(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID.");

            return cartDAL.ClearCart(userId);
        }

        /// <summary>
        /// Update cart item quantity
        /// </summary>
        public bool UpdateCartItemQuantity(int cartItemId, int newQuantity)
        {
            if (cartItemId <= 0)
                throw new ArgumentException("Invalid cart item ID.");

            if (newQuantity <= 0)
                throw new ArgumentException("Quantity must be greater than zero.");

            return cartDAL.UpdateCartItemQuantity(cartItemId, newQuantity);
        }
    }
}
