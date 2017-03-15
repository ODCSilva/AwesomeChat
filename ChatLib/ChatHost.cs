using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

namespace ChatLib
{
    public delegate void NewMessageCallback(string message);

    public abstract class ChatHost
    {
        protected NetworkStream stream;

        /// <summary>
        /// Host port the client will connect to.
        /// </summary>
        private int port;

        /// <summary>
        /// Host port the client will connnect to.
        /// </summary>
        public int Port {
            get {return port;}
            set{port = value;}
        }

        /// <summary>
        /// Host address the client will connect to.
        /// </summary>
        private string hostname;

        /// <summary>
        /// Host address the client will connect to.
        /// </summary>
        public string Hostname {
            get { return hostname; }
            set { hostname = value; }
        }

        /// <summary>
        /// TcpListener client. Initialized when a connection with a client has been established.
        /// </summary>
        private TcpClient client;

        public TcpClient Client {
            get { return client; }
            set { client = value; }
        }

        /// <summary>
        /// Sends a message to network stream.
        /// </summary>
        /// <param name="message"></param>
        /// <exception cref="IOException">The underlying Socket is closed.</exception> 
        /// <exception cref="ObjectDisposedException">The NetworkStream is closed.</exception> 
        public bool SendMessage(string message) {

            if (stream.CanWrite)
            {
                // Encode text to ASCII and store it as a byte array
                byte[] data = Encoding.ASCII.GetBytes(message);
                // Send the message
                stream.Write(data, 0, data.Length);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reads any incoming messages. If a messages is present, it is sent through
        /// the NewMessageCallback delegate function.
        /// </summary>
        /// <param name="newMessageCallback">Reference to a NewMessageCallback delegate function.</param>
        /// <returns>True if able to read from stream, false otherwise</returns>
        /// <exception cref="IOException">The underlying Socket is closed.</exception> 
        /// <exception cref="ObjectDisposedException">The NetworkStream is closed. -or- There was a failure reading from the network.</exception> 
        public bool GetPendingMessage(NewMessageCallback newMessageCallback) {
            if (stream.CanRead)
            {
                byte[] data = new byte[256];
                string responseData = string.Empty;

                // Perform read if data is available
                while (stream.DataAvailable) {
                    int bytes = stream.Read(data, 0, data.Length);
                    responseData += Encoding.ASCII.GetString(data, 0, bytes);
                }

                if (responseData.Length > 0) {
                    newMessageCallback(responseData);
                }
                return true;    
            }
            return false;
        }

        /// <summary>
        /// Polls the Client Socket to check if the connection is still alive.
        /// </summary>
        virtual public bool PollConnection()
        {
            return Client.Client.Poll(200, SelectMode.SelectRead);
        }
    }
}
