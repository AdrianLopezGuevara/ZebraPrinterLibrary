using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary
{
    internal class Commands
    {
        public const string calibarteLabel = "~JC\n";
        public const string calibrateRFID = "! U1 setvar \"rfid.tag.calibrate\" \"run\"\n";
        public const string restorePrinterCommand = "^XA^JUF^XZ\n";
        public const string resetCounterCommand = "! U1 setvar \"odometer.rfid.valid_resettable\" \"0\" \n ! U1 setvar \"odometer.rfid.void_resettable\" \"0\" \n";
        public const string deleteJobsCommand = "~JA\n";
        public const string pauseCommand = "~PP\n";
        public const string getVoidLabelsCommand = "! U1 getvar \"odometer.rfid.void_resettable\"\n";
        public const string getValidLabelsCommand = "! U1 getvar \"odometer.rfid.valid_resettable\"\n";
        public const string getLabelLengthCommand = "! U1 getvar \"zpl.label_length\"\n";
        public const string getSpeedCommand = "! U1 getvar \"media.speed\"\n";
        public const string getResolution = "! U1 getvar \"head.resolution.in_dpi\"\n";
        public const string cleanBufferCommand = "! U1 setvar \"rfid.log.clear\" \"\"";
        public const string getRIDFLogs = "~HL\n";
        public const string continueCommand = "~PS\n";
        public const string getStatus = "! U1 getvar \"device.status\"\n";
    }
}
