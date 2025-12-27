using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class AuthorsWindow : Window
    {
        private List<Author> authors;

        public AuthorsWindow()
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
                    authors = db.GetAllAuthors();
                }
                AuthorsDataGrid.ItemsSource = null;
                AuthorsDataGrid.ItemsSource = authors;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AuthorsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = AuthorsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AuthorEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertAuthor(dialog.Author);
                    }
                    LoadData();
                    MessageBox.Show("Автор успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления автора: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = AuthorsDataGrid.SelectedItem as Author;
            if (selected == null) return;

            var dialog = new AuthorEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateAuthor(dialog.Author);
                    }
                    LoadData();
                    MessageBox.Show("Автор успешно обновлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления автора: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = AuthorsDataGrid.SelectedItem as Author;
            if (selected == null) return;

            var result = MessageBox.Show($"Вы действительно хотите удалить автора '{selected.FullName}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteAuthor(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Автор успешно удален!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления автора: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class AuthorEditDialog : Window
    {
        public Author Author { get; private set; }

        private TextBox firstNameTextBox;
        private TextBox lastNameTextBox;
        private TextBox countryTextBox;
        private DatePicker birthDatePicker;
        private TextBox biographyTextBox;

        public AuthorEditDialog(Author author = null)
        {
            Author = author ?? new Author();
            
            Title = author == null ? "Добавить автора" : "Редактировать автора";
            Width = 500;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            for (int i = 0; i < 7; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Имя
            grid.Children.Add(CreateLabel("Имя:", row));
            firstNameTextBox = CreateTextBox(Author.FirstName);
            Grid.SetRow(firstNameTextBox, row++);

            // Фамилия
            grid.Children.Add(CreateLabel("Фамилия*:", row));
            lastNameTextBox = CreateTextBox(Author.LastName);
            Grid.SetRow(lastNameTextBox, row++);

            // Страна
            grid.Children.Add(CreateLabel("Страна:", row));
            countryTextBox = CreateTextBox(Author.Country);
            Grid.SetRow(countryTextBox, row++);

            // Дата рождения
            grid.Children.Add(CreateLabel("Дата рождения:", row));
            birthDatePicker = new DatePicker { Margin = new Thickness(0, 5, 0, 15) };
            if (Author.BirthDate.HasValue)
                birthDatePicker.SelectedDate = Author.BirthDate.Value;
            Grid.SetRow(birthDatePicker, row++);
            grid.Children.Add(birthDatePicker);

            // Биография
            grid.Children.Add(CreateLabel("Биография:", row));
            biographyTextBox = new TextBox 
            { 
                Text = Author.Biography, 
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100,
                Margin = new Thickness(0, 5, 0, 15)
            };
            Grid.SetRow(biographyTextBox, row++);
            grid.Children.Add(biographyTextBox);

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
            if (string.IsNullOrWhiteSpace(lastNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите фамилию автора.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Author.FirstName = firstNameTextBox.Text.Trim();
            Author.LastName = lastNameTextBox.Text.Trim();
            Author.Country = countryTextBox.Text.Trim();
            Author.BirthDate = birthDatePicker.SelectedDate;
            Author.Biography = biographyTextBox.Text.Trim();

            DialogResult = true;
        }
    }
}
