using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZebraPrinterLibrary.Interfaces
{
    public interface IZebraPrinterService
    {
        Task SendCommandToPrinterAsync(string command);
        Task<string> GetPrinterResponseAsync(string command);
        Task<string> GetStatusAsync();
        Task<string> GetLogsRFIDAsync();
        Task<string> GetSpeedAsync();
        Task<string> GetResolutionAsync();
        Task<string> GetLabelLengthAsync();
        Task<string> GetValidLabelsAsync();
        Task<string> GetVoidLabelsAsync();
        Task ResetCounterAsync();
        Task RestorePrinterAsync();
        Task CalibrateRFIDAsync();
        Task CalibrateLabelAsync();
        Task DeleteJobsAsync();
        Task CleanBufferAsync();
        Task ContinueAsync();
        Task PauseAsync();
    }
}
