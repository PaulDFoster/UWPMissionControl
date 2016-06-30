using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Sat_Apps_Mission_Control
{
    public sealed partial class RPMView : UserControl
    {
        DispatcherTimer timer;
        bool calculating = false;
        private ObservableCollection<SIKData> cache;
        public void SetCache(ref ObservableCollection<SIKData> input)
        {
            cache = input;
        }

        public RPMView()
        {
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(5);

        }

        private void timer_Tick(object sender, object e)
        {
            // calculate avg, max, min from current cache, totalling time
            int heldEnd = cache.Count-1; // zero base
            bool firstRun = true;
            DateTime lastTime = DateTime.Now;
            TimeSpan totalTime;
            
            int totalReading=0;
            int avgReading=0;
            int maxReading=0;
            int signalReading=0;
            int countReadings = 0;
            int hits = 0;

            // Calculate measures for last 5 seconds
            for (int x = heldEnd; x>0; x--)
            {
                if (firstRun)
                {
                    lastTime = cache[x].MissionControlTS;
                    maxReading = cache[x].Visible;
                    totalReading =  cache[x].Visible;
                    firstRun = false;
                }
                else
                {
                    
                    totalTime = totalTime.Add(lastTime.Subtract(cache[x].MissionControlTS));
                    totalReading = totalReading + cache[x].Visible;
                    if (maxReading < cache[x].Visible) maxReading = cache[x].Visible;
                    countReadings++;
                }
                if (totalTime.TotalSeconds > 10) break;
            }

            // Now have total readings or 5seconds worth
            avgReading = totalReading / countReadings;
            signalReading = (maxReading - avgReading) / 2;
            if (signalReading < 300)
            {
                gaugeRPM.Value = 0;
                return; //Difference isn't good enough to detect
            }

            // Now count occurances that can be identified as peaks

            for(int x = countReadings; x>0;x--)
            {
                if ((cache[heldEnd - x].Visible - avgReading)> signalReading) hits++;
            }

            // sanity check
            if(totalTime.TotalSeconds>60)
            {
                //oops how did that happen!
            }
            else
                gaugeRPM.Value = hits * (60 / totalTime.TotalSeconds);
        }

        private void buttonRPMStart_Click(object sender, RoutedEventArgs e)
        {
            if (cache != null)
            {
                calculating = !calculating;
                if (calculating)
                {
                    timer.Start();
                    buttonRPMStart.Content = "Stop";
                }
                else
                {
                    timer.Stop();
                    buttonRPMStart.Content = "Start";
                }
            }
        }
    }
}
