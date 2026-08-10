using System.Runtime.Serialization.Formatters;
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
            txtStats.Text = "Connected";
            
            await ListenToRadarAsync();
        }
        private async Task ListenToRadarAsync()
        {
            while (true)
            {
                TargetData? target =
                    await commandClient.CommandClientReadAsync();

                if (target == null)
                {
                    break;
                }
                txtStats2.Text +=
                $"Name: {target.Value.Name}\n" +
                $"X: {target.Value.X:F0}\n" +
                $"Y: {target.Value.Y:F0}\n" +
                $"Vx: {target.Value.Vx:F0}\n" +
                $"Vy: {target.Value.Vy:F0}\n" +
                $"--------------------\n";
            }
        }
    }
}