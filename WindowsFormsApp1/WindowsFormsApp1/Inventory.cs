using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using WindowsFormsApp1.BusinessLogic;

namespace WindowsFormsApp1
{
    public partial class Inventory : Form
    {
        private readonly ProductService productService;

        public Inventory()
        {
            InitializeComponent();
            productService = new ProductService();
            
            // Add hover effects
            button1.MouseEnter += Button_MouseEnter;
            button1.MouseLeave += Button_MouseLeave;
            button2.MouseEnter += Button_MouseEnter;
            button2.MouseLeave += Button_MouseLeave;
            button3.MouseEnter += Button_MouseEnter;
            button3.MouseLeave += Button_MouseLeave;
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.BackColor = Color.CornflowerBlue;
                btn.ForeColor = Color.White;
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                btn.BackColor = Color.FromArgb(0, 100, 255);
                btn.ForeColor = Color.White;
            }
        }

        private void Inventory_Load(object sender, EventArgs e)
        {
            LoadAllProducts();
            StyleDataGridView();
        }

        /// <summary>
        /// Load all products into DataGridView
        /// </summary>
        private void LoadAllProducts()
        {
            try
            {
                DataTable products = productService.GetAllProducts();
                dataGridView1.DataSource = products;
                AddUpdateButton();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Style DataGridView
        /// </summary>
        private void StyleDataGridView()
        {
            dataGridView1.BorderStyle = BorderStyle.None;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(238, 239, 249);
            dataGridView1.DefaultCellStyle.SelectionBackColor = Color.FromArgb(55, 125, 255);
            dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            dataGridView1.DefaultCellStyle.BackColor = Color.White;
            dataGridView1.DefaultCellStyle.Font = new Font("Calibri", 12);
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(20, 25, 72);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Calibri", 13, FontStyle.Bold);
            dataGridView1.RowHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dataGridView1.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridView1.GridColor = Color.FromArgb(193, 199, 206);

            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
        }

        /// <summary>
        /// Add Update button column
        /// </summary>
        private void AddUpdateButton()
        {
            if (!dataGridView1.Columns.Contains("Update"))
            {
                DataGridViewButtonColumn updateButton = new DataGridViewButtonColumn
                {
                    Name = "Update",
                    HeaderText = "Update Stock",
                    Text = "Update",
                    UseColumnTextForButtonValue = true
                };
                dataGridView1.Columns.Add(updateButton);
            }
        }

        /// <summary>
        /// Button 1: View Low Stock Products
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (OracleConnection con = new OracleConnection(Connection.connect))
                {
                    con.Open();
                    string query = "SELECT product_id, name, stock_quantity, price FROM products WHERE stock_quantity < 20 ORDER BY stock_quantity ASC";
                    
                    using (OracleCommand cmd = new OracleCommand(query, con))
                    using (OracleDataAdapter adapter = new OracleDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                        AddUpdateButton();
                        MessageBox.Show($"Found {dt.Rows.Count} low stock products.", "Low Stock Alert", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Button 2: View All Products
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            LoadAllProducts();
        }

        /// <summary>
        /// Button 3: Refresh
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            LoadAllProducts();
            MessageBox.Show("Product list refreshed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// DataGridView cell click event for Update button
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Update"].Index && e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                int productId = Convert.ToInt32(row.Cells["product_id"].Value);
                string productName = row.Cells["name"].Value.ToString();
                int currentStock = Convert.ToInt32(row.Cells["stock_quantity"].Value);

                // Show dialog to update stock
                string input = Microsoft.VisualBasic.Interaction.InputBox(
                    $"Enter new stock quantity for '{productName}':\nCurrent Stock: {currentStock}",
                    "Update Inventory",
                    currentStock.ToString()
                );

                if (!string.IsNullOrWhiteSpace(input))
                {
                    if (int.TryParse(input, out int newStock) && newStock >= 0)
                    {
                        try
                        {
                            productService.UpdateInventory(productId, newStock);
                            MessageBox.Show("Inventory updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAllProducts();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error updating inventory: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid quantity (must be >= 0).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }
    }
}
