using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary.Interfaces
{
    public interface IZebraPrinterServiceFactory
    {
        IZebraPrinterService Create(string ip, int port);
    }
}
