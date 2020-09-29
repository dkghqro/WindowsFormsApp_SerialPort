using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp_SerialPort
{
    public class SerialCommDisconnectedEventArgs : EventArgs
    {
        public string DeviceID { get; set; }
    }
}
