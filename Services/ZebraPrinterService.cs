using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZebraPrinterLibrary.Interfaces;

namespace ZebraPrinterLibrary.Services
{
    public class ZebraPrinterService : IDisposable, IZebraPrinterService
    {
        private Socket _socket;
        private bool _disposed = false;

        public ZebraPrinterService(string ip, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(ip, port);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to the printer.", ex);
            }
        }

        public async Task SendCommandToPrinterAsync(string command)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ZebraPrinterService));

            if (!_socket.Connected)
                throw new InvalidOperationException("No connection to the printer.");

            try
            {
                byte[] commandBytes = Encoding.UTF8.GetBytes(command);
                await _socket.SendAsync(commandBytes, SocketFlags.None);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to send command to the printer.", ex);
            }
        }

        public async Task<string> GetPrinterResponseAsync(string command)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ZebraPrinterService));

            if (!_socket.Connected)
                throw new InvalidOperationException("No connection to the printer.");

            string response = "";
            try
            {
                byte[] commandBytes = Encoding.ASCII.GetBytes(command);
                await _socket.SendAsync(commandBytes, SocketFlags.None);
                byte[] bytes = new byte[3072];
                int bytesRec = await _socket.ReceiveAsync(bytes, SocketFlags.None);
                response = Encoding.ASCII.GetString(bytes, 0, bytesRec).Replace("\"", "");
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Failed to get response from the printer.", ex);
            }
            return response;
        }

        public async Task<string> GetStatusAsync()
        {
            string status = "Ok";
            string sHostStatus = await GetPrinterResponseAsync(Commands.getStatus);
            string pattern = @"\d+";
            MatchCollection matches = Regex.Matches(sHostStatus, pattern);
            string statusResponse = string.Join("", matches);

            Dictionary<int, Dictionary<char, string>> Errors = new()
                {
                    { 13, new Dictionary<char, string>
                        {
                            { '8', "Marca negra no encontrada" },
                            { '4', "Error de calibración de marca negra" },
                            { '2', "Tiempo de espera de la función de retracción agotado" },
                            { '1', "Impresora en pausa" }
                        }
                    },
                    { 14, new Dictionary<char, string>
                        {
                            { '8', "Fallo al limpiar la ruta del papel" },
                            { '4', "Error de alimentación de papel" },
                            { '2', "Presentador no en ejecución" },
                            { '1', "Atasco de papel durante la retracción" }
                        }
                    },
                    { 15, new Dictionary<char, string>
                        {
                            { '2', "Termistor de cabezal de impresión abierto" },
                            { '1', "Configuración de firmware no válida" }
                        }
                    },
                    { 16, new Dictionary<char, string>
                        {
                            { '8', "Error de detección de cabezal de impresión" },
                            { '4', "Elemento de cabezal de impresión defectuoso" },
                            { '2', "Motor sobre temperatura" },
                            { '1', "Cabezal de impresión sobre temperatura" }
                        }
                    },
                    { 17, new Dictionary<char, string>
                        {
                            { '8', "Fallo de la cortadora" },
                            { '4', "Cabezal abierto" },
                            { '2', "Cinta agotada" },
                            { '1', "Medios agotados" }
                        }
                    }
                };

            if (statusResponse[0].Equals('1'))
            {
                status = "";
                foreach (var index in Errors)
                {
                    foreach (var error in index.Value)
                    {
                        if (statusResponse[index.Key].Equals(error.Key))
                        {
                            status += $"- {error.Value}.";
                        }
                    }
                }
            }

            return status;
        }

        public async Task<string> GetLogsRFIDAsync()
        {
            return await GetPrinterResponseAsync(Commands.getRIDFLogs);
        }

        public async Task<string> GetSpeedAsync()
        {
            return await GetPrinterResponseAsync(Commands.getSpeedCommand);
        }

        public async Task<string> GetResolutionAsync()
        {
            return await GetPrinterResponseAsync(Commands.getResolution);
        }

        public async Task<string> GetLabelLengthAsync()
        {
            return await GetPrinterResponseAsync(Commands.getLabelLengthCommand);
        }

        public async Task<string> GetValidLabelsAsync()
        {
            return await GetPrinterResponseAsync(Commands.getValidLabelsCommand);
        }

        public async Task<string> GetVoidLabelsAsync()
        {
            return await GetPrinterResponseAsync(Commands.getVoidLabelsCommand);
        }

        public async Task ResetCounterAsync()
        {
            await SendCommandToPrinterAsync(Commands.resetCounterCommand);
        }

        public async Task RestorePrinterAsync()
        {
            await SendCommandToPrinterAsync(Commands.restorePrinterCommand);
        }

        public async Task CalibrateRFIDAsync()
        {
            await SendCommandToPrinterAsync(Commands.calibrateRFID);
        }

        public async Task CalibrateLabelAsync()
        {
            await SendCommandToPrinterAsync(Commands.calibarteLabel);
        }

        public async Task DeleteJobsAsync()
        {
            await SendCommandToPrinterAsync(Commands.deleteJobsCommand);
        }

        public async Task CleanBufferAsync()
        {
            await SendCommandToPrinterAsync(Commands.cleanBufferCommand);
        }

        public async Task ContinueAsync()
        {
            await SendCommandToPrinterAsync(Commands.continueCommand);
        }

        public async Task PauseAsync()
        {
            await SendCommandToPrinterAsync(Commands.pauseCommand);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _socket?.Close();
            _disposed = true;
        }

    }
}
