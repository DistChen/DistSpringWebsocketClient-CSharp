using Dist.SpringWebsocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace DistSpringWebsocketClient_App
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
        }

        string url = "ws://127.0.0.1:8080/dist";
        string destination = "/topic/greet/1";
        string target = "/send/greet/1";
        Client client;
        
        private void Form1_Load(object sender, EventArgs e)
        {
            this.client = new Client(url, new Receive(delegate(StompFrame frame)
            {
                txtContent.Text += frame.Code.ToString() + ":" + frame.Content + Environment.NewLine;
                
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("login", "chenyp");
            headers.Add("passcode", "pass");
            this.client.Connect(headers, new Receive(delegate(StompFrame frame)
            {
                txtContent.Text += frame.Code.ToString() + ":" + frame.Content + Environment.NewLine;
            }));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.client.IsSubscribed(destination))
            {
                txtContent.Text += "您已订阅:" + destination+Environment.NewLine;
            }
            else
            {
                this.client.Subscribe(destination, new Receive(delegate(StompFrame frame)
                {
                    txtContent.Text += frame.Code.ToString() + ":" + JObject.Parse(frame.Content).GetValue("content") + Environment.NewLine;
                }));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.client.UnSubscribe(destination);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.client.DisConnect();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string input = this.txtInput.Text.Trim();
            if (input.Length == 0)
            {
                MessageBox.Show("请输入内容");
                return;
            }
            JObject o = new JObject();
            o.Add("name", input);
            this.client.Send(target, o.ToString());
        }
    }
}
