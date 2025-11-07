using System;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace ShoesShopSystem
{
    static class Program
    {
        public static User CurrentUser { get; set; }

        // Используйте вашу строку подключения
        public static string ConnectionString = "Server=localhost;Database=ShoesShop;Uid=root;Pwd=12345;";

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }

    public class User
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public string CategoryName { get; set; }
        public string ManufacturerName { get; set; }
        public string SupplierName { get; set; }
        public decimal Price { get; set; }
        public string Unit { get; set; }
        public int StockQuantity { get; set; }
        public decimal Discount { get; set; }
        public string ImagePath { get; set; }
        public decimal FinalPrice => Price * (1 - Discount / 100);
    }
}