using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary.Commands
{
    internal class GetCommands
    {
        public const string getVoidLabelsCommand = "! U1 getvar \"odometer.rfid.void_resettable\"\n";
        public const string getValidLabelsCommand = "! U1 getvar \"odometer.rfid.valid_resettable\"\n";
        public const string getLabelLengthCommand = "! U1 getvar \"zpl.label_length\"\n";
        public const string getSpeedCommand = "! U1 getvar \"media.speed\"\n";
        public const string getResolution = "! U1 getvar \"head.resolution.in_dpi\"\n";
        public const string system_error = "! U1 getvar \"zpl.system_error\"\n";
        public const string getStatus = "! U1 getvar \"device.status\"\n";
    }
}
