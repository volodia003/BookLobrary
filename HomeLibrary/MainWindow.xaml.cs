using System;
using System.Windows;
using HomeLibrary.Data;
using HomeLibrary.Views;

namespace HomeLibrary
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = LoginWindow.CurrentUser;
            if (currentUser != null)
            {
                UserNameText.Text = $"👤 {currentUser.FullName}";
                WelcomeText.Text = currentUser.IsAdmin 
                    ? "Вы вошли как администратор. Вам доступны все функции системы управления библиотекой."
                    : $"Добро пожаловать, {currentUser.FullName}! Здесь вы можете управлять своей домашней библиотекой.";

                // Скрыть кнопку пользователей для обычных пользователей
                if (!currentUser.IsAdmin)
                {
                    UsersButton.Visibility = Visibility.Collapsed;
                }
            }

            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var books = db.GetAllBooks();
                    var authors = db.GetAllAuthors();
                    var genres = db.GetAllGenres();
                    var users = db.GetAllUsers();
                    var activeLoans = db.GetActiveLoans();

                    TotalBooksText.Text = books.Count.ToString();
                    TotalAuthorsText.Text = authors.Count.ToString();
                    TotalGenresText.Text = genres.Count.ToString();
                    TotalUsersText.Text = users.Count.ToString();

                    int availableBooks = 0;
                    int loanedBooks = 0;
                    foreach (var book in books)
                    {
                        if (book.Status == "В наличии")
                            availableBooks++;
                        else
                            loanedBooks++;
                    }

                    AvailableBooksText.Text = availableBooks.ToString();
                    LoanedBooksText.Text = loanedBooks.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BooksButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new BooksWindow();
            window.ShowDialog();
            LoadStatistics(); // Обновить статистику после закрытия
        }

        private void AuthorsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new AuthorsWindow();
            window.ShowDialog();
            LoadStatistics();
        }

        private void GenresButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new GenresWindow();
            window.ShowDialog();
            LoadStatistics();
        }

        private void LocationsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LocationsWindow();
            window.ShowDialog();
            LoadStatistics();
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("У вас нет прав доступа к этому разделу.", 
                    "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new UsersWindow();
            window.ShowDialog();
            LoadStatistics();
        }

        private void LoansButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new LoansWindow();
            window.ShowDialog();
            LoadStatistics();
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new ReportsWindow();
            window.ShowDialog();
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            var window = new StatisticsWindow();
            window.ShowDialog();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы действительно хотите выйти из системы?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                Close();
            }
        }
    }
}
