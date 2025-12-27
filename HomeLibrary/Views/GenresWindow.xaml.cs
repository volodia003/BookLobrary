using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class GenresWindow : Window
    {
        private List<Genre> genres;

        public GenresWindow()
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
                    genres = db.GetAllGenres();
                }
                GenresDataGrid.ItemsSource = null;
                GenresDataGrid.ItemsSource = genres;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenresDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = GenresDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection && LoginWindow.CurrentUser.IsAdmin;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может добавлять жанры.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new GenreEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertGenre(dialog.Genre);
                    }
                    LoadData();
                    MessageBox.Show("Жанр успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления жанра: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может редактировать жанры.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = GenresDataGrid.SelectedItem as Genre;
            if (selected == null) return;

            var dialog = new GenreEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateGenre(dialog.Genre);
                    }
                    LoadData();
                    MessageBox.Show("Жанр успешно обновлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления жанра: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может удалять жанры.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = GenresDataGrid.SelectedItem as Genre;
            if (selected == null) return;

            var result = MessageBox.Show($"Вы действительно хотите удалить жанр '{selected.Name}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteGenre(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Жанр успешно удален!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления жанра: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class GenreEditDialog : Window
    {
        public Genre Genre { get; private set; }

        private TextBox nameTextBox;
        private TextBox descriptionTextBox;

        public GenreEditDialog(Genre genre = null)
        {
            Genre = genre ?? new Genre();
            
            Title = genre == null ? "Добавить жанр" : "Редактировать жанр";
            Width = 500;
            Height = 300;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            for (int i = 0; i < 4; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Название
            grid.Children.Add(CreateLabel("Название*:", row));
            nameTextBox = CreateTextBox(Genre.Name);
            Grid.SetRow(nameTextBox, row++);

            // Описание
            grid.Children.Add(CreateLabel("Описание:", row));
            descriptionTextBox = new TextBox 
            { 
                Text = Genre.Description, 
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
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите название жанра.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Genre.Name = nameTextBox.Text.Trim();
            Genre.Description = descriptionTextBox.Text.Trim();

            DialogResult = true;
        }
    }
}
