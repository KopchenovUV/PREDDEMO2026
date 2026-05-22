// Data/AppData.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Zanovo.Models;

namespace Zanovo.Data
{
    public class AppData
    {
        private static Moloko_ISEntities2 _context;

        public static Moloko_ISEntities2 GetContext()
        {
            if (_context == null)
                _context = new Moloko_ISEntities2();
            return _context;
        }

        // Проверка пользователя при авторизации
        public static Users Authenticate(string login, string password)
        {
            var context = GetContext();
            // Если пароли НЕ хэшированные (хранятся как есть)
            var user = context.Users.FirstOrDefault(u => u.Login == login && u.Password_Hash == password);
            return user;
        }

        // Проверка блокировки
        public static bool IsUserBlocked(string login)
        {
            var context = GetContext();
            var user = context.Users.FirstOrDefault(u => u.Login == login);
            return user != null && user.Is_Blocked == true; // сравниваем с true
        }

        // Увеличение счётчика неверных попыток
        public static void IncrementBlockAttempts(string login)
        {
            var context = GetContext();
            var user = context.Users.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                // Если Failed_Attempts - int? (Nullable<int>)
                user.Failed_Attempts = user.Failed_Attempts + 1;
                if (user.Failed_Attempts >= 3)
                {
                    user.Is_Blocked = true;
                }
                context.SaveChanges();
            }
        }

        // Сброс счётчика после успешного входа
        public static void ResetBlockAttempts(string login)
        {
            var context = GetContext();
            var user = context.Users.FirstOrDefault(u => u.Login == login);
            if (user != null)
            {
                user.Failed_Attempts = 0;
                context.SaveChanges();
            }
        }

        // Добавление нового пользователя (только для администратора)
        public static bool AddUser(string login, string password, int roleId)
        {
            var context = GetContext();

            // Проверка на существование логина
            if (context.Users.Any(u => u.Login == login))
                return false;

            // Проверка существования роли
            var roleExists = context.Roles.Any(r => r.ID_Role == roleId);
            if (!roleExists)
                return false;

            var newUser = new Users
            {
                Login = login,
                Password_Hash = password, // здесь можно позже добавить ComputeHash(password)
                ID_Role = roleId,
                Is_Blocked = false,
                Failed_Attempts = 0
            };
            context.Users.Add(newUser);
            context.SaveChanges();
            return true;
        }

        // Обновление данных пользователя (админ)
        public static bool UpdateUser(int userId, string login, string password, int? roleId, bool? isBlocked)
        {
            var context = GetContext();
            var user = context.Users.Find(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(login))
                user.Login = login;

            if (!string.IsNullOrEmpty(password))
                user.Password_Hash = password;

            if (roleId.HasValue && roleId.Value > 0)
                user.ID_Role = roleId.Value;

            if (isBlocked.HasValue)
                user.Is_Blocked = isBlocked.Value;

            context.SaveChanges();
            return true;
        }

        // Получение всех пользователей с их ролями (для отображения в DataGrid)
        public static IQueryable<Users> GetAllUsers()
        {
            return GetContext().Users;
        }

        // Получение всех ролей (для выпадающего списка)
        public static IQueryable<Roles> GetAllRoles()
        {
            return GetContext().Roles;
        }

        // Хэширование (опционально, для production)
        private static string ComputeHash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes);
            }
        }

        // Добавьте этот метод в класс AppData

        // Получение названия роли по ID
        public static string GetUserRole(int? roleId)
        {
            if (roleId == null) return "user";
            var role = GetContext().Roles.FirstOrDefault(r => r.ID_Role == roleId);
            return role?.Name ?? "user";
        }
    }
}