/// <summary>
/// Application       NETIO PowerBOX 4Kx Json Demo Application with power consumption
/// Author            Johannes Grunedal (grunedal@gmail.com)
/// Environment       Built, run and tested with Visual Studio 2019
///                   Target framework: .Net 5
///                   Platform: x64
///                   
/// Version           0.1 (2021-04-12)
/// 
/// NOTE!             This application is tested with NETIO PowerBOX 4KF, minor changes may be needed to be run on other devices.
///                   To keep demo simple and short, data validation and error handling is reduced to a minimum.
/// 
/// Getting started                   
///                    1. Enable read/write in web config
///                       M2M API Protocols -> JSON API -> Enable JSON API -> Enable READ-WRITE
///                    2. Enter username and password (default is netio).
///                    3. Click Save Changes.
///                    4. Note current NETIO IP address.
///                    5. Build and run this application.
///                    6. Enter IP address, user and password.
///                    7. Click Connect button.
///                    8. If successful, Info field is populated with current NETIO data (if not, check above settings).
///                    9. Set selected outputs to ON/OFF using the Control buttons.
///                   10. Click Status button to read current NETIO putput status.
///                   11. Every second voltage, current and power values are read and updated.
///                   
/// Run standalone
///                   1. Create a new C#/WPF project.
///                   2. Add NetIoDriver.cs to your project.
///                   3. Include it: using NetIo;
///                   4. Depending on your settings, one or more namespaces may have to be included (see using's below).
///                   5. Init driver: 
///                      netIoDriver = new NetIoDriver("192.168.11.250", netio, netio); // init driver
///                   6. Test connection:
///                      var agent = netIoDriver.GetAgent(); // get current device info
///                   7. Read data:
///                      var model = agent.Model;            // reads out all agent info
///                      var root = netIoDriver.GetRoot();   // get all data
///                   8. Set output:
///                      var isOutput2Set = netIoDriver.SetState(NetIoDriver.OutputName.Output_2, NetIoDriver.Action.On); // turn on output 2
///                   9. Read output:
///                      var output4 = netIoDriver.GetOutput(NetIoDriver.OutputName.Output_4); // read output 4
///                  10. Enjoy!
///                      
/// 
/// NETIO PowerBOX 4Kx Json Demo Application.
/// Copyright © Johannes Grunedal 2021 (grunedal@gmail.com). All rights reserved.
/// Redistribution and use in source and binary forms, with or without modification, 
/// are permitted provided that the following conditions are met:
/// 
///  - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
///  - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer 
///    in the documentation and/or other materials provided with the distribution.
///  - Neither the name of Django nor the names of its contributors may be used to endorse or promote products derived from this 
///    software without specific prior written permission.
///    
/// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
/// BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.IN NO EVENT SHALL 
/// THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES 
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
/// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
/// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </summary>
namespace NetIoJsonDemo
{
    using NetIo;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Threading.Tasks;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // variables
        NetIoDriver netIoDriver;

        // constants
        const string version = "0.1 (2021-04-12)";

        // functions
        /// <summary>
        /// Application start.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            setup();
        }

        /// <summary>
        /// Main setup.
        /// </summary>
        private void setup()
        {
            guiVersion.Content = version;
            enableControls(false);
        }

        /// <summary>
        /// Try to connect/communicate with NETIO device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void onConnectClick(object sender, RoutedEventArgs e)
        {
            // init driver
            netIoDriver = new NetIoDriver(guiIpAddress.Text, guiUsername.Text, guiPassword.Text);

            // test communication with NETIO, read out Agent info
            var agent = netIoDriver.GetAgent();
                        
            // show result 
            if (agent is null)
            {
                MessageBox.Show($"Could not find/connect to NETIO device.\r\nMake sure IP address, username and/or password is correct.", "Connect error", MessageBoxButton.OK, MessageBoxImage.Error);
                enableControls(false);
                return;
            }
            else
            {
                guiModel.Text = agent.Model;
                guiModelVersion.Text = agent.Version;
                guiJsonVersion.Text = agent.JSONVer;
                guiDeviceName.Text = agent.DeviceName;
                guiOemId.Text = agent.OemID.ToString();
                guiSerialNumber.Text = agent.SerialNumber;
                guiNoOfOutputs.Text = agent.NumOutputs.ToString();

                enableControls(true);
                updateGuiWithCurrentPowerswitchStatus();
                await startPowerIntervalReaderAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Read current powerswitch settings and adjust GUI accordingly.
        /// </summary>
        private void updateGuiWithCurrentPowerswitchStatus()
        {
            var root = netIoDriver.GetRoot();
            rbStatusOutput_1.IsChecked = NetIoDriver.OutputStatus.On == root.Outputs[0].State;
            rbStatusOutput_2.IsChecked = NetIoDriver.OutputStatus.On == root.Outputs[1].State;
            rbStatusOutput_3.IsChecked = NetIoDriver.OutputStatus.On == root.Outputs[2].State;
            rbStatusOutput_4.IsChecked = NetIoDriver.OutputStatus.On == root.Outputs[3].State;
        }

        /// <summary>
        /// Update GUI output power status every second.
        /// </summary>
        /// <returns></returns>
        private Task startPowerIntervalReaderAsync()
        {
            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            guiOutput1Volt.Foreground = guiOutput2Volt.Foreground = guiOutput3Volt.Foreground = guiOutput4Volt.Foreground = Brushes.Black;
                            guiOutput1Current.Foreground = guiOutput2Current.Foreground = guiOutput3Current.Foreground = guiOutput4Current.Foreground = Brushes.Black;
                            guiOutput1Power.Foreground = guiOutput2Power.Foreground = guiOutput3Power.Foreground = guiOutput4Power.Foreground = Brushes.Black;
                        });
                        Task.Delay(850).Wait();

                        // get all data
                        var root = netIoDriver.GetRoot();

                        Dispatcher.Invoke(() =>
                        {
                            // get output 1-4 voltage [V] (NOTE! Manually forced to show U = 0 V when output state is off)
                            var voltage = root.GlobalMeasure.Voltage.DoubleToString();
                            guiOutput1Volt.Content = NetIoDriver.OutputStatus.On == root.Outputs[0].State ? voltage : "0";
                            guiOutput2Volt.Content = NetIoDriver.OutputStatus.On == root.Outputs[1].State ? voltage : "0";
                            guiOutput3Volt.Content = NetIoDriver.OutputStatus.On == root.Outputs[2].State ? voltage : "0";
                            guiOutput4Volt.Content = NetIoDriver.OutputStatus.On == root.Outputs[3].State ? voltage : "0";

                            // get output 1-4 current [mA]
                            guiOutput1Current.Content = root.Outputs[0].Current.ToString();
                            guiOutput2Current.Content = root.Outputs[1].Current.ToString();
                            guiOutput3Current.Content = root.Outputs[2].Current.ToString();
                            guiOutput4Current.Content = root.Outputs[3].Current.ToString();

                            // get output 1-4 power [W]
                            guiOutput1Power.Content = root.Outputs[0].Load.ToString();
                            guiOutput2Power.Content = root.Outputs[1].Load.ToString();
                            guiOutput3Power.Content = root.Outputs[2].Load.ToString();
                            guiOutput4Power.Content = root.Outputs[3].Load.ToString();
                        });


                        Dispatcher.Invoke(() =>
                        {
                            guiOutput1Volt.Foreground = guiOutput2Volt.Foreground = guiOutput3Volt.Foreground = guiOutput4Volt.Foreground = Brushes.Tomato;
                            guiOutput1Current.Foreground = guiOutput2Current.Foreground = guiOutput3Current.Foreground = guiOutput4Current.Foreground = Brushes.Tomato;
                            guiOutput1Power.Foreground = guiOutput2Power.Foreground = guiOutput3Power.Foreground = guiOutput4Power.Foreground = Brushes.Tomato;
                        });
                        Task.Delay(150).Wait();
                    }
                }
                catch
                {
                    // something went wrong, handle!
                    return;
                }
            });
        }

        /// <summary>
        /// Action button x clicked, set selected output and action.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void actionButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // get clicked button
                var button = ((Button)sender);

                // convert to output and action      
                var output = button.ButtonToOutput();
                var action = button.ButtonToAction();

                // set NETIO output and action        
                if (!netIoDriver.SetState(output, action))
                    throw new Exception($"Failed to set output: {Enum.GetName(output)} and action = {Enum.GetName(action)}");

                // wait for relays to set
                Thread.Sleep(666);

                // update gui with set/toggled output(s)
                updateGuiStatus();
            }
            catch
            {
                MessageBox.Show("Button click failed");
            }
        }

        /// <summary>
        /// On status button click. Show current output status info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onStatusButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var root = netIoDriver.GetRoot();
                var voltage = root.GlobalMeasure.Voltage.DoubleToString();
                var frequency = root.GlobalMeasure.Frequency.DoubleToString(1);
                var status = string.Empty;

                foreach (var output in root.Outputs)
                {
                    status += $"ID: {output.ID}\r\n";
                    status += $"Name: {output.Name}\r\n";
                    status += $"Action: {output.Action.EnumToString()}\r\n";
                    status += $"State: {output.State.EnumToString()}\r\n";
                    status += $"Power: {output.Load} [W]\r\n";
                    status += $"Current: {output.Current} [mA]\r\n";
                    status += $"Voltage: {voltage} [V]\r\n";
                    status += $"Power factor: {output.PowerFactor.DoubleToString(2)}\r\n";
                    status += $"Phase: {output.PowerFactor.DoubleToString(2)} [\u00B0]\r\n";
                    status += $"Frequency: {frequency} [Hz]\r\n";
                    status += $"Energy: {output.Energy} [Wh]\r\n\r\n";
                }

                MessageBox.Show(status, "NETIO 4 Status");
            }
            catch
            {
                MessageBox.Show("Failed to get output status!");
            }
        }

        /// <summary>
        /// Update current NETIO output status in GUI.
        /// </summary>
        private void updateGuiStatus()
        {
            var output = netIoDriver.GetAllOutputs();

            rbStatusOutput_1.IsChecked = NetIoDriver.OutputStatus.On == output[0].State;
            rbStatusOutput_2.IsChecked = NetIoDriver.OutputStatus.On == output[1].State;
            rbStatusOutput_3.IsChecked = NetIoDriver.OutputStatus.On == output[2].State;
            rbStatusOutput_4.IsChecked = NetIoDriver.OutputStatus.On == output[3].State;
        }

        /// <summary>
        /// Show/hide selected GUI controls.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void enableControls(bool isEnabled)
        {
            // show/hide 'info' controls         
            List<TextBox> textboxes = new();
            getAllChildren<TextBox>(guiInfoGrid, textboxes);
            textboxes.ForEach(textbox => textbox.IsEnabled = isEnabled);

            // show/hide 'control' controls
            List<Button> buttons = new();
            getAllChildren<Button>(guiControlGrid, buttons);
            buttons.ForEach(button => button.IsEnabled = isEnabled);

            List<RadioButton> radiobuttons = new();
            getAllChildren<RadioButton>(guiControlGrid, radiobuttons);
            radiobuttons.ForEach(radiobutton => radiobutton.IsEnabled = isEnabled);

            // set connect button
            btnConnect.IsEnabled = !isEnabled;
        }

        private static void getAllChildren<T>(DependencyObject parent, List<T> collection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);

            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject dependencyObject = child as DependencyObject;
                    if (child is T)
                        collection.Add(child as T);

                    getAllChildren(dependencyObject, collection);
                }
            }
        }

        private void onInfoClick(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show($"Version {version}\r\n\r\nBy Johannes Grunedal 2021\r\ngrunedal@gmail.com", "NETIO PowerBOX 4KF Json Demo Application", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
