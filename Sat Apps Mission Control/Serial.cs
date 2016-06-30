using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.UI.Xaml.Controls;

namespace Sat_Apps_Mission_Control
{
    public sealed partial class MainPage : Page
    {
        const int bufferSize = 128;

        enum State
        {
            Outside_of_object,
            Inside_object
        }

        private async Task ReadJSON(SerialDevice device)
        {
            var currentStr = "";
            State state = State.Outside_of_object;
            var buffer = new Windows.Storage.Streams.Buffer(bufferSize);

            while (true)
            {
                try
                {
                    var result = await device.InputStream.ReadAsync(buffer, bufferSize, Windows.Storage.Streams.InputStreamOptions.None);
                    if (result.Length > 0)
                    {
                        lock (lockObj)
                        {
                            totalBytesReadFromSerial += result.Length;
                            if (bPauseDataRead)
                            {
                                continue;
                            }
                        }

                        this.state.serialWire.Update(WireState.Solid);
                        this.state.serialWire.Update(DataFlow.Active);

                        var str = System.Text.Encoding.ASCII.GetString(result.ToArray());
                        //Debug.WriteLine(string.Format("[{0}]", str));
                        foreach (var c in str)
                        {
                            switch (c)
                            {
                                case '{':
                                    switch (state)
                                    {
                                        case State.Outside_of_object:
                                            currentStr = c.ToString();
                                            state = State.Inside_object;
                                            break;
                                        case State.Inside_object:
                                            // throw new NotSupportedException("Nested JSON valued are not supported");
                                            // We got into a weird state. Get out.
                                            currentStr = c.ToString();
                                            state = State.Inside_object;
                                            break;
                                    }
                                    break;
                                case '}':
                                    switch (state)
                                    {
                                        case State.Outside_of_object:
                                            // we started reading mid-stream, but now we're truly outside
                                            break;
                                        case State.Inside_object:
                                            currentStr += c;
                                            cq.Enqueue(currentStr);
                                            await HandleJSONObject(currentStr);
                                            // Nested are not supported
                                            state = State.Outside_of_object;
                                            currentStr = "";
                                            break;
                                    }
                                    break;
                                case '\n':
                                    //noop
                                    break;
                                default:
                                    switch (state)
                                    {
                                        case State.Outside_of_object:
                                            // skip this char
                                            break;
                                        case State.Inside_object:
                                            // accumulate it:
                                            currentStr += c;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    this.state.serialWire.Update(DataFlow.Stopped);
                    this.state.serialWire.Update(WireState.Cut);
                    device.Dispose();
                    break;
                }
            }
        }

        private async Task ReadDataFromSerialPort(string id)
        {
            //string aqs = SerialDevice.GetDeviceSelector();
            //var dis = await DeviceInformation.FindAllAsync(aqs);

            //if (dis.Count == 0) return;

            //var device = await SerialDevice.FromIdAsync(dis[0].Id);
            
            var device = await SerialDevice.FromIdAsync(id);

            if (device == null) return;

            device.BaudRate = 115200;

            lock (lockObj)
            {
                this.bPauseDataRead = false;
                this.totalBytesReadFromSerial = 0;
            }

            await ReadJSON(device);
        }

        private async Task HandleJSONObject(string json)
        {
            Debug.WriteLine(string.Format("[{0}]", json));

            // Submit to concurrent queue for processing
            // Concurrent queue listener will
            // 1) Process fields into current state collection, triggering UI update
            // 2) Store in file as CSV for later analysis
            // 3) Post to IoTHub

            //cq.Enqueue(json);
            
            
        }
    }
}
