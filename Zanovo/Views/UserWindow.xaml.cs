using System.Linq;
using System.Windows;
using Zanovo.Data;
using Zanovo.Models;

namespace Zanovo.Views
{
    public partial class UserWindow : Window
    {
        private Users _currentUser;

        public UserWindow(Users user)
        {
            InitializeComponent();
            _currentUser = user;
            tbWelcome.Text = $"Здравствуйте, {user.Login}!\n\nВаша роль: {GetRoleName(user.ID_Role)}";
        }

        private string GetRoleName(int? roleId)
        {
            if (roleId == null) return "Неизвестно";
            var role = AppData.GetAllRoles().FirstOrDefault(r => r.ID_Role == roleId);
            return role?.Name ?? "Неизвестно";
        }

        // Кнопка выхода
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Выход",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // Закрываем текущее окно
                this.Close();

                // Открываем окно авторизации заново
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}