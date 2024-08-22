using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using ZebraPrinterLibrary.Commands;
using ZebraPrinterLibrary.Interfaces;

namespace ZebraPrinterLibrary.Services
{
    public class ZebraPrinterService : IZebraPrinterService
    {
        private string _printerIp = string.Empty;
        private int _printerPort = 0;

        public ZebraPrinterService()
        {
        }

        public void Connect(string ip, int port)
        {
            if (string.IsNullOrWhiteSpace(ip))
            {
                throw new ArgumentException("IP address cannot be null or empty.", nameof(ip));
            }

            if (!IPAddress.TryParse(ip, out _))
            {
                throw new ArgumentException("Invalid IP address format.", nameof(ip));
            }

            if (port <= 0 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Port number must be between 1 and 65535.");
            }

            _printerIp = ip;
            _printerPort = port;
        }

        public async Task SendCommandToPrinterAsync(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("Command cannot be null or empty.", nameof(command));
            }

            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    await socket.ConnectAsync(_printerIp, _printerPort);
                    await socket.SendAsync(Encoding.Latin1.GetBytes(command));
                }
            }
            catch (SocketException ex)
            {
                throw new InvalidOperationException("Failed to send command to the printer.", ex);
            }
        }

        public async Task<string> GetPrinterResponseAsync(string command)
        {
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("Command cannot be null or empty.", nameof(command));
            }

            string response = "";
            try
            {
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    await socket.ConnectAsync(_printerIp, _printerPort);
                    await socket.SendAsync(Encoding.ASCII.GetBytes(command));

                    byte[] buffer = new byte[3072];
                    int bytesReceived = await socket.ReceiveAsync(buffer, SocketFlags.None);
                    response = Encoding.ASCII.GetString(buffer, 0, bytesReceived).Replace("\"", "");
                }
            }
            catch (SocketException ex)
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
            string response = await GetPrinterResponseAsync(ZPLCommands.getRIDFLogs);

            var charsToRemove = new[] { "\"", "<start>", "<end>", "\r", "\u0000", "\u0002", "\u0003" };

            foreach (var c in charsToRemove)
            {
                response = response.Replace(c, "");
            }

            return response.Trim();
        }

        public async Task<float> GetSpeedAsync()
        {
            string response = await GetPrinterResponseAsync(GetCommands.getSpeedCommand);
            if (float.TryParse(response, out float speed))
            {
                return speed;
            }
            else
            {
                throw new InvalidDataException("La respuesta del impresor no es un valor de velocidad válido.");
            }
        }

        public async Task<float> GetResolutionAsync()
        {
            string response = await GetPrinterResponseAsync(GetCommands.getResolution);
            if (float.TryParse(response, out float resolution))
            {
                return resolution;
            }
            else
            {
                throw new InvalidDataException("La respuesta del impresor no es un valor de resolución válido.");
            }
        }

        public async Task<float> GetLabelLengthAsync()
        {
            string response = await GetPrinterResponseAsync(GetCommands.getLabelLengthCommand);

            if (float.TryParse(response, out float labelLength))
            {
                return labelLength;
            }
            else
            {
                throw new InvalidDataException("La respuesta del impresor no es un valor de longitud de etiqueta válido.");
            }
        }

        public async Task<int> GetValidLabelsAsync()
        {
            string response = await GetPrinterResponseAsync(GetCommands.getValidLabelsCommand);
            if (int.TryParse(response, out int validLabels))
            {
                return validLabels;
            }
            else
            {
                throw new InvalidDataException("La respuesta del impresor no es un valor de etiquetas válidas válido.");
            }
        }

        public async Task<int> GetVoidLabelsAsync()
        {
            string response = await GetPrinterResponseAsync(GetCommands.getVoidLabelsCommand);
            if (int.TryParse(response, out int voidLabels))
            {
                return voidLabels;
            }
            else
            {
                throw new InvalidDataException("La respuesta del impresor no es un valor de etiquetas inválidas válido.");
            }
        }

        public async Task ResetCounterAsync()
        {
            await SendCommandToPrinterAsync(SetCommands.resetCounterCommand);
        }

        public async Task RestorePrinterAsync()
        {
            await SendCommandToPrinterAsync(ZPLCommands.restorePrinterCommand);
        }

        public async Task CalibrateRFIDAsync()
        {
            await SendCommandToPrinterAsync(SetCommands.calibrateRFID);
        }

        public async Task CalibrateLabelAsync()
        {
            await SendCommandToPrinterAsync(ZPLCommands.calibarteLabel);
        }

        public async Task DeleteJobsAsync()
        {
            await SendCommandToPrinterAsync(ZPLCommands.deleteJobsCommand);
        }

        public async Task CleanBufferAsync()
        {
            await SendCommandToPrinterAsync(SetCommands.cleanBufferCommand);
        }

        public async Task ContinueAsync()
        {
            await SendCommandToPrinterAsync(ZPLCommands.continueCommand);
        }

        public async Task PauseAsync()
        {
            await SendCommandToPrinterAsync(ZPLCommands.pauseCommand);
        }

    }
}
