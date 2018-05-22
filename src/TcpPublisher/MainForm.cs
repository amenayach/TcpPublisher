using TcpAgent;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpPublisher
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            this.Stick(tbAgentIps, tbCommands);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            var agents = GetAgentIps();

            if (agents?.Any() ?? false)
            {
                foreach (var ip in agents)
                {
                    var commands = GetCommands();

                    foreach (var command in commands)
                    {
                        if (command.ToLower().StartsWith("wait"))
                        {
                            Wait(command.Split('|').LastOrDefault()?.Trim());
                        }
                        else
                        {
                            TcpClient.Send(ip, command, (done, responseMessage, errorMessage) =>
                            {
                                this.Invoke((Action)(() =>
                                {
                                    var sentStatus = done ? "Sent" : "Failed";

                                    Log($"{sentStatus} to {ip} at {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}\n{responseMessage}");
                                }));
                            });
                        }
                    }
                }
            }
        }

        private void Wait(string seconds)
        {
            if (decimal.TryParse(seconds, out decimal sec))
            {
                System.Threading.Thread.Sleep((int)(sec * 1000));
            }
        }

        private string[] GetCommands()
        {
            return tbCommands.Text.Split('\n').Where(command => !command?.Trim().StartsWith("#") ?? false).ToArray();
        }

        private void Log(string message)
        {
            tbStatus.AppendText(message + Environment.NewLine);
            tbStatus.ScrollToCaret();
            Logger.Log(message, "ERROR");
        }

        private List<string> GetAgentIps()
        {
            if (!string.IsNullOrWhiteSpace(tbAgentIps.Text))
            {
                return tbAgentIps.Text.Split('\n').Where(ip => !ip?.Trim().StartsWith("#") ?? false).ToList();
            }

            return new List<string>();
        }
    }
}
