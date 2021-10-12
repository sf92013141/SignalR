using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SignalRClient
{
    public partial class Form1 : Form
    {


        private static readonly string HubName = "BroadcastHub";
        public HubConnection hubConnection = new HubConnection(@"http://localhost:59904/");

        private IHubProxy broadcastHubProxy;



        public Form1()
        {
            InitializeComponent();
            FormClosed += Form1_FormClosed;
            // 建立 Hub Proxy
            this.broadcastHubProxy = this.hubConnection.CreateHubProxy(HubName);

            // 註冊給伺服端呼叫的方法
            this.broadcastHubProxy.On("ShowMessage", (string name, string message) =>
            {
                this.MessagesBlock.InvokeIfNecessary(() =>
                {
                    this.MessagesBlock.Text += ($"{name}:{message}\r\n");
                });
            });

            this.broadcastHubProxy.On("RefreshMember", (List<string> ConnectionId) =>
            {
                this.listBox1.InvokeIfNecessary(() =>
                {
                    listBox1.Items.Clear();
                    foreach (var Online in ConnectionId)
                    {
                        this.listBox1.Items.Add(Online);
                    }
                });
            });

            // 連線到 SignalR 伺服器
            try
            {
                this.hubConnection.Start().Wait();
                this.broadcastHubProxy.Invoke("LoginSignalR", hubConnection.ConnectionId);
                Text = hubConnection.ConnectionId;
            }
            catch (Exception e)
            {

            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                this.broadcastHubProxy.Invoke("LogOutSignalR", hubConnection.ConnectionId);
            }
            catch (Exception ex)
            {
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text.Replace(" ", "")))
            {
                this.broadcastHubProxy.Invoke("Broadcast", hubConnection.ConnectionId, "<全體>" + textBox1.Text);
                textBox1.Text = null;
            }
        }
        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(textBox1.Text.Replace(" ", "")))
            {
                this.MessagesBlock.Text += ($"你對" + hubConnection.ConnectionId + ":<私訊>" + textBox1.Text + "\r\n");
                this.broadcastHubProxy.Invoke("PrivateMessage", hubConnection.ConnectionId, listBox1.SelectedItem, "<私訊>" + textBox1.Text);
                textBox1.Text = null;
            }
        }
    }

    static class Extension
    {
        public static void InvokeIfNecessary(this System.Windows.Forms.Control control, MethodInvoker action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
