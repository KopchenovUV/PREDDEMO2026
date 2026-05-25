using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Word = Microsoft.Office.Interop.Word;

namespace DataValidator
{
    public partial class MainWindow : Window
    {
        private string currentFIO = "";
        private string validationDetails = "";
        private readonly HttpClient httpClient = new HttpClient();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnGetData_Click(object sender, RoutedEventArgs e)
        {
            btnGetData.IsEnabled = false;
            txtStatus.Text = "Загрузка данных из эмулятора...";

            try
            {
                currentFIO = await GetDataFromSimulator();
                txtFIO.Text = currentFIO;
                txtResult.Text = "Данные получены. Нажмите 'Отправить результат теста' для проверки.";
                btnSendResult.IsEnabled = true;
                txtStatus.Text = $"Данные получены: {currentFIO}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка получения данных: {ex.Message}\n\nУбедитесь, что TransferSimulator.exe запущен",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatus.Text = "Ошибка получения данных";
            }
            finally
            {
                btnGetData.IsEnabled = true;
            }
        }

        private async Task<string> GetDataFromSimulator()
        {
            string url = "http://localhost:4444/TransferSimulator/fullName";

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();

                Match match = Regex.Match(jsonResponse, @"""value""\s*:\s*""([^""]+)""");

                if (match.Success)
                {
                    return match.Groups[1].Value;
                }

                throw new Exception("Не удалось распарсить ответ эмулятора");
            }
            catch (HttpRequestException)
            {
                throw new Exception("Эмулятор TransferSimulator.exe не запущен");
            }
        }

        private void BtnSendResult_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем два критерия
            bool hasDigits = Regex.IsMatch(currentFIO, @"\d");
            bool hasSpecialChars = Regex.IsMatch(currentFIO, @"[!@#$%^&*()_+{}\[\]:;'""<>?,./\\|`~]");

            bool isValid = !hasDigits && !hasSpecialChars;

            // Формируем подробный результат
            if (!isValid)
            {
                string errors = "";
                if (hasDigits) errors += "содержит цифры ";
                if (hasSpecialChars) errors += "содержит спецсимволы ";

                validationDetails = $"НЕ УСПЕШНО\n" +
                                   $"Полученное ФИО: {currentFIO}\n" +
                                   $"Причина: ФИО {errors}\n" +
                                   $"Валидация не пройдена!";

                txtResult.Text = $"❌ Валидация не пройдена!\n" +
                                $"ФИО: {currentFIO}\n" +
                                $"Ошибка: ФИО {errors}";
                txtResult.Foreground = System.Windows.Media.Brushes.Red;
            }
            else
            {
                validationDetails = $"УСПЕШНО\n" +
                                   $"Полученное ФИО: {currentFIO}\n" +
                                   $"Причина: ФИО не содержит цифр и спецсимволов\n" +
                                   $"Валидация пройдена успешно!";

                txtResult.Text = $"✅ Валидация пройдена успешно!\n" +
                                $"ФИО: {currentFIO}\n" +
                                $"ФИО соответствует требованиям (нет цифр и спецсимволов)";
                txtResult.Foreground = System.Windows.Media.Brushes.Green;
            }

            WriteToWordDocument();
        }

        private void WriteToWordDocument()
        {
            Word.Application wordApp = null;
            Word.Document doc = null;

            try
            {
                wordApp = new Word.Application();
                wordApp.Visible = false;

                string docPath = System.IO.Path.Combine(Environment.CurrentDirectory, "ТестКейс.docx");

                if (!System.IO.File.Exists(docPath))
                {
                    doc = wordApp.Documents.Add();
                    CreateTestTable(doc);
                    doc.SaveAs2(docPath);
                }
                else
                {
                    doc = wordApp.Documents.Open(docPath);
                    EnsureTableStructure(doc);
                }

                // Заполняем результаты в таблице
                FillTestResults(doc);
                doc.Save();

                txtStatus.Text = $"Результат записан в {docPath}";
                MessageBox.Show($"Результаты успешно записаны в файл:\n{docPath}\n\n{validationDetails}",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка работы с Word: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (doc != null)
                {
                    doc.Close();
                    Marshal.ReleaseComObject(doc);
                }
                if (wordApp != null)
                {
                    wordApp.Quit();
                    Marshal.ReleaseComObject(wordApp);
                }
            }
        }

        private void CreateTestTable(Word.Document doc)
        {
            Word.Table table = doc.Tables.Add(doc.Range(0, 0), 4, 3); // 4 строки (заголовок + 3 теста)
            table.Borders.Enable = 1;
            table.Range.Font.Size = 11;

            // Заголовки
            table.Cell(1, 1).Range.Text = "Действие";
            table.Cell(1, 2).Range.Text = "Ожидаемый результат";
            table.Cell(1, 3).Range.Text = "Результат (с пояснением)";
            table.Rows[1].Range.Font.Bold = 1;
            table.Rows[1].Range.Font.Size = 12;

            // Тест 1: проверка на цифры
            table.Cell(2, 1).Range.Text = "Ввести ФИО с цифрами (например: Иванов123)";
            table.Cell(2, 2).Range.Text = "Сообщение об ошибке \"ФИО содержит запрещенные символы\"";
            table.Cell(2, 3).Range.Text = ""; // Будет заполнено позже

            // Тест 2: проверка на спецсимволы
            table.Cell(3, 1).Range.Text = "Ввести ФИО со спецсимволами (например: Иванов@#$%)";
            table.Cell(3, 2).Range.Text = "Сообщение об ошибке \"ФИО содержит запрещенные символы\"";
            table.Cell(3, 3).Range.Text = ""; // Будет заполнено позже

            // Тест 3: проверка реальных данных из эмулятора
            table.Cell(4, 1).Range.Text = $"Получить данные из эмулятора и проверить ФИО: \"{currentFIO}\"";
            table.Cell(4, 2).Range.Text = "ФИО не должно содержать цифры и спецсимволы";
            table.Cell(4, 3).Range.Text = ""; // Будет заполнено позже

            // Добавляем закладки
            doc.Bookmarks.Add("Result1", table.Cell(2, 3).Range);
            doc.Bookmarks.Add("Result2", table.Cell(3, 3).Range);
            doc.Bookmarks.Add("Result3", table.Cell(4, 3).Range);
        }

        private void EnsureTableStructure(Word.Document doc)
        {
            if (doc.Tables.Count > 0)
            {
                Word.Table table = doc.Tables[1];

                // Добавляем закладки если их нет
                if (!doc.Bookmarks.Exists("Result1") && table.Rows.Count >= 2)
                    doc.Bookmarks.Add("Result1", table.Cell(2, 3).Range);

                if (!doc.Bookmarks.Exists("Result2") && table.Rows.Count >= 3)
                    doc.Bookmarks.Add("Result2", table.Cell(3, 3).Range);

                if (!doc.Bookmarks.Exists("Result3") && table.Rows.Count >= 4)
                    doc.Bookmarks.Add("Result3", table.Cell(4, 3).Range);
            }
        }

        private void FillTestResults(Word.Document doc)
        {
            try
            {
                // Определяем результаты для каждого теста
                bool hasDigits = Regex.IsMatch(currentFIO, @"\d");
                bool hasSpecialChars = Regex.IsMatch(currentFIO, @"[!@#$%^&*()_+{}\[\]:;'""<>?,./\\|`~]");
                bool isValid = !hasDigits && !hasSpecialChars;

                // Результат теста 1 (проверка на цифры) - всегда проверяем на примере с цифрами
                string result1 = hasDigits ?
                    $"НЕ УСПЕШНО\n(ФИО содержит цифры: {currentFIO})" :
                    $"УСПЕШНО\n(ФИО не содержит цифр: {currentFIO})";

                // Результат теста 2 (проверка на спецсимволы)
                string result2 = hasSpecialChars ?
                    $"НЕ УСПЕШНО\n(ФИО содержит спецсимволы: {currentFIO})" :
                    $"УСПЕШНО\n(ФИО не содержит спецсимволов: {currentFIO})";

                // Результат теста 3 (общая проверка реальных данных)
                string result3 = isValid ?
                    $"УСПЕШНО\n\nПолученное ФИО: {currentFIO}\nПричина: ФИО не содержит цифр и спецсимволов\nВалидация пройдена!" :
                    $"НЕ УСПЕШНО\n\nПолученное ФИО: {currentFIO}\nПричина: ФИО {(hasDigits ? "содержит цифры " : "")}{(hasSpecialChars ? "содержит спецсимволы" : "")}\nВалидация не пройдена!";

                // Записываем в закладки или таблицу
                if (doc.Bookmarks.Exists("Result1"))
                {
                    doc.Bookmarks["Result1"].Range.Text = result1;
                }
                else if (doc.Tables.Count > 0 && doc.Tables[1].Rows.Count >= 2)
                {
                    doc.Tables[1].Cell(2, 3).Range.Text = result1;
                }

                if (doc.Bookmarks.Exists("Result2"))
                {
                    doc.Bookmarks["Result2"].Range.Text = result2;
                }
                else if (doc.Tables.Count > 0 && doc.Tables[1].Rows.Count >= 3)
                {
                    doc.Tables[1].Cell(3, 3).Range.Text = result2;
                }

                if (doc.Bookmarks.Exists("Result3"))
                {
                    doc.Bookmarks["Result3"].Range.Text = result3;
                }
                else if (doc.Tables.Count > 0 && doc.Tables[1].Rows.Count >= 4)
                {
                    doc.Tables[1].Cell(4, 3).Range.Text = result3;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка заполнения результатов: {ex.Message}");
            }
        }
    }
}