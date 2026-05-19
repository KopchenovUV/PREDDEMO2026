using System.Windows;

namespace Zanovo
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Устанавливаем, что приложение не завершается при закрытии главного окна
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Открываем окно авторизации
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
        }
    }
}