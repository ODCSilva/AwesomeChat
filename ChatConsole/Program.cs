using System;
using System.Threading;
using ChatLib;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;

namespace ChatConsole
{
    /// <summary>
    /// Main console application driver class.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Server name for chat.
        /// </summary>
        public const string SERVER_NAME = "Server";

        /// <summary>
        /// Console prompt symbol when in messaging mode.
        /// </summary>
        public const string PROMPT = " >> ";

        // Console Arguments
        public const string SERVER_ARG = "-server";
        public const string SERVER_ARG_SHORT = "-s";
        public const string HELP_ARG = "-help";

        /// <summary>
        /// Regex to validate ip address from the command line argument.
        /// </summary>
        public const string VALID_IP_REGEX = "^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$";

        /// <summary>
        /// Regex to validate port from the command line argument.
        /// </summary>
        public const string VALID_PORT_REGEX = "^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$";

        public const string QUIT_COMMAND = "/quit";
        public const ConsoleKey MESSAGE_KEY = ConsoleKey.I;

        private string[] anim = { "[-]", "[\\]", "[|]", "[/]" };
        private string userName = string.Empty;

        /// <summary>
        /// Static void main. Handles command arguments.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            string host = string.Empty;
            int port = 0;

            // Create a new instance of the Program class to escape static space.
            Program program = new Program();

            Console.Clear();

            // Handle command args
            switch (args.Length) {
                case 0:
                    program.StartClient();
                    Console.Read();
                    break;
                case 1:
                    if (args[0] == SERVER_ARG || args[0] == SERVER_ARG_SHORT) {
                        // Start in server mode
                        program.StartServer();
                        Console.Read();
                    }
                    else if (args[0] == HELP_ARG ) {
                        ShowArgumentHelp();
                    }
                    break;
                case 2:
                    bool valid = true;

                    if (Regex.IsMatch(args[0], VALID_IP_REGEX)) {
                        host = args[0];
                    }
                    else {
                        Console.WriteLine("Invalid IP Address");
                        valid = false;
                    }

                    if (Regex.IsMatch(args[1], VALID_PORT_REGEX)) {
                        int.TryParse(args[1], out port);
                    }
                    else {
                        Console.WriteLine("Invalid Port");
                        valid = false;
                    }

                    if (valid) {
                        program.StartClient(host, port);
                        Console.Read();
                    }
                    
                    break;
                default:
                    ShowArgumentHelp();
                    break;
            }
        }

        /// <summary>
        /// Display command argument help for the program.
        /// </summary>
        public static void ShowArgumentHelp() {
            Console.WriteLine("USAGE:");
            Console.WriteLine("\t ChatConsole \t\t\t Starts in client mode and attempts to connect to the default hostname \n\t\t\t\t\t (127.0.0.1), and port (130000).");
            Console.WriteLine("\t ChatConsole -server|-s \t Starts chat in server mode.");
            Console.WriteLine("\t ChatConsole [hostname] [port] \t Connects chat client to the specified hostname (or IP Address) and Port");
        }

        /// <summary>
        /// Checks to see if use has initiated message mode and reads user input from console.
        /// </summary>
        /// <returns>Input text from user.</returns>
        public string CheckForUserInput() {
            string line = string.Empty;

            // Prevent blocking call by checking if a Key is available to be read first
            if (Console.KeyAvailable) {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                if (keyInfo.Key == MESSAGE_KEY) {
                    Console.Write(PROMPT);
                    line = Console.ReadLine();
                }
            }
            return line;
        }

        /// <summary>
        /// Starts chat in server mode. Listens for incomming connections, and once established, begins listening for incoming messages.
        /// Also captures user input to be send through chat.
        /// </summary>
        public void StartServer() {

            Server server = new Server();
            string line = string.Empty;
            NewMessageCallback callBack = new NewMessageCallback(NewMessage);
            userName = SERVER_NAME;

            server.Start();

            try {
                int frame = 0;

                while (true) {
                    Console.SetCursorPosition(0,0);
                    Console.WriteLine("\n -------------------------------------------------");
                    Console.WriteLine(" Awesome Chat Server : version 0.1.10");
                    Console.WriteLine(" -------------------------------------------------");
                    Console.WriteLine(" Listening for incoming connections...{0}", anim[frame]);

                    if (server.ListenForIncomingConnections()) {

                        Console.Clear();
                        Console.WriteLine("\n -------------------------------------------------");
                        Console.WriteLine(" Client connected!");
                        Console.WriteLine(" -------------------------------------------------");
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine(" Tip: Press 'I' to write and message and ENTER to send it.\n");
                        Console.ResetColor();

                        // Listen for incoming messages and handle user input
                        while (true) {

                            line = CheckForUserInput();
                            if (line.Length > 0) {

                                if (line.Equals(QUIT_COMMAND)) {
                                    server.Dispose();
                                    return;
                                }

                                if (!server.SendMessage(string.Format(" <{0}>: {1}", userName, line))) {
                                    Console.WriteLine(" Unable to write to network stream, closing connection...");
                                    server.Dispose();
                                    break;
                                }
                            }

                            if (!server.GetPendingMessage(callBack)) {
                                Console.WriteLine(" Unable to read from network stream, closing connection...");
                                server.Dispose();
                                break;
                            }

                            if (server.PollConnection()) {
                                Console.Clear();
                                Console.SetCursorPosition(0, 6);
                                Console.WriteLine("\n [Client disconnected. What a loser.]");
                                server.Dispose();
                                break;
                            }
                            Thread.Sleep(200);
                        }
                    }

                    frame++;
                    if (frame > anim.Length - 1) {
                        frame = 0;
                    }
                    Thread.Sleep(200);
                }    
            }
            catch (InvalidOperationException e) {
                Console.WriteLine(" InvalidOperationException: {0}", e.Message);
            }
            catch (SocketException e) {
                Console.WriteLine(" SocketException: {0}", e.Message);
            }
            catch (IOException e) {
                Console.WriteLine(" IOException: {0}", e.Message);
            }
            finally {
                server.Stop();
            }
        }

        /// <summary>
        /// Starts chat client, trying to establish a connection. If successful, begins listening for incoming messages. 
        /// Also captures user input to be send through chat.
        /// </summary>
        public void StartClient(string hostname = "127.0.0.1", int port = 13000) {
            Client client = new Client(port, hostname);

            try {
                client.Connect();

                // Greeting
                Console.WriteLine("\n -------------------------------------------------");
                Console.WriteLine(" Connected to Chat @ [{0}], using port [{1}]\n", client.Hostname, client.Port);
                Console.WriteLine(" -------------------------------------------------");
                Console.SetCursorPosition(0, 3);

                Console.Write(" Enter your chat nickname: ");
                userName = Console.ReadLine();

                Console.Clear();
                Console.WriteLine("\n -------------------------------------------------");
                Console.WriteLine(" Welcome to chat {0}, enjoy!", userName);
                Console.WriteLine(" -------------------------------------------------");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine(" Tip: Press 'I' to write and message and ENTER to send it.\n");
                Console.ResetColor();

                string line = string.Empty;
                NewMessageCallback callBack = new NewMessageCallback(NewMessage);

                // Listen for incoming messages and handle user input
                while (true) {

                    // Get input from user
                    line = CheckForUserInput();
                    if (line.Length > 0) {

                        if (line.Equals(QUIT_COMMAND)) {
                            break;
                        }

                        if (!client.SendMessage(string.Format(" <{0}>: {1}", userName, line))) {
                            Console.WriteLine(" Unable to write to network stream, closing connection...");
                        }
                    }

                    // Check for messages and print them if available
                    if (!client.GetPendingMessage(callBack)) {
                        Console.WriteLine(" Unable to read from network stream, closing connection...");
                        break;
                    }

                    if (client.PollConnection()) {
                        Console.WriteLine("\n [Connection terminated by remote host. Rude.]");
                        break;
                    }

                    Thread.Sleep(200);
                }
            }
            catch (InvalidOperationException e) {
                Console.WriteLine(" InvalidOperationException: {0}", e.Message);
            }
            catch (SocketException e) {
                Console.WriteLine(" SocketException: {0}", e.Message);
            }
            catch (IOException e) {
                Console.WriteLine(" IOException: {0}", e.Message);
            }
            catch (ArgumentOutOfRangeException e) {
                Console.WriteLine(" ArgumentOutOfRangeException: {0}", e.Message);
            }
            finally {
                client.Disconnect();
            }
        }

        /// <summary>
        /// Callback function used with NewMessageCallback delegate and called when a new chat message has been received.
        /// The new message will be displayed in the console.
        /// </summary>
        /// <param name="message">The incoming message.</param>
        public static void NewMessage(string message) {
            Console.WriteLine(message);
        }
    }
}
