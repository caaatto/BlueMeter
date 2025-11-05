using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BlueMeter.WPF.Views
{
    /// <summary>
    /// MessageView.xaml 的交互逻辑
    /// </summary>
    public partial class MessageView : Window
    {
        public MessageView()
        {
            InitializeComponent();
        }

        public MessageView(string title, string content) : this()
        {
            DataContext = new ViewModels.MessageViewModel
            {
                Title = title,
                Content = content
            };
        }

    private void Footer_ConfirmClick(object sender, RoutedEventArgs e)
    {
        try { DialogResult = true; } catch { /* Ignored if not modal */ }
        Close();
    }

    private void Footer_CancelClick(object sender, RoutedEventArgs e)
    {
        try { DialogResult = false; } catch { /* Ignored if not modal */ }
        Close();
    }
    }
}
