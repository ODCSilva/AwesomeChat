
namespace ChatLib
{
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Chat server class.
    /// </summary>
    public class Server : ChatHost
    {
        /// <summary>
        /// TcpListener server
        /// </summary>
        private TcpListener server;

        /// <summary>
        /// Server constructor.
        /// </summary>
        /// <param name="host">The hostname used by the server.</param>
        /// <param name="port">The listening port for the server.</param>
        public Server(string host = "", int port = 13000) {
            Hostname = host;
            Port = port;

            IPAddress localAddress;
            if (string.IsNullOrEmpty(host)) {
                localAddress = IPAddress.Any;
            }
            else {
                localAddress = IPAddress.Parse(host);
            }
            
            server = new TcpListener(localAddress, Port);
            server.ExclusiveAddressUse = true;
        }

        /// <summary>
        /// Starts server and begin listening for incoming connections.
        /// </summary>
        /// <exception cref="InvalidOperationException">The server has not been started with a call to Start.</exception>
        /// <exception cref="SocketException">An error occurred when accessing the socket.</exception>
        /// <exception cref="ObjectDisposedException">TcpClient is closed.</exception> 
        public void Start() {
            server.Start();


        }

        /// <summary>
        /// Listen for incoming client connections. Does not block.
        /// </summary>
        /// <returns>True if a connection has been established, false otherwise.</returns>
        public bool ListenForIncomingConnections() {
            if (Client == null && server.Pending()) {
                Client = server.AcceptTcpClient();
                stream = Client.GetStream();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Stops the server and closes any TCPClient connections and/or NetworkStream. 
        /// </summary>
        public void Stop() {
            if (Client != null) {
                Client.Close();
                Client = null;
            }

            if (stream != null) {
                stream.Close();
                stream = null;
            }
            server.Stop();
        }

        /// <summary>
        /// Closes the client socket and the NetworkStream and sets them to null.
        /// </summary>
        public void Dispose() {
            if (Client != null) {
                Client.Close();
                Client = null;
            }

            if (stream != null) {
                stream.Close();
                stream = null;
            }
        }
    }
}
