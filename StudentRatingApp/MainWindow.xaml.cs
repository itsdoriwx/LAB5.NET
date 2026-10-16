using Microsoft.Win32;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StudentRatingApp
{
    public partial class MainWindow : Window
    {
        private readonly StudentManager _manager;

        private string _activeGroupFilter = string.Empty;
        private SortMode _currentSortMode = SortMode.None;

        private enum SortMode  // можливі стани сортування списку
        {
            None, ByRating, ByName
        }

        public MainWindow()
        {
            InitializeComponent();

            var fileReader = new JsonFileReader();
            var fileSaver = new JsonFileWriter();
            _manager = new StudentManager(fileReader, fileSaver);
        }

        private void ApplyViewSettings()
        {
            List<Student> displayList;

            if (!string.IsNullOrWhiteSpace(_activeGroupFilter))
            {
                displayList = _manager.FilterByGroup(_activeGroupFilter);
                TxtActiveFilterInfo.Text = $"Група: {_activeGroupFilter}";
                BorderActiveFilter.Visibility = Visibility.Visible;
            }
            else
            {
                displayList = _manager.GetAllStudents();
                BorderActiveFilter.Visibility = Visibility.Collapsed;
            }

            if (_currentSortMode == SortMode.ByRating)
            {
                displayList.Sort();
                TxtActiveSortInfo.Text = "Сортування за рейтингом";
                BorderActiveSort.Visibility = Visibility.Visible;
            }
            else if (_currentSortMode == SortMode.ByName)
            {
                displayList.Sort(new StudentNameComparer());
                TxtActiveSortInfo.Text = "Сортування за алфавітом";
                BorderActiveSort.Visibility = Visibility.Visible;
            }
            else
            {
                BorderActiveSort.Visibility = Visibility.Collapsed;
            }

            LstStudents.ItemsSource = null;
            LstStudents.ItemsSource = displayList;
        }

        private void ClearInputFields()
        {
            TxtFullName.Clear();
            TxtCourse.Clear();
            TxtGroup.Clear();
            TxtRating.Clear();
            TxtFullName.Focus();
        }

        private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = TxtFullName.Text.Trim();
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("ПІБ студента має бути вказаним.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtFullName.Focus();
                    return;
                }

                if (!int.TryParse(TxtCourse.Text.Trim(), out int course))
                {
                    MessageBox.Show("Курс має бути цілим значенням від 1 до 4.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtCourse.Focus();
                    return;
                }

                string group = TxtGroup.Text.Trim();
                if (string.IsNullOrWhiteSpace(group))
                {
                    MessageBox.Show("Назва групи має бути вказаною.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtGroup.Focus();
                    return;
                }

                string ratingInput = TxtRating.Text.Trim().Replace(',', '.'); // заміна коми на крапку
                if (!double.TryParse(ratingInput, NumberStyles.Any, CultureInfo.InvariantCulture, out double ratingScore))
                {
                    MessageBox.Show("Рейтинговий бал має бути від 60 до 100 балів.", "Помилка введення", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtRating.Focus();
                    return;
                }

                var student = new Student(fullName, course, group, ratingScore);
                _manager.AddStudent(student);

                ApplyViewSettings(); // оновлюємо відображення таблиці
                ClearInputFields();
            }
            catch (ArgumentException ex)
            {
                string cleanMessage = ex.Message.Split(new[] { " (Parameter" }, StringSplitOptions.None)[0];
                MessageBox.Show(cleanMessage, "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Помилка додавання", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnSortByRating_Click(object sender, RoutedEventArgs e)
        {
            if (_manager.GetAllStudents().Count == 0)
            {
                MessageBox.Show("Список студентів порожній. Немає даних для сортування.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentSortMode = SortMode.ByRating; // встановлюємо режим сортування за рейтингом
            ApplyViewSettings(); // застосовуємо налаштування відображення
        }

        private void BtnSortByName_Click(object sender, RoutedEventArgs e)
        {
            if (_manager.GetAllStudents().Count == 0)
            {
                MessageBox.Show("Список студентів порожній. Немає даних для сортування.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _currentSortMode = SortMode.ByName;
            ApplyViewSettings();
        }

        private void BtnClearSortBadge_Click(object sender, RoutedEventArgs e)
        {
            _currentSortMode = SortMode.None;
            ApplyViewSettings();
        }

        private void BtnFilter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_manager.GetAllStudents().Count == 0)
                {
                    throw new InvalidOperationException("Список студентів порожній. Неможливо виконати фільтрацію.");
                }

                string groupQuery = TxtFilterGroup.Text.Trim();
                if (string.IsNullOrWhiteSpace(groupQuery))
                {
                    MessageBox.Show("Введіть назву групи для фільтрації.", "Попередження", MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtFilterGroup.Focus();
                    return;
                }

                var matches = _manager.FilterByGroup(groupQuery);
                if (matches.Count == 0)
                {
                    throw new InvalidOperationException($"Студентів групи «{groupQuery}» не знайдено.");
                }

                _activeGroupFilter = groupQuery; // встановлюємо активний фільтр групи
                ApplyViewSettings();
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Результати пошуку", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
       
        private void BtnResetFilter_Click(object sender, RoutedEventArgs e) 
        {
            TxtFilterGroup.Clear();
            _activeGroupFilter = string.Empty;
            ApplyViewSettings();
        }

        private void BtnClearFilterBadge_Click(object sender, RoutedEventArgs e)
        {
            TxtFilterGroup.Clear();
            _activeGroupFilter = string.Empty;
            ApplyViewSettings();
        }

        private void BtnRemoveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (LstStudents.SelectedItem is Student selectedStudent) // перевірка, чи обрано студента зі списку
            {
                bool confirmed = ShowConfirmDialog($"Ви дійсно бажаєте видалити студента:\n{selectedStudent.FullName}?", "Підтвердження видалення");

                if (confirmed)
                {
                    _manager.RemoveStudent(selectedStudent);
                    ApplyViewSettings();
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, оберіть студента зі списку для видалення.", "Повідомлення", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private bool ShowConfirmDialog(string message, string title)
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        private async void BtnSaveToFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_manager.GetAllStudents().Count == 0)
                {
                    throw new InvalidOperationException("Список студентів порожній. Немає даних для збереження.");
                }

                var saveDialog = new SaveFileDialog
                {
                    Title = "Збереження рейтингового списку",
                    Filter = "Файли JSON (*.json)|*.json|Усі файли (*.*)|*.*",
                    DefaultExt = ".json",
                    FileName = "students.json"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await _manager.SaveToFileAsync(saveDialog.FileName);
                    MessageBox.Show("Дані успішно збережено!", "Повідомлення", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося зберегти файл:\n{ex.Message}", "Помилка збереження", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            var openDialog = new OpenFileDialog
            {
                Title = "Вибір файлу для завантаження",
                Filter = "Файли JSON (*.json)|*.json|Усі файли (*.*)|*.*",
                DefaultExt = ".json"
            };

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    await _manager.LoadFromFileAsync(openDialog.FileName);
                    ApplyViewSettings();
                    MessageBox.Show("Дані успішно завантажено!", "Повідомлення", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося завантажити файл:\n{ex.Message}", "Помилка читання", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
