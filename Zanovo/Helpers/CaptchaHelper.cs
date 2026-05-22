using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Zanovo.Helpers
{
    public class CaptchaHelper
    {
        private List<int> _correctOrder = new List<int> { 1, 2, 3, 4 };
        private List<int> _clickedOrder = new List<int>();
        private List<Border> _fragments = new List<Border>();

        public bool IsCaptchaSolved { get; private set; } = false;

        // Создаёт сетку 2x2 с перемешанными фрагментами
        public Grid CreateCaptchaGrid(int fragmentSize = 100)
        {
            var grid = new Grid();
            grid.Width = fragmentSize * 2;
            grid.Height = fragmentSize * 2;

            // Создаём 2 строки и 2 колонки
            for (int i = 0; i < 2; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new System.Windows.GridLength(fragmentSize) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(fragmentSize) });
            }

            // Перемешанные номера фрагментов (1,2,3,4 в случайном порядке)
            var shuffled = _correctOrder.OrderBy(x => Guid.NewGuid()).ToList();

            int index = 0;
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 2; col++)
                {
                    int fragmentNumber = shuffled[index];

                    var border = new Border
                    {
                        Width = fragmentSize,
                        Height = fragmentSize,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new System.Windows.Thickness(1),
                        Margin = new System.Windows.Thickness(2),
                        Tag = fragmentNumber,
                        Cursor = System.Windows.Input.Cursors.Hand
                    };

                    // Загружаем картинку из ресурсов
                    var image = new Image
                    {
                        Source = LoadImage(fragmentNumber),
                        Stretch = Stretch.Fill
                    };
                    border.Child = image;

                    // Обработчик клика
                    border.MouseLeftButtonDown += (s, e) => OnFragmentClick(s as Border);

                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    grid.Children.Add(border);

                    _fragments.Add(border);
                    index++;
                }
            }

            return grid;
        }

        private BitmapImage LoadImage(int number)
        {
            var uri = new Uri($"C:/Users/kopch/OneDrive/Desktop/демоФ/2-3/Zanovo/Zanovo/Images/captcha/{number}.png");
            return new BitmapImage(uri);
        }

        private void OnFragmentClick(Border clickedBorder)
        {
            if (IsCaptchaSolved) return;

            int fragmentNumber = (int)clickedBorder.Tag;

            // Если этот фрагмент уже был кликнут в правильном порядке - игнорируем
            if (_clickedOrder.Contains(fragmentNumber))
                return;

            // Ожидаемый следующий номер
            int expectedNext = _clickedOrder.Count + 1;

            if (fragmentNumber == expectedNext)
            {
                // Правильный клик
                _clickedOrder.Add(fragmentNumber);
                clickedBorder.BorderBrush = Brushes.Green;
                clickedBorder.BorderThickness = new System.Windows.Thickness(3);

                // Проверяем, собран ли весь порядок
                if (_clickedOrder.Count == 4)
                {
                    IsCaptchaSolved = true;
                }
            }
            else
            {
                // Неправильный клик - сбрасываем прогресс
                ResetProgress();
            }
        }

        private void ResetProgress()
        {
            _clickedOrder.Clear();
            foreach (var border in _fragments)
            {
                border.BorderBrush = Brushes.Black;
                border.BorderThickness = new System.Windows.Thickness(1);
            }
        }

        // Сброс капчи (при ошибке авторизации)
        public void Reset()
        {
            ResetProgress();
            IsCaptchaSolved = false;
        }
    }
}