using System;
using System.Linq;
using System.Windows;
using Zanovo.Data;
using Zanovo.Models;

namespace Zanovo.Views
{
    public partial class AddEditUserDialog : Window
    {
        private Users _editingUser;
        private bool _isEditMode = false;

        public string Login => txtLogin.Text.Trim();
        public string Password => txtPassword.Password;
        public int SelectedRoleId => (int)cmbRole.SelectedValue;
        public bool IsBlocked => chkIsBlocked.IsChecked ?? false;

        public AddEditUserDialog(Users user = null)
        {
            InitializeComponent();
            LoadRoles();

            if (user != null)
            {
                _isEditMode = true;
                _editingUser = user;
                Title = "Редактирование пользователя";
                txtLogin.Text = user.Login;
                cmbRole.SelectedValue = user.ID_Role;
                chkIsBlocked.IsChecked = user.Is_Blocked;

                // Для редактирования: пароль не обязателен
                chkChangePassword.Visibility = Visibility.Visible;
                txtPassword.IsEnabled = false;
                chkChangePassword.Checked += (s, e) => { txtPassword.IsEnabled = true; txtPassword.Focus(); };
                chkChangePassword.Unchecked += (s, e) => { txtPassword.IsEnabled = false; txtPassword.Clear(); };
            }
            else
            {
                Title = "Добавление пользователя";
                panelBlocked.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadRoles()
        {
            var roles = AppData.GetAllRoles().ToList();
            cmbRole.ItemsSource = roles;
            if (roles.Any())
                cmbRole.SelectedIndex = 0;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrEmpty(Login))
            {
                MessageBox.Show("Введите логин", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_isEditMode && string.IsNullOrEmpty(Password))
            {
                MessageBox.Show("Введите пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbRole.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}