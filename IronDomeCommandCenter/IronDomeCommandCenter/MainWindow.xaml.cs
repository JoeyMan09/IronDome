using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IronDomeCommandCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CommandClient commandClient;
        public MainWindow()
        {
            InitializeComponent();

            commandClient = new CommandClient();
        }
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            await commandClient.ConnectToRadarAsync();

            string message = await commandClient.CommandClientReadAsync();

            MessageBox.Show(message);
        }
    }
}