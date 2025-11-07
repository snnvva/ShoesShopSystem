using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ShoesShopSystem
{
    public partial class LoginForm : Form
    {
        private TextBox txtLogin, txtPassword;
        private Button btnLogin, btnGuest;
        private Label lblTitle, lblLogin, lblPassword;

        public LoginForm()
        {
            CreateForm();
        }

        private void CreateForm()
        {
            // Form settings
            this.Text = "Вход в систему - Обувной магазин";
            this.Size = new Size(450, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            lblTitle = new Label
            {
                Text = "ОБУВНОЙ МАГАЗИН",
                Font = new Font("Times New Roman", 18, FontStyle.Bold),
                ForeColor = Color.Black,
                Size = new Size(300, 40),
                Location = new Point(75, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Login label and textbox
            lblLogin = new Label
            {
                Text = "Логин:",
                Font = new Font("Times New Roman", 12),
                Size = new Size(80, 25),
                Location = new Point(50, 100),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtLogin = new TextBox
            {
                Font = new Font("Times New Roman", 11),
                Size = new Size(250, 30),
                Location = new Point(140, 100),
                BackColor = Color.White
            };

            // Password label and textbox
            lblPassword = new Label
            {
                Text = "Пароль:",
                Font = new Font("Times New Roman", 12),
                Size = new Size(80, 25),
                Location = new Point(50, 140),
                TextAlign = ContentAlignment.MiddleLeft
            };

            txtPassword = new TextBox
            {
                Font = new Font("Times New Roman", 11),
                Size = new Size(250, 30),
                Location = new Point(140, 140),
                UseSystemPasswordChar = true,
                BackColor = Color.White
            };

            // Buttons
            btnLogin = new Button
            {
                Text = "ВОЙТИ",
                Font = new Font("Times New Roman", 12, FontStyle.Bold),
                Size = new Size(150, 40),
                Location = new Point(50, 190),
                BackColor = Color.FromArgb(0, 250, 154), // #00FA9A
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };

            btnGuest = new Button
            {
                Text = "ВОЙТИ КАК ГОСТЬ",
                Font = new Font("Times New Roman", 10),
                Size = new Size(150, 35),
                Location = new Point(240, 190),
                BackColor = Color.FromArgb(127, 255, 0), // #7FFF00
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.Black
            };

            // Список ваших пользователей для подсказки
            var usersInfo = new Label
            {
                Text = "Доступные пользователи:\n" +
                       "• 94d5ous@gmail.com / uzWC67 (Admin)\n" +
                       "• uth4iz@mail.com / 2L6KZG (Admin)\n" +
                       "• 1diph5e@tutanota.com / 8ntwUp (Manager)\n" +
                       "• 5d4zbu@tutanota.com / rwVDh9 (Client)",
                Font = new Font("Times New Roman", 8),
                Size = new Size(350, 80),
                Location = new Point(50, 240),
                ForeColor = Color.DarkGray
            };

            // Remove button borders
            btnLogin.FlatAppearance.BorderSize = 0;
            btnGuest.FlatAppearance.BorderSize = 0;

            // Events
            btnLogin.Click += BtnLogin_Click;
            btnGuest.Click += BtnGuest_Click;
            txtPassword.KeyPress += TxtPassword_KeyPress;

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblTitle, lblLogin, txtLogin, lblPassword, txtPassword,
                btnLogin, btnGuest, usersInfo
            });
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            AuthenticateUser();
        }

        private void BtnGuest_Click(object sender, EventArgs e)
        {
            Program.CurrentUser = new User { Role = "Guest", FullName = "Гость" };
            var mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }

        private void TxtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                AuthenticateUser();
            }
        }

        private void AuthenticateUser()
        {
            if (string.IsNullOrEmpty(txtLogin.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка входа",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(Program.ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT UserID, Login, Password, FullName, Role FROM Users WHERE Login = @Login AND Password = @Password";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Login", txtLogin.Text);
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Program.CurrentUser = new User
                                {
                                    UserID = reader.GetInt32("UserID"),
                                    Login = reader.GetString("Login"),
                                    Password = reader.GetString("Password"),
                                    FullName = reader.GetString("FullName"),
                                    Role = reader.GetString("Role")
                                };

                                var mainForm = new MainForm();
                                mainForm.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль", "Ошибка входа",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка подключения к базе данных:\n{ex.Message}\n\n" +
                    "Проверьте:\n" +
                    "1. Запущен ли MySQL сервер\n" +
                    "2. Правильность строки подключения в Program.cs\n" +
                    "3. Существует ли база данных 'ShoesShop'",
                    "Ошибка базы данных", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}