using System;
using System.Windows;
using Zanovo.Data;
using Zanovo.Helpers;

namespace Zanovo.Views
{
    public partial class LoginWindow : Window
    {
        private CaptchaHelper _captcha;
        private int _failedCaptchaCount = 0;
        private string _currentLogin = "";

        public LoginWindow()
        {
            InitializeComponent();
            LoadCaptcha();
        }

        private void LoadCaptcha()
        {
            _captcha = new CaptchaHelper();
            var captchaGrid = _captcha.CreateCaptchaGrid(fragmentSize: 100);

            CaptchaContainer.Children.Clear();
            CaptchaContainer.Children.Add(captchaGrid);
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password;

            tbError.Visibility = Visibility.Collapsed;

            // 1. Проверка на пустые поля
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Заполните логин и пароль");
                return;
            }

            _currentLogin = login;

            // 2. Проверка блокировки в БД
            if (AppData.IsUserBlocked(login))
            {
                ShowError("Вы заблокированы. Обратитесь к администратору");
                return;
            }

            // 3. Проверка капчи
            if (!_captcha.IsCaptchaSolved)
            {
                _failedCaptchaCount++;
                if (_failedCaptchaCount >= 3)
                {
                    AppData.IncrementBlockAttempts(login);
                    ShowError("3 неудачные попытки сборки пазла. Учётная запись заблокирована");
                    return;
                }
                ShowError($"Соберите пазл правильно (попытка {_failedCaptchaCount} из 3)");
                ResetAndReloadCaptcha();
                return;
            }

            // 4. Аутентификация
            var user = AppData.Authenticate(login, password);
            if (user == null)
            {
                AppData.IncrementBlockAttempts(login);
                ShowError("Вы ввели неверный логин или пароль");
                ResetAndReloadCaptcha();
                _failedCaptchaCount = 0;
                return;
            }

            // 5. Успешный вход
            AppData.ResetBlockAttempts(login);
            MessageBox.Show("Вы успешно авторизовались", "Успех",
                          MessageBoxButton.OK, MessageBoxImage.Information);

            // 6. Получаем название роли и открываем нужное окно
            string roleName = AppData.GetUserRole(user.ID_Role);

            if (roleName == "admin")
            {
                AdminWindow adminWindow = new AdminWindow(user);
                adminWindow.Show();
            }
            else
            {
                UserWindow userWindow = new UserWindow(user);
                userWindow.Show();
            }

            this.Close();
        }

        private void ResetAndReloadCaptcha()
        {
            _captcha.Reset();
            LoadCaptcha();
        }

        private void ShowError(string message)
        {
            tbError.Text = message;
            tbError.Visibility = Visibility.Visible;
        }
    }
}