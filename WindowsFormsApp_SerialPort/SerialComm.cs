using DevExpress.Utils.Svg;
using System;
using System.CodeDom;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using WindowsFormsApp_SerialPort;

namespace SerialCommLib
{
    public class SerialComm
    {
        public delegate void SerialCommDataReceivedHandler(object sender, SerialCommDataReceivedEventArgs e);
        public event SerialCommDataReceivedHandler DataReceived;

        public delegate void DisconnectedHandler(object sender,  SerialCommDisconnectedEventArgs e);
        public event DisconnectedHandler Disconnected;

        public SerialPort serialPort;
        private int sleepTimeRecv = 100;
        private int sleepTimeCheck = 200;
        private string _deviceID = "";
        
        public bool IsOpen
        {
            get
            {
                if (serialPort != null) return serialPort.IsOpen;
                return false;
            }
        }

        // serial port check
        private Thread threadCheckSerialOpen;
        private bool isThreadCheckSerialOpen = false;

        public SerialComm()
        {
        }
        public SerialComm(string deviceID)
        {
            _deviceID = deviceID;
        }
        public string OpenComm(string portName)
        {
            return this.OpenComm(portName, 9600, 8, StopBits.One, Parity.None, Handshake.None);
        }

        public string OpenComm(string portName, int baudrate, int databits, StopBits stopbits, Parity parity, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort
                {
                    PortName = portName,
                    BaudRate = baudrate,
                    DataBits = databits,
                    StopBits = stopbits,
                    Parity = parity,
                    Handshake = handshake,

                    // 디바이스에 설정에 따라 분기
                    Encoding = new System.Text.ASCIIEncoding(),
                    NewLine = "\r\n"
                };
                serialPort.NewLine = Environment.NewLine;
                serialPort.ErrorReceived += serialPort_ErrorReceived;
                serialPort.DataReceived += serialPort_DataReceived;
                serialPort.Open();

                StartCheckSerialOpenThread();

                return "open success(" + portName + ")" + "\r\n";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                return "open failed(" + portName + ")" + "\r\n";
            }
        }

        public string CloseComm()
        {
            try
            {
                if (serialPort != null)
                {
                    StopCheckSerialOpenThread();

                    var closePortName = serialPort.PortName;
                    
                    serialPort.Close();
                    serialPort = null;
                
                    return "port closed success(" + closePortName + ")"+ "\r\n";
                }

                return "Undefined port" + "\r\n";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

                return "port closed failed(" + serialPort.PortName + ")" + "\r\n" + ex?.ToString() + "\r\n";
            }
        }

        public bool Send(string sendData)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Write(sendData);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool Send(byte[] sendData)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Write(sendData, 0, sendData.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        public bool Send(byte[] sendData, int offset, int count)
        {
            try
            {
                if (serialPort != null && serialPort.IsOpen)
                {
                    serialPort.Write(sendData, offset, count);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            return false;
        }

        private byte[] ReadSerialByteData()
        {
            serialPort.ReadTimeout = 100;

            byte[] bytesBuffer = new byte[serialPort.BytesToRead];
            int bufferOffset = 0;
            int bytesToRead = serialPort.BytesToRead;

            while (bytesToRead > 0)
            {
                try
                {
                    int readBytes = serialPort.Read(bytesBuffer, bufferOffset, bytesToRead - bufferOffset);
                    bytesToRead -= readBytes;
                    bufferOffset += readBytes;
                }
                catch (TimeoutException ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }

            return bytesBuffer;
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(sleepTimeRecv);

                byte[] bytesBuffer = ReadSerialByteData();
                string strBuffer = Encoding.ASCII.GetString(bytesBuffer);
                DataReceived?.Invoke(this, new SerialCommDataReceivedEventArgs {DeviceID = _deviceID, ReceivedByteData = bytesBuffer, ReceivedStringData = strBuffer });
                Debug.WriteLine("received(" + strBuffer.Length + ") : " + strBuffer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.WriteLine(e.ToString());
        }

        private void StartCheckSerialOpenThread()
        {
            isThreadCheckSerialOpen = true;
            threadCheckSerialOpen = new Thread(new ThreadStart(ThreadCheckSerialOpen));
            threadCheckSerialOpen.Start();
        }

        private void StopCheckSerialOpenThread()
        {
            if (isThreadCheckSerialOpen)
            {
                isThreadCheckSerialOpen = false;

                if (Thread.CurrentThread != threadCheckSerialOpen)
                    threadCheckSerialOpen.Join();
            }
        }

        private void ThreadCheckSerialOpen()
        {
            while (isThreadCheckSerialOpen)
            {
                Thread.Sleep(sleepTimeCheck);

                try
                {
                    if (serialPort == null || !serialPort.IsOpen)
                    {
                        Debug.WriteLine("seriaport disconnected");
                        Disconnected?.Invoke(this, new SerialCommDisconnectedEventArgs { DeviceID = _deviceID });
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

    }

}
