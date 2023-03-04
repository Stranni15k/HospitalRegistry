using Npgsql;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net.Mail;
using System.Net;
using Microsoft.VisualBasic.ApplicationServices;

namespace HospitalRegistry
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string connectionString = "Ваши данные для подключения к БД";
        public UserInfo CurrentUser { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            RefreshRequests();
        }

        public void RefreshRequests()
        {
            if(CurrentUser == null)
            {
                AuthorizButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                LogoutButton.Visibility = Visibility.Visible;
                AuthorizButton.Visibility = Visibility.Collapsed;
            }


            RequestsStackPanel.Children.Clear();
            List<Request> requests = GetRequests();
            foreach (var request in requests)
            {
                Border border = new Border
                {
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    Padding = new Thickness(5)
                };

                StackPanel stackPanel = new StackPanel();
                TextBlock fullNameTextBlock = new TextBlock
                {
                    Text = request.FullName,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16
                };
                TextBlock birthDateTextBlock = new TextBlock
                {
                    Text = request.BirthDate.ToShortDateString(),
                    Margin = new Thickness(0, 5, 0, 0)
                };
                TextBlock additionalInfoTextBlock = new TextBlock
                {
                    Text = request.AdditionalInfo,
                    Margin = new Thickness(0, 5, 0, 0)
                };
                TextBlock statusTextBlock = new TextBlock
                {
                    Text = request.Status,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                if(CurrentUser != null && CurrentUser.Priveleges == "admin") 
                {
                    Button approveButton = new Button
                    {
                        Content = "Одобрить",
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    Button rejectButton = new Button
                    {
                        Content = "Отказать",
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    Button deleteButton = new Button
                    {
                        Content = "Удалить",
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    approveButton.Tag = request.Id;
                    rejectButton.Tag = request.Id;
                    deleteButton.Tag = request.Id;

                    approveButton.Click += Approve_Click;
                    rejectButton.Click += Reject_Click;
                    deleteButton.Click += Delete_Click;

                    stackPanel.Children.Add(fullNameTextBlock);
                    stackPanel.Children.Add(birthDateTextBlock);
                    stackPanel.Children.Add(additionalInfoTextBlock);
                    stackPanel.Children.Add(statusTextBlock);

                    stackPanel.Children.Add(approveButton);
                    stackPanel.Children.Add(rejectButton);
                    stackPanel.Children.Add(deleteButton);
                }
                else
                {
                    stackPanel.Children.Add(fullNameTextBlock);
                    stackPanel.Children.Add(birthDateTextBlock);
                    stackPanel.Children.Add(additionalInfoTextBlock);
                    stackPanel.Children.Add(statusTextBlock);
                }

                border.Child = stackPanel;
                RequestsStackPanel.Children.Add(border);
            }
        }

        private void UpdateRequestStatus(int requestId, string newStatus)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("UPDATE requests SET status = @status WHERE id = @id", connection);
                command.Parameters.AddWithValue("id", requestId);
                command.Parameters.AddWithValue("status", newStatus);
                command.ExecuteNonQuery();
            }
        }

        private void DeleteRequest(int requestId)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("DELETE FROM requests WHERE id = @id", connection);
                command.Parameters.AddWithValue("id", requestId);
                command.ExecuteNonQuery();
            }
        }

        private string GetUserEmailById(int userId)
        {
            string query = "SELECT user_id FROM requests WHERE Id = @Id";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", userId);

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    if (reader.IsDBNull(0))
                    {
                        return null;
                    }
                    else
                    {
                        int user_id = reader.GetInt32(0);
                        string usermail = "";
                        reader.Close();
                        NpgsqlCommand userEmail = new NpgsqlCommand("SELECT Email FROM users WHERE Id = @Id", connection);
                        userEmail.Parameters.AddWithValue("@Id", user_id);
                        NpgsqlDataReader readerEmail = userEmail.ExecuteReader();
                        if (readerEmail.Read())
                        {
                            string userEmailString = readerEmail.GetString(0);
                            usermail = userEmailString;
                        }
                        readerEmail.Close();
                        return usermail;
                    }
                }
                else
                {
                    reader.Close();
                    throw new Exception("Пользователь с указанным ID не найден.");
                }
            }
        }

        private string GetFullNameById(int requestId)
        {
            string query = "SELECT full_name FROM requests WHERE Id = @Id";

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", requestId);

                connection.Open();
                NpgsqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return reader.GetString(0);
                }
                else
                {
                    throw new Exception("Пользователь с указанным ID не найден.");
                }
            }
        }

        public List<Request> GetRequests()
        {
            List<Request> requests = new List<Request>();
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand("SELECT * FROM requests", connection);
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Request request = new Request();
                    request.Id = (int)reader["id"];
                    request.FullName = reader["full_name"].ToString();
                    request.BirthDate = (DateTime)reader["birth_date"];
                    request.AdditionalInfo = reader["additional_info"].ToString();
                    request.Status = reader["status"].ToString();
                    requests.Add(request);
                }
            }
            return requests;
        }

        private void AddRequest_Click(object sender, RoutedEventArgs e)
        {
            AddRequestWindow addRequestWindow = new AddRequestWindow();
            addRequestWindow.CurrentUser = CurrentUser;
            addRequestWindow.ShowDialog();
            RefreshRequests();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var LoginWindow = new LoginWindow();
            LoginWindow.ShowDialog();
            RefreshRequests();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            if(sender is Button button1 && button1.Tag is int requestID)
            {
                string userEmail = GetUserEmailById(requestID);

                if (userEmail != null)
                {
                    if (sender is Button button && button.Tag is int requestId)
                    {
                        UpdateRequestStatus(requestId, "Одобрено");
                        RefreshRequests();

                        string userName = GetFullNameById(requestId);

                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("Ваш_Email");
                            mail.To.Add(userEmail);
                            mail.Subject = "Информационное письмо Mega";
                            mail.Body = $"Уважаемый(ая) {userName} ваша заявка в клинику <Ваше название> была одобрена";

                            using (SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587))
                            {
                                smtp.Credentials = new NetworkCredential("Ваш_Email", "Пароль");
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }
                    }
                }
                else
                {
                    if (sender is Button button && button.Tag is int requestId)
                    {
                        UpdateRequestStatus(requestId, "Одобрено");
                        RefreshRequests();
                    }
                }
            }
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button1 && button1.Tag is int requestID)
            {
                string userEmail = GetUserEmailById(requestID);

                if (userEmail != null)
                {
                    if (sender is Button button && button.Tag is int requestId)
                    {
                        UpdateRequestStatus(requestId, "Отказано");
                        RefreshRequests();

                        string userName = GetFullNameById(requestId);

                        using (MailMessage mail = new MailMessage())
                        {
                            mail.From = new MailAddress("Ваш_Email");
                            mail.To.Add(userEmail);
                            mail.Subject = "Информационное письмо Mega";
                            mail.Body = $"Уважаемый(ая) {userName} ваша заявка в клинику <Ваше название> была отклонена";

                            using (SmtpClient smtp = new SmtpClient("smtp.mail.ru", 587))
                            {
                                smtp.Credentials = new NetworkCredential("Ваш_Email", "Пароль");
                                smtp.EnableSsl = true;
                                smtp.Send(mail);
                            }
                        }
                    }
                }
                else
                {
                    if (sender is Button button && button.Tag is int requestId)
                    {
                        UpdateRequestStatus(requestId, "Отказано");
                        RefreshRequests();
                    }
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int requestId)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту заявку?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    DeleteRequest(requestId);
                    RefreshRequests();
                }
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser = null;
            RefreshRequests();
        }
    }
}
