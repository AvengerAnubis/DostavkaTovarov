using System.Configuration;
using System.Data;
using System.Windows;
using ChatbotGui.QANEditor.Views;
using ChatbotLib.Interfaces;
using ChatbotLib.Services;
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

                    // ViewModels


                    // Страницы
                    services.AddSingleton<DataViewerView>();
                    services.AddKeyedTransient<NodeEditorView>();

                    // Окна
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await AppHost!.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await AppHost!.StopAsync();
            AppHost.Dispose();
            base.OnExit(e);
        }
    }

}
