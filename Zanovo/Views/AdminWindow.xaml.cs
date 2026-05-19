using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Zanovo.Data;
using Zanovo.Models;

namespace Zanovo.Views
{
    public partial class AdminWindow : Window
    {
        private Users _currentAdmin;

        public AdminWindow(Users admin)
        {
            InitializeComponent();
            _currentAdmin = admin;
            LoadUsers();
        }

        private void LoadUsers()
        {
            // Получаем пользователей вместе с названиями ролей
            var usersWithRoles = from u in AppData.GetAllUsers()
                                 join r in AppData.GetAllRoles() on u.ID_Role equals r.ID_Role
                                 select new
                                 {
                                     u.ID_User,
                                     u.Login,
                                     RoleName = r.Name,
                                     u.Is_Blocked,
                                     u.Failed_Attempts,
                                     u.ID_Role
                                 };

            dgUsers.ItemsSource = usersWithRoles.ToList();
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddEditUserDialog();
            if (dialog.ShowDialog() == true)
            {
                bool result = AppData.AddUser(dialog.Login, dialog.Password, dialog.SelectedRoleId);
                if (!result)
                {
                    MessageBox.Show("Пользователь с таким логином уже существует или роль не найдена",
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Пользователь успешно добавлен", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadUsers();
                }
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            var user = AppData.GetAllUsers().FirstOrDefault(u => u.ID_User == userId);
            if (user != null)
            {
                var dialog = new AddEditUserDialog(user);
                if (dialog.ShowDialog() == true)
                {
                    bool result = AppData.UpdateUser(userId, dialog.Login, dialog.Password,
                                                    dialog.SelectedRoleId, dialog.IsBlocked);
                    if (result)
                    {
                        MessageBox.Show("Данные пользователя обновлены", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                }
            }
        }

        private void BtnUnblockUser_Click(object sender, RoutedEventArgs e)
        {
            int userId = (int)((Button)sender).Tag;

            var result = MessageBox.Show("Снять блокировку с пользователя?", "Подтверждение",
                                        MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                AppData.UpdateUser(userId, null, null, null, false);
                LoadUsers();
                MessageBox.Show("Блокировка снята", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        // Кнопка выхода
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из системы?", "Выход",
                                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
            }
        }
    }
}