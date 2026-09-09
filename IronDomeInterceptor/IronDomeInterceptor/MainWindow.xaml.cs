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
using System.Windows.Threading;

namespace IronDomeInterceptor
{
    public partial class MainWindow : Window
    {
        private InterceptorClient interceptorClient;
        private List<FlyingEntity> flyingEntities;
        private List<Interceptor> interceptors;
        private DispatcherTimer simulationTimer;
        private const double dt = 0.05;
        private List<Point> targetTrail = new List<Point>();
        private List<Point> interceptorTrail = new List<Point>();
        public MainWindow()
        {
            InitializeComponent();
            interceptorClient = new InterceptorClient();
            flyingEntities = new List<FlyingEntity>();
            interceptors = new List<Interceptor>();
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(50);
            simulationTimer.Tick += SimulationTimer_Tick;

        }
        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            if (flyingEntities.Count == 0 || interceptors.Count == 0)
                return;

            FlyingEntity target = flyingEntities[0];
            Interceptor interceptor = interceptors[0];

            foreach (FlyingEntity entity in flyingEntities)
            {
                entity.UpdatePosition(dt);
            }

            foreach (Interceptor inter in interceptors)
            {
                inter.UpdatePosition(dt);
            }
            targetTrail.Add(new Point(
                target.getX(),
                target.getY()
            ));

            interceptorTrail.Add(new Point(
                interceptor.getX(),
                interceptor.getY()
            ));

            DrawSimulation();

            UpdateTelemetry();
        }
        private Point WorldToCanvas(double x, double y)
        {
            double centerX = InterceptorCanvas.ActualWidth / 2;
            double centerY = InterceptorCanvas.ActualHeight / 2;

            double worldRadius = 25000.0;

            double scaleX = (InterceptorCanvas.ActualWidth / 2) / worldRadius;
            double scaleY = (InterceptorCanvas.ActualHeight / 2) / worldRadius;

            double scale = Math.Min(scaleX, scaleY);

            double canvasX = centerX + x * scale;
            double canvasY = centerY - y * scale;

            return new Point(canvasX, canvasY);
        }
        private void UpdateTelemetry()
        {
            if (flyingEntities.Count == 0 || interceptors.Count == 0)
                return;

            FlyingEntity target = flyingEntities[0];
            Interceptor interceptor = interceptors[0];

            double dx = target.getX() - interceptor.getX();
            double dy = target.getY() - interceptor.getY();

            double distance = Math.Sqrt(dx * dx + dy * dy);

            txtDistance.Text = $"{distance:F0} m";
            txtTimeToIntercept.Text = $"{interceptor.TimeToIntercept():F1} s";

            if (interceptor.GetHasInterceptedTarget())
                txtState.Text = "INTERCEPTED";
            else
                txtState.Text = "TRACKING";

            txtInterceptStatus.Text = interceptor.ToString();
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
            txtCurrentCommand.Text = interceptorTarget.ToString();
            Interceptor interceptor = new Interceptor( 0,  0, "Interceptor-1", 600,0, 1,1000);

            interceptor.EngageTarget(target);

            flyingEntities.Add(target);
            interceptors.Add(interceptor);
            txtInterceptStatus.Text = interceptor.ToString();
            simulationTimer.Start();
        }
        private void DrawSimulation()
        {
            InterceptorCanvas.Children.Clear();
            Point batteryPoint = WorldToCanvas(0, 0);
            Rectangle battery = new Rectangle();
            battery.Width = 14;
            battery.Height = 14;
            battery.Fill = Brushes.Green;
            InterceptorCanvas.Children.Add(battery);
            Canvas.SetLeft(battery, batteryPoint.X - 7);
            Canvas.SetTop(battery, batteryPoint.Y - 7);
            foreach (FlyingEntity entity in flyingEntities)
            {
                Point point = WorldToCanvas(
                    entity.getX(),
                    entity.getY()
                );

                Ellipse target = new Ellipse();

                target.Width = 4;
                target.Height = 4;
                target.Fill = Brushes.Red;

                InterceptorCanvas.Children.Add(target);

                Canvas.SetLeft(target, point.X - 2);
                Canvas.SetTop(target, point.Y - 2);
            }
        }
        private void btnAbort_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}