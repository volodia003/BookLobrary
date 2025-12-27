using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;

namespace HomeLibrary.Views
{
    public partial class LoansWindow : Window
    {
        private List<Loan> loans;
        private List<Book> availableBooks;
        private List<User> users;

        public LoansWindow()
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
                    loans = db.GetActiveLoans();
                    availableBooks = db.GetAllBooks().Where(b => b.Status == "В наличии").ToList();
                    users = db.GetAllUsers();
                }
                LoansDataGrid.ItemsSource = null;
                LoansDataGrid.ItemsSource = loans;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAllLoansButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    loans = db.GetAllLoans();
                }
                LoansDataGrid.ItemsSource = null;
                LoansDataGrid.ItemsSource = loans;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowActiveLoansButton_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoansDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool hasSelection = LoansDataGrid.SelectedItem != null;
            var selected = LoansDataGrid.SelectedItem as Loan;
            
            EditButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            ReturnButton.IsEnabled = hasSelection && selected != null && !selected.IsReturned;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LoanEditDialog(availableBooks, users);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.InsertLoan(dialog.Loan);
                    }
                    LoadData();
                    MessageBox.Show("Займ успешно добавлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка добавления займа: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = LoansDataGrid.SelectedItem as Loan;
            if (selected == null || selected.IsReturned) return;

            var result = MessageBox.Show($"Отметить возврат книги '{selected.BookTitle}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    selected.ReturnDate = DateTime.Now;
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateLoan(selected);
                    }
                    LoadData();
                    MessageBox.Show("Возврат книги успешно отмечен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления займа: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = LoansDataGrid.SelectedItem as Loan;
            if (selected == null) return;

            // Для редактирования нужны все книги
            List<Book> allBooks;
            using (var db = new DatabaseContext())
            {
                allBooks = db.GetAllBooks();
            }

            var dialog = new LoanEditDialog(allBooks, users, selected);
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.UpdateLoan(dialog.Loan);
                    }
                    LoadData();
                    MessageBox.Show("Займ успешно обновлен!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления займа: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = LoansDataGrid.SelectedItem as Loan;
            if (selected == null) return;

            var result = MessageBox.Show($"Вы действительно хотите удалить займ книги '{selected.BookTitle}'?", 
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new DatabaseContext())
                    {
                        db.DeleteLoan(selected.Id);
                    }
                    LoadData();
                    MessageBox.Show("Займ успешно удален!", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления займа: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class LoanEditDialog : Window
    {
        public Loan Loan { get; private set; }

        private ComboBox bookComboBox;
        private ComboBox borrowerComboBox;
        private DatePicker loanDatePicker;
        private DatePicker dueDatePicker;
        private DatePicker returnDatePicker;
        private TextBox notesTextBox;

        public LoanEditDialog(List<Book> books, List<User> users, Loan loan = null)
        {
            Loan = loan ?? new Loan();
            
            Title = loan == null ? "Добавить займ" : "Редактировать займ";
            Width = 500;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            for (int i = 0; i < 8; i++)
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            int row = 0;

            // Книга
            grid.Children.Add(CreateLabel("Книга*:", row));
            bookComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15), DisplayMemberPath = "Title" };
            foreach (var book in books)
            {
                bookComboBox.Items.Add(book);
            }
            if (loan != null)
            {
                for (int i = 0; i < bookComboBox.Items.Count; i++)
                {
                    if ((bookComboBox.Items[i] as Book).Id == loan.BookId)
                    {
                        bookComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            Grid.SetRow(bookComboBox, row++);
            grid.Children.Add(bookComboBox);

            // Кому
            grid.Children.Add(CreateLabel("Кому*:", row));
            borrowerComboBox = new ComboBox { Margin = new Thickness(0, 5, 0, 15), DisplayMemberPath = "FullName" };
            foreach (var user in users)
            {
                borrowerComboBox.Items.Add(user);
            }
            if (loan != null)
            {
                for (int i = 0; i < borrowerComboBox.Items.Count; i++)
                {
                    if ((borrowerComboBox.Items[i] as User).Id == loan.BorrowerId)
                    {
                        borrowerComboBox.SelectedIndex = i;
                        break;
                    }
                }
            }
            Grid.SetRow(borrowerComboBox, row++);
            grid.Children.Add(borrowerComboBox);

            // Дата займа
            grid.Children.Add(CreateLabel("Дата займа*:", row));
            loanDatePicker = new DatePicker { Margin = new Thickness(0, 5, 0, 15), SelectedDate = loan.LoanDate };
            Grid.SetRow(loanDatePicker, row++);
            grid.Children.Add(loanDatePicker);

            // Срок возврата
            grid.Children.Add(CreateLabel("Срок возврата:", row));
            dueDatePicker = new DatePicker { Margin = new Thickness(0, 5, 0, 15), SelectedDate = loan.DueDate };
            Grid.SetRow(dueDatePicker, row++);
            grid.Children.Add(dueDatePicker);

            // Дата возврата
            grid.Children.Add(CreateLabel("Дата возврата:", row));
            returnDatePicker = new DatePicker { Margin = new Thickness(0, 5, 0, 15), SelectedDate = loan.ReturnDate };
            Grid.SetRow(returnDatePicker, row++);
            grid.Children.Add(returnDatePicker);

            // Заметки
            grid.Children.Add(CreateLabel("Заметки:", row));
            notesTextBox = new TextBox 
            { 
                Text = Loan.Notes, 
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 60,
                Margin = new Thickness(0, 5, 0, 15)
            };
            Grid.SetRow(notesTextBox, row++);
            grid.Children.Add(notesTextBox);

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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = bookComboBox.SelectedItem as Book;
            var selectedBorrower = borrowerComboBox.SelectedItem as User;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedBorrower == null)
            {
                MessageBox.Show("Пожалуйста, выберите пользователя.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!loanDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите дату займа.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Loan.BookId = selectedBook.Id;
            Loan.BorrowerId = selectedBorrower.Id;
            Loan.LoanDate = loanDatePicker.SelectedDate.Value;
            Loan.DueDate = dueDatePicker.SelectedDate;
            Loan.ReturnDate = returnDatePicker.SelectedDate;
            Loan.Notes = notesTextBox.Text.Trim();

            DialogResult = true;
        }
    }
}
