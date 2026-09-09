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

namespace IronDomeCommandCenter
{
    public partial class MainWindow : Window
    {
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
        }
        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            await commandClient.ConnectToRadarAsync();
            txtStats.Text = "Connected";
            
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

        private async void btnIntercept_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTarget == null)
            {
                MessageBox.Show("Select a target first");
                return;
            }
            InterceptCommand intercept= new InterceptCommand(selectedTarget.Value);
            await interceptorServer.SendInterceptCommandAsync(intercept);
        }
        private async void btnStartInterceptorServer_Click(
    object sender,
    RoutedEventArgs e)
        {
            await interceptorServer.CommandServerListenAsync();
        }
    }
}