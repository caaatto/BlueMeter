using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using Microsoft.Extensions.DependencyInjection;
using StarResonanceDpsAnalysis.WPF.ViewModels;
using StarResonanceDpsAnalysis.WPF.Views;

namespace StarResonanceDpsAnalysis.WPF.Services;

public class MessageDialogService(IServiceProvider provider) : IMessageDialogService
{
    public bool? Show(string title, string content, Window? owner = null)
    {
        var view = provider.GetRequiredService<MessageView>();
        view.DataContext = new MessageViewModel { Title = title, Content = content };

        var hostOwner = owner;
        if (hostOwner == null)
        {
            var app = Application.Current;
            var active = app?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w != view);
            hostOwner = active ?? (app?.MainWindow != view ? app?.MainWindow : null);
        }

        if (hostOwner != null)
        {
            view.Owner = hostOwner;
            view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }
        else
        {
            view.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        return view.ShowDialog();
    }
}

public static class MessageDialogServiceExtensions
{
    public static IServiceCollection AddMessageDialogService(this IServiceCollection services)
    {
        services.AddTransient<MessageView>();
        services.AddSingleton<IMessageDialogService, MessageDialogService>();
        return services;
    }
}
