using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace ShoesShopSystem
{
    public partial class OrderEditForm : Form
    {
        private int orderId = -1;

        private TextBox txtOrderCode, txtDeliveryDate;
        private ComboBox cmbStatus, cmbPickupPoint, cmbUser, cmbProduct;
        private NumericUpDown numQuantity;
        private Button btnSave, btnCancel, btnAddItem, btnRemoveItem;
        private DataGridView dataGridOrderItems;
        private List<OrderItem> orderItems;

        public OrderEditForm(int orderId = -1)
        {
            this.orderId = orderId;
            orderItems = new List<OrderItem>();
            BuildForm();
            if (orderId != -1)
                LoadOrderData();
        }

        private void BuildForm()
        {
            this.Text = orderId == -1 ? "Добавление заказа" : "Редактирование заказа";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Order Code
            CreateControl("Артикул заказа:", 20, out txtOrderCode, 200);
            if (orderId == -1)
            {
                txtOrderCode.Text = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmss");
            }

            // Status
            CreateComboBox("Статус заказа:", 50, out cmbStatus);
            cmbStatus.Items.AddRange(new[] { "Новый", "В обработке", "Выполнен", "Отменен" });

            // Pickup Point
            CreateComboBox("Пункт выдачи:", 80, out cmbPickupPoint);

            // User
            CreateComboBox("Клиент:", 110, out cmbUser);

            // Delivery Date
            CreateControl("Дата доставки:", 140, out txtDeliveryDate, 150);
            txtDeliveryDate.Text = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");

            // Order Items Section
            var lblItems = new Label
            {
                Text = "Товары в заказе:",
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Location = new Point(20, 180),
                Size = new Size(150, 20)
            };

            // Product selection for adding items
            var lblProduct = new Label
            {
                Text = "Товар:",
                Location = new Point(20, 210),
                Size = new Size(50, 20),
                Font = new Font("Times New Roman", 9)
            };

            cmbProduct = new ComboBox
            {
                Location = new Point(80, 210),
                Size = new Size(200, 20),
                Font = new Font("Times New Roman", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblQuantity = new Label
            {
                Text = "Количество:",
                Location = new Point(300, 210),
                Size = new Size(70, 20),
                Font = new Font("Times New Roman", 9)
            };

            numQuantity = new NumericUpDown
            {
                Location = new Point(380, 210),
                Size = new Size(60, 20),
                Minimum = 1,
                Maximum = 100,
                Value = 1
            };

            btnAddItem = new Button
            {
                Text = "Добавить товар",
                Location = new Point(450, 210),
                Size = new Size(100, 25),
                Font = new Font("Times New Roman", 8),
                BackColor = Color.FromArgb(0, 250, 154)
            };

            // Order Items DataGrid
            dataGridOrderItems = new DataGridView
            {
                Location = new Point(20, 250),
                Size = new Size(650, 200),
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dataGridOrderItems.Columns.Add("ProductName", "Товар");
            dataGridOrderItems.Columns.Add("Quantity", "Количество");
            dataGridOrderItems.Columns.Add("UnitPrice", "Цена за единицу");
            dataGridOrderItems.Columns.Add("Total", "Сумма");

            btnRemoveItem = new Button
            {
                Text = "Удалить выбранный",
                Location = new Point(20, 460),
                Size = new Size(120, 25),
                Font = new Font("Times New Roman", 8),
                BackColor = Color.LightCoral
            };

            // Buttons
            btnSave = new Button
            {
                Text = "Сохранить",
                Location = new Point(200, 500),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(0, 250, 154),
                Font = new Font("Times New Roman", 10, FontStyle.Bold)
            };

            btnCancel = new Button
            {
                Text = "Отмена",
                Location = new Point(320, 500),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 10)
            };

            // Events
            btnAddItem.Click += (s, e) => AddOrderItem();
            btnRemoveItem.Click += (s, e) => RemoveOrderItem();
            btnSave.Click += (s, e) => SaveOrder();
            btnCancel.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            // Load data
            LoadComboBoxData();

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblItems, lblProduct, cmbProduct, lblQuantity, numQuantity, btnAddItem,
                dataGridOrderItems, btnRemoveItem, btnSave, btnCancel
            });
        }

        private void CreateControl(string labelText, int y, out TextBox textBox, int width)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(20, y),
                Size = new Size(120, 20),
                Font = new Font("Times New Roman", 9)
            };

            textBox = new TextBox
            {
                Location = new Point(150, y),
                Size = new Size(width, 20),
                Font = new Font("Times New Roman", 9)
            };

            this.Controls.Add(label);
            this.Controls.Add(textBox);
        }

        private void CreateComboBox(string labelText, int y, out ComboBox comboBox)
        {
            var label = new Label
            {
                Text = labelText,
                Location = new Point(20, y),
                Size = new Size(120, 20),
                Font = new Font("Times New Roman", 9)
            };

            comboBox = new ComboBox
            {
                Location = new Point(150, y),
                Size = new Size(200, 20),
                Font = new Font("Times New Roman", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            this.Controls.Add(label);
            this.Controls.Add(comboBox);
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    // Pickup Points
                    cmbPickupPoint.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT PointID, Address FROM PickupPoints", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbPickupPoint.Items.Add(new ComboBoxItem(
                                reader.GetString("Address"),
                                reader.GetInt32("PointID")
                            ));
                        }
                    }

                    // Users
                    cmbUser.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT UserID, FullName FROM Users WHERE Role IN ('Client', 'Manager', 'Admin')", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cmbUser.Items.Add(new ComboBoxItem(
                                reader.GetString("FullName"),
                                reader.GetInt32("UserID")
                            ));
                        }
                    }

                    // Products
                    cmbProduct.Items.Clear();
                    using (var cmd = new MySqlCommand("SELECT ProductID, ProductName, Price FROM Products WHERE StockQuantity > 0", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var product = new Product
                            {
                                ProductID = reader.GetInt32("ProductID"),
                                ProductName = reader.GetString("ProductName"),
                                Price = reader.GetDecimal("Price")
                            };
                            cmbProduct.Items.Add(product);
                        }
                    }

                    // Set defaults
                    if (cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0;
                    if (cmbPickupPoint.Items.Count > 0) cmbPickupPoint.SelectedIndex = 0;
                    if (cmbUser.Items.Count > 0) cmbUser.SelectedIndex = 0;
                    if (cmbProduct.Items.Count > 0) cmbProduct.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadOrderData()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    // Load order data
                    string orderQuery = "SELECT * FROM Orders WHERE OrderID = @OrderID";
                    using (var cmd = new MySqlCommand(orderQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtOrderCode.Text = reader["OrderCode"].ToString();
                                cmbStatus.SelectedItem = reader["Status"].ToString();
                                txtDeliveryDate.Text = reader["DeliveryDate"] != DBNull.Value ?
                                    Convert.ToDateTime(reader["DeliveryDate"]).ToString("yyyy-MM-dd") : "";

                                // Set combobox values
                                SetComboBoxValue(cmbPickupPoint, Convert.ToInt32(reader["PickupPointID"]));
                                SetComboBoxValue(cmbUser, Convert.ToInt32(reader["UserID"]));
                            }
                        }
                    }

                    // Load order items - исправленный запрос
                    string itemsQuery = @"SELECT oi.*, p.ProductName, p.Price 
                                        FROM OrderItems oi 
                                        JOIN Products p ON oi.ProductID = p.ProductID 
                                        WHERE oi.OrderID = @OrderID";
                    using (var cmd = new MySqlCommand(itemsQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var item = new OrderItem
                                {
                                    ProductID = reader.GetInt32("ProductID"),
                                    ProductName = reader.GetString("ProductName"),
                                    Quantity = reader.GetInt32("Quantity"),
                                    UnitPrice = reader.GetDecimal("Price") // Берем цену из Products
                                };
                                orderItems.Add(item);
                                AddItemToGrid(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetComboBoxValue(ComboBox comboBox, int value)
        {
            foreach (ComboBoxItem item in comboBox.Items)
            {
                if (item.Value == value)
                {
                    comboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void AddOrderItem()
        {
            if (cmbProduct.SelectedItem is Product selectedProduct)
            {
                var item = new OrderItem
                {
                    ProductID = selectedProduct.ProductID,
                    ProductName = selectedProduct.ProductName,
                    Quantity = (int)numQuantity.Value,
                    UnitPrice = selectedProduct.Price
                };

                orderItems.Add(item);
                AddItemToGrid(item);

                // Reset quantity
                numQuantity.Value = 1;
            }
        }

        private void AddItemToGrid(OrderItem item)
        {
            dataGridOrderItems.Rows.Add(
                item.ProductName,
                item.Quantity,
                item.UnitPrice.ToString("C"),
                item.Total.ToString("C")
            );
        }

        private void RemoveOrderItem()
        {
            if (dataGridOrderItems.SelectedRows.Count > 0)
            {
                int index = dataGridOrderItems.SelectedRows[0].Index;
                if (index < orderItems.Count)
                {
                    orderItems.RemoveAt(index);
                    dataGridOrderItems.Rows.RemoveAt(index);
                }
            }
        }

        private void SaveOrder()
        {
            if (!ValidateInput()) return;

            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    if (orderId == -1)
                    {
                        // Add new order
                        string orderQuery = @"INSERT INTO Orders (OrderCode, Status, PickupPointID, UserID, OrderDate, DeliveryDate) 
                                          VALUES (@OrderCode, @Status, @PickupPointID, @UserID, @OrderDate, @DeliveryDate);
                                          SELECT LAST_INSERT_ID();";

                        using (var cmd = new MySqlCommand(orderQuery, connection))
                        {
                            SetOrderParameters(cmd);
                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // Add order items
                        AddOrderItems(connection);

                        MessageBox.Show("Заказ успешно добавлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // Update existing order
                        string orderQuery = @"UPDATE Orders SET 
                                          OrderCode = @OrderCode,
                                          Status = @Status,
                                          PickupPointID = @PickupPointID,
                                          UserID = @UserID,
                                          DeliveryDate = @DeliveryDate
                                          WHERE OrderID = @OrderID";

                        using (var cmd = new MySqlCommand(orderQuery, connection))
                        {
                            SetOrderParameters(cmd);
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        // Update order items - delete old and add new
                        string deleteItemsQuery = "DELETE FROM OrderItems WHERE OrderID = @OrderID";
                        using (var cmd = new MySqlCommand(deleteItemsQuery, connection))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        AddOrderItems(connection);

                        MessageBox.Show("Заказ успешно обновлен", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetOrderParameters(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@OrderCode", txtOrderCode.Text);
            cmd.Parameters.AddWithValue("@Status", cmbStatus.SelectedItem.ToString());
            cmd.Parameters.AddWithValue("@PickupPointID", ((ComboBoxItem)cmbPickupPoint.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@UserID", ((ComboBoxItem)cmbUser.SelectedItem).Value);
            cmd.Parameters.AddWithValue("@OrderDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@DeliveryDate",
                DateTime.TryParse(txtDeliveryDate.Text, out DateTime deliveryDate) ? deliveryDate : DateTime.Now.AddDays(7));
        }

        private void AddOrderItems(MySqlConnection connection)
        {
            foreach (var item in orderItems)
            {
                string itemQuery = @"INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice) 
                                  VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice)";

                using (var cmd = new MySqlCommand(itemQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@ProductID", item.ProductID);
                    cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                    cmd.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(txtOrderCode.Text))
            {
                MessageBox.Show("Введите артикул заказа", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (cmbStatus.SelectedItem == null || cmbPickupPoint.SelectedItem == null || cmbUser.SelectedItem == null)
            {
                MessageBox.Show("Заполните все обязательные поля", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (orderItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар в заказ", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }
    }

    public class OrderItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}