using System.Configuration;
using System.Data;
using System.Windows;
using ChatbotGui.QANEditor.Classes.Messages;
using ChatbotGui.QANEditor.ViewModels;
using ChatbotGui.QANEditor.Views;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ChatbotGui.QANEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? AppHost { get; private set; }

        public App()
        {
            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // Сервисы
                    services.AddSingleton<IDataSavingService, DataSavingService>();
                    services.AddSingleton<IChatService, ChatService>();
                    services.AddSingleton<IAnswerFinderService, AnswerFinderService>();
                    services.AddSingleton<IMessenger, WeakReferenceMessenger>();

                    // ViewModels
                    services.AddSingleton<QuestionAnswerHierarchyViewModel>();
                    services.AddSingleton<NodeEditorViewModel>();
                    services.AddTransient<QuestionAnswerNodeViewModel>();

                    // Страницы
                    services.AddSingleton<NodeEditorView>();

                    // Окна
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            Current.DispatcherUnhandledException += OnExceptionOccurs;
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            base.OnStartup(e);
        }

        private void OnExceptionOccurs(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
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

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            AppHost.Dispose();
            Current.DispatcherUnhandledException -= OnExceptionOccurs;
            base.OnExit(e);
        }
    }

}
