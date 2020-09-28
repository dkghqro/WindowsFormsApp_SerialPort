using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace SerialCommLib
{
    public class SerialComm
    {
        public delegate void DataReceivedHandlerFunc(string deviceID, byte[] receiveData);
        public DataReceivedHandlerFunc DataReceivedHandler;

        public delegate void DisconnectedHandlerFunc(string deviceID);
        public DisconnectedHandlerFunc DisconnectedHandler;

        public SerialPort serialPort;
        public int sleepTimeRecv = 100;
        public int sleepTimeCheck = 200;
        public string deviceID = "";

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

        public string OpenComm(string portName, int baudrate, int databits, StopBits stopbits, Parity parity, Handshake handshake)
        {
            try
            {
                serialPort = new SerialPort();
                serialPort.PortName = portName;
                serialPort.BaudRate = baudrate;
                serialPort.DataBits = databits;
                serialPort.StopBits = stopbits;
                serialPort.Parity = parity;
                serialPort.Handshake = handshake;

                // 디바이스에 설정에 따라 분기
                serialPort.Encoding = new System.Text.ASCIIEncoding();
                serialPort.NewLine = "\r\n";
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
                DataReceivedHandler?.Invoke(deviceID, bytesBuffer);

                string strBuffer = Encoding.ASCII.GetString(bytesBuffer);
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

                        DisconnectedHandler?.Invoke(deviceID);

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
