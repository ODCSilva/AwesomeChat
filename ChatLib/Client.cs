

namespace ChatLib
{
    using System.Net.Sockets;

    /// <summary>
    /// Chat client class.
    /// </summary>
    public class Client : ChatHost { 

        /// <summary>
        /// Client constructor.
        /// </summary>
        /// <param name="port">Host port to connect to.</param>
        /// <param name="host">Host address to connect to.</param>
        public Client(int port = 13000, string host = "127.0.0.1") {
            Port = port;
            Hostname = host;
            Client = new TcpClient();
        }

        /// <summary>
        /// Connect the client to the host and port specified by the Host and Port properties.
        /// </summary>
        /// <exception cref="ArgumentNullException">The hostname parameter is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The port parameter is not between MinPort and MaxPort.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        /// <exception cref="ObjectDisposedException">TcpClient is closed.</exception> 
        /// <exception cref="InvalidOperationException">The port parameter is not between MinPort and MaxPort.</exception>
        public void Connect() {
            if (!Client.Connected) {
                Client.Connect(Hostname, Port);
                stream = Client.GetStream();
            }
        }

        /// <summary>
        /// Closes the Client and NetworkStream. 
        /// </summary>
        public void Disconnect() {

            if (stream != null) {
                stream.Flush();
                stream.Dispose();
                stream.Close();
                stream = null;
            }

            Client.Close();
        }
    }
}
