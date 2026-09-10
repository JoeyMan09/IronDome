using IronDomeInterceptor.inteceptor;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IronDomeInterceptor
{
    public partial class MainWindow : Window
    {
        private InterceptorClient interceptorClient;

        private List<FlyingEntity> flyingEntities;
        private List<Interceptor> interceptors;

        private DispatcherTimer simulationTimer;

        private MediaPlayer backgroundMusic;
        private MediaPlayer launchSound;
        private MediaPlayer explosionSound;

        private const double dt = 0.05;

        private List<Point> targetTrail = new();
        private List<Point> interceptorTrail = new();


        public MainWindow()
        {
            InitializeComponent();

            interceptorClient = new InterceptorClient();

            flyingEntities = new List<FlyingEntity>();
            interceptors = new List<Interceptor>();

            simulationTimer = new DispatcherTimer();
            simulationTimer.Interval = TimeSpan.FromMilliseconds(50);
            simulationTimer.Tick += SimulationTimer_Tick;

            SetupSounds();
        }


        private void SetupSounds()
        {
            string backgroundPath =
                @"C:\Users\idank\source\repos\IronDome\IronDomeSystem\IronDomeInterceptor\IronDomeInterceptor\טרוליםחדש.wav";

            string launchPath =
                @"C:\Users\idank\Downloads\428360__jacco18__missile.wav";

            string explosionPath =
                @"C:\Users\idank\source\repos\IronDome\IronDomeSystem\IronDomeInterceptor\IronDomeInterceptor\אוריאלחדש.wav";


            backgroundMusic = new MediaPlayer();
            launchSound = new MediaPlayer();
            explosionSound = new MediaPlayer();


            backgroundMusic.Open(new Uri(backgroundPath, UriKind.Absolute));
            launchSound.Open(new Uri(launchPath, UriKind.Absolute));
            explosionSound.Open(new Uri(explosionPath, UriKind.Absolute));


            backgroundMusic.Volume = 0.01;
            launchSound.Volume = 0.3;
            explosionSound.Volume = 0.5;


            backgroundMusic.MediaEnded += (s, e) =>
            {
                backgroundMusic.Position = TimeSpan.Zero;
                backgroundMusic.Play();
            };


            backgroundMusic.Play();
        }


        private void SimulationTimer_Tick(object? sender, EventArgs e)
        {
            foreach (FlyingEntity entity in flyingEntities)
                entity.UpdatePosition(dt);


            foreach (Interceptor interceptor in interceptors)
                interceptor.UpdatePosition(dt);


            UpdateBackgroundMusicVolume();


            foreach (FlyingEntity target in flyingEntities)
            {
                targetTrail.Add(
                    new Point(target.getX(), target.getY())
                );
            }


            foreach (Interceptor interceptor in interceptors)
            {
                interceptorTrail.Add(
                    new Point(interceptor.getX(), interceptor.getY())
                );
            }


            List<FlyingEntity> targetsToRemove = new();
            List<Interceptor> interceptorsToRemove = new();


            foreach (Interceptor interceptor in interceptors)
            {
                if (!interceptor.GetHasInterceptedTarget())
                    continue;


                FlyingEntity target = interceptor.GetTarget();


                if (target != null)
                    targetsToRemove.Add(target);


                interceptorsToRemove.Add(interceptor);
            }


            foreach (FlyingEntity target in targetsToRemove)
                flyingEntities.Remove(target);


            foreach (Interceptor interceptor in interceptorsToRemove)
                interceptors.Remove(interceptor);


            UpdateInterceptorList();
            DrawSimulation();


            foreach (FlyingEntity target in targetsToRemove)
            {
                ShowExplosion(
                    target.getX(),
                    target.getY()
                );
            }


            UpdateTelemetry();


            if (flyingEntities.Count == 0 &&
                interceptors.Count == 0)
            {
                simulationTimer.Stop();

                txtState.Text = "READY";

                backgroundMusic.Volume = 0.01;
            }
        }


        private void UpdateBackgroundMusicVolume()
        {
            if (interceptors.Count == 0)
            {
                backgroundMusic.Volume = 0.01;
                return;
            }


            double closestDistance = double.MaxValue;


            foreach (Interceptor interceptor in interceptors)
            {
                FlyingEntity target = interceptor.GetTarget();

                if (target == null)
                    continue;


                double dx =
                    target.getX() - interceptor.getX();

                double dy =
                    target.getY() - interceptor.getY();


                double distance =
                    Math.Sqrt(dx * dx + dy * dy);


                if (distance < closestDistance)
                    closestDistance = distance;
            }


            if (closestDistance == double.MaxValue)
            {
                backgroundMusic.Volume = 0.01;
                return;
            }


            const double maxDistance = 25000.0;

            const double minVolume = 0.01;
            const double maxVolume = 0.50;


            double closeness =
                1.0 - Math.Clamp(
                    closestDistance / maxDistance,
                    0.0,
                    1.0
                );


            backgroundMusic.Volume =
                minVolume +
                closeness * (maxVolume - minVolume);
        }


        private void lstInterceptors_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (lstInterceptors.SelectedItem is not Interceptor interceptor)
                return;


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
            txtTimeToIntercept.Text = $"{interceptor.TimeToIntercept():F1} s";

            txtState.Text =
                interceptor.GetHasInterceptedTarget()
                ? "INTERCEPTED"
                : "TRACKING";
        }


        private void ShowExplosion(double worldX, double worldY)
        {
            explosionSound.Position = TimeSpan.Zero;
            explosionSound.Play();


            Point point = WorldToCanvas(worldX, worldY);


            Ellipse outerExplosion = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = Brushes.OrangeRed,
                Stroke = Brushes.Yellow,
                StrokeThickness = 4,
                Opacity = 1
            };


            Ellipse innerExplosion = new Ellipse
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Yellow,
                Opacity = 1
            };


            TextBlock boomText = new TextBlock
            {
                Text = "BOOM!",
                Foreground = Brushes.Yellow,
                FontWeight = FontWeights.Bold,
                FontSize = 18
            };


            Canvas.SetLeft(outerExplosion, point.X - 10);
            Canvas.SetTop(outerExplosion, point.Y - 10);

            Canvas.SetLeft(innerExplosion, point.X - 5);
            Canvas.SetTop(innerExplosion, point.Y - 5);

            Canvas.SetLeft(boomText, point.X - 30);
            Canvas.SetTop(boomText, point.Y - 20);


            InterceptorCanvas.Children.Add(outerExplosion);
            InterceptorCanvas.Children.Add(innerExplosion);
            InterceptorCanvas.Children.Add(boomText);


            DoubleAnimation outerSize = new DoubleAnimation
            {
                From = 20,
                To = 120,
                Duration = TimeSpan.FromMilliseconds(600)
            };


            DoubleAnimation innerSize = new DoubleAnimation
            {
                From = 10,
                To = 70,
                Duration = TimeSpan.FromMilliseconds(450)
            };


            DoubleAnimation fade = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(700)
            };


            DoubleAnimation textSize = new DoubleAnimation
            {
                From = 18,
                To = 48,
                Duration = TimeSpan.FromMilliseconds(450)
            };


            fade.Completed += (s, e) =>
            {
                InterceptorCanvas.Children.Remove(outerExplosion);
                InterceptorCanvas.Children.Remove(innerExplosion);
                InterceptorCanvas.Children.Remove(boomText);
            };


            outerExplosion.BeginAnimation(WidthProperty, outerSize);
            outerExplosion.BeginAnimation(HeightProperty, outerSize);

            innerExplosion.BeginAnimation(WidthProperty, innerSize);
            innerExplosion.BeginAnimation(HeightProperty, innerSize);

            outerExplosion.BeginAnimation(OpacityProperty, fade);
            innerExplosion.BeginAnimation(OpacityProperty, fade);
            boomText.BeginAnimation(OpacityProperty, fade);

            boomText.BeginAnimation(
                TextBlock.FontSizeProperty,
                textSize
            );
        }


        private Point WorldToCanvas(double x, double y)
        {
            double centerX = InterceptorCanvas.ActualWidth / 2;
            double centerY = InterceptorCanvas.ActualHeight / 2;

            double worldRadius = 25000.0;

            double scaleX =
                (InterceptorCanvas.ActualWidth / 2) / worldRadius;

            double scaleY =
                (InterceptorCanvas.ActualHeight / 2) / worldRadius;

            double scale = Math.Min(scaleX, scaleY);

            double canvasX = centerX + x * scale;
            double canvasY = centerY - y * scale;

            return new Point(canvasX, canvasY);
        }


        private void UpdateTelemetry()
        {
            if (lstInterceptors.SelectedItem is not Interceptor interceptor)
            {
                txtInterceptStatus.Text = "No interceptor selected";

                txtDistance.Text = "-";
                txtTimeToIntercept.Text = "-";
                txtState.Text = "READY";

                return;
            }


            FlyingEntity target = interceptor.GetTarget();

            if (target == null)
                return;


            double dx = target.getX() - interceptor.getX();
            double dy = target.getY() - interceptor.getY();

            double distance = Math.Sqrt(dx * dx + dy * dy);


            txtDistance.Text = $"{distance:F0} m";

            txtTimeToIntercept.Text =
                $"{interceptor.TimeToIntercept():F1} s";


            if (interceptor.GetHasInterceptedTarget())
                txtState.Text = "INTERCEPTED";
            else
                txtState.Text = "TRACKING";


            txtInterceptStatus.Text = interceptor.ToString();
        }


        private async void btnConnectToCommand_Click(
            object sender,
            RoutedEventArgs e)
        {
            await interceptorClient.ConnectToCommandAsync();

            txtConnectionStatus.Text =
                "Command Center: Connected";

            txtConnectionStatus.Foreground =
                Brushes.Green;

            await ListenToCommandsAsync();
        }


        private async Task ListenToCommandsAsync()
        {
            while (true)
            {
                InterceptCommand? command =
                    await interceptorClient.InterceptorClientReadAsync();


                if (command == null)
                    break;


                FlyingEntity target = new FlyingEntity(
                    command.TargetX,
                    command.TargetY,
                    $"Target-{command.TargetId}",
                    command.TargetVx,
                    command.TargetVy,
                    1
                );


                Interceptor interceptor = new Interceptor(
                    0,
                    0,
                    $"Interceptor-{interceptors.Count + 1}",
                    600,
                    0,
                    1,
                    1000
                );


                interceptor.EngageTarget(target);


                // Missile launch sound
                launchSound.Position = TimeSpan.Zero;
                launchSound.Play();


                flyingEntities.Add(target);
                interceptors.Add(interceptor);


                UpdateInterceptorList();


                if (lstInterceptors.SelectedItem == null &&
                    lstInterceptors.Items.Count > 0)
                {
                    lstInterceptors.SelectedIndex = 0;
                }


                if (!simulationTimer.IsEnabled)
                    simulationTimer.Start();
            }
        }


        private void UpdateInterceptorList()
        {
            int selectedId = -1;


            if (lstInterceptors.SelectedItem is Interceptor selected)
                selectedId = selected.getId();


            lstInterceptors.Items.Clear();


            foreach (Interceptor interceptor in interceptors)
                lstInterceptors.Items.Add(interceptor);


            foreach (Interceptor interceptor in lstInterceptors.Items)
            {
                if (interceptor.getId() != selectedId)
                    continue;


                lstInterceptors.SelectedItem = interceptor;

                break;
            }
        }


        private void DrawSimulation()
        {
            InterceptorCanvas.Children.Clear();


            Point batteryPoint = WorldToCanvas(0, 0);


            Rectangle battery = new Rectangle
            {
                Width = 14,
                Height = 14,
                Fill = Brushes.Green
            };


            InterceptorCanvas.Children.Add(battery);

            Canvas.SetLeft(
                battery,
                batteryPoint.X - 7
            );

            Canvas.SetTop(
                battery,
                batteryPoint.Y - 7
            );


            foreach (FlyingEntity entity in flyingEntities)
            {
                Point point =
                    WorldToCanvas(
                        entity.getX(),
                        entity.getY()
                    );


                Ellipse target = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = Brushes.Red
                };


                InterceptorCanvas.Children.Add(target);

                Canvas.SetLeft(target, point.X - 2);
                Canvas.SetTop(target, point.Y - 2);
            }


            foreach (Interceptor interceptor in interceptors)
            {
                Point point =
                    WorldToCanvas(
                        interceptor.getX(),
                        interceptor.getY()
                    );


                Ellipse intercept = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    Fill = Brushes.Orange
                };


                InterceptorCanvas.Children.Add(intercept);

                Canvas.SetLeft(intercept, point.X - 2);
                Canvas.SetTop(intercept, point.Y - 2);
            }
        }


        private void btnAbort_Click(
            object sender,
            RoutedEventArgs e)
        {

        }
    }
}