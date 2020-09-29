using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using StarMicronics.StarIO;

namespace StarSteadyLANSettingLabs
{
    public class CommunicationResult
    {
        public Communication.Result Result { get; set; }

        public int Code { get; set; }

        public CommunicationResult()
        {
            Result = Communication.Result.ErrorUnknown;
            Code = StarResultCode.ErrorFailed;
        }
    }

    public static class Communication
    {

        public enum Result
        {
            Success,
            ErrorUnknown,
            ErrorOpenPort,
            ErrorBeginCheckedBlock,
            ErrorEndCheckedBlock,
            ErrorWritePort,
            ErrorReadPort,
        }


        /// <summary>
        /// Sample : Sending commands to printer.
        /// </summary>
        public static CommunicationResult SendCommands(byte[] commands, string portName, string portSettings, int timeout)
        {
            Result result = Result.ErrorUnknown;
            int code = StarResultCode.ErrorFailed;

            IPort port = null;

            try
            {
                result = Result.ErrorOpenPort;

                if (portName == null)
                {
                    throw new PortException("portName is null.");
                }

                port = Factory.I.GetPort(portName, portSettings, timeout);

                if (port == null)
                {
                    throw new PortException("port is null.");
                }

                StarPrinterStatus status;

                result = Result.ErrorWritePort;

                status = port.GetParsedStatus();

                if (status.Offline == true)
                {
                    throw new PortException("Printer is Offline.");
                }

                uint commandsLength = (uint)commands.Length;

                uint writtenLength = port.WritePort(commands, 0, commandsLength);

                if (writtenLength != commandsLength)
                {
                    throw new PortException("WritePort failed.");
                }

                result = Result.Success;
                code = StarResultCode.Succeeded;
            }
            catch (PortException ex)
            {
                code = ex.ErrorCode;
            }
            finally
            {
                if (port != null)
                {
                    Factory.I.ReleasePort(port);
                }
            }

            return new CommunicationResult()
            {
                Result = result,
                Code = code
            };
        }


 
        /// <summary>
        /// Sample : Getting steadylan setting.
        /// </summary>
        public static CommunicationResult ConfirmSteadyLANSetting(ref string receiveSettting, string portName, string portSettings, int timeout)
        {
            Result result = Result.ErrorUnknown;
            int code = StarResultCode.ErrorFailed;
            receiveSettting = "";
            IPort port = null;

            try
            {
                result = Result.ErrorOpenPort;

                if (portName == null)
                {
                    throw new PortException("portName is null.");
                }

                port = Factory.I.GetPort(portName, portSettings, timeout);

                if (port == null)
                {
                    throw new PortException("port is null.");
                }

                StarPrinterStatus status;

                result = Result.ErrorWritePort;

                status = port.GetParsedStatus();

                if (status.Offline == true)
                {
                    throw new PortException("Printer is Offline.");
                }

                byte[] commands = new byte[] { 0x1b, 0x1d, 0x29, 0x4e, 0x02, 0x00, 0x49, 0x01 };  //confirm SteadyLAN setting

                port.WritePort(commands, 0, (uint)commands.Length);

                result = Result.ErrorReadPort;
                byte[] readBuffer = new byte[1024];
                List<byte> allReceiveData = new List<byte>();

                uint startDate = (uint)Environment.TickCount;

                while (true)
                {
                    if ((UInt32)Environment.TickCount - startDate >= 3000) // Timeout
                    {
                        throw new PortException("ReadPort timeout.");
                    }

                    uint receiveSize = port.ReadPort(ref readBuffer, 0, (uint)readBuffer.Length);

                    if (receiveSize == 0)
                    {
                        continue;
                    }

                    byte[] receiveData = new byte[receiveSize];
                    Array.Copy(readBuffer, 0, receiveData, 0, receiveSize);

                    allReceiveData.AddRange(receiveData);

                    bool receiveResponse = false;

                    int totalReceiveSize = allReceiveData.Count;

                    //Check the steadyLAN setting value
                    //The following format is transmitted.
                    //  0x1b 0x1d 0x29 0x4e 0x02 0x00 0x49 0x01 [n] 0x0a 0x00
                    //The value of [n] indicates the SteadyLAN setting.
                    //  0x00: Invalid, 0x01: Valid(For iOS), 0x02: Valid(For Android), 0x03: Valid(For Windows)
                    if (totalReceiveSize >= 11)
                    {
                        for (int i = 0; i < totalReceiveSize; i++)
                        {
                            if (allReceiveData[i + 0] == 0x1b &&
                                allReceiveData[i + 1] == 0x1d &&
                                allReceiveData[i + 2] == 0x29 &&
                                allReceiveData[i + 3] == 0x4e &&
                                allReceiveData[i + 4] == 0x02 &&
                                allReceiveData[i + 5] == 0x00 &&
                                allReceiveData[i + 6] == 0x49 &&
                                allReceiveData[i + 7] == 0x01 &&
                            //  allReceiveData[i + 8] is stored the SteadyLAN setting value.
                                allReceiveData[i + 9] == 0x0a &&
                                allReceiveData[i + 10] ==0x00)
                            {
                                switch (allReceiveData[i + 8])
                                {
                                //  case 0x00:
                                    default:
                                        receiveSettting = "SteadyLAN(Disable).";
                                        break;
                                    case 0x01:
                                        receiveSettting = "SteadyLAN(for iOS).";
                                        break;
                                    case 0x02:
                                        receiveSettting = "SteadyLAN(for Android).";
                                        break;
                                    case 0x03:
                                        receiveSettting = "SteadyLAN(for Windows).";
                                        break;
                                }

                                receiveResponse = true;
                                break;
                            }
                        }
                    }

                    if (receiveResponse)
                    {
                        break;
                    }
                }

                result = Result.Success;
                code = StarResultCode.Succeeded;
            }
            catch (PortException ex)
            {
                code = ex.ErrorCode;
            }
            finally
            {
                if (port != null)
                {
                    Factory.I.ReleasePort(port);
                }
            }

            return new CommunicationResult()
            {
                Result = result,
                Code = code
            };
        }



        public static void ShowCommunicationResultMessage(CommunicationResult result)
        {
            string resultMessage = GetCommunicationResultMessage(result);

            MessageBox.Show(resultMessage, "Communication Result");
        }

        public static string GetCommunicationResultMessage(CommunicationResult result)
        {
            StringBuilder builder = new StringBuilder();

            switch (result.Result)
            {
                case Result.Success:
                    builder.Append("Success!");
                    break;
                case Result.ErrorOpenPort:
                    builder.Append("Fail to openPort");
                    break;
                case Result.ErrorBeginCheckedBlock:
                    builder.Append("Printer is offline (BeginCheckedBlock)");
                    break;
                case Result.ErrorEndCheckedBlock:
                    builder.Append("Printer is offline (EndCheckedBlock)");
                    break;
                case Result.ErrorReadPort:
                    builder.Append("Read port error (ReadPort)");
                    break;
                case Result.ErrorWritePort:
                    builder.Append("Write port error (WritePort)");
                    break;
                default:
                case Result.ErrorUnknown:
                    builder.Append("Unknown error");
                    break;
            }

            if (result.Result != Result.Success)
            {
                builder.Append(Environment.NewLine);
                builder.Append(Environment.NewLine);
                builder.Append("Error code: ");
                builder.Append(result.Code.ToString());

                if (result.Code == StarResultCode.ErrorFailed)
                {
                    builder.Append(" (Failed)");
                }
                else if (result.Code == StarResultCode.ErrorInUse)
                {
                    builder.Append(" (In use)");
                }
            }

            return builder.ToString();
        }

    }
}

