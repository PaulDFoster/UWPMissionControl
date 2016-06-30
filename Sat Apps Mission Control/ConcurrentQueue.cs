using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Sat_Apps_Mission_Control
{

    public sealed partial class MainPage : Page
    {
        bool cloudFlag = false;

        ConcurrentQueue<string> cq = new ConcurrentQueue<string>();

        // ConcurrentQueue listener

        async Task StartBackgroundLoop()
        {
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            try
            {
                await Task.Run(
                  async () =>
                  {
                  SIKData SIKdata = new SIKData();
                  string data;
                  char[] braces = new char[] { '{', '}' };
                  while (true)
                  {
                      if (cq.TryDequeue(out data))
                      {
                          //Debug.WriteLine(string.Format("{0}**", data));
                          // Process into fields
                          SIKdata = await ProcessData(data.Trim(braces));
                          try
                          {
                              // Update local cache
                              Cache.Add(SIKdata);
                              CurrentRecordCache = (SIKDataViewModel)SIKdata;
                              RunOnGUI(() =>
                                { 
                                    SensorsLive.SIKdata = CurrentRecordCache;
                                });
                                  
                                  
                              }
                              catch (Exception ex)
                              {
                                  Debug.WriteLine(ex.Message);
                              }

                              try
                              {
                                  // Store record as CSV via a datawriter to a stream. How to do with calls?
                                  // await WriteSIKData(data);
                                  // Post to IoT Hub if enabled by user
                                  if(cloudFlag) SendToCloud(SIKdata);
                              }
                              catch (Exception exf)
                              {
                                  Debug.WriteLine(exf.Message);
                              }

                          }
                      }

                  }, cancelToken.Token

                  );
            }
            catch (OperationCanceledException)
            {

            }
        }

        async Task<SIKData> ProcessData(string data)
        {
            SIKData SIKdata = new SIKData();
            //Debug.WriteLine(string.Format("{0}***", data));
            try
            {
                string[] fields = data.Split(',');

                foreach (string f in fields)
                {
                    string[] keyData = f.Split(':');
                    switch (keyData[0])
                    {
                        // No processing for N = Satellite Id
                        case "I":
                            SIKdata.IR = Convert.ToInt32(keyData[1]);
                            break;
                        case "U":
                            SIKdata.UV = Convert.ToInt32(keyData[1]);
                            break;
                        case "V":
                            SIKdata.Visible = Convert.ToInt32(keyData[1]);
                            break;
                        case "T":
                            SIKdata.Temperature = Convert.ToInt32(keyData[1]);
                            break;
                        case "H":
                            SIKdata.Heading = Convert.ToInt32(keyData[1]);
                            SIKdata.ModifiedHeading = SIKdata.Heading - 46;
                            break;
                        case "X":
                            SIKdata.X = Convert.ToInt32(keyData[1]);
                            break;
                        case "Y":
                            SIKdata.Y = Convert.ToInt32(keyData[1]);
                            break;
                        case "Z":
                            SIKdata.Z = Convert.ToInt32(keyData[1]);
                            break;
                        case "P":
                            SIKdata.Pitch = Convert.ToInt32(keyData[1]);
                            break;
                        case "R":
                            SIKdata.Roll = Convert.ToInt32(keyData[1]);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return SIKdata;
        }

        void SendToCloud(SIKData SIKdata)
        {

            try
            {
                SendDeviceToCloudMessagesAsync(SIKdata);
                this.state.cloudWire.Update(DataFlow.Active);
                this.state.cloudWire.Update(WireState.Solid);

                RunOnGUI(() =>
                {
                    totalMessagesSent += 1;
                    this.state.messagesSent.Update(totalMessagesSent.ToString());
                });
            }
                 
            catch
            {
                state.cloudWire.Update(DataFlow.Stopped);
                state.cloudWire.Update(WireState.Cut);
                return;
            }
}

    }
}
