using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class BooksWindow : Window
    {
        private List<Book> allBooks;
        private List<Genre> allGenres;
        private List<Author> allAuthors;
        private List<Location> allLocations;

        public BooksWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    allBooks = db.GetAllBooks();
                    allGenres = db.GetAllGenres();
                    allAuthors = db.GetAllAuthors();
                    allLocations = db.GetAllLocations();
                }

                // Заполнить фильтры
                GenreFilterComboBox.Items.Add(new Genre { Id = 0, Name = "Все жанры" });
                foreach (var genre in allGenres)
                {
                    GenreFilterComboBox.Items.Add(genre);
                }
                GenreFilterComboBox.SelectedIndex = 0;

                StatusFilterComboBox.Items.Add("Все");
                StatusFilterComboBox.Items.Add("В наличии");
                StatusFilterComboBox.Items.Add("Одолжена");
                StatusFilterComboBox.SelectedIndex = 0;

                RefreshBooksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBooksList()
        {
            BooksDataGrid.ItemsSource = null;
            BooksDataGrid.ItemsSource = allBooks;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string searchText = SearchTextBox.Text.Trim();
                int? genreId = null;
                string status = null;
                int? year = null;

                var selectedGenre = GenreFilterComboBox.SelectedItem as Genre;
                if (selectedGenre != null && selectedGenre.Id > 0)
                    genreId = selectedGenre.Id;

                var selectedStatus = StatusFilterComboBox.SelectedItem as string;
                if (selectedStatus != "Все")
                    status = selectedStatus;

                if (!string.IsNullOrWhiteSpace(YearFilterTextBox.Text))
                {
                    if (int.TryParse(YearFilterTextBox.Text, out int parsedYear))
                        year = parsedYear;
                }

                using (var db = new DatabaseContext())
                {
                    allBooks = db.SearchBooks(searchText, genreId, status, year, null);
                }

                RefreshBooksList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка поиска: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            GenreFilterComboBox.SelectedIndex = 0;
            StatusFilterComboBox.SelectedIndex = 0;
            YearFilterTextBox.Clear();
            LoadData();
        }

        private void BooksDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = BooksDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            AddReviewButton.IsEnabled = hasSelection;
            ViewReviewsButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new BookEditDialog(allAuthors, allGenres, allLocations);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertBook(dialog.Book);
                    }
                    LoadData();
                    MessageBox.Show("Книга успешно добавлена!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления книги: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = BooksDataGrid.SelectedItem as Book;
            if (selected == null) return;

            var dialog = new BookEditDialog(allAuthors, allGenres, allLocations, selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateBook(dialog.Book);
                    }
                    LoadData();
                    MessageBox.Show("Книга успешно обновлена!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления книги: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = BooksDataGrid.SelectedItem as Book;
            if (selected == null) return;

            var result = MessageBox.Show($"Вы действительно хотите удалить книгу '{selected.Title}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteBook(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Книга успешно удалена!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления книги: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddReviewButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = BooksDataGrid.SelectedItem as Book;
            if (selected == null) return;

            var dialog = new ReviewEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertReview(dialog.Review);
                    }
                    MessageBox.Show("Отзыв успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления отзыва: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ViewReviewsButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = BooksDataGrid.SelectedItem as Book;
            if (selected == null) return;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var reviews = db.GetReviewsByBook(selected.Id);
                    if (reviews.Count == 0)
                    {
                        MessageBox.Show("У этой книги еще нет отзывов.", "Информация", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    var message = $"Отзывы о книге '{selected.Title}':\n\n";
                    foreach (var review in reviews)
                    {
                        message += $"★ Рейтинг: {review.Rating}/5\n";
                        message += $"Пользователь: {review.UserName}\n";
                        message += $"Прочитано: {(review.IsRead ? "Да" : "Нет")}\n";
                        message += $"Отзыв: {review.ReviewText}\n";
                        message += $"Дата: {review.ReviewDate:dd.MM.yyyy}\n";
                        message += new string('-', 50) + "\n\n";
                    }

                    MessageBox.Show(message, "Отзывы", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки отзывов: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    // Диалог редактирования книги
    public class BookEditDialog : Window
    {
        public Book Book { get; private set; }

        private TextBox titleTextBox;
        private TextBox isbnTextBox;
        private TextBox yearTextBox;
        private TextBox publisherTextBox;
        private TextBox pagesTextBox;
        private TextBox descriptionTextBox;
        private ComboBox authorComboBox;
        private ComboBox genreComboBox;
        private ComboBox locationComboBox;

        public BookEditDialog(List<Author> authors, List<Genre> genres, List<Location> locations, Book book = null)
        {
            Book = book ?? new Book();
            
            Title = book == null ? "Добавить книгу" : "Редактировать книгу";
            Width = 600;
            Height = 550;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Название
            grid.Children.Add(CreateLabel("Название*:", row));
            titleTextBox = CreateTextBox(Book.Title);
            Grid.SetRow(titleTextBox, row++);

            // ISBN
            grid.Children.Add(CreateLabel("ISBN:", row));
            isbnTextBox = CreateTextBox(Book.ISBN);
            Grid.SetRow(isbnTextBox, row++);

            // Автор
            grid.Children.Add(CreateLabel("Автор:", row));
            authorComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15) };
            authorComboBox.Items.Add(new Author { Id = 0, LastName = "Не выбран" });
            foreach (var author in authors)
            {
                authorComboBox.Items.Add(author);
            }
            authorComboBox.DisplayMemberPath = "FullName";
            authorComboBox.SelectedIndex = 0;
            if (Book.AuthorId.HasValue)
            {
                for (int i = 0; i < authorComboBox.Items.Count; i++)
                {
                    if ((authorComboBox.Items[i] as Author).Id == Book.AuthorId.Value)
                    {
                        authorComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            Grid.SetRow(authorComboBox, row++);
            grid.Children.Add(authorComboBox);

            // Жанр
            grid.Children.Add(CreateLabel("Жанр:", row));
            genreComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15) };
            genreComboBox.Items.Add(new Genre { Id = 0, Name = "Не выбран" });
            foreach (var genre in genres)
            {
                genreComboBox.Items.Add(genre);
            }
            genreComboBox.DisplayMemberPath = "Name";
            genreComboBox.SelectedIndex = 0;
            if (Book.GenreId.HasValue)
            {
                for (int i = 0; i < genreComboBox.Items.Count; i++)
                {
                    if ((genreComboBox.Items[i] as Genre).Id == Book.GenreId.Value)
                    {
                        genreComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            Grid.SetRow(genreComboBox, row++);
            grid.Children.Add(genreComboBox);

            // Место хранения
            grid.Children.Add(CreateLabel("Место хранения:", row));
            locationComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15) };
            locationComboBox.Items.Add(new Location { Id = 0, Name = "Не выбрано" });
            foreach (var location in locations)
            {
                locationComboBox.Items.Add(location);
            }
            locationComboBox.DisplayMemberPath = "Name";
            locationComboBox.SelectedIndex = 0;
            if (Book.LocationId.HasValue)
            {
                for (int i = 0; i < locationComboBox.Items.Count; i++)
                {
                    if ((locationComboBox.Items[i] as Location).Id == Book.LocationId.Value)
                    {
                        locationComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            Grid.SetRow(locationComboBox, row++);
            grid.Children.Add(locationComboBox);

            // Год издания
            grid.Children.Add(CreateLabel("Год издания:", row));
            yearTextBox = CreateTextBox(Book.PublicationYear?.ToString() ?? "");
            Grid.SetRow(yearTextBox, row++);

            // Издательство
            grid.Children.Add(CreateLabel("Издательство:", row));
            publisherTextBox = CreateTextBox(Book.Publisher);
            Grid.SetRow(publisherTextBox, row++);

            // Количество страниц
            grid.Children.Add(CreateLabel("Страниц:", row));
            pagesTextBox = CreateTextBox(Book.PageCount?.ToString() ?? "");
            Grid.SetRow(pagesTextBox, row++);

            // Описание
            grid.Children.Add(CreateLabel("Описание:", row));
            descriptionTextBox = new TextBox 
            { 
                Text = Book.Description, 
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 80,
                Margin = new Thickness(0, 5, 0, 15)
            };
            Grid.SetRow(descriptionTextBox, row++);
            grid.Children.Add(descriptionTextBox);

            // Кнопки
            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var saveButton = new Button { Content = "Сохранить", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            saveButton.Click += SaveButton_Click;
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            cancelButton.Click += (s, e) => DialogResult = false;
            buttonsPanel.Children.Add(saveButton);
            buttonsPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonsPanel, row++);
            grid.Children.Add(buttonsPanel);

            Content = grid;
        }

        private TextBlock CreateLabel(string text, int row)
        {
            var label = new TextBlock { Text = text, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 5, 0, 5) };
            Grid.SetRow(label, row);
            return label;
        }

        private TextBox CreateTextBox(string text)
        {
            return new TextBox { Text = text, Margin = new Thickness(0, 5, 0, 15) };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(titleTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название книги.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Book.Title = titleTextBox.Text.Trim();
            Book.ISBN = isbnTextBox.Text.Trim();
            Book.Publisher = publisherTextBox.Text.Trim();
            Book.Description = descriptionTextBox.Text.Trim();

            var selectedAuthor = authorComboBox.SelectedItem as Author;
            Book.AuthorId = selectedAuthor?.Id > 0 ? selectedAuthor.Id : (int?)null;

            var selectedGenre = genreComboBox.SelectedItem as Genre;
            Book.GenreId = selectedGenre?.Id > 0 ? selectedGenre.Id : (int?)null;

            var selectedLocation = locationComboBox.SelectedItem as Location;
            Book.LocationId = selectedLocation?.Id > 0 ? selectedLocation.Id : (int?)null;

            if (!string.IsNullOrWhiteSpace(yearTextBox.Text))
            {
                if (int.TryParse(yearTextBox.Text, out int year))
                    Book.PublicationYear = year;
            }

            if (!string.IsNullOrWhiteSpace(pagesTextBox.Text))
            {
                if (int.TryParse(pagesTextBox.Text, out int pages))
                    Book.PageCount = pages;
            }

            DialogResult = true;
        }
    }

    // Диалог добавления отзыва
    public class ReviewEditDialog : Window
    {
        public Review Review { get; private set; }

        private ComboBox ratingComboBox;
        private TextBox reviewTextBox;
        private CheckBox isReadCheckBox;

        public ReviewEditDialog(Book book)
        {
            Review = new Review { BookId = book.Id, UserId = LoginWindow.CurrentUser.Id };

            Title = "Добавить отзыв";
            Width = 500;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Название книги
            var bookLabel = new TextBlock 
            { 
                Text = $"Книга: {book.Title}", 
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(bookLabel, row++);
            grid.Children.Add(bookLabel);

            // Рейтинг
            var ratingLabel = new TextBlock { Text = "Рейтинг*:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(ratingLabel, row);
            grid.Children.Add(ratingLabel);

            ratingComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15) };
            for (int i = 1; i <= 5; i++)
            {
                ratingComboBox.Items.Add($"★ {i} из 5");
            }
            ratingComboBox.SelectedIndex = 4; // По умолчанию 5
            Grid.SetRow(ratingComboBox, row++);
            grid.Children.Add(ratingComboBox);

            // Отзыв
            var textLabel = new TextBlock { Text = "Отзыв:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(textLabel, row);
            grid.Children.Add(textLabel);

            reviewTextBox = new TextBox 
            { 
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 150,
                Margin = new Thickness(0, 5, 0, 15)
            };
            Grid.SetRow(reviewTextBox, row++);
            grid.Children.Add(reviewTextBox);

            // Прочитано
            isReadCheckBox = new CheckBox { Content = "Я прочитал(а) эту книгу", Margin = new Thickness(0, 0, 0, 20) };
            Grid.SetRow(isReadCheckBox, row++);
            grid.Children.Add(isReadCheckBox);

            // Кнопки
            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var saveButton = new Button { Content = "Сохранить", Width = 100, Margin = new Thickness(0, 0, 10, 0) };
            saveButton.Click += SaveButton_Click;
            var cancelButton = new Button { Content = "Отмена", Width = 100 };
            cancelButton.Click += (s, e) => DialogResult = false;
            buttonsPanel.Children.Add(saveButton);
            buttonsPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonsPanel, row++);
            grid.Children.Add(buttonsPanel);

            Content = grid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Review.Rating = ratingComboBox.SelectedIndex + 1;
            Review.ReviewText = reviewTextBox.Text.Trim();
            Review.IsRead = isReadCheckBox.IsChecked ?? false;

            DialogResult = true;
        }
    }
}
