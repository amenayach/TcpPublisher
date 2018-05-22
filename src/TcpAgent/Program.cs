namespace TcpAgent
{
    using System;
    using System.Linq;

    class Program
    {
        private static CommandExecuter commandExecuter;

        static void Main(string[] args)
        {
            var tcpListener = new TCPListener();

            commandExecuter = new CommandExecuter();

            tcpListener.Listen("", "13000", (source, message, stream) =>
            {
                var response = Exec(source, message);

                //if (!string.IsNullOrWhiteSpace(response))
                //{
                //    var responseBytes = Encoding.UTF8.GetBytes(response);

                //    stream.Write(responseBytes, 0, responseBytes.Length);
                //}
                //else
                //{
                //    var responseBytes = Encoding.UTF8.GetBytes("no response");

                //    stream.Write(responseBytes, 0, responseBytes.Length);
                //}

                Log($"{source} - {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}{Environment.NewLine}{message}");
            });
        }

        private static string Exec(string source, string message)
        {
            var spl = message.Split('|');

            var methodName = spl[0]?.Trim();

            var method = typeof(CommandExecuter).GetMethods().FirstOrDefault(m => m.Name.ToLower() == methodName.ToLower());

            if (method != null)
            {
                return method.Invoke(commandExecuter, spl.Skip(1).Select(m => m?.Trim()).ToArray()) as string;
            }

            return string.Empty;
        }

        private static void Log(string message)
        {
            Console.WriteLine(message);
            Logger.Log(message);
        }
    }
}
