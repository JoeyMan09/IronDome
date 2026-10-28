using IronDomeInterceptor.inteceptor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IronDomeInterceptor
{
    public partial class MainWindow : Window
    {
        private readonly InterceptorClient interceptorClient;

        private readonly List<FlyingEntity> flyingEntities;
        private readonly List<Interceptor> interceptors;

        private readonly Dictionary<FlyingEntity, InterceptCommand>
            targetCommands = new();

        private readonly DispatcherTimer simulationTimer;

        private const double dt = 0.05;

        private bool boom;
        private bool missionFailed;
        private bool simulationEnded;
        private bool processingTick;

        private int interceptorNumber = 0;

        public MainWindow()
        {
            InitializeComponent();

            interceptorClient = new InterceptorClient();

            flyingEntities = new List<FlyingEntity>();
            interceptors = new List<Interceptor>();

            simulationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(50)
            };

            simulationTimer.Tick += SimulationTimer_Tick;
        }

        // =============================================
        // TARGET LIST ITEM
        // =============================================

        private class TargetListItem
        {
            public FlyingEntity Target { get; set; } = null!;

            public override string ToString()
            {
                return Target.getName();
            }
        }

        // =============================================
        // SIMULATION
        // =============================================

        private async void SimulationTimer_Tick(
            object? sender,
            EventArgs e)
        {
            if (simulationEnded || processingTick)
                return;

            processingTick = true;

            try
            {
                // Move all targets
                foreach (FlyingEntity target in flyingEntities)
                {
                    target.UpdatePosition(dt);
                }

                // Move all interceptors
                foreach (Interceptor interceptor in interceptors)
                {
                    interceptor.UpdatePosition(dt);
                }

                List<FlyingEntity> destroyedTargets = new();
                List<Interceptor> completedInterceptors = new();

                // Detect successful interceptions
                // Detect successful and failed interceptions
                foreach (Interceptor interceptor in interceptors)
                {
                    FlyingEntity? target = interceptor.GetTarget();

                    if (interceptor.GetHasInterceptedTarget())
                    {
                        if (target != null &&
                            flyingEntities.Contains(target) &&
                            !destroyedTargets.Contains(target))
                        {
                            destroyedTargets.Add(target);
                        }

                        completedInterceptors.Add(interceptor);
                    }
                    else if (interceptor.GetHasFailedInterception())
                    {
                        if (interceptor.HasFinishedFalling())
                        {
                            completedInterceptors.Add(interceptor);
                            txtState.Text = "INTERCEPTION FAILED";

                            txtInterceptStatus.Text =
                                $"FAILED: {interceptor.getName()}\n" +
                                $"TARGET STILL ACTIVE: {target?.getName()}";
                        }
                    }
                }

                // Remove successful interceptions before
                // checking whether surviving targets impacted.
                foreach (FlyingEntity target in destroyedTargets)
                {
                    flyingEntities.Remove(target);
                }

                foreach (Interceptor interceptor in completedInterceptors)
                {
                    interceptors.Remove(interceptor);
                }

                UpdateTargetList();
                UpdateInterceptorList();
                DrawSimulation();

                foreach (FlyingEntity target in destroyedTargets)
                {
                    ShowExplosion(target.getX(), target.getY());

                    if (targetCommands.TryGetValue(
                        target,
                        out InterceptCommand? command))
                    {
                        targetCommands.Remove(target);

                        try
                        {
                            await interceptorClient.SendInterceptResultAsync(
                                command.CommandId,
                                command.TargetId,
                                true
                            );
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(
                                $"Failed to report interception: {ex.Message}"
                            );
                        }
                    }
                }

                // Check only surviving hostile targets.
                CheckForFailedInterception();

                if (missionFailed)
                    return;

                UpdateTelemetry();

                if (flyingEntities.Count == 0 &&
                    interceptors.Count == 0)
                {
                    simulationTimer.Stop();
                    txtState.Text = "READY";
                }
            }
            finally
            {
                processingTick = false;
            }
        }

        // =============================================
        // FAILED INTERCEPTION
        // =============================================

        private void CheckForFailedInterception()
        {
            if (simulationEnded)
                return;

            foreach (FlyingEntity target in flyingEntities)
            {
                if (target.getIsFriendly())
                    continue;

                if (target.HasImpacted())
                {
                    ShowMissionFailed(target);
                    return;
                }
            }
        }

        private void ShowMissionFailed(FlyingEntity target)
        {
            if (missionFailed)
                return;

            missionFailed = true;
            simulationEnded = true;

            simulationTimer.Stop();

            txtState.Text = "MISSION FAILED";

            txtFailedTarget.Text =
                $"TARGET: {target.getName()}\n" +
                "TARGET REACHED ITS IMPACT POINT";

            MissionFailedOverlay.Visibility = Visibility.Visible;
        }

        // =============================================
        // CONNECTION
        // =============================================

        private async void btnConnectToCommand_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (simulationEnded)
                return;

            btnConnectToCommand.IsEnabled = false;

            try
            {
                await interceptorClient.ConnectToCommandAsync();

                txtConnectionStatus.Text =
                    "Command Center: Connected";

                txtConnectionStatus.Foreground = Brushes.Green;

                await ListenToCommandsAsync();
            }
            catch (Exception ex)
            {
                txtConnectionStatus.Text = "CONNECTION FAILED";
                txtConnectionStatus.Foreground = Brushes.Red;

                MessageBox.Show(ex.Message);
            }
            finally
            {
                btnConnectToCommand.IsEnabled = true;
            }
        }

        // =============================================
        // RECEIVE TARGETS - NO AUTOMATIC LAUNCH
        // =============================================

        private async Task ListenToCommandsAsync()
        {
            while (!simulationEnded)
            {
                InterceptCommand? command =
                    await interceptorClient.InterceptorClientReadAsync();

                if (command == null)
                    break;

                if (simulationEnded)
                    break;

                // Prevent duplicate targets.
                bool alreadyExists = targetCommands.Values.Any(
                    c => c.TargetId == command.TargetId
                );

                if (alreadyExists)
                    continue;

                FlyingEntity target = new FlyingEntity(
      command.TargetX,
      command.TargetY,
      $"Target-{command.TargetId}",
      command.TargetVx,
      command.TargetVy,
      1,
      command.EntityType,
      command.isFriendly
  );

                if (command.HasImpactPoint)
                {
                    target.SetImpactPoint(
                        command.ImpactX,
                        command.ImpactY
                    );
                }

                flyingEntities.Add(target);
                targetCommands[target] = command;
                if (command.HasImpactPoint)
                {
                    target.SetImpactPoint(
                        command.ImpactX,
                        command.ImpactY
                    );
                }
                txtCurrentCommand.Text =
                    $"TARGET #{command.TargetId}\n" +
                    $"TYPE: {command.EntityType}\n" +
                    $"STATUS: AWAITING INTERCEPTION";

                UpdateTargetList();
                DrawSimulation();

                if (!simulationTimer.IsEnabled)
                    simulationTimer.Start();

                txtState.Text = "TARGET DETECTED";
            }
        }

        // =============================================
        // TARGET LIST
        // =============================================

        private void UpdateTargetList()
        {
            FlyingEntity? selected =
                (lstTargets.SelectedItem as TargetListItem)?.Target;

            lstTargets.Items.Clear();

            foreach (FlyingEntity target in flyingEntities)
            {
                if (target.getIsFriendly())
                    continue;

                TargetListItem item = new TargetListItem
                {
                    Target = target
                };

                lstTargets.Items.Add(item);

                if (ReferenceEquals(target, selected))
                {
                    lstTargets.SelectedItem = item;
                }
            }
        }

        // =============================================
        // MANUAL INTERCEPTION
        // =============================================

        private bool HasActiveInterceptor(FlyingEntity target)
        {
            return interceptors.Any(i =>
                ReferenceEquals(i.GetTarget(), target) &&
                !i.GetHasFailedInterception() &&
                !i.GetHasInterceptedTarget());
        }

        private void LaunchInterceptor(FlyingEntity target)
        {
            if (simulationEnded || target.getIsFriendly())
                return;

            if (!flyingEntities.Contains(target))
                return;

            if (HasActiveInterceptor(target))
                return;

            IronDomeBattery battery = GetBatteryForTarget(target);

            interceptorNumber++;

            Interceptor interceptor = new Interceptor(
                battery.X,
                battery.Y,
                $"Interceptor-{interceptorNumber}",
                600,
                0,
                1,
                100,
                EntityType.Entitytype.interceptor
            );

            interceptor.EngageTarget(target);
            interceptors.Add(interceptor);

            UpdateInterceptorList();
            DrawSimulation();

            txtState.Text = "INTERCEPTING";

            txtCurrentCommand.Text =
                $"BATTERY: {battery.City}\n" +
                $"TARGET: {target.getName()}\n" +
                "STATUS: INTERCEPTOR LAUNCHED";

            if (!simulationTimer.IsEnabled)
                simulationTimer.Start();
        }

        private void btnInterceptSelected_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (lstTargets.SelectedItem is not TargetListItem item)
            {
                MessageBox.Show("Select a target first.");
                return;
            }

            LaunchInterceptor(item.Target);
        }

        private void btnInterceptAll_Click(
            object sender,
            RoutedEventArgs e)
        {
            foreach (FlyingEntity target in flyingEntities.ToList())
            {
                LaunchInterceptor(target);
            }
        }

        // =============================================
        // INTERCEPTOR LIST
        // =============================================

        private void UpdateInterceptorList()
        {
            Interceptor? selected =
                lstInterceptors.SelectedItem as Interceptor;

            lstInterceptors.Items.Clear();

            foreach (Interceptor interceptor in interceptors)
            {
                lstInterceptors.Items.Add(interceptor);

                if (ReferenceEquals(interceptor, selected))
                {
                    lstInterceptors.SelectedItem = interceptor;
                }
            }
        }

        private void lstInterceptors_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            UpdateTelemetry();
        }

        // =============================================
        // TELEMETRY
        // =============================================

        private void UpdateTelemetry()
        {
            if (simulationEnded)
                return;

            if (lstInterceptors.SelectedItem
                is not Interceptor interceptor)
            {
                txtInterceptStatus.Text =
                    "No interceptor selected";

                txtDistance.Text = "-";
                txtTimeToIntercept.Text = "-";

                return;
            }

            FlyingEntity target = interceptor.GetTarget();

            if (target == null)
                return;

            double dx = target.getX() - interceptor.getX();
            double dy = target.getY() - interceptor.getY();

            double distance = Math.Sqrt(dx * dx + dy * dy);

            txtInterceptStatus.Text =
                $"Interceptor: {interceptor.getName()}\n" +
                $"Target: {target.getName()}";

            txtDistance.Text = $"{distance:F0} m";

            txtTimeToIntercept.Text =
                $"{interceptor.TimeToIntercept():F1} s";

            txtState.Text =
                interceptor.GetHasInterceptedTarget()
                    ? "INTERCEPTED"
                    : "TRACKING";
        }

        // =============================================
        // DRAW SIMULATION
        // =============================================

        private void DrawSimulation()
        {
            InterceptorCanvas.Children.Clear();

            DrawAllBatteries();

            // Targets
            foreach (FlyingEntity target in flyingEntities)
            {
                Point p = WorldToCanvas(
                    target.getX(),
                    target.getY()
                );

                Image icon = CreateTargetIcon(target);

                icon.RenderTransformOrigin = new Point(0.5, 0.5);

                icon.RenderTransform = new RotateTransform(
                    GetRotationAngle(
                        target.getVx(),
                        target.getVy()
                    )
                );

                InterceptorCanvas.Children.Add(icon);

                Canvas.SetLeft(icon, p.X - icon.Width / 2);
                Canvas.SetTop(icon, p.Y - icon.Height / 2);
            }

            // Interceptors
            foreach (Interceptor interceptor in interceptors)
            {
                Point p = WorldToCanvas(
                    interceptor.getX(),
                    interceptor.getY()
                );

                Image icon = CreateInterceptorIcon();

                icon.RenderTransformOrigin = new Point(0.5, 0.5);

                icon.RenderTransform = new RotateTransform(
                    GetRotationAngle(
                        interceptor.getVx(),
                        interceptor.getVy()
                    )
                );

                InterceptorCanvas.Children.Add(icon);

                Canvas.SetLeft(icon, p.X - icon.Width / 2);
                Canvas.SetTop(icon, p.Y - icon.Height / 2);

                FlyingEntity target = interceptor.GetTarget();

                if (target == null)
                    continue;

                Point predicted = CalcX(interceptor, target);

                if (!double.IsNaN(predicted.X) &&
                    !double.IsNaN(predicted.Y))
                {
                    DrawX(predicted);
                }
            }
        }
        private void DrawAllBatteries()
        {
            foreach (IronDomeBattery battery in batteries)
            {
                Point p = WorldToCanvas(battery.X, battery.Y);

                Ellipse marker = new Ellipse
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.LimeGreen,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.5
                };

                InterceptorCanvas.Children.Add(marker);

                Canvas.SetLeft(marker, p.X - 7);
                Canvas.SetTop(marker, p.Y - 7);

                TextBlock label = new TextBlock
                {
                    Text = battery.City,
                    Foreground = Brushes.LimeGreen,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    Background = new SolidColorBrush(
                        Color.FromArgb(190, 5, 12, 18))
                };

                InterceptorCanvas.Children.Add(label);

                Canvas.SetLeft(label, p.X + 11);
                Canvas.SetTop(label, p.Y - 8);
            }
        }
        private Image CreateTargetIcon(FlyingEntity entity)
        {
            string imagePath;

            switch (entity.GetEntityType())
            {
                case EntityType.Entitytype.ballistic:
                    imagePath = "ballistic.png";
                    break;

                case EntityType.Entitytype.supersonic:
                    imagePath = "supersonic.png";
                    break;

                case EntityType.Entitytype.drone:
                    imagePath = "drone.png";
                    break;

                case EntityType.Entitytype.aircraft:
                    imagePath = entity.getIsFriendly()
                        ? "AirCraft.png"
                        : "fighter.png";
                    break;

                default:
                    imagePath = "fighter.png";
                    break;
            }

            return CreateIcon(imagePath, 32);
        }

        private Image CreateInterceptorIcon()
        {
            return CreateIcon("interceptor.png", 32);
        }

        private Image CreateIcon(string path, double size)
        {
            Image icon = new Image
            {
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform
            };

            icon.Source = new BitmapImage(
                new Uri(
                    $"pack://application:,,,/{path}",
                    UriKind.Absolute
                )
            );

            return icon;
        }

        private Point WorldToCanvas(double x, double y)
        {
            double centerX = InterceptorCanvas.ActualWidth / 2;
            double centerY = InterceptorCanvas.ActualHeight / 2;

            const double worldRadius = 25000.0;

            double scaleX = centerX / worldRadius;
            double scaleY = centerY / worldRadius;

            double scale = Math.Min(scaleX, scaleY);

            return new Point(
                centerX + x * scale,
                centerY - y * scale
            );
        }

        private double GetRotationAngle(double vx, double vy)
        {
            return Math.Atan2(-vy, vx) * 180.0 / Math.PI + 90;
        }

        // =============================================
        // PREDICT INTERCEPT POINT
        // =============================================

        private Point CalcX(
            Interceptor interceptor,
            FlyingEntity target)
        {
            double dx = target.getX() - interceptor.getX();
            double dy = target.getY() - interceptor.getY();

            double vx = target.getVx();
            double vy = target.getVy();

            double speed = interceptor.getSpeed();

            double a = vx * vx + vy * vy - speed * speed;
            double b = 2 * (dx * vx + dy * vy);
            double c = dx * dx + dy * dy;

            double t;

            if (Math.Abs(a) < 0.000001)
            {
                if (Math.Abs(b) < 0.000001)
                    return new Point(double.NaN, double.NaN);

                t = -c / b;
            }
            else
            {
                double discriminant = b * b - 4 * a * c;

                if (discriminant < 0)
                    return new Point(double.NaN, double.NaN);

                double sqrt = Math.Sqrt(discriminant);

                double t1 = (-b - sqrt) / (2 * a);
                double t2 = (-b + sqrt) / (2 * a);

                t = double.PositiveInfinity;

                if (t1 > 0)
                    t = t1;

                if (t2 > 0 && t2 < t)
                    t = t2;
            }

            if (t <= 0 || double.IsInfinity(t))
                return new Point(double.NaN, double.NaN);

            return new Point(
                target.getX() + vx * t,
                target.getY() + vy * t
            );
        }

        private void DrawX(Point worldPoint)
        {
            Point p = WorldToCanvas(
                worldPoint.X,
                worldPoint.Y
            );

            const double size = 8;

            Line a = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y - size,
                X2 = p.X + size,
                Y2 = p.Y + size,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            Line b = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y + size,
                X2 = p.X + size,
                Y2 = p.Y - size,
                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            InterceptorCanvas.Children.Add(a);
            InterceptorCanvas.Children.Add(b);
        }

        // =============================================
        // EXPLOSION
        // =============================================

        private void PlayBoomSound()
        {
            if (!boom)
                return;

            string path =
                @"C:\Users\idank\source\repos\IronDome\IronDomeSystem\IronDomeInterceptor\IronDomeInterceptor\אוריאלחדש.wav";

            if (!System.IO.File.Exists(path))
                return;

            ExplosionAudio.Source = new Uri(
                path,
                UriKind.Absolute
            );

            ExplosionAudio.Position = TimeSpan.Zero;
            ExplosionAudio.Volume = 1.0;
            ExplosionAudio.Play();
        }

        private void ShowExplosion(double worldX, double worldY)
        {
            PlayBoomSound();

            Point p = WorldToCanvas(worldX, worldY);

            Ellipse explosion = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.OrangeRed,
                Stroke = Brushes.Yellow,
                StrokeThickness = 3
            };

            InterceptorCanvas.Children.Add(explosion);

            Canvas.SetLeft(explosion, p.X - 10);
            Canvas.SetTop(explosion, p.Y - 10);

            DoubleAnimation expand = new DoubleAnimation
            {
                From = 20,
                To = 120,
                Duration = TimeSpan.FromMilliseconds(600)
            };

            DoubleAnimation fade = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(700)
            };

            fade.Completed += (s, e) =>
            {
                InterceptorCanvas.Children.Remove(explosion);
            };

            explosion.BeginAnimation(WidthProperty, expand);
            explosion.BeginAnimation(HeightProperty, expand);
            explosion.BeginAnimation(OpacityProperty, fade);
        }

        // =============================================
        // ABORT
        // =============================================

        private void btnAbortAll_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (simulationEnded)
                return;

            foreach (Interceptor interceptor in interceptors)
            {
                interceptor.DisEngageTarget();
            }

            interceptors.Clear();

            UpdateInterceptorList();
            DrawSimulation();

            txtState.Text = "ABORTED";
        }

        private void btnBoom_Click(
            object sender,
            RoutedEventArgs e)
        {
            boom = !boom;
        }
        private readonly List<IronDomeBattery> batteries = new()
{
    new("Haifa", -1500, 7000),
    new("Tel Aviv", -1200, 2500),
    new("Jerusalem", 1200, 1500),
    new("Ashdod", -1000, 500),
    new("Ashkelon", -900, -1000),
    new("Beer Sheva", 0, -4000),
    new("Eilat", 500, -8500)
}; private IronDomeBattery GetBatteryForTarget(FlyingEntity target)
        {
            if (targetCommands.TryGetValue(target, out InterceptCommand? command)
                && command.HasImpactPoint)
            {
                return batteries
                    .OrderBy(b =>
                        Math.Pow(b.X - command.ImpactX, 2) +
                        Math.Pow(b.Y - command.ImpactY, 2))
                    .First();
            }

            // Fallback if impact point was not received
            return batteries
                .OrderBy(b =>
                    Math.Pow(b.X - target.getX(), 2) +
                    Math.Pow(b.Y - target.getY(), 2))
                .First();
        }
    }
}