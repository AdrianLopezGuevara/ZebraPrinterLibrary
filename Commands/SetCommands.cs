using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary.Commands
{
    internal class SetCommands
    {
        public const string calibrateRFID = "! U1 setvar \"rfid.tag.calibrate\" \"run\"\n";
        public const string resetCounterCommand = "! U1 setvar \"odometer.rfid.valid_resettable\" \"0\" \n ! U1 setvar \"odometer.rfid.void_resettable\" \"0\" \n";
        public const string cleanBufferCommand = "! U1 setvar \"rfid.log.clear\" \"\"";

    }
}
