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
using IronDomeInterceptor.inteceptor;

namespace IronDomeInterceptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InterceptorClient interceptorClient;
        private List<FlyingEntity> flyingEntities;
        private List<Interceptor> interceptors;
        public MainWindow()
        {
            InitializeComponent();
            interceptorClient = new InterceptorClient();
            flyingEntities = new List<FlyingEntity>();
            interceptors = new List<Interceptor>();
        }
        private async void btnConnectToCommand_Click(object sender, RoutedEventArgs e)
        {
            await interceptorClient.ConnectToCommandAsync();

            txtConnectionStatus.Text = "Command Center: Connected";
            txtConnectionStatus.Foreground = Brushes.Green;

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

            flyingEntities.Add(target);
            interceptors.Add(interceptor);
        }

        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}