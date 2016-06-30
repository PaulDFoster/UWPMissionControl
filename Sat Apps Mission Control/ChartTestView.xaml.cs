using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WinRTXamlToolkit.Tools;
using System.Collections.ObjectModel;

namespace Sat_Apps_Mission_Control
{
    public sealed partial class ChartTestView : UserControl
    {
        private bool isInitialized;
        private int dataSetIndex = 0;
        private int state = 0;
        private int dataSetOffSet = 0;
        
        public ChartTestView()
        {
            this.InitializeComponent();
            this.isInitialized = true;
            //UpdateCharts();
        }

        private ObservableCollection<SIKData> cache;
        public void SetCache(ref ObservableCollection<SIKData> input)
        {
            cache = input;
        }

        private Random _random = new Random();
        private bool axisLabelsHidden = false;
        private UIElement TargetElement;
        private int dataField = 0;
        private char[] requiredData = { 'X', 'Y', 'Z', 'U', 'I', 'V', 'T', 'P', 'R' };

        private EventThrottler _updateThrottler = new EventThrottler();

        private void RunIfSelected(UIElement element, Action action)
        {
            if ( TargetElement == element)
            {
                action.Invoke();
            }
        }

        private void UpdateCharts()
        {
            if (!this.isInitialized)
            {
                return;
            }

            var items = new List<NameValueItem>();
            int state = 0;

            if (cache != null)
            {
                if (cache.Count > NumberOfIitemsNumericUpDown.Value)
                {
                    // We're connected to the cache to create the required series
                    state = cache.Count;
                    Debug.WriteLine(state);
                    int index = dataSetIndex - (dataSetOffSet * (int)NumberOfIitemsNumericUpDown.Value);
                    if (index < 0)
                    {
                        index = 0;
                        dataSetOffSet = 0;
                    }
                    if (index > (state -1)) index = (state - ((int)NumberOfIitemsNumericUpDown.Value +1));
                    for (int i = index; i < (index + NumberOfIitemsNumericUpDown.Value); i++)
                    {
                        //index = index + i;
                        Debug.WriteLine(i);
                        if (i >= (state - 1)) break;
                        switch (requiredData[dataField])
                        {
                            case 'U':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].UV });
                                break;
                            case 'I':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].IR });
                                break;
                            case 'V':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Visible });
                                break;
                            case 'X':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].X });
                                break;
                            case 'Y':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Y });
                                break;
                            case 'Z':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Z });
                                break;
                            case 'H':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Heading });
                                break;
                            case 'P':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Pitch });
                                break;
                            case 'R':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Roll });
                                break;
                            case 'T':
                                items.Add(new NameValueItem { Name = cache[i].MissionControlTS.ToString("ddMMyyyy HH:mm:ss:ff"), Value = cache[i].Temperature });
                                break;
                        }

                    }
                }

            }
            else
            {
                Debug.WriteLine("\n\nNull cache\n\n");

                for (int i = 0; i < NumberOfIitemsNumericUpDown.Value; i++)
                {
                    items.Add(new NameValueItem { Name = "Test" + i, Value = _random.Next(10, 100) });
                }
            }


            RunIfSelected(this.AccelX, () => ((LineSeries)this.AccelX.Series[0]).ItemsSource = items);;
            RunIfSelected(this.AccelY, () => ((LineSeries)this.AccelY.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.AccelZ, () => ((LineSeries)this.AccelZ.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.LightIR, () => ((LineSeries)this.LightIR.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.LightUV, () => ((LineSeries)this.LightUV.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.LightVisible, () => ((LineSeries)this.LightVisible.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.Temp, () => ((LineSeries)this.Temp.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.PitchScatterChart, () => ((ScatterSeries)this.PitchScatterChart.Series[0]).ItemsSource = items);;
            RunIfSelected(this.RollScatterChart, () => ((ScatterSeries)this.RollScatterChart.Series[0]).ItemsSource = items); ;
        }

        public void SelectionChanged(int index)
        {
            TargetElement = (UIElement) ChartsList.Items[index];
            
            dataField = index;
            ChartsList.SelectedIndex = index;

            //this.ThrottledUpdate();
        }

        public class NameValueItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        private void OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            this.ThrottledUpdate();
        }

        private void OnPrevButtonClick(object sender, RoutedEventArgs e)
        {
            state = cache.Count - 1;
            if (state > NumberOfIitemsNumericUpDown.Value) dataSetOffSet++;
            this.ThrottledUpdate();
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            state = cache.Count - 1;
            if (state > (dataSetIndex + NumberOfIitemsNumericUpDown.Value)) dataSetOffSet--;
            this.ThrottledUpdate();
        }

        private void OnLastButtonClick(object sender, RoutedEventArgs e)
        {
            dataSetOffSet = 0;
            state = cache.Count - 1;
            if((state - NumberOfIitemsNumericUpDown.Value)>0) dataSetIndex = state - (int)NumberOfIitemsNumericUpDown.Value;
            this.ThrottledUpdate();
        }

        private void OnFirstButtonClick(object sender, RoutedEventArgs e)
        {
            dataSetIndex = 0;
            dataSetOffSet = 0;
            this.ThrottledUpdate();
        }

        private void ThrottledUpdate()
        {
            _updateThrottler.Run(
                async () =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    this.UpdateCharts();
                    sw.Stop();
                    await Task.Delay(sw.Elapsed);
                });
        }

        private void NumberOfIitemsNumericUpDown_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            dataSetOffSet = 0;
            this.ThrottledUpdate();
        }

        private void ChartsList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged(ChartsList.SelectedIndex);
            this.ThrottledUpdate();
        }

    }
}
