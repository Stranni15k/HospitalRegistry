using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HospitalRegistry
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string connectionString = "Ваши данные для подключения к БД";


        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;
            string password = passwordBox.Password;
            string sql = "SELECT id, name, password, privileges, email FROM users WHERE name = @login AND password = @password";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))

            using (NpgsqlCommand command = new NpgsqlCommand(sql, connection))
            {

                command.Parameters.AddWithValue("login", login);
                command.Parameters.AddWithValue("password", password);

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Read();
                    UserInfo user = new UserInfo
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Password = reader.GetString(2),
                        Priveleges = reader.GetString(3),
                        Email = reader.GetString(4)
                    };

                    ((MainWindow)Application.Current.MainWindow).CurrentUser = user;

                    this.Close();

                    errorMessage.Text = "Авторизация прошла успешно";
                }
                else
                {
                    errorMessage.Text = "Неверный логин или пароль";
                }
            }
        }
    }
}
