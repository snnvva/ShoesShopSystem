using System;
using System.Drawing;
using System.Windows.Forms;

namespace ShoesShopSystem
{
    public partial class MainForm : Form
    {
        private Panel panelHeader;
        private Label lblTitle, lblUserInfo, lblRoleInfo;
        private Button btnProducts, btnOrders, btnLogout;

        public MainForm()
        {
            CreateForm();
        }

        private void CreateForm()
        {
            // Form settings
            this.Text = "Главная - Обувной магазин";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Header panel
            panelHeader = new Panel
            {
                BackColor = Color.FromArgb(127, 255, 0), // #7FFF00
                Size = new Size(600, 100),
                Location = new Point(0, 0)
            };

            // Title in header
            lblTitle = new Label
            {
                Text = "ОБУВНОЙ МАГАЗИН",
                Font = new Font("Times New Roman", 20, FontStyle.Bold),
                ForeColor = Color.Black,
                Size = new Size(400, 40),
                Location = new Point(100, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // User info
            lblUserInfo = new Label
            {
                Text = $"Добро пожаловать, {Program.CurrentUser.FullName}!",
                Font = new Font("Times New Roman", 14, FontStyle.Bold),
                Size = new Size(400, 30),
                Location = new Point(100, 120),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblRoleInfo = new Label
            {
                Text = $"Роль: {Program.CurrentUser.Role}",
                Font = new Font("Times New Roman", 12, FontStyle.Italic),
                Size = new Size(400, 25),
                Location = new Point(100, 150),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkGray
            };

            // Buttons
            btnProducts = new Button
            {
                Text = "📦 УПРАВЛЕНИЕ ТОВАРАМИ",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(150, 200),
                BackColor = Color.FromArgb(0, 250, 154), // #00FA9A
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };

            btnOrders = new Button
            {
                Text = "📋 УПРАВЛЕНИЕ ЗАКАЗАМИ",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Size = new Size(300, 50),
                Location = new Point(150, 270),
                BackColor = Color.FromArgb(0, 250, 154), // #00FA9A
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black,
                Visible = Program.CurrentUser.Role == "Manager" || Program.CurrentUser.Role == "Admin"
            };

            btnLogout = new Button
            {
                Text = "🚪 ВЫХОД",
                Font = new Font("Times New Roman", 11),
                Size = new Size(150, 40),
                Location = new Point(225, 350),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };

            // Remove button borders
            btnProducts.FlatAppearance.BorderSize = 0;
            btnOrders.FlatAppearance.BorderSize = 0;
            btnLogout.FlatAppearance.BorderSize = 0;

            // Events
            btnProducts.Click += (s, e) => ShowProductsForm();
            btnOrders.Click += (s, e) => ShowOrdersForm();
            btnLogout.Click += (s, e) => Logout();

            // Add controls
            panelHeader.Controls.Add(lblTitle);
            this.Controls.AddRange(new Control[] {
                panelHeader, lblUserInfo, lblRoleInfo, btnProducts, btnOrders, btnLogout
            });
        }

        private void ShowProductsForm()
        {
            var productsForm = new ProductsForm();
            productsForm.ShowDialog();
        }

        private void ShowOrdersForm()
        {
            var ordersForm = new OrdersForm();
            ordersForm.ShowDialog();
        }

        private void Logout()
        {
            if (MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Program.CurrentUser = null;
                var loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
        }
    }
}