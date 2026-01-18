using System;
using WindowsFormsApp1.DataAccess;

namespace WindowsFormsApp1.BusinessLogic
{
    /// <summary>
    /// Business Logic Layer for User operations
    /// </summary>
    public class UserService
    {
        private readonly UserDAL userDAL;

        public UserService()
        {
            userDAL = new UserDAL();
        }

        /// <summary>
        /// Register a new user with validation
        /// </summary>
        public bool RegisterUser(int userId, string username, string password, string email, string role)
        {
            // Business validation
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be empty.");

            if (password.Length < 6)
                throw new ArgumentException("Password must be at least 6 characters long.");

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Valid email is required.");

            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role must be selected.");

            return userDAL.RegisterUser(userId, username, password, email, role);
        }

        /// <summary>
        /// Authenticate user login
        /// </summary>
        public (bool success, int userId, string username, string role) Login(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password are required.");

            return userDAL.AuthenticateUser(username, password, role);
        }

        /// <summary>
        /// Check if username already exists
        /// </summary>
        public bool UsernameExists(string username)
        {
            return userDAL.UsernameExists(username);
        }
    }
}
