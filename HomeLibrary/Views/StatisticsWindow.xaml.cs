using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HomeLibrary.Data;

namespace HomeLibrary.Views
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow()
        {
            InitializeComponent();
            Loaded += StatisticsWindow_Loaded;
        }

        private void StatisticsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
        }

        private void LoadStatistics()
        {
            try
            {
                using (var db = new DatabaseContext())
                {
                    // Загрузка данных
                    var books = db.GetAllBooks();
                    var authors = db.GetAllAuthors();
                    var genres = db.GetAllGenres();
                    var reviews = db.GetAllReviews();
                    
                    var genreStats = db.GetGenreStatistics();
                    var ratingStats = db.GetRatingStatistics();
                    var readingStats = db.GetReadingStatisticsByYear();

                    // Диаграмма по жанрам
                    var genreData = genreStats.Where(s => s.Value > 0)
                        .Select(s => new KeyValuePair<string, int>(s.Key, s.Value))
                        .ToList();
                    GenrePieSeries.ItemsSource = genreData;

                    // Диаграмма по рейтингам
                    var ratingData = ratingStats
                        .Select(s => new KeyValuePair<string, int>($"{s.Key} ★", s.Value))
                        .ToList();
                    RatingColumnSeries.ItemsSource = ratingData;

                    // Диаграмма статистики чтения
                    var readingData = readingStats
                        .Select(s => new KeyValuePair<string, int>(s.Key.ToString(), s.Value))
                        .ToList();
                    ReadingLineSeries.ItemsSource = readingData;

                    // Общая статистика
                    TotalBooksText.Text = books.Count.ToString();
                    TotalAuthorsText.Text = authors.Count.ToString();
                    TotalGenresText.Text = genres.Count.ToString();
                    TotalReviewsText.Text = reviews.Count.ToString();
                    
                    int readBooks = reviews.Count(r => r.IsRead);
                    ReadBooksText.Text = readBooks.ToString();

                    if (reviews.Count > 0)
                    {
                        double avgRating = reviews.Average(r => r.Rating);
                        AverageRatingText.Text = $"{avgRating:F2} / 5.0";
                    }
                    else
                    {
                        AverageRatingText.Text = "Нет данных";
                    }

                    // Топ-5 жанров
                    TopGenresPanel.Children.Clear();
                    var topGenres = genreStats.OrderByDescending(s => s.Value).Take(5);
                    int rank = 1;
                    foreach (var genre in topGenres)
                    {
                        var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 5) };
                        
                        var rankText = new TextBlock 
                        { 
                            Text = $"{rank}.", 
                            FontWeight = FontWeights.Bold,
                            Width = 30,
                            FontSize = 14
                        };
                        
                        var nameText = new TextBlock 
                        { 
                            Text = genre.Key,
                            Width = 300,
                            FontSize = 14
                        };
                        
                        var countText = new TextBlock 
                        { 
                            Text = $"{genre.Value} книг(и)",
                            FontWeight = FontWeights.SemiBold,
                            Foreground = System.Windows.Media.Brushes.DarkGreen,
                            FontSize = 14
                        };

                        panel.Children.Add(rankText);
                        panel.Children.Add(nameText);
                        panel.Children.Add(countText);
                        TopGenresPanel.Children.Add(panel);
                        rank++;
                    }

                    if (topGenres.Count() == 0)
                    {
                        TopGenresPanel.Children.Add(new TextBlock { Text = "Нет данных", FontStyle = FontStyles.Italic, Foreground = System.Windows.Media.Brushes.Gray });
                    }

                    // Последние книги
                    RecentBooksPanel.Children.Clear();
                    var recentBooks = books.OrderByDescending(b => b.AddedDate).Take(5);
                    foreach (var book in recentBooks)
                    {
                        var panel = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
                        
                        var titleText = new TextBlock 
                        { 
                            Text = book.Title,
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 14
                        };
                        
                        var detailsText = new TextBlock 
                        { 
                            Text = $"Автор: {book.AuthorName ?? "неизвестен"} | Жанр: {book.GenreName ?? "не указан"} | Добавлено: {book.AddedDate:dd.MM.yyyy}",
                            FontSize = 12,
                            Foreground = System.Windows.Media.Brushes.Gray
                        };

                        panel.Children.Add(titleText);
                        panel.Children.Add(detailsText);
                        RecentBooksPanel.Children.Add(panel);
                        
                        if (book != recentBooks.Last())
                        {
                            RecentBooksPanel.Children.Add(new Separator { Margin = new Thickness(0, 5, 0, 5) });
                        }
                    }

                    if (recentBooks.Count() == 0)
                    {
                        RecentBooksPanel.Children.Add(new TextBlock { Text = "Нет данных", FontStyle = FontStyles.Italic, Foreground = System.Windows.Media.Brushes.Gray });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки статистики: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshReadingStatsButton_Click(object sender, RoutedEventArgs e)
        {
            LoadStatistics();
            MessageBox.Show("Статистика обновлена!", "Информация", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
