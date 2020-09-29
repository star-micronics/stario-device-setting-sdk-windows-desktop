using StarMicronics.StarIO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace StarSteadyLANSettingLabs
{
    public partial class MainWindow : Window
    {
        private string portName;
        private string modeName;
        private string macAddress;

        public MainWindow()
        {
            InitializeComponent();
        }


        private void applyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Apply SteadyLAN Setting");

            byte[] commands;
            if (steadyLANSettingComboBox.SelectedIndex == 1)
            {
                commands = new byte[]{ 0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x39, 0x01, 0x03,  //set to SteadyLAN(for Windows)
                                       0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x70, 0x01, 0x00}; //apply setting. Note: The printer is reset to apply setting when writing this command is completed.};

                //The settings for other OSs are as follows. But it will not work on Windows devices.
            //  commands = new byte[]{ 0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x39, 0x01, 0x01,  //set to SteadyLAN(for iOS)
            //                         0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x70, 0x01, 0x00}; //apply setting. Note: The printer is reset to apply setting when writing this command is completed.};
            //  commands = new byte[]{ 0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x39, 0x01, 0x02,  //set to SteadyLAN(for Android)
            //                         0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x70, 0x01, 0x00}; //apply setting. Note: The printer is reset to apply setting when writing this command is completed.};

            }
            else
            {
                commands = new byte[]{ 0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x39, 0x01, 0x00,  //set to SteadyLAN(Disable)
                                       0x1b, 0x1d, 0x29, 0x4e, 0x03, 0x00, 0x70, 0x01, 0x00}; //apply setting. Note: The printer is reset to apply setting when writing this command is completed.};
            }

            CommunicationResult result = Communication.SendCommands(commands, portName, "", 10000);

            Communication.ShowCommunicationResultMessage(result);

        }


        private void readSettingButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Read SteadyLAN Setting");

            string steadyLANSetting = "";

            CommunicationResult result = Communication.ConfirmSteadyLANSetting(ref steadyLANSetting, portName, "", 10000);

            if (result.Result == Communication.Result.Success)
            {
                MessageBox.Show(steadyLANSetting, "SteadyLAN Setting");
            }
            else
            {
                Communication.ShowCommunicationResultMessage(result);
            }
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Search Star Printer");

            List<PortInfo> portInfoList = new List<PortInfo>();

            portInfoList = Factory.I.SearchPrinter(); //ALL
//            portInfoList = Factory.I.SearchPrinter(PrinterInterfaceType.Ethernet);        //LAN
//            portInfoList = Factory.I.SearchPrinter(PrinterInterfaceType.Bluetooth);       //Bluetooth
//            portInfoList = Factory.I.SearchPrinter(PrinterInterfaceType.USBPrinterClass); //USBPrinterClass

            portListBox.ItemsSource = PortInfoManager.CreatePortInfoManager(portInfoList.ToArray());

        }

        private void PortListBoxItem_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;

            PortInfo selectedPortInfo = (PortInfo)clickedButton.Tag;

            this.portName   = selectedPortInfo.PortName;
            this.modeName   = selectedPortInfo.ModelName;
            this.macAddress = selectedPortInfo.MacAddress;
        }


    }
}
