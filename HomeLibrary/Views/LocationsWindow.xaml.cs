using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class LocationsWindow : Window
    {
        private List<Location> locations;

        public LocationsWindow()
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
                    locations = db.GetAllLocations();
                }
                LocationsDataGrid.ItemsSource = null;
                LocationsDataGrid.ItemsSource = locations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LocationsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = LocationsDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection && LoginWindow.CurrentUser.IsAdmin;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может добавлять места хранения.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new LocationEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertLocation(dialog.Location);
                    }
                    LoadData();
                    MessageBox.Show("Место хранения успешно добавлено!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления места хранения: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может редактировать места хранения.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = LocationsDataGrid.SelectedItem as Location;
            if (selected == null) return;

            var dialog = new LocationEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateLocation(dialog.Location);
                    }
                    LoadData();
                    MessageBox.Show("Место хранения успешно обновлено!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления места хранения: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!LoginWindow.CurrentUser.IsAdmin)
            {
                MessageBox.Show("Только администратор может удалять места хранения.", "Доступ запрещен", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selected = LocationsDataGrid.SelectedItem as Location;
            if (selected == null) return;

            var result = MessageBox.Show($"Вы действительно хотите удалить место хранения '{selected.Name}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteLocation(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Место хранения успешно удалено!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления места хранения: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class LocationEditDialog : Window
    {
        public Location Location { get; private set; }

        private TextBox nameTextBox;
        private TextBox roomTextBox;
        private TextBox shelfTextBox;
        private TextBox descriptionTextBox;

        public LocationEditDialog(Location location = null)
        {
            Location = location ?? new Location();
            
            Title = location == null ? "Добавить место хранения" : "Редактировать место хранения";
            Width = 500;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Название
            grid.Children.Add(CreateLabel("Название*:", row));
            nameTextBox = CreateTextBox(Location.Name);
            Grid.SetRow(nameTextBox, row++);

            // Комната
            grid.Children.Add(CreateLabel("Комната:", row));
            roomTextBox = CreateTextBox(Location.Room);
            Grid.SetRow(roomTextBox, row++);

            // Полка/Шкаф
            grid.Children.Add(CreateLabel("Полка/Шкаф:", row));
            shelfTextBox = CreateTextBox(Location.Shelf);
            Grid.SetRow(shelfTextBox, row++);

            // Описание
            grid.Children.Add(CreateLabel("Описание:", row));
            descriptionTextBox = new TextBox 
            { 
                Text = Location.Description, 
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
                MessageBox.Show("Пожалуйста, введите название места хранения.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Location.Name = nameTextBox.Text.Trim();
            Location.Room = roomTextBox.Text.Trim();
            Location.Shelf = shelfTextBox.Text.Trim();
            Location.Description = descriptionTextBox.Text.Trim();

            DialogResult = true;
        }
    }
}
