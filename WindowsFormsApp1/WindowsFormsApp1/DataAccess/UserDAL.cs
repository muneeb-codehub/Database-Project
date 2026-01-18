using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace WindowsFormsApp1.DataAccess
{
    /// <summary>
    /// Data Access Layer for User operations
    /// </summary>
    public class UserDAL
    {
        private readonly string connectionString = Connection.connect;

        /// <summary>
        /// Register a new user
        /// </summary>
        public bool RegisterUser(int userId, string username, string password, string email, string role)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();

                // Check if username exists
                string checkQuery = "SELECT COUNT(*) FROM users WHERE USERNAME = :USERNAME";
                using (OracleCommand checkCmd = new OracleCommand(checkQuery, con))
                {
                    checkCmd.Parameters.Add("USERNAME", OracleDbType.Varchar2).Value = username;
                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    if (count > 0)
                        throw new Exception("Username already exists.");
                }

                // Insert new user
                string insertQuery = @"
                    INSERT INTO users(USER_ID, USERNAME, PASSWORD, EMAIL, ROLE, CREATED_AT) 
                    VALUES(:USER_ID, :USERNAME, :PASSWORD, :EMAIL, :ROLE, :CREATED_AT)";

                using (OracleCommand cmd = new OracleCommand(insertQuery, con))
                {
                    cmd.Parameters.Add("USER_ID", OracleDbType.Int32).Value = userId;
                    cmd.Parameters.Add("USERNAME", OracleDbType.Varchar2).Value = username;
                    cmd.Parameters.Add("PASSWORD", OracleDbType.Varchar2).Value = password;
                    cmd.Parameters.Add("EMAIL", OracleDbType.Varchar2).Value = email;
                    cmd.Parameters.Add("ROLE", OracleDbType.Varchar2).Value = role;
                    cmd.Parameters.Add("CREATED_AT", OracleDbType.Date).Value = DateTime.Now;

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
        }

        /// <summary>
        /// Authenticate user credentials
        /// </summary>
        public (bool success, int userId, string username, string role) AuthenticateUser(string username, string password, string role)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "SELECT user_id, username, role FROM users WHERE username = :username AND password = :password AND role = :role";

                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("username", OracleDbType.Varchar2).Value = username;
                    cmd.Parameters.Add("password", OracleDbType.Varchar2).Value = password;
                    cmd.Parameters.Add("role", OracleDbType.Varchar2).Value = role;

                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int userId = Convert.ToInt32(reader["user_id"]);
                            string uname = reader["username"].ToString();
                            string urole = reader["role"].ToString();

                            // Set session
                            Sessional.UserId = userId;
                            Sessional.Username = uname;

                            return (true, userId, uname, urole);
                        }
                    }
                }
            }
            return (false, 0, null, null);
        }

        /// <summary>
        /// Check if username exists
        /// </summary>
        public bool UsernameExists(string username)
        {
            using (OracleConnection con = new OracleConnection(connectionString))
            {
                con.Open();
                string query = "SELECT COUNT(*) FROM users WHERE USERNAME = :USERNAME";
                using (OracleCommand cmd = new OracleCommand(query, con))
                {
                    cmd.Parameters.Add("USERNAME", OracleDbType.Varchar2).Value = username;
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}
