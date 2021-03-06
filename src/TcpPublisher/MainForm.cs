﻿using TcpAgent;
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

            var ipColors = GetIpsColors(agents, out bool noDuplicate);

            if (!noDuplicate)
            {
                return;
            }

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

                                    Log($"{sentStatus} to {ip} at {DateTime.Now:yyyy-MM-dd HH:mm:ss.ffffff}", ipColors[ip], true);
                                }));
                            });
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please enter the agents IPs");
            }
        }

        private Dictionary<string, Color> GetIpsColors(List<string> agents, out bool noDuplicate)
        {
            noDuplicate = true;

            var result = new Dictionary<string, Color>();

            if (agents?.Any() ?? false)
            {
                var random = new Random();
                
                foreach (var ip in agents)
                {
                    if (result.ContainsKey(ip))
                    {
                        noDuplicate = false;
                        MessageBox.Show("Duplicate IPs !!!");

                        return null;
                    }

                    result.Add(ip, Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)));
                }
            }

            return result;
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

        private void Log(string message, Color color, bool logInFile = true)
        {
            tbStatus.SelectionStart = tbStatus.TextLength;
            tbStatus.SelectionLength = 0;
            tbStatus.SelectionColor = color;
            tbStatus.AppendText(message + Environment.NewLine);
            tbStatus.ScrollToCaret();
            tbStatus.SelectionColor = tbStatus.ForeColor;

            if (logInFile)
            {
                Logger.Log(message, "ERROR");
            }
        }

        private List<string> GetAgentIps()
        {
            if (!string.IsNullOrWhiteSpace(tbAgentIps.Text))
            {
                return tbAgentIps.Text.Split('\n').Where(ip => !ip?.Trim().StartsWith("#") ?? false).ToList();
            }

            return new List<string>();
        }

        private void btnHelp_Click(object sender, EventArgs e)
        {
            using (var helpForm = new HelpForm())
            {
                helpForm.ShowDialog();
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                btnHelp.PerformClick();
            }
        }
    }
}
