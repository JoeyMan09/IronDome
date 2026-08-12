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

namespace IronDomeInterceptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InterceptorClient interceptorClient;
        public MainWindow()
        {
            InitializeComponent();
            interceptorClient = new InterceptorClient();
        }
        private async void btnConnectToCommand_Click(object sender, RoutedEventArgs e)
        {
            await interceptorClient.ConnectToCommandAsync();

            txtConnectionStatus.Text = "Command Center: Connected";

            InterceptCommand? command =
                await interceptorClient.InterceptorClientReadAsync();

            if (command == null)
                return;

            txtCurrentCommand.Text = txtCurrentCommand.Text +=
    $"Command ID: {command.CommandId}\n" +
    $"Target ID: {command.TargetId}\n" +
    $"Target X: {command.TargetX:F0}\n" +
    $"Target Y: {command.TargetY:F0}\n" +
    $"Target Vx: {command.TargetVx:F0}\n" +
    $"Target Vy: {command.TargetVy:F0}\n" +
    $"--------------------\n";

        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}