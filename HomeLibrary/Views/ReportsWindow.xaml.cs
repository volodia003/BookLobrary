using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;
using HomeLibrary.Models;
using Microsoft.Win32;

namespace HomeLibrary.Views
{
    public partial class ReportsWindow : Window
    {
        private string currentReportContent = "";

        public ReportsWindow()
        {
            InitializeComponent();
        }

        private void LoanedBooksButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var activeLoans = db.GetActiveLoans();

                    ReportTitleText.Text = $"Отчет: Одолженные книги (всего: {activeLoans.Count})";
                    
                    if (activeLoans.Count == 0)
                    {
                        ReportTextBlock.Text = "В данный момент нет одолженных книг.";
                        ReportDataGrid.Visibility = Visibility.Collapsed;
                        ReportTextBlock.Visibility = Visibility.Visible;
                        currentReportContent = ReportTextBlock.Text;
                        return;
                    }

                    // Настройка DataGrid
                    ReportDataGrid.Columns.Clear();
                    ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Книга", Binding = new System.Windows.Data.Binding("BookTitle"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
                    ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Кому", Binding = new System.Windows.Data.Binding("BorrowerName"), Width = 150 });
                    ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Дата займа", Binding = new System.Windows.Data.Binding("LoanDate") { StringFormat = "dd.MM.yyyy" }, Width = 100 });
                    ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Срок возврата", Binding = new System.Windows.Data.Binding("DueDate") { StringFormat = "dd.MM.yyyy" }, Width = 110 });
                    ReportDataGrid.Columns.Add(new DataGridTextColumn { Header = "Просрочено", Binding = new System.Windows.Data.Binding("IsOverdue"), Width = 90 });

                    ReportDataGrid.ItemsSource = activeLoans;
                    ReportDataGrid.Visibility = Visibility.Visible;
                    ReportTextBlock.Visibility = Visibility.Collapsed;

                    // Подготовка текста для экспорта
                    var sb = new StringBuilder();
                    sb.AppendLine("ОТЧЕТ: ОДОЛЖЕННЫЕ КНИГИ");
                    sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine($"Всего одолженных книг: {activeLoans.Count}");
                    sb.AppendLine();
                    sb.AppendLine(new string('=', 100));
                    sb.AppendLine();

                    foreach (var loan in activeLoans)
                    {
                        sb.AppendLine($"Книга: {loan.BookTitle}");
                        sb.AppendLine($"Кому: {loan.BorrowerName}");
                        sb.AppendLine($"Дата займа: {loan.LoanDate:dd.MM.yyyy}");
                        sb.AppendLine($"Срок возврата: {(loan.DueDate.HasValue ? loan.DueDate.Value.ToString("dd.MM.yyyy") : "не указан")}");
                        sb.AppendLine($"Статус: {(loan.IsOverdue ? "ПРОСРОЧЕНО" : "В срок")}");
                        if (!string.IsNullOrEmpty(loan.Notes))
                            sb.AppendLine($"Заметки: {loan.Notes}");
                        sb.AppendLine(new string('-', 100));
                        sb.AppendLine();
                    }

                    currentReportContent = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadingByYearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    var stats = db.GetReadingStatisticsByYear();

                    ReportTitleText.Text = "Отчет: Статистика чтения по годам";
                    
                    if (stats.Count == 0)
                    {
                        ReportTextBlock.Text = "Нет данных о прочитанных книгах.";
                        ReportDataGrid.Visibility = Visibility.Collapsed;
                        ReportTextBlock.Visibility = Visibility.Visible;
                        currentReportContent = ReportTextBlock.Text;
                        return;
                    }

                    var sb = new StringBuilder();
                    sb.AppendLine("ОТЧЕТ: СТАТИСТИКА ЧТЕНИЯ ПО ГОДАМ");
                    sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine();

                    int totalBooks = 0;
                    foreach (var stat in stats.OrderBy(s => s.Key))
                    {
                        sb.AppendLine($"Год {stat.Key}: {stat.Value} книг(и)");
                        totalBooks += stat.Value;
                    }

                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine($"ВСЕГО ПРОЧИТАНО: {totalBooks} книг(и)");

                    ReportTextBlock.Text = sb.ToString();
                    ReportDataGrid.Visibility = Visibility.Collapsed;
                    ReportTextBlock.Visibility = Visibility.Visible;
                    currentReportContent = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReadingByMonthButton_Click(object sender, RoutedEventArgs e)
        {
            // Диалог выбора года
            var yearDialog = new YearSelectionDialog();
            if (yearDialog.ShowDialog() != true)
                return;

            int selectedYear = yearDialog.SelectedYear;

            try
            {
                using (var db = new DatabaseContext())
                {
                    var stats = db.GetReadingStatisticsByMonth(selectedYear);

                    ReportTitleText.Text = $"Отчет: Статистика чтения по месяцам ({selectedYear} год)";
                    
                    var sb = new StringBuilder();
                    sb.AppendLine($"ОТЧЕТ: СТАТИСТИКА ЧТЕНИЯ ПО МЕСЯЦАМ ({selectedYear} ГОД)");
                    sb.AppendLine($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}");
                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine();

                    string[] monthNames = { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", 
                        "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };

                    int totalBooks = 0;
                    for (int i = 1; i <= 12; i++)
                    {
                        string monthKey = i.ToString("00");
                        int count = stats.ContainsKey(monthKey) ? stats[monthKey] : 0;
                        sb.AppendLine($"{monthNames[i - 1]}: {count} книг(и)");
                        totalBooks += count;
                    }

                    sb.AppendLine();
                    sb.AppendLine(new string('=', 80));
                    sb.AppendLine($"ВСЕГО ЗА ГОД: {totalBooks} книг(и)");

                    ReportTextBlock.Text = sb.ToString();
                    ReportDataGrid.Visibility = Visibility.Collapsed;
                    ReportTextBlock.Visibility = Visibility.Visible;
                    currentReportContent = sb.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(currentReportContent))
            {
                MessageBox.Show("Сначала сформируйте отчет.", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                DefaultExt = "txt",
                FileName = $"Отчет_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, currentReportContent, Encoding.UTF8);
                    MessageBox.Show($"Отчет успешно сохранен в файл:\n{dialog.FileName}", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сохранения файла: {ex.Message}", "Ошибка", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class YearSelectionDialog : Window
    {
        public int SelectedYear { get; private set; }

        private ComboBox yearComboBox;

        public YearSelectionDialog()
        {
            Title = "Выбор года";
            Width = 300;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock 
            { 
                Text = "Выберите год:", 
                FontWeight = FontWeights.SemiBold, 
                Margin = new Thickness(0, 0, 0, 10) 
            };
            Grid.SetRow(label, 0);
            grid.Children.Add(label);

            yearComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 20) };
            int currentYear = DateTime.Now.Year;
            for (int year = currentYear; year >= currentYear - 10; year--)
            {
                yearComboBox.Items.Add(year);
            }
            yearComboBox.SelectedIndex = 0;
            Grid.SetRow(yearComboBox, 1);
            grid.Children.Add(yearComboBox);

            var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 80, Margin = new Thickness(0, 0, 10, 0) };
            okButton.Click += (s, e) => 
            { 
                SelectedYear = (int)yearComboBox.SelectedItem;
                DialogResult = true; 
            };
            var cancelButton = new Button { Content = "Отмена", Width = 80 };
            cancelButton.Click += (s, e) => DialogResult = false;
            buttonsPanel.Children.Add(okButton);
            buttonsPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonsPanel, 2);
            grid.Children.Add(buttonsPanel);

            Content = grid;
        }
    }
}
