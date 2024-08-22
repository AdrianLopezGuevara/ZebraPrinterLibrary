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
    void Connect(string ip, int port);
    Task<string> GetPrinterResponseAsync(string command);
    Task<string> GetStatusAsync();
    Task<string> GetLogsRFIDAsync();
    Task<float> GetSpeedAsync();
    Task<float> GetResolutionAsync();
    Task<float> GetLabelLengthAsync();
    Task<int> GetValidLabelsAsync();
    Task<int> GetVoidLabelsAsync();
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
