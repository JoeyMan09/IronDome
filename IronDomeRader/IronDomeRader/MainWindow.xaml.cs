using IronDomeRader.Flying_Entity;
using IronDomeRader.Flying_Entity.AirCraft;
using IronDomeRader.Flying_Entity.Missle;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IronDomeRader
{
    public partial class MainWindow : Window
    {
        private RadarSystem radarSystem;
        private DispatcherTimer timer;
        private Random rand;
        private double _sweepAngle = 0;
        private double _cx, _cy, _radius;
        private Line _sweepLine;
        private List<UIElement> _sweepFade = new();
        private Canvas _entityLayer = new Canvas();
        private Stopwatch _clock = new Stopwatch();
        private double _lastTime = 0;
        private const double WorldSize = 50000.0;
        private RadarServer radarServer;
        private FlyingEntity selectedEntity;
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += OnLoaded;
            radarSystem = new RadarSystem();
            rand = new Random();
            radarServer = new RadarServer();
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _cx = RadarCanvas.ActualWidth / 2;
            _cy = RadarCanvas.ActualHeight / 2;
            _radius = Math.Min(_cx, _cy) - 15;

            DrawStaticRadar();

            // Add entity layer on top — never cleared
            RadarCanvas.Children.Add(_entityLayer);
        }
        private void DrawStaticRadar()
        {
            // Concentric rings
            for (int i = 1; i <= 4; i++)
            {
                double r = _radius * i / 4;
                var ring = new Ellipse
                {
                    Width = r * 2,
                    Height = r * 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 200, 0)),
                    StrokeThickness = 1
                };
                Canvas.SetLeft(ring, _cx - r);
                Canvas.SetTop(ring, _cy - r);
                RadarCanvas.Children.Add(ring);

                // Range label
                var label = new TextBlock
                {
                    Text = $"{i * 25} km",
                    Foreground = new SolidColorBrush(Color.FromArgb(150, 0, 200, 0)),
                    FontSize = 9
                };
                Canvas.SetLeft(label, _cx + 3);
                Canvas.SetTop(label, _cy - r - 13);
                RadarCanvas.Children.Add(label);
            }

            // Crosshair lines
            AddStaticLine(_cx - _radius, _cy, _cx + _radius, _cy);
            AddStaticLine(_cx, _cy - _radius, _cx, _cy + _radius);

            // Diagonal lines
            double d = _radius * Math.Cos(45 * Math.PI / 180);
            AddStaticLine(_cx - d, _cy - d, _cx + d, _cy + d);
            AddStaticLine(_cx + d, _cy - d, _cx - d, _cy + d);

            // Center dot
            var center = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0))
            };
            Canvas.SetLeft(center, _cx - 4);
            Canvas.SetTop(center, _cy - 4);
            RadarCanvas.Children.Add(center);
        }

        private void AddStaticLine(double x1, double y1, double x2, double y2)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = new SolidColorBrush(Color.FromArgb(100, 0, 200, 0)),
                StrokeThickness = 1
            };
            RadarCanvas.Children.Add(line);
        }

        // ── Sweep animation ────────────────────────────────────────────────
        public void StartRadar()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(30);
            timer.Tick += OnTick;

            _clock.Start();
            _lastTime = 0;

            timer.Start();
        }

        public void StopRadar()
        {
            timer?.Stop();
        }

        private void OnTick(object sender, EventArgs e)
        {
            double now = _clock.Elapsed.TotalSeconds;
            double dt = now - _lastTime;
            _lastTime = now;

            _sweepAngle = (_sweepAngle + 120 * dt) % 360;

            radarSystem.MoveAll(dt);

            if (radarSystem.GetFlyingEntities().Count < 5)
                radarSystem.SpawnFlyingEntity(rand);

            //Title = $"Entities: {radarSystem.GetFlyingEntities().Count}";

            DrawSweep();
            DrawRadar();
        }

        private void DrawSweep()
        {
            // Remove old sweep elements
            if (_sweepLine != null)
                RadarCanvas.Children.Remove(_sweepLine);

            foreach (var el in _sweepFade)
                RadarCanvas.Children.Remove(el);
            _sweepFade.Clear();

            double rad = _sweepAngle * Math.PI / 180;

            // Trailing glow wedge (5 fading lines behind sweep)
            for (int i = 1; i <= 8; i++)
            {
                double trailAngle = (_sweepAngle - i * 3) * Math.PI / 180;
                byte alpha = (byte)(60 - i * 7);
                if (alpha == 0) continue;

                var trail = new Line
                {
                    X1 = _cx,
                    Y1 = _cy,
                    X2 = _cx + Math.Cos(trailAngle) * _radius,
                    Y2 = _cy + Math.Sin(trailAngle) * _radius,
                    Stroke = new SolidColorBrush(Color.FromArgb(alpha, 0, 255, 0)),
                    StrokeThickness = 2
                };
                RadarCanvas.Children.Add(trail);
                _sweepFade.Add(trail);
            }

            // Main sweep line
            _sweepLine = new Line
            {
                X1 = _cx,
                Y1 = _cy,
                X2 = _cx + Math.Cos(rad) * _radius,
                Y2 = _cy + Math.Sin(rad) * _radius,
                Stroke = new SolidColorBrush(Color.FromArgb(220, 0, 255, 0)),
                StrokeThickness = 2
            };
            RadarCanvas.Children.Add(_sweepLine);
        }
        private void DrawRadar()
        {
            _entityLayer.Children.Clear();

            double scale = _radius / (WorldSize / 2);

            foreach (var entity in radarSystem.GetFlyingEntities())
            {
                double canvasX = _cx + entity.getX() * scale;
                double canvasY = _cy - entity.getY() * scale;

                double dx = canvasX - _cx;
                double dy = canvasY - _cy;

                if (Math.Sqrt(dx * dx + dy * dy) > _radius)
                    continue;

                var dot = new Ellipse
                {
                    Width = 8,
                    Height = 8,
                    Fill = Brushes.Cyan,

                    Tag = entity
                };

                if (entity is SupersonicMissile)
                    dot.Fill = Brushes.Red;

                else if (entity is BallisticMissile)
                    dot.Fill = Brushes.Yellow;

                else if (entity is Drone)
                    dot.Fill = Brushes.Blue;
                else
                    dot.Fill= Brushes.Green;
                dot.MouseLeftButtonDown +=
                    Dot_MouseLeftButtonDown;

                Canvas.SetLeft(dot, canvasX - 5);
                Canvas.SetTop(dot, canvasY - 5);

                _entityLayer.Children.Add(dot);

                var label = new TextBlock
                {
                    Text = $"{entity.getName()}\n" +
                           $"Speed: {entity.getSpeed():F0}\n" +
                           $"TTI: {entity.TimeToImpact():F1}s",
                    Foreground = Brushes.White,
                    FontSize = 11
                };

                Canvas.SetLeft(label, canvasX + 10);
                Canvas.SetTop(label, canvasY - 10);

                _entityLayer.Children.Add(label);
            }
        }
        private void Dot_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse dot = sender as Ellipse;

            if (dot != null)
            {
                selectedEntity = dot.Tag as FlyingEntity;
                if (selectedEntity != null)
                {
                    Title = $"Selected: {selectedEntity.getName()}";
                }
            }
        }
        private async void btnSenToCommand_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEntity == null)
            {
                MessageBox.Show("Select a target first");
                return;
            }
            TargetData target = new TargetData(
                selectedEntity.getId(),
                selectedEntity.getName(),
                selectedEntity.getX(),
                selectedEntity.getY(),
                selectedEntity.getVx(),
                selectedEntity.getVy(),
                selectedEntity.getThreatLvl(),
                selectedEntity.GetEntityType(),
                selectedEntity.getIsFriendly()
                
            );

            await radarServer.SendTargetAsync(target);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StopRadar();
        }

        private async void btnSenAllToCommand_Click(object sender, RoutedEventArgs e)
        {
            foreach (FlyingEntity entity in radarSystem.GetFlyingEntities())
            {
                TargetData target = new TargetData(
                    entity.getId(),
                    entity.getName(),
                    entity.getX(),
                    entity.getY(),
                    entity.getVx(),
                    entity.getVy(),
                    entity.getThreatLvl(),
                    entity.GetEntityType(), entity.getIsFriendly()
                );

                await radarServer.SendTargetAsync(target);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            StartRadar();
        }
        private async void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            await radarServer.RaderServerListenAsync();
            txtStats.Text = "Server Connected";
            txtStats.Foreground = Brushes.Green;
        }
    }
}