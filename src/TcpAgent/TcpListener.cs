using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpAgent
{
    /// <summary>
    /// An object that listen through TCP within an isolated thread and post back every received strings
    /// </summary>
    public class TCPListener
    {
        /// <summary>
        /// The main thread
        /// </summary>
        private Thread mainThread = null;

        /// <summary>
        /// Initialize the listen thread
        /// </summary>
        /// <param name="ip">the IP to listen</param>
        /// <param name="port">the port to listen</param>
        /// <param name="action">the callback to call when something received</param>
        public void Listen(string ip, string port, Action<string, string, NetworkStream> action)
        {
            if (mainThread != null)
            {
                mainThread.Abort();
                mainThread = null;
            }

            mainThread = new Thread(() => { DoListen(ip, port, action); });
            mainThread.Start();

        }

        /// <summary>
        /// The listen action
        /// </summary>
        /// <param name="ip">the IP to listen</param>
        /// <param name="port">the port to listen</param>
        /// <param name="action">the callback to call when something received</param>
        private void DoListen(string ip, string port, Action<string, string, NetworkStream> action)
        {
            TcpListener server = null;
            try
            {

                server = new TcpListener(IPAddress.Any, Int32.Parse(port));

                server.Start();

                var bytes = new byte[256];
                string data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    System.Net.Sockets.TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");
                    Console.ForegroundColor = ConsoleColor.White;

                    data = string.Empty;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while (stream.CanRead && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data += Encoding.UTF8.GetString(bytes, 0, i);
                    }

                    action?.Invoke(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString(), data, stream);
                    
                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

        }

        /// <summary>
        /// Stops the main listen thread if exists
        /// </summary>
        public void Stop()
        {
            if (mainThread != null)
            {
                mainThread.Abort();
                mainThread = null;
            }
        }
    }
}
