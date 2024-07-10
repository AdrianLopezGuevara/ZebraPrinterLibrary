using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary.Commands
{
    internal class ZPLCommands
    {
        public const string calibarteLabel = "~JC\n";
        public const string restorePrinterCommand = "^XA^JUF^XZ\n";
        public const string deleteJobsCommand = "~JA\n";
        public const string pauseCommand = "~PP\n";
        public const string getRIDFLogs = "~HL\n";
        public const string continueCommand = "~PS\n";
    }
}
