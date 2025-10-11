using ChatbotGui.Chat.ViewModels;
using ChatbotGui.Chat.Views;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Wpf.Ui;

namespace ChatbotGui.Chat
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(static c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); })
            .ConfigureServices((context, services) =>
            {
                // Сервисы
                services.AddSingleton<IDataSavingService, DataSavingService>();
                services.AddSingleton<IChatService, ChatService>();
                services.AddSingleton<IAnswerFinderService, AnswerFinderService>();
                services.AddSingleton<IMessenger, WeakReferenceMessenger>();

                // ViewModels
                services.AddSingleton<ChatViewModel>();
                services.AddTransient<ChatMessageViewModel>();

                // Страницы
                services.AddSingleton<ChatView>();

                // Окна
                services.AddSingleton<MainWindow>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services
        {
            get { return _host.Services; }
        }

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            await _host.StartAsync();
            Services.GetRequiredService<MainWindow>().Show();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var choice = MessageBox.Show(MainWindow,
                $"Необработанное исключение:\n" +
                $"{e.Exception.Message}\n" +
                $"Напишите Issue на GitHub (https://github.com/AvengerAnubis/DostavkaTovarov/issues)\n" +
                $"Программа может работать некорректно, возможно необратимое повреждение данных!\n" +
                $"Продолжить выполнение?",
                "Необработанное исключение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (choice == MessageBoxResult.Yes)
                e.Handled = true;
            else
                e.Handled = false;
        }
    }
}
