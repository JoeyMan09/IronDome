using IronDomeInterceptor.inteceptor;
using System;
using System.Collections.Generic;
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
        private InterceptorClient interceptorClient;

        private List<FlyingEntity> flyingEntities;
        private List<Interceptor> interceptors;

        private DispatcherTimer simulationTimer;

        private const double dt = 0.05;

        private List<Point> targetTrail = new List<Point>();
        private List<Point> interceptorTrail = new List<Point>();
        private List<Point> MarkerX = new List<Point>();


        public MainWindow()
        {
            InitializeComponent();

            interceptorClient = new InterceptorClient();

            flyingEntities = new List<FlyingEntity>();
            interceptors = new List<Interceptor>();

            simulationTimer = new DispatcherTimer();

            simulationTimer.Interval =
                TimeSpan.FromMilliseconds(50);

            simulationTimer.Tick += SimulationTimer_Tick;
        }


        // =========================================================
        // SIMULATION TIMER
        // =========================================================

        private void SimulationTimer_Tick(
            object? sender,
            EventArgs e)
        {
            // Move targets
            foreach (FlyingEntity entity in flyingEntities)
            {
                entity.UpdatePosition(dt);
            }

            // Move interceptors
            foreach (Interceptor interceptor in interceptors)
            {
                interceptor.UpdatePosition(dt);
            }

            // Save target trails
            foreach (FlyingEntity target in flyingEntities)
            {
                targetTrail.Add(
                    new Point(
                        target.getX(),
                        target.getY()
                    )
                );
            }

            // Save interceptor trails
            foreach (Interceptor interceptor in interceptors)
            {
                interceptorTrail.Add(
                    new Point(
                        interceptor.getX(),
                        interceptor.getY()
                    )
                );
            }


            // Objects that need to be removed after interception
            List<FlyingEntity> targetsToRemove =
                new List<FlyingEntity>();

            List<Interceptor> interceptorsToRemove =
                new List<Interceptor>();


            foreach (Interceptor interceptor in interceptors)
            {
                if (interceptor.GetHasInterceptedTarget())
                {
                    FlyingEntity target =
                        interceptor.GetTarget();

                    if (target != null)
                    {
                        targetsToRemove.Add(target);
                    }

                    interceptorsToRemove.Add(interceptor);
                }
            }


            // Remove targets
            foreach (FlyingEntity target in targetsToRemove)
            {
                flyingEntities.Remove(target);
            }

            // Remove interceptors
            foreach (Interceptor interceptor in interceptorsToRemove)
            {
                interceptors.Remove(interceptor);
            }


            UpdateInterceptorList();

            DrawSimulation();


            // Explosion must happen AFTER DrawSimulation
            // because DrawSimulation clears the Canvas
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
            }
        }


        // =========================================================
        // INTERCEPTOR LIST SELECTION
        // =========================================================

        private void lstInterceptors_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (lstInterceptors.SelectedItem
                is not Interceptor interceptor)
            {
                return;
            }

            FlyingEntity target =
                interceptor.GetTarget();

            if (target == null)
                return;


            double dx =
                target.getX() -
                interceptor.getX();

            double dy =
                target.getY() -
                interceptor.getY();

            double distance =
                Math.Sqrt(
                    dx * dx +
                    dy * dy
                );


            txtInterceptStatus.Text =
                $"Interceptor: {interceptor.getName()}\n" +
                $"Target: {target.getName()}";

            txtDistance.Text =
                $"{distance:F0} m";

            txtTimeToIntercept.Text =
                $"{interceptor.TimeToIntercept():F1} s";

            txtState.Text =
                interceptor.GetHasInterceptedTarget()
                    ? "INTERCEPTED"
                    : "TRACKING";
        }


        // =========================================================
        // EXPLOSION
        // =========================================================

        private void ShowExplosion(
            double worldX,
            double worldY)
        {
            Point point =
                WorldToCanvas(
                    worldX,
                    worldY
                );


            Ellipse outerExplosion =
                new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.OrangeRed,
                    Stroke = Brushes.Yellow,
                    StrokeThickness = 4,
                    Opacity = 1
                };


            Ellipse innerExplosion =
                new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = Brushes.Yellow,
                    Opacity = 1
                };


            TextBlock boomText =
                new TextBlock
                {
                    Text = "BOOM!",
                    Foreground = Brushes.Yellow,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18
                };


            Canvas.SetLeft(
                outerExplosion,
                point.X - 10
            );

            Canvas.SetTop(
                outerExplosion,
                point.Y - 10
            );


            Canvas.SetLeft(
                innerExplosion,
                point.X - 5
            );

            Canvas.SetTop(
                innerExplosion,
                point.Y - 5
            );


            Canvas.SetLeft(
                boomText,
                point.X - 30
            );

            Canvas.SetTop(
                boomText,
                point.Y - 20
            );


            InterceptorCanvas.Children.Add(
                outerExplosion
            );

            InterceptorCanvas.Children.Add(
                innerExplosion
            );

            InterceptorCanvas.Children.Add(
                boomText
            );


            DoubleAnimation outerSize =
                new DoubleAnimation
                {
                    From = 20,
                    To = 120,
                    Duration =
                        TimeSpan.FromMilliseconds(600)
                };


            DoubleAnimation innerSize =
                new DoubleAnimation
                {
                    From = 10,
                    To = 70,
                    Duration =
                        TimeSpan.FromMilliseconds(450)
                };


            DoubleAnimation fade =
                new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration =
                        TimeSpan.FromMilliseconds(700)
                };


            DoubleAnimation textSize =
                new DoubleAnimation
                {
                    From = 18,
                    To = 48,
                    Duration =
                        TimeSpan.FromMilliseconds(450)
                };


            fade.Completed += (s, e) =>
            {
                InterceptorCanvas.Children.Remove(
                    outerExplosion
                );

                InterceptorCanvas.Children.Remove(
                    innerExplosion
                );

                InterceptorCanvas.Children.Remove(
                    boomText
                );
            };


            outerExplosion.BeginAnimation(
                WidthProperty,
                outerSize
            );

            outerExplosion.BeginAnimation(
                HeightProperty,
                outerSize
            );


            innerExplosion.BeginAnimation(
                WidthProperty,
                innerSize
            );

            innerExplosion.BeginAnimation(
                HeightProperty,
                innerSize
            );


            outerExplosion.BeginAnimation(
                OpacityProperty,
                fade
            );

            innerExplosion.BeginAnimation(
                OpacityProperty,
                fade
            );

            boomText.BeginAnimation(
                OpacityProperty,
                fade
            );


            boomText.BeginAnimation(
                TextBlock.FontSizeProperty,
                textSize
            );
        }


        // =========================================================
        // WORLD -> CANVAS
        // =========================================================

        private Point WorldToCanvas(
            double x,
            double y)
        {
            double centerX =
                InterceptorCanvas.ActualWidth / 2;

            double centerY =
                InterceptorCanvas.ActualHeight / 2;


            double worldRadius =
                25000.0;


            double scaleX =
                (InterceptorCanvas.ActualWidth / 2)
                / worldRadius;

            double scaleY =
                (InterceptorCanvas.ActualHeight / 2)
                / worldRadius;


            double scale =
                Math.Min(
                    scaleX,
                    scaleY
                );


            double canvasX =
                centerX +
                x * scale;

            double canvasY =
                centerY -
                y * scale;


            return new Point(
                canvasX,
                canvasY
            );
        }


        // =========================================================
        // TELEMETRY
        // =========================================================

        private void UpdateTelemetry()
        {
            if (lstInterceptors.SelectedItem
                is not Interceptor interceptor)
            {
                txtInterceptStatus.Text =
                    "No interceptor selected";

                txtDistance.Text = "-";

                txtTimeToIntercept.Text = "-";

                txtState.Text = "READY";

                return;
            }


            FlyingEntity target =
                interceptor.GetTarget();


            if (target == null)
                return;


            double dx =
                target.getX() -
                interceptor.getX();

            double dy =
                target.getY() -
                interceptor.getY();


            double distance =
                Math.Sqrt(
                    dx * dx +
                    dy * dy
                );


            txtDistance.Text =
                $"{distance:F0} m";


            txtTimeToIntercept.Text =
                $"{interceptor.TimeToIntercept():F1} s";


            txtState.Text =
                interceptor.GetHasInterceptedTarget()
                    ? "INTERCEPTED"
                    : "TRACKING";


            txtInterceptStatus.Text =
                interceptor.ToString();
        }


        // =========================================================
        // ICON CREATION
        // =========================================================

        private Image CreateTargetIcon(FlyingEntity entity)
        {
            string imagePath;

            EntityType.Entitytype type = entity.GetEntityType();

            switch (type)
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
                    if (entity.getIsFriendly())
                        imagePath = "AirCraft.png";
                    else
                        imagePath = "fighter.png";
                    break;

                default:
                    imagePath = "fighter.png";
                    break;
            }

            Image image = new Image
            {
                Width = 32,
                Height = 32,
                Stretch = Stretch.Uniform
            };

            image.Source = new BitmapImage(
                new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                )
            );

            return image;
        }


        private Image CreateInterceptorIcon()
        {
            return CreateIcon(
                "interceptor.png",
                32
            );
        }


        private Image CreateIcon(
            string imagePath,
            double size)
        {
            Image icon =
                new Image
                {
                    Width = size,
                    Height = size,
                    Stretch =
                        Stretch.Uniform
                };


            BitmapImage bitmap =
                new BitmapImage();


            bitmap.BeginInit();

            bitmap.UriSource =
                new Uri(
                    $"pack://application:,,,/{imagePath}",
                    UriKind.Absolute
                );

            bitmap.CacheOption =
                BitmapCacheOption.OnLoad;

            bitmap.EndInit();


            icon.Source = bitmap;


            return icon;
        }


        // =========================================================
        // CONNECT TO COMMAND CENTER
        // =========================================================

        private async void btnConnectToCommand_Click(
            object sender,
            RoutedEventArgs e)
        {
            await interceptorClient
                .ConnectToCommandAsync();


            txtConnectionStatus.Text =
                "Command Center: Connected";

            txtConnectionStatus.Foreground =
                Brushes.Green;


            await ListenToCommandsAsync();
        }


        // =========================================================
        // LISTEN FOR INTERCEPT COMMANDS
        // =========================================================

        private async Task ListenToCommandsAsync()
        {
            while (true)
            {
                InterceptCommand? command =
                    await interceptorClient
                        .InterceptorClientReadAsync();


                if (command == null)
                    break;


                FlyingEntity target =
                    new FlyingEntity(
                        command.TargetX,
                        command.TargetY,
                        $"Target-{command.TargetId}",
                        command.TargetVx,
                        command.TargetVy,
                        1,
                        command.EntityType,command.isFriendly
                    );
                if (!target.getIsFriendly())
                {
                    Interceptor interceptor =
                    new Interceptor(
                        0,
                        0,
                        $"Interceptor-" +
                        $"{interceptors.Count + 1}",
                        600,
                        0,
                        1,
                        1000,
                        EntityType.Entitytype.interceptor
                    );interceptor.EngageTarget(
                    target
                );


                flyingEntities.Add(
                    target
                );

                interceptors.Add(
                    interceptor
                );
                }
                


                


                UpdateInterceptorList();


                if (lstInterceptors.SelectedItem == null &&
                    lstInterceptors.Items.Count > 0)
                {
                    lstInterceptors.SelectedIndex = 0;
                }


                if (!simulationTimer.IsEnabled)
                {
                    simulationTimer.Start();
                }
            }
        }


        // =========================================================
        // UPDATE INTERCEPTOR LIST
        // =========================================================

        private void UpdateInterceptorList()
        {
            int selectedId = -1;


            if (lstInterceptors.SelectedItem
                is Interceptor selected)
            {
                selectedId =
                    selected.getId();
            }


            lstInterceptors.Items.Clear();


            foreach (Interceptor interceptor
                     in interceptors)
            {
                lstInterceptors.Items.Add(
                    interceptor
                );
            }


            foreach (Interceptor interceptor
                     in lstInterceptors.Items)
            {
                if (interceptor.getId()
                    == selectedId)
                {
                    lstInterceptors.SelectedItem =
                        interceptor;

                    break;
                }
            }
        }


        // =========================================================
        // DRAW
        // =========================================================

        private void DrawSimulation()
        {
            InterceptorCanvas.Children.Clear();


            // -----------------------------------------------------
            // Battery
            // -----------------------------------------------------

            Point batteryPoint =
                WorldToCanvas(
                    0,
                    0
                );


            Rectangle battery =
                new Rectangle
                {
                    Width = 14,
                    Height = 14,
                    Fill = Brushes.Green
                };


            InterceptorCanvas.Children.Add(
                battery
            );


            Canvas.SetLeft(
                battery,
                batteryPoint.X - 7
            );

            Canvas.SetTop(
                battery,
                batteryPoint.Y - 7
            );


            // -----------------------------------------------------
            // Targets
            // -----------------------------------------------------

            foreach (FlyingEntity entity in flyingEntities)
            {
                Point point = WorldToCanvas(
                    entity.getX(),
                    entity.getY()
                );

                // יוצר Image חדש
                Image targetIcon = CreateTargetIcon(entity);

                // מחשב זווית
                double angle = GetRotationAngle(
                    entity.getVx(),
                    entity.getVy()
                );

                // מסובב אותו
                targetIcon.RenderTransformOrigin =
                    new Point(0.5, 0.5);

                targetIcon.RenderTransform =
                    new RotateTransform(angle);

                // מוסיף ל-Canvas פעם אחת בלבד
                InterceptorCanvas.Children.Add(targetIcon);

                Canvas.SetLeft(
                    targetIcon,
                    point.X - targetIcon.Width / 2
                );

                Canvas.SetTop(
                    targetIcon,
                    point.Y - targetIcon.Height / 2
                );
            }


            // -----------------------------------------------------
            // Interceptors
            // -----------------------------------------------------

            foreach (Interceptor interceptor
                     in interceptors)
            {
                Point point =
                    WorldToCanvas(
                        interceptor.getX(),
                        interceptor.getY()
                    );


                Image interceptorIcon =
                    CreateInterceptorIcon();
                double angle = GetRotationAngle(
    interceptor.getVx(),
    interceptor.getVy()
);

                interceptorIcon.RenderTransformOrigin =
                    new Point(0.5, 0.5);

                interceptorIcon.RenderTransform =
                    new RotateTransform(angle);


                InterceptorCanvas.Children.Add(
                    interceptorIcon
                );


                Canvas.SetLeft(
                    interceptorIcon,
                    point.X -
                    interceptorIcon.Width / 2
                );


                Canvas.SetTop(
                    interceptorIcon,
                    point.Y -
                    interceptorIcon.Height / 2
                );
            }
            for (int i = 0; i < interceptors.Count; i++)
            {
                FlyingEntity target = interceptors[i].GetTarget();

                if (target == null)
                    continue;

                Point p = CalcX(
                    interceptors[i],
                    target
                );

                if (double.IsNaN(p.X) || double.IsNaN(p.Y))
                    continue;

                DrawX(p);
            }
        }
        private void DrawX(Point worldPoint)
        {
            Point p = WorldToCanvas(
                worldPoint.X,
                worldPoint.Y
            );

            double size = 8;

            Line line1 = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y - size,
                X2 = p.X + size,
                Y2 = p.Y + size,

                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            Line line2 = new Line
            {
                X1 = p.X - size,
                Y1 = p.Y + size,
                X2 = p.X + size,
                Y2 = p.Y - size,

                Stroke = Brushes.Red,
                StrokeThickness = 2
            };

            InterceptorCanvas.Children.Add(line1);
            InterceptorCanvas.Children.Add(line2);
        }
        private Point CalcX(Interceptor interceptor, FlyingEntity flyingEntity)
        {
            double tx = flyingEntity.getX();
            double ty = flyingEntity.getY();

            double tvx = flyingEntity.getVx();
            double tvy = flyingEntity.getVy();

            double ix = interceptor.getX();
            double iy = interceptor.getY();

            double interceptorSpeed = interceptor.getSpeed();

            // המרחק ההתחלתי בין המיירט למטרה
            double dx = tx - ix;
            double dy = ty - iy;

            // משוואה ריבועית:
            // |targetPosition + targetVelocity*t - interceptorPosition|
            //      = interceptorSpeed * t

            double a =
                tvx * tvx +
                tvy * tvy -
                interceptorSpeed * interceptorSpeed;

            double b =
                2 * (dx * tvx + dy * tvy);

            double c =
                dx * dx +
                dy * dy;

            double discriminant =
                b * b - 4 * a * c;

            // אין נקודת יירוט אפשרית
            if (discriminant < 0)
                return new Point(double.NaN, double.NaN);

            double sqrt =
                Math.Sqrt(discriminant);

            double t1 =
                (-b - sqrt) / (2 * a);

            double t2 =
                (-b + sqrt) / (2 * a);

            // רוצים את הזמן החיובי הקטן ביותר
            double t = double.PositiveInfinity;

            if (t1 > 0)
                t = t1;

            if (t2 > 0 && t2 < t)
                t = t2;

            if (double.IsInfinity(t))
                return new Point(double.NaN, double.NaN);

            // איפה המטרה תהיה בזמן t
            double interceptX =
                tx + tvx * t;

            double interceptY =
                ty + tvy * t;

            return new Point(
                interceptX,
                interceptY
            );
        }

        // =========================================================
        // ABORT
        // =========================================================
        private double GetRotationAngle(double vx, double vy)
        {
            double angleRadians = Math.Atan2(-vy, vx);
            double angleDegrees = angleRadians * 180.0 / Math.PI;

            return angleDegrees + 90;
        }
        private void btnAbortAll_Click(
    object sender,
    RoutedEventArgs e)
        {
            foreach (Interceptor interceptor in interceptors)
            {
                interceptor.DisEngageTarget();
            }

            interceptors.Clear();
            interceptorTrail.Clear();
            MarkerX.Clear();

            DrawSimulation();

            txtState.Text = "ABORTED";
        }
    }
}