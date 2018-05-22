using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TcpAgent
{
    public class TcpClient
    {
        private const int portNum = 13000;

        /// <summary>
        /// Send a TCP string message
        /// </summary>
        /// <param name="ip">IP to send to</param>
        /// <param name="message">Message to send</param>
        /// <param name="action">action(done, response, errorMessage)</param>
        public static Thread Send(string ip, string message, Action<bool, string, string> action)
        {

            var thread = new Thread(() =>
            {
                try
                {
                    System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient(ip, portNum);

                    NetworkStream ns = client.GetStream();

                    byte[] byteTime = Encoding.ASCII.GetBytes(message);

                    ns.Write(byteTime, 0, byteTime.Length);

                    //var agentResponse = ReadStream(ns);

                    ns.Close();
                    client.Close();

                    if (action != null)
                    {
                        action(true, string.Empty, string.Empty);
                    }
                }
                catch (Exception e)
                {
                    if (action != null)
                    {
                        action(false, string.Empty, e.Message);
                    }
                }

            });

            thread.Start();
            //thread.Join();
            //if (action != null)
            //{
            //    action(false, "thread done");
            //}

            return thread;
        }

        private static string ReadStream(NetworkStream stream)
        {
            int i;
            var data = string.Empty;
            var bytes = new byte[256];

            while (stream.CanRead && (i = stream.Read(bytes, 0, bytes.Length)) != 0)
            {
                data += Encoding.UTF8.GetString(bytes, 0, i);
            }

            return data;
        }

        /// <summary>
        /// Retrieves the local IP address
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIPAddress()
        {
            var aaaa = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

            return Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.Where(
                    f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).Select(o => o.ToString())
                .Aggregate((f1, f2) => f1 + ", " + f2);

            throw new Exception("Local IP Address Not Found!");
        }
    }
}
