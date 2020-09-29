using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp_SerialPort
{
    public class SerialCommDataReceivedEventArgs : EventArgs
    {
        public string DeviceID { get; set; }
        public byte[] ReceivedByteData { get; set; }
        public string ReceivedStringData { get; internal set; }
    }
}
