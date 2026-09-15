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
                return;

            TargetData target = selectedTarget.Value;

            txtSelectedTarget.Text =
            $"TARGET ID: {target.Id}\n" +
            $"TYPE:      {target.Name}\n" +
            $"POSITION:  ({target.X:F0}, {target.Y:F0})\n" +
            $"VELOCITY:  ({target.Vx:F0}, {target.Vy:F0})\n" +
            $"SPEED:     {target.GetSpeed():F0} m/s";


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
            
        }

        private async void btnIntercept_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTarget == null)
            {
                MessageBox.Show("Select a target first");
                return;
            }

            // בדיקה זמנית
            MessageBox.Show(
                $"Name: {selectedTarget.Value.Name}\n" +
                $"EntityType: {selectedTarget.Value.EntityType}"
            );

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