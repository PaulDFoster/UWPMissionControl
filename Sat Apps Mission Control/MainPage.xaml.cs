using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Sat_Apps_Mission_Control
{
    // Stops the 'not awaited' green underlines...
    #pragma warning disable 4014 

    [DebuggerDisplay("Name = {Name}, IsEnabled = {IsEnabled}")]
    public class UsbDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool IsEnabled { get; set; }
    }

    [DebuggerDisplay("Name = {Name}, IsEnabled = {IsEnabled}")]
    public class SerialDeviceInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public bool Active { get; set; }
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private Storyboard serialDataTransfer;
        private Storyboard cloudDataTransfer;
        private DispatcherTimer timer;
        private SIKDataViewModel CurrentRecordCache = new SIKDataViewModel();
        private ObservableCollection<SIKData> Cache = new ObservableCollection<SIKData>();
        private bool flagCloud = false;
        private bool flagLocation = false;

        static Storyboard MakeDataTransferStoryBoard(UIElement uiElement, double seconds)
        {
            var storyBoard = new Storyboard();
            TranslateTransform moveTransform = new TranslateTransform();
            uiElement.RenderTransform = moveTransform;
            Duration duration = new Duration(TimeSpan.FromSeconds(seconds));
            DoubleAnimation myDoubleAnimationX = new DoubleAnimation();
            myDoubleAnimationX.Duration = duration;
            myDoubleAnimationX.To = 125;
            storyBoard.Children.Add(myDoubleAnimationX);
            Storyboard.SetTarget(myDoubleAnimationX, moveTransform);
            Storyboard.SetTargetProperty(myDoubleAnimationX, "X");
            storyBoard.AutoReverse = false;
            storyBoard.RepeatBehavior = RepeatBehavior.Forever;
            return storyBoard;
        }


        public MainPage()
        {
            this.usbDevices = new List<UsbDeviceInfo>();
            this.serialDevices = new List<SerialDeviceInfo>();

            this.InitializeComponent();

            SetupControls();
            //CreateNewCSVFile(); // Needs to be done per experiment in the UI
            //SensorsLive.DataContext = CurrentRecordCache;

            IoTHubSetup();

            ExperimentCharts.SetCache(ref Cache);
            RPMGauge.SetCache(ref Cache);

            // Start concurrent queue listener
            Task.Run(StartBackgroundLoop);

            // Start/Stop data read
            lock (lockObj)
            {
                this.bPauseDataRead = !this.bPauseDataRead;
            }

            if (this.bPauseDataRead)
            {
                this.state.serialWire.Update(DataFlow.Stopped);
            }
            else
            {

            }
        }

        async Task SetupControls()
        {
            imgWire1.Source = new BitmapImage(new Uri("ms-appx:///Assets/wire-disconnected.png"));
            imgWire2.Source = new BitmapImage(new Uri("ms-appx:///Assets/wire-disconnected.png"));

            this.serialDataTransfer = MakeDataTransferStoryBoard(this.imgBall1, 2);
            this.cloudDataTransfer = MakeDataTransferStoryBoard(this.imgBall2, 3);

            this.state.serialWire = new ConnectionState(this.imgWire1, this.serialDataTransfer, RunOnGUI);
            this.state.cloudWire = new ConnectionState(this.imgWire2, this.cloudDataTransfer, RunOnGUI);

            this.state.messagesSent = new TextBlockState(this.textBlockMsgCount, "0", RunOnGUI);

            this.state.serialWire.Update(WireState.Cut);
            this.state.serialWire.Update(DataFlow.Stopped);
            this.state.cloudWire.Update(WireState.Cut); // Should be set on cloud status,get value from IoT Hub connection object?
            this.state.cloudWire.Update(DataFlow.Stopped);

            SetupDeviceWatchers();

            PanelToggle.Click += PanelToggle_Click;
            SetupArea.Visibility = Visibility.Collapsed;
            LiveDataArea.Visibility = Visibility.Collapsed;

            LightExperimentArea.Visibility = Visibility.Collapsed;
            RPMExperimentArea.Visibility = Visibility.Collapsed;

            FlightsGrid.Visibility = Visibility.Visible;
            BlueYonderGrid.Visibility = Visibility.Visible;

            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Start();
        }

        private void PanelToggle_Click(object sender, RoutedEventArgs e)
        {
            if (MenuSplitView.IsPaneOpen)
            {
                MenuSplitView.IsPaneOpen = false;
                if (MenuSplitView.DisplayMode == SplitViewDisplayMode.Inline)
                {
                    MenuSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
                }
            }
            else
            {
                MenuSplitView.IsPaneOpen = true;
            }
        }

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Need to work out current UI state here to close the menu panel if in narrowstate

            StackPanel sp = (StackPanel)sender;
            Debug.WriteLine(sp.Name);
            SetupArea.Visibility = Visibility.Collapsed;
            LiveDataArea.Visibility = Visibility.Collapsed;
            LightExperimentArea.Visibility = Visibility.Collapsed;
            RPMExperimentArea.Visibility = Visibility.Collapsed;
            FlightsGrid.Visibility = Visibility.Collapsed;
            BlueYonderGrid.Visibility = Visibility.Collapsed;
            ExperimentArea.Visibility = Visibility.Collapsed;

            //scrollViewer.IsEnabled = false;
            //double fixHeight = scrollViewer.ExtentHeight - PromoArea.ActualHeight;

            switch (sp.Name)
            {
                case "Home":
                    FlightsGrid.Visibility = Visibility.Visible;
                    BlueYonderGrid.Visibility = Visibility.Visible;
                    //scrollViewer.IsEnabled = true;
                    break;
                case "Setup":

                    SetupArea.Visibility = Visibility.Visible;
                    break;
                case "LiveData":

                    LiveDataArea.Visibility = Visibility.Visible;
                    break;
                case "Experiment1": // Accel X

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(0);
                    break;
                case "Experiment2": // Accel Y

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(1);
                    break;

                case "Experiment3": // Accel Z

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(2);
                    break;
                case "Experiment4": // Light UV

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(3);
                    break;
                case "Experiment5": // Light IR

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(4);
                    break;
                case "Experiment6":// Light Visible

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(5);
                    break;
                case "Experiment7":// Temp

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(6);
                    break;
                case "Experiment8":// Pitch

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(7);
                    break;
                case "Experiment9":// Roll

                    ExperimentArea.Visibility = Visibility.Visible;
                    ExperimentCharts.SelectionChanged(8);
                    break;
                case "Experiment10": // RPM

                    RPMExperimentArea.Visibility = Visibility.Visible;
                    break;
                default:
                    SetupArea.Visibility = Visibility.Collapsed;
                    LiveDataArea.Visibility = Visibility.Visible;
                    LightExperimentArea.Visibility = Visibility.Collapsed;
                    RPMExperimentArea.Visibility = Visibility.Collapsed;
                    FlightsGrid.Visibility = Visibility.Collapsed;
                    BlueYonderGrid.Visibility = Visibility.Collapsed;
                    ExperimentArea.Visibility = Visibility.Collapsed;
                    PromoArea.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void RunOnGUI(Action action)
        {
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                    action();
                });
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            cloudFlag = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            cloudFlag = false;
            state.cloudWire.Update(DataFlow.Stopped);
            state.cloudWire.Update(WireState.Cut);
        }
    }
}
