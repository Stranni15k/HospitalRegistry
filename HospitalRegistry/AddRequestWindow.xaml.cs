using Npgsql;
using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для AddRequestWindow.xaml
    /// </summary>
    public partial class AddRequestWindow : Window
    {
        private string connectionString = "Ваши данные для подключения к БД";
        public UserInfo CurrentUser { get; set; }
        public int UserId;
        public string UserName;
        public string UserPrivileges;

        public AddRequestWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if(UserName != null)
            {
                string fullName = fullNameTextBox.Text;
                DateTime birthDate = BirthDatePicker.SelectedDate ?? DateTime.Now;
                string additionalInfo = AdditionalInfoTextBox.Text;
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO requests (id ,full_name, birth_date, additional_info, status, user_id) VALUES (nextval('requests_id_seq') ,@fullName, @birthDate, @additionalInfo, 'Новая заявка', @user_id)", connection);
                    command.Parameters.AddWithValue("fullName", fullName);
                    command.Parameters.AddWithValue("birthDate", birthDate);
                    command.Parameters.AddWithValue("additionalInfo", additionalInfo);
                    command.Parameters.AddWithValue("user_id", UserId);
                    command.ExecuteNonQuery();
                }
                Close();
            }
            else
            {
                string fullName = fullNameTextBox.Text;
                DateTime birthDate = BirthDatePicker.SelectedDate ?? DateTime.Now;
                string additionalInfo = AdditionalInfoTextBox.Text;
                using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand("INSERT INTO requests (id ,full_name, birth_date, additional_info, status) VALUES (nextval('requests_id_seq') ,@fullName, @birthDate, @additionalInfo, 'Новая заявка')", connection);
                    command.Parameters.AddWithValue("fullName", fullName);
                    command.Parameters.AddWithValue("birthDate", birthDate);
                    command.Parameters.AddWithValue("additionalInfo", additionalInfo);
                    command.ExecuteNonQuery();
                }
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if(CurrentUser != null)
            {
                UserId = CurrentUser.Id;
                UserName = CurrentUser.Name;
                UserPrivileges = CurrentUser.Priveleges;
            }
            else
            {

            }
        }

    }
}
