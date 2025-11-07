using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace ShoesShopSystem
{
    public partial class ProductsForm : Form
    {
        private FlowLayoutPanel flowPanel;
        private TextBox txtSearch;
        private ComboBox cmbSupplier;
        private ComboBox cmbSort;
        private Button btnBack;
        private Button btnAdd;
        private DataTable productsTable;
        private List<ProductCard> productCards;

        public ProductsForm()
        {
            BuildForm();
            LoadProducts();
            LoadSuppliers();
            ApplyRoleRestrictions();
        }

        private void BuildForm()
        {
            // Form
            this.Text = "Каталог товаров - Обувной магазин";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header panel
            var panelHeader = new Panel
            {
                BackColor = Color.FromArgb(127, 255, 0),
                Location = new Point(0, 0),
                Size = new Size(1100, 60)
            };

            var lblTitle = new Label
            {
                Text = "КАТАЛОГ ТОВАРОВ",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                Size = new Size(300, 30),
                Location = new Point(400, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Search and filter controls
            var lblSearch = new Label
            {
                Text = "Поиск:",
                Location = new Point(20, 70),
                Size = new Size(50, 20),
                Font = new Font("Times New Roman", 10)
            };

            txtSearch = new TextBox
            {
                Location = new Point(80, 70),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 10)
            };

            var lblSupplier = new Label
            {
                Text = "Поставщик:",
                Location = new Point(250, 70),
                Size = new Size(70, 20),
                Font = new Font("Times New Roman", 10)
            };

            cmbSupplier = new ComboBox
            {
                Location = new Point(330, 70),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 10)
            };

            var lblSort = new Label
            {
                Text = "Сортировка:",
                Location = new Point(500, 70),
                Size = new Size(80, 20),
                Font = new Font("Times New Roman", 10)
            };

            cmbSort = new ComboBox
            {
                Location = new Point(590, 70),
                Size = new Size(150, 25),
                Font = new Font("Times New Roman", 10)
            };
            cmbSort.Items.AddRange(new[] { "По названию А-Я", "По названию Я-А", "По цене ↑", "По цене ↓", "По количеству ↑", "По количеству ↓" });
            cmbSort.SelectedIndex = 0;

            // Flow panel for product cards
            flowPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 110),
                Size = new Size(1050, 480),
                AutoScroll = true,
                BackColor = Color.White
            };

            // Buttons
            btnBack = new Button
            {
                Text = "← Назад",
                Location = new Point(20, 610),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightGray
            };

            btnAdd = new Button
            {
                Text = "➕ Добавить товар",
                Location = new Point(900, 610),
                Size = new Size(150, 35),
                BackColor = Color.FromArgb(0, 250, 154),
                Font = new Font("Times New Roman", 10)
            };

            // Events
            txtSearch.TextChanged += (s, e) => ApplyFilters();
            cmbSupplier.SelectedIndexChanged += (s, e) => ApplyFilters();
            cmbSort.SelectedIndexChanged += (s, e) => ApplySorting();
            btnBack.Click += (s, e) => this.Close();
            btnAdd.Click += (s, e) => AddProduct();

            // Add controls
            panelHeader.Controls.Add(lblTitle);
            this.Controls.AddRange(new Control[] {
                panelHeader, lblSearch, txtSearch, lblSupplier, cmbSupplier,
                lblSort, cmbSort, flowPanel, btnBack, btnAdd
            });
        }

        private void LoadProducts()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();
                    string query = @"SELECT p.*, c.CategoryName, m.ManufacturerName, s.SupplierName 
                                   FROM Products p
                                   LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                                   LEFT JOIN Manufacturers m ON p.ManufacturerID = m.ManufacturerID
                                   LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID";

                    var adapter = new MySqlDataAdapter(query, connection);
                    productsTable = new DataTable();
                    adapter.Fill(productsTable);

                    CreateProductCards();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateProductCards()
        {
            flowPanel.Controls.Clear();
            productCards = new List<ProductCard>();

            if (productsTable == null || productsTable.Rows.Count == 0)
            {
                var noProductsLabel = new Label
                {
                    Text = "Товары не найдены",
                    Font = new Font("Times New Roman", 14, FontStyle.Bold),
                    Size = new Size(200, 30),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = Color.Red
                };
                flowPanel.Controls.Add(noProductsLabel);
                return;
            }

            foreach (DataRow row in productsTable.Rows)
            {
                try
                {
                    var product = new Product
                    {
                        ProductID = Convert.ToInt32(row["ProductID"]),
                        ProductName = row["ProductName"]?.ToString() ?? "Без названия",
                        Description = row["Description"]?.ToString() ?? "Нет описания",
                        CategoryName = row["CategoryName"]?.ToString() ?? "Без категории",
                        ManufacturerName = row["ManufacturerName"]?.ToString() ?? "Неизвестно",
                        SupplierName = row["SupplierName"]?.ToString() ?? "Неизвестно",
                        Price = Convert.ToDecimal(row["Price"]),
                        Unit = row["Unit"]?.ToString() ?? "шт.",
                        StockQuantity = Convert.ToInt32(row["StockQuantity"]),
                        Discount = Convert.ToDecimal(row["Discount"]),
                        ImagePath = row["ImagePath"]?.ToString()
                    };

                    var card = new ProductCard(product);
                    card.Width = 500;
                    card.Height = 220;
                    card.Margin = new Padding(10);

                    // Добавляем кнопки для администратора
                    if (Program.CurrentUser.Role == "Admin")
                    {
                        AddAdminButtons(card, product);
                    }

                    flowPanel.Controls.Add(card);
                    productCards.Add(card);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка создания карточки товара: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AddAdminButtons(ProductCard card, Product product)
        {
            var editButton = new Button
            {
                Text = "Редактировать",
                Font = new Font("Times New Roman", 9),
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(100, 25),
                Location = new Point(310, 200),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            var deleteButton = new Button
            {
                Text = "Удалить",
                Font = new Font("Times New Roman", 9),
                BackColor = Color.FromArgb(255, 230, 230),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(80, 25),
                Location = new Point(420, 200),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            editButton.Click += (s, e) => EditProduct(product.ProductID);
            deleteButton.Click += (s, e) => DeleteProduct(product.ProductID, product.ProductName);

            card.Controls.Add(editButton);
            card.Controls.Add(deleteButton);
        }

        private void EditProduct(int productId)
        {
            var editForm = new ProductEditForm(productId);
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts(); // Refresh after edit
            }
        }

        private void DeleteProduct(int productId, string productName)
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить товар '{productName}'?",
                "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (var connection = new MySqlConnection(Program.ConnectionString))
                    {
                        connection.Open();

                        // Проверяем, есть ли товар в заказах
                        string checkQuery = "SELECT COUNT(*) FROM OrderItems WHERE ProductID = @ProductID";
                        using (var cmd = new MySqlCommand(checkQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            var count = Convert.ToInt32(cmd.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show("Нельзя удалить товар, который присутствует в заказе",
                                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                        }

                        // Удаляем товар
                        string deleteQuery = "DELETE FROM Products WHERE ProductID = @ProductID";
                        using (var cmd = new MySqlCommand(deleteQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@ProductID", productId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Товар успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadProducts();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления товара: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadSuppliers()
        {
            cmbSupplier.Items.Clear();
            cmbSupplier.Items.Add("Все поставщики");

            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM Suppliers";

                    using (var cmd = new MySqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbSupplier.Items.Add(reader.GetString("SupplierName"));
                        }
                    }
                }
                cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilters()
        {
            if (productsTable != null && productCards != null)
            {
                var filter = "";
                var searchText = txtSearch.Text;

                if (!string.IsNullOrEmpty(searchText))
                {
                    filter += $"(ProductName LIKE '%{searchText}%' OR Description LIKE '%{searchText}%' OR ManufacturerName LIKE '%{searchText}%')";
                }

                if (cmbSupplier.SelectedIndex > 0)
                {
                    if (!string.IsNullOrEmpty(filter)) filter += " AND ";
                    filter += $"SupplierName = '{cmbSupplier.SelectedItem}'";
                }

                productsTable.DefaultView.RowFilter = filter;
                ApplySorting();
                UpdateCardVisibility();
            }
        }

        private void ApplySorting()
        {
            if (productsTable != null)
            {
                string sortExpression = "";
                switch (cmbSort.SelectedIndex)
                {
                    case 0: sortExpression = "ProductName ASC"; break;
                    case 1: sortExpression = "ProductName DESC"; break;
                    case 2: sortExpression = "Price ASC"; break;
                    case 3: sortExpression = "Price DESC"; break;
                    case 4: sortExpression = "StockQuantity ASC"; break;
                    case 5: sortExpression = "StockQuantity DESC"; break;
                }
                productsTable.DefaultView.Sort = sortExpression;
            }
        }

        private void UpdateCardVisibility()
        {
            if (productCards == null) return;

            foreach (var card in productCards)
            {
                bool isVisible = false;

                foreach (DataRowView row in productsTable.DefaultView)
                {
                    if (Convert.ToInt32(row["ProductID"]) == card.Product.ProductID)
                    {
                        isVisible = true;
                        break;
                    }
                }

                card.Visible = isVisible;
            }
        }

        private void ApplyRoleRestrictions()
        {
            var isManagerOrAdmin = Program.CurrentUser.Role == "Manager" || Program.CurrentUser.Role == "Admin";
            var isAdminOnly = Program.CurrentUser.Role == "Admin";

            txtSearch.Visible = isManagerOrAdmin;
            cmbSupplier.Visible = isManagerOrAdmin;
            cmbSort.Visible = isManagerOrAdmin;

            btnAdd.Visible = isAdminOnly;
        }

        private void AddProduct()
        {
            var editForm = new ProductEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts();
            }
        }
    }
}