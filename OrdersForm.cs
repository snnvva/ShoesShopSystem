using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace ShoesShopSystem
{
    public partial class OrdersForm : Form
    {
        private DataGridView dataGridView;
        private Button btnBack, btnAdd, btnEdit, btnDelete;
        private DataTable ordersTable;

        public OrdersForm()
        {
            BuildForm();
            LoadOrders();
        }

        private void BuildForm()
        {
            this.Text = "Управление заказами - Обувной магазин";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Header panel
            var panelHeader = new Panel
            {
                BackColor = Color.FromArgb(127, 255, 0),
                Location = new Point(0, 0),
                Size = new Size(900, 60)
            };

            var lblTitle = new Label
            {
                Text = "УПРАВЛЕНИЕ ЗАКАЗАМИ",
                Font = new Font("Times New Roman", 16, FontStyle.Bold),
                Size = new Size(300, 30),
                Location = new Point(300, 15),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // DataGridView
            dataGridView = new DataGridView
            {
                Location = new Point(20, 80),
                Size = new Size(850, 400),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                Font = new Font("Times New Roman", 9),
                RowHeadersVisible = false
            };

            // Buttons
            btnBack = new Button
            {
                Text = "← Назад",
                Location = new Point(20, 500),
                Size = new Size(100, 35),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightGray
            };

            btnAdd = new Button
            {
                Text = "➕ Добавить заказ",
                Location = new Point(600, 500),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 250, 154),
                Font = new Font("Times New Roman", 10)
            };

            btnEdit = new Button
            {
                Text = "✏️ Редактировать",
                Location = new Point(730, 500),
                Size = new Size(120, 35),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.FromArgb(0, 250, 154)
            };

            btnDelete = new Button
            {
                Text = "🗑️ Удалить",
                Location = new Point(730, 545),
                Size = new Size(120, 35),
                Font = new Font("Times New Roman", 10),
                BackColor = Color.LightCoral
            };

            // Events
            btnBack.Click += (s, e) => this.Close();
            btnAdd.Click += (s, e) => AddOrder();
            btnEdit.Click += (s, e) => EditOrder();
            btnDelete.Click += (s, e) => DeleteOrder();
            dataGridView.SelectionChanged += (s, e) => UpdateButtonsState();

            // Apply role restrictions
            ApplyRoleRestrictions();

            panelHeader.Controls.Add(lblTitle);
            this.Controls.AddRange(new Control[] {
                panelHeader, dataGridView, btnBack, btnAdd, btnEdit, btnDelete
            });
        }

        private void LoadOrders()
        {
            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();

                    // Исправленный запрос - используем цену из Products
                    string query = @"SELECT 
                        o.OrderID,
                        o.OrderCode as 'Артикул заказа',
                        o.Status as 'Статус заказа',
                        p.Address as 'Адрес пункта выдачи',
                        o.OrderDate as 'Дата заказа',
                        o.DeliveryDate as 'Дата доставки',
                        u.FullName as 'Клиент',
                        (SELECT SUM(oi.Quantity * pr.Price) 
                         FROM OrderItems oi 
                         JOIN Products pr ON oi.ProductID = pr.ProductID
                         WHERE oi.OrderID = o.OrderID) as 'Сумма заказа'
                    FROM Orders o
                    LEFT JOIN PickupPoints p ON o.PickupPointID = p.PointID
                    LEFT JOIN Users u ON o.UserID = u.UserID
                    ORDER BY o.OrderDate DESC";

                    var adapter = new MySqlDataAdapter(query, connection);
                    ordersTable = new DataTable();
                    adapter.Fill(ordersTable);

                    dataGridView.DataSource = ordersTable;
                    UpdateButtonsState();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyRoleRestrictions()
        {
            var isAdmin = Program.CurrentUser.Role == "Admin";

            btnAdd.Visible = isAdmin;
            btnEdit.Visible = isAdmin;
            btnDelete.Visible = isAdmin;
        }

        private void UpdateButtonsState()
        {
            bool hasSelection = dataGridView.SelectedRows.Count > 0;
            btnEdit.Enabled = hasSelection;
            btnDelete.Enabled = hasSelection;
        }

        private void AddOrder()
        {
            var editForm = new OrderEditForm();
            if (editForm.ShowDialog() == DialogResult.OK)
            {
                LoadOrders(); // Refresh after add
            }
        }

        private void EditOrder()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var orderId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["OrderID"].Value);
                var editForm = new OrderEditForm(orderId);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadOrders(); // Refresh after edit
                }
            }
        }

        private void DeleteOrder()
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                var orderId = Convert.ToInt32(dataGridView.SelectedRows[0].Cells["OrderID"].Value);
                var orderCode = dataGridView.SelectedRows[0].Cells["Артикул заказа"].Value.ToString();

                if (MessageBox.Show($"Вы уверены, что хотите удалить заказ '{orderCode}'?",
                    "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        using (var connection = new MySqlConnection(Program.ConnectionString))
                        {
                            connection.Open();

                            // Сначала удаляем элементы заказа
                            string deleteItemsQuery = "DELETE FROM OrderItems WHERE OrderID = @OrderID";
                            using (var cmd = new MySqlCommand(deleteItemsQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.ExecuteNonQuery();
                            }

                            // Затем удаляем сам заказ
                            string deleteOrderQuery = "DELETE FROM Orders WHERE OrderID = @OrderID";
                            using (var cmd = new MySqlCommand(deleteOrderQuery, connection))
                            {
                                cmd.Parameters.AddWithValue("@OrderID", orderId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("Заказ успешно удален", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления заказа: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}