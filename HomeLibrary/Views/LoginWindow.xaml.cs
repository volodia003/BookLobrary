using System.Windows;
using System.Windows.Input;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class LoginWindow : Window
    {
        public static User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            UsernameTextBox.Focus();
            
            // Enter для входа
            UsernameTextBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) PasswordBox.Focus(); };
            PasswordBox.KeyDown += (s, e) => { if (e.Key == Key.Enter) LoginButton_Click(null, null); };
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите логин и пароль");
                return;
            }

            try
            {
                using (var db = new DatabaseContext())
                {
                    var user = db.GetUserByUsername(username);
                    
                    if (user != null && user.Password == password)
                    {
                        CurrentUser = user;
                        var mainWindow = new MainWindow();
                        mainWindow.Show();
                        Close();
                    }
                    else
                    {
                        ShowError("Неверный логин или пароль");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;
        }
    }
}
