﻿using SerialCommLib;
using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp_SerialPort
{
    public partial class Form1 : Form
    {
        SerialComm scan1;
        SerialComm scan2;

        public Form1()
        {
            InitializeComponent();

            #region 단일 포트 테스트

            //comboBox1.DataSource = SerialPort.GetPortNames();
            //comboBox2.DataSource = Enum.GetValues(typeof(StopBits));
            //comboBox3.DataSource = Enum.GetValues(typeof(Parity));

            //textBox1.Text = "9600";
            //textBox2.Text = "8";
            //comboBox2.Text = "One";

            #endregion 단일 포트 테스트
        }

        private void DataReceivedHandler_SerialComm(string deviceID, byte[] recvData)
        {
            //Console.WriteLine(recvData);

            this.Invoke((MethodInvoker)delegate
            {
                string recvDataStr = Encoding.ASCII.GetString(recvData);

                switch (deviceID)
                {
                    case "SCAN1":
                    case "SCAN2":
                        richTextBox1.AppendText("[" + deviceID + "]" + recvDataStr);
                        richTextBox1.Focus();

                        break;
                };

            });
        }

        private void DisconnectedHandler_SerialComm(string deviceID)
        {
            //Console.WriteLine("serial disconnected");

            this.Invoke((MethodInvoker)delegate
            {
                switch (deviceID)
                {
                    case "SCAN1":
                    case "SCAN2":
                        richTextBox1.AppendText("serial disconnected(" + scan1.serialPort.PortName + ")\r\n");
                        richTextBox1.Focus();
                        break;
                };

            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            #region 단일 포트 테스트

            //var portName = comboBox1.Text;
            //var baudRate = Convert.ToInt32(textBox1.Text);
            //var dataBits = Convert.ToInt32(textBox2.Text);
            //var stopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBox2.Text);
            //var parity = (Parity)Enum.Parse(typeof(Parity), comboBox3.Text);

            //scan1 = new SerialComm();
            //scan1._deviceID = "SCAN1";
            //scan1.DataReceivedHandler = DataReceivedHandler;
            //scan1.DisconnectedHandler = DisconnectedHandler;

            //if (scan1.IsOpen) scan1.CloseComm();
            //var openResult = scan1.OpenComm(portName, baudRate, dataBits, stopBits, parity, Handshake.None);

            //if (openResult) richTextBox1.AppendText("Open success(" + scan1.serialPort.PortName + ")\r\n");
            //else richTextBox1.AppendText("Open failed(" + scan1.serialPort.PortName + ")\r\n");

            #endregion 단일 포트 테스트

            #region 다중 포트 테스트

            scan1 = new SerialComm();
            scan1.deviceID = "SCAN1";
            scan1.sleepTimeRecv = 100;
            scan1.DataReceivedHandler = DataReceivedHandler_SerialComm;
            scan1.DisconnectedHandler = DisconnectedHandler_SerialComm;

            if (scan1.IsOpen) scan1.CloseComm();

            var resultMsg1 = scan1.OpenComm("COM2", 9600, 8, StopBits.One, Parity.None, Handshake.None);
            richTextBox1.AppendText(resultMsg1);

            scan2 = new SerialComm();
            scan2.deviceID = "SCAN2";
            scan2.sleepTimeRecv = 100;
            scan2.DataReceivedHandler = DataReceivedHandler_SerialComm;
            scan2.DisconnectedHandler = DisconnectedHandler_SerialComm;

            if (scan2.IsOpen) scan2.CloseComm();

            var resultMsg2 = scan2.OpenComm("COM3", 9600, 8, StopBits.One, Parity.None, Handshake.None);
            richTextBox1.AppendText(resultMsg2);

            #endregion 다중 포트 테스트

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (scan1.IsOpen)
            {
                var resultMsg1 = scan1.CloseComm();
                richTextBox1.AppendText(resultMsg1);
            }

            if (scan2.IsOpen)
            {
                var resultMsg1 = scan2.CloseComm();
                richTextBox1.AppendText(resultMsg1);
            }
        }

    }
}
