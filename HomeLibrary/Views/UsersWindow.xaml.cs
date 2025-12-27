using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class UsersWindow : Window
    {
        private List<User> users;

        public UsersWindow()
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
                    users = db.GetAllUsers();
                }
                UsersDataGrid.ItemsSource = null;
                UsersDataGrid.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UsersDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = UsersDataGrid.SelectedItem != null;
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new UserEditDialog();
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertUser(dialog.User);
                    }
                    LoadData();
                    MessageBox.Show("Пользователь успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления пользователя: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersDataGrid.SelectedItem as User;
            if (selected == null) return;

            var dialog = new UserEditDialog(selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateUser(dialog.User);
                    }
                    LoadData();
                    MessageBox.Show("Пользователь успешно обновлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления пользователя: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = UsersDataGrid.SelectedItem as User;
            if (selected == null) return;

            if (selected.Id == LoginWindow.CurrentUser.Id)
            {
                MessageBox.Show("Вы не можете удалить свою учетную запись.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы действительно хотите удалить пользователя '{selected.FullName}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteUser(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Пользователь успешно удален!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class UserEditDialog : Window
    {
        public User User { get; private set; }

        private TextBox usernameTextBox;
        private PasswordBox passwordBox;
        private TextBox fullNameTextBox;
        private CheckBox isAdminCheckBox;

        public UserEditDialog(User user = null)
        {
            User = user ?? new User();
            
            Title = user == null ? "Добавить пользователя" : "Редактировать пользователя";
            Width = 450;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            for (int i = 0; i < 6; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Логин
            grid.Children.Add(CreateLabel("Логин*:", row));
            usernameTextBox = CreateTextBox(User.Username);
            Grid.SetRow(usernameTextBox, row++);

            // Пароль
            grid.Children.Add(CreateLabel("Пароль*:", row));
            passwordBox = new PasswordBox { Password = User.Password, Margin = new Thickness(0, 5, 0, 15) };
            Grid.SetRow(passwordBox, row++);
            grid.Children.Add(passwordBox);

            // ФИО
            grid.Children.Add(CreateLabel("ФИО*:", row));
            fullNameTextBox = CreateTextBox(User.FullName);
            Grid.SetRow(fullNameTextBox, row++);

            // Администратор
            isAdminCheckBox = new CheckBox 
            { 
                Content = "Права администратора", 
                IsChecked = User.IsAdmin, 
                Margin = new Thickness(0, 5, 0, 25) 
            };
            Grid.SetRow(isAdminCheckBox, row++);
            grid.Children.Add(isAdminCheckBox);

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
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите логин.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Пожалуйста, введите пароль.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(fullNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите ФИО.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User.Username = usernameTextBox.Text.Trim();
            User.Password = passwordBox.Password;
            User.FullName = fullNameTextBox.Text.Trim();
            User.IsAdmin = isAdminCheckBox.IsChecked ?? false;

            DialogResult = true;
        }
    }
}
