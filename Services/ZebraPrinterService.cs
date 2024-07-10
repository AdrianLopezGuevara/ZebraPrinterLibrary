using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZebraPrinterLibrary.Commands;
using ZebraPrinterLibrary.Interfaces;

namespace ZebraPrinterLibrary.Services
{
    public class ZebraPrinterService : IDisposable, IZebraPrinterService
    {
        private Socket _socket;
        private bool _disposed = false;

        public ZebraPrinterService()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public async Task ConnectAsync(string ip, int port)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ZebraPrinterService));

            if (_socket.Connected)
                throw new InvalidOperationException("Already connected to the printer.");

            try
            {
                await _socket.ConnectAsync(ip, port);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to connect to the printer.", ex);
            }
        }

        public async Task SendCommandAsync(string command)
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

        #region GetFunctions
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
            string sHostStatus = await GetPrinterResponseAsync(GetCommands.system_error);
            Console.WriteLine(sHostStatus);
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
            return await GetPrinterResponseAsync(ZPLCommands.getRIDFLogs);
        }

        public async Task<string> GetSpeedAsync()
        {
            return await GetPrinterResponseAsync(GetCommands.getSpeedCommand);
        }

        public async Task<string> GetResolutionAsync()
        {
            return await GetPrinterResponseAsync(GetCommands.getResolution);
        }

        public async Task<string> GetLabelLengthAsync()
        {
            return await GetPrinterResponseAsync(GetCommands.getLabelLengthCommand);
        }

        public async Task<string> GetValidLabelsAsync()
        {
            return await GetPrinterResponseAsync(GetCommands.getValidLabelsCommand);
        }

        public async Task<string> GetVoidLabelsAsync()
        {
            return await GetPrinterResponseAsync(GetCommands.getVoidLabelsCommand);
        }
        #endregion

        public async Task ResetCounterAsync()
        {
            await SendCommandAsync(SetCommands.resetCounterCommand);
        }

        public async Task RestorePrinterAsync()
        {
            await SendCommandAsync(ZPLCommands.restorePrinterCommand);
        }

        public async Task CalibrateRFIDAsync()
        {
            await SendCommandAsync(SetCommands.calibrateRFID);
        }

        public async Task CalibrateLabelAsync()
        {
            await SendCommandAsync(ZPLCommands.calibarteLabel);
        }

        public async Task DeleteJobsAsync()
        {
            await SendCommandAsync(ZPLCommands.deleteJobsCommand);
        }

        public async Task CleanBufferAsync()
        {
            await SendCommandAsync(SetCommands.cleanBufferCommand);
        }

        public async Task ContinueAsync()
        {
            await SendCommandAsync(ZPLCommands.continueCommand);
        }

        public async Task PauseAsync()
        {
            await SendCommandAsync(ZPLCommands.pauseCommand);
        }

        public void Dispose()
        {
            if (_disposed) return;

            _socket?.Close();
            _disposed = true;
        }

    }
}
