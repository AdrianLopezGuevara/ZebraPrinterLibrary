using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZebraPrinterLibrary.Interfaces;

namespace ZebraPrinterLibrary.Services
{
    public class ZebraPrinterServiceFactory : IZebraPrinterServiceFactory
    {
        public IZebraPrinterService Create(string ip, int port)
        {
            return new ZebraPrinterService(ip, port);
        }
    }
}
