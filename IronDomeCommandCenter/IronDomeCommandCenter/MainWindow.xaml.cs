using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool isUpdatingTargetList = false;
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
            interceptorServer.InterceptCompleted += HandleInterceptResultAsync;
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
                    break;

                // Do not restore an already destroyed target
                if (destroyedTargetIds.Contains(target.Value.Id))
                    continue;

                targets[target.Value.Id] = target.Value;

                UpdateTargetList();
                DrawCommandMap();
            }
        }
        private int GetEntityPriority(Entitytype.EntityType type)
        {
            switch (type)
            {
                case Entitytype.EntityType.supersonic: return 0;
                case Entitytype.EntityType.ballistic: return 1;
                case Entitytype.EntityType.drone: return 2;
                case Entitytype.EntityType.aircraft: return 3;
                default: return 4;
            }
        }

        private List<TargetData> GetSortedTargets()
        {
            return targets.Values
                .OrderBy(t => t.GetTimeToImpact())
                .ThenBy(t => GetEntityPriority(t.EntityType))
                .ThenBy(t => t.impactLocations)
                .ThenBy(t => t.Id)
                .ToList();
        }
        private readonly HashSet<int> destroyedTargetIds = new();

        private async Task HandleInterceptResultAsync(InterceptResult result)
        {
            if (!result.Success)
                return;

            // Prevent duplicate interception reports
            bool isNewInterception = false;

            await Dispatcher.InvokeAsync(() =>
            {
                isNewInterception = destroyedTargetIds.Add(result.TargetId);
            });

            if (!isNewInterception)
                return;

            try
            {
                // Notify Radar to remove the target
                await commandClient.SendTargetDestroyedAsync(result.TargetId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"Failed to notify Radar: {ex.Message}"
                );

                await Dispatcher.InvokeAsync(() =>
                {
                    destroyedTargetIds.Remove(result.TargetId);
                });

                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                // Remove target from Command Center
                targets.Remove(result.TargetId);

                // Clear selection if destroyed target was selected
                if (selectedTarget?.Id == result.TargetId)
                {
                    selectedTarget = null;
                }

                // Update table, details and map immediately
                UpdateTargetList();
                PrintSelectedTarget();
                DrawCommandMap();
            });
        }
        private void UpdateTargetList()
        {
            int? selectedId = selectedTarget?.Id;
            isUpdatingTargetList = true;
            try
            {
                lstTargets.Items.Clear();
                foreach (TargetData target in GetSortedTargets())
                    lstTargets.Items.Add(target);

                if (selectedId.HasValue)
                {
                    foreach (TargetData target in lstTargets.Items)
                    {
                        if (target.Id == selectedId.Value)
                        {
                            lstTargets.SelectedItem = target;
                            break;
                        }
                    }
                }
            }
            finally
            {
                isUpdatingTargetList = false;
            }
        }
        // =============================================
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
        private void lstTargets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUpdatingTargetList) return;
            if (lstTargets.SelectedItem == null)
            {
                selectedTarget = null;
                PrintSelectedTarget();
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
                $"IMPACT:     {target.impactLocations}\n" +
                $"NAME:       {target.Name}\n" +
                $"ENTITY:     {target.EntityType}\n" +
                $"POSITION:   ({target.X:F0}, {target.Y:F0})\n" +
                $"VELOCITY:   ({target.Vx:F0}, {target.Vy:F0})\n" +
                $"SPEED:      {Math.Sqrt(target.Vx * target.Vx + target.Vy * target.Vy):F0} m/s\n" +
                $"ETA:        {(double.IsInfinity(target.GetTimeToImpact()) ? "N/A" : target.GetTimeToImpact().ToString("F1") + "s")}\n" +
                $"FRIENDLY:   {friendlyText}";
        }
        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            List<int> ids = targets.Keys.ToList();

            foreach (int id in ids)
            {
                if (!targets.TryGetValue(id, out TargetData target))
                    continue;

                target.X += target.Vx * dt;
                target.Y += target.Vy * dt;

                targets[id] = target;
            }

            if (selectedTarget != null)
            {
                int selectedId = selectedTarget.Value.Id;

                if (targets.TryGetValue(selectedId, out TargetData updatedTarget))
                {
                    selectedTarget = updatedTarget;
                }
                else
                {
                    selectedTarget = null;
                }

                PrintSelectedTarget();
            }

            UpdateTargetList();
            DrawCommandMap();
        }

        private void DrawCommandMap()
        {
            CommandCanvas.Children.Clear();

            foreach (TargetData target in targets.Values)
            {
                if (target.isFriendly)
                    continue;

                DrawImpactX(target.impactLocations, target.Id, target);
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
     int targetId, TargetData target)
        {
            Point p = CityToMap(city);

            const double size = 8;
            Brush color;
            switch (target.EntityType)
            {
                case Entitytype.EntityType.supersonic: color = Brushes.Red; break;
                case Entitytype.EntityType.ballistic: color = Brushes.Yellow; break;
                case Entitytype.EntityType.drone: color = Brushes.DodgerBlue; break;
                case Entitytype.EntityType.aircraft: color = Brushes.Orange; break;
                default: color = Brushes.White; break;
            }

            Line a = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y - size,
                X2 = p.X + size,
                Y2 = p.Y + size,
                Stroke = color,
                StrokeThickness = 3
            };

            Line b = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y + size,
                X2 = p.X + size,
                Y2 = p.Y - size,
                Stroke = color,
                StrokeThickness = 3
            };

            CommandCanvas.Children.Add(a);
            CommandCanvas.Children.Add(b);

            TextBlock label = new TextBlock
            {
                Text = $"{city} #{targetId} | {target.EntityType}",
                Foreground = color,
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



            if (selectedTarget.Value.isFriendly)
            {
                MessageBox.Show("Cannot intercept a friendly target");
                return;
            }
            InterceptCommand intercept = new InterceptCommand(selectedTarget.Value);

            await interceptorServer.SendInterceptCommandAsync(intercept);
        }
        private async void btnStartInterceptorServer_Click(object sender, RoutedEventArgs e)
        {
            await interceptorServer.CommandServerListenAsync();
        }

        private async void btnInterceptAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (TargetData target in GetSortedTargets().Where(t => !t.isFriendly))
            {
                InterceptCommand intercept =
                    new InterceptCommand(target);

                await interceptorServer.SendInterceptCommandAsync(intercept);
            }
        }
    }
}