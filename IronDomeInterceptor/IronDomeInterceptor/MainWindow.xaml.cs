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

            FlyingEntity target = new FlyingEntity(
                command.TargetX,
                command.TargetY,
                $"Target-{command.TargetId}",
                command.TargetVx,
                command.TargetVy,
                1
            );

            FlyingEntity interceptorTarget = target;

            Interceptor interceptor = new Interceptor( 0,  0, "Interceptor-1", 600,0, 1,1000);

            interceptor.EngageTarget(target);

        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}