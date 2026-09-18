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
using System.Windows.Threading;

namespace IronDomeCommandCenter
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer simulationTimer;
        private const double dt = 0.05;
        private CommandClient commandClient;
        private Dictionary<int, TargetData> targets;
        private TargetData? selectedTarget;
        private InterceptorServer interceptorServer;
        // =============================================
        // SIMULATED ISRAEL AREA
        // =============================================

        private const double IsraelMinX = -5000;
        private const double IsraelMaxX = 5000;

        private const double IsraelMinY = -12000;
        private const double IsraelMaxY = 12000;
        public MainWindow()
        {
            InitializeComponent();
            targets = new Dictionary<int, TargetData>();
            commandClient = new CommandClient();
            interceptorServer = new InterceptorServer();
            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(50);
            simulationTimer.Tick += SimulationTimer_Tick;
        }
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            await commandClient.ConnectToRadarAsync();

            txtStats.Text = "Connected";
            txtStats.Foreground = Brushes.Green;

            simulationTimer.Start();

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

                targets[target.Value.Id] = target.Value;
               UpdateTargetList();

            }
        }
        private void UpdateTargetList()
        {
            int? selectedId = null;

            if (selectedTarget != null)
            {
                selectedId = selectedTarget.Value.Id;
            }

            lstTargets.Items.Clear();

            foreach (var target in targets.Values)
            {
                lstTargets.Items.Add(target);
            }

            if (selectedId != null)
            {
                foreach (var item in lstTargets.Items)
                {
                    TargetData target = (TargetData)item;

                    if (target.Id == selectedId)
                    {
                        lstTargets.SelectedItem = target;
                        break;
                    }
                }
            }
        }// =============================================
         // WORLD -> MAP
         // =============================================

        private Point WorldToCanvas(double x, double y)
        {
            double width = CommandCanvas.ActualWidth;
            double height = CommandCanvas.ActualHeight;

            if (width <= 0 || height <= 0)
                return new Point(0, 0);

            // World coordinates used by the radar
            const double worldMinX = -25000;
            const double worldMaxX = 25000;

            const double worldMinY = -25000;
            const double worldMaxY = 25000;

            double normalizedX =
                (x - worldMinX) /
                (worldMaxX - worldMinX);

            double normalizedY =
                (y - worldMinY) /
                (worldMaxY - worldMinY);

            double canvasX =
                normalizedX * width;

            // WPF Y goes downward
            double canvasY =
                height - normalizedY * height;

            return new Point(
                canvasX,
                canvasY
            );
        }
        private void lstTargets_SelectionChanged( object sender,SelectionChangedEventArgs e)
        {
            if (lstTargets.SelectedItem == null)
            {
                selectedTarget = null;
                return;
            }

            selectedTarget = (TargetData)lstTargets.SelectedItem;
            PrintSelectedTarget();
        }
        private void PrintSelectedTarget()
        {
            if (selectedTarget == null)
            {
                txtSelectedTarget.Text = "NO TARGET SELECTED";
                return;
            }

            TargetData target = selectedTarget.Value;

            string friendlyText;

            if (target.isFriendly)
                friendlyText = "YES";
            else
                friendlyText = "NO";

            txtSelectedTarget.Text =
                $"TARGET ID:  {target.Id}\n" +
                $"Impact Location:{ target.impactLocations}\n" +
                $"TYPE:       {target.Name}" +
                $"ENTITY:     {target.EntityType}\n" +
                $"POSITION:   ({target.X:F0}, {target.Y:F0})\n" +
                $"VELOCITY:   ({target.Vx:F0}, {target.Vy:F0})\n" +
                $"SPEED:      {target.GetSpeed():F0} m/s";
        }
        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            List<int> ids = targets.Keys.ToList();

            foreach (int id in ids)
            {
                TargetData target = targets[id];

                target.X += target.Vx * dt;
                target.Y += target.Vy * dt;

                targets[id] = target;
            }

            UpdateTargetList();

            if (selectedTarget != null)
            {
                int selectedId = selectedTarget.Value.Id;

                if (targets.ContainsKey(selectedId))
                {
                    selectedTarget = targets[selectedId];
                    PrintSelectedTarget();
                }
            }

            DrawCommandMap();
        }

        private void DrawCommandMap()
        {
            CommandCanvas.Children.Clear();

            foreach (TargetData target in targets.Values)
            {
                if (target.isFriendly)
                    continue;

                if (target.EntityType != Entitytype.EntityType.ballistic &&
                    target.EntityType != Entitytype.EntityType.supersonic)
                {
                    continue;
                }

                DrawImpactX(
                    target.impactLocations,
                    target.Id
                );
            }
        }

        private Point CityToMap(
    ImpactLocation.ImpactLocations city)
        {
            double imageWidth = IsraelMap.ActualWidth;
            double imageHeight = IsraelMap.ActualHeight;

            double canvasWidth = CommandCanvas.ActualWidth;
            double canvasHeight = CommandCanvas.ActualHeight;

            double imageLeft =
                (canvasWidth - imageWidth) / 2;

            double imageTop =
                (canvasHeight - imageHeight) / 2;

            double xPercent;
            double yPercent;

            switch (city)
            {
                case ImpactLocation.ImpactLocations.Haifa:
                    xPercent = 0.38;
                    yPercent = 0.26;
                    break;

                case ImpactLocation.ImpactLocations.TelAviv:
                    xPercent = 0.30;
                    yPercent = 0.39;
                    break;

                case ImpactLocation.ImpactLocations.Jerusalem:
                    xPercent = 0.42;
                    yPercent = 0.45;
                    break;

                case ImpactLocation.ImpactLocations.Ashdod:
                    xPercent = 0.28;
                    yPercent = 0.48;
                    break;

                case ImpactLocation.ImpactLocations.Ashkelon:
                    xPercent = 0.27;
                    yPercent = 0.52;
                    break;

                case ImpactLocation.ImpactLocations.BeerSheva:
                    xPercent = 0.36;
                    yPercent = 0.60;
                    break;

                case ImpactLocation.ImpactLocations.Eilat:
                    xPercent = 0.35;
                    yPercent = 0.91;
                    break;

                default:
                    xPercent = 0.5;
                    yPercent = 0.5;
                    break;
            }

            return new Point(
                imageLeft + imageWidth * xPercent,
                imageTop + imageHeight * yPercent
            );
        }
        private void DrawImpactX(
     ImpactLocation.ImpactLocations city,
     int targetId)
        {
            Point p = CityToMap(city);

            const double size = 8;

            Line a = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y - size,
                X2 = p.X + size,
                Y2 = p.Y + size,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };

            Line b = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y + size,
                X2 = p.X + size,
                Y2 = p.Y - size,
                Stroke = Brushes.Red,
                StrokeThickness = 3
            };

            CommandCanvas.Children.Add(a);
            CommandCanvas.Children.Add(b);

            TextBlock label = new TextBlock
            {
                Text = $"{city} #{targetId}",
                Foreground = Brushes.Red,
                FontWeight = FontWeights.Bold,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 9
            };

            CommandCanvas.Children.Add(label);

            Canvas.SetLeft(label, p.X + 11);
            Canvas.SetTop(label, p.Y - 6);
        }
        private async void btnIntercept_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTarget == null)
            {
                MessageBox.Show("Select a target first");
                return;
            }

            

            InterceptCommand intercept = new InterceptCommand(selectedTarget.Value);

            await interceptorServer.SendInterceptCommandAsync(intercept);
        }
        private async void btnStartInterceptorServer_Click( object sender,RoutedEventArgs e)
        {
            await interceptorServer.CommandServerListenAsync();
        }

        private async void btnInterceptAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TargetData target in targets.Values)
            {
                InterceptCommand intercept =
                    new InterceptCommand(target);

                await interceptorServer.SendInterceptCommandAsync(intercept);
            }
        }
    }
}