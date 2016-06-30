using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace Sat_Apps_Mission_Control
{
    public sealed partial class MainPage
    {
        public class NameValueItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        private Random _random = new Random();

        private void SetUpCharts()
        {
            var items = new List<NameValueItem>();

            for (int i = 0; i < 5; i++)
            {
                items.Add(new NameValueItem { Name = "Test" + i, Value = _random.Next(10, 100) });
            }

            RunIfSelected(this.ColumnChart, () => ((ColumnSeries)this.ColumnChart.Series[0]).ItemsSource = items); ;
            //RunIfSelected(this.BarChart, () => ((BarSeries)this.BarChart.Series[0]).ItemsSource = items); ;
            //RunIfSelected(this.LineChart, () => ((LineSeries)this.LineChart.Series[0]).ItemsSource = items); ;
            RunIfSelected(this.LineChartWithAxes, () => ((LineSeries)this.LineChartWithAxes.Series[0]).ItemsSource = items); ;
        }

        private void RunIfSelected(UIElement element, Action action)
        {
                action.Invoke();
        }
    }
}
