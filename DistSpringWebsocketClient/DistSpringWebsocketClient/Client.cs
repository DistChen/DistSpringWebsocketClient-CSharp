using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using WebSocket4Net;

namespace Dist.SpringWebsocket
{
    public delegate void Receive(StompFrame frame);

    public class Client
    {
        /// <summary>
        /// 支持的stomp协议版本
        /// </summary>
        public static string StompPrototypeVersion = "accept-version:1.1,1.0";
        /// <summary>
        /// 心跳时间
        /// </summary>
        public static string HeartBeating = "heart-beat:10000,10000";
        /// <summary>
        /// 换行符，用于构造 Stomp 消息包
        /// </summary>
        public static Char LF = Convert.ToChar(10);
        /// <summary>
        /// 空字符，用于构造 Stomp 消息包
        /// </summary>
        public static Char NULL = Convert.ToChar(0);
        /// <summary>
        /// 当前连接类型
        /// </summary>
        public static string TYPE = "client";

        private static string SubscriptionHeader = "subscription";
        private static string DestinationHeader = "destination";
        private static string ContentLengthHeader = "content-length";
        private static string CID = "dist-connect";

        private Dictionary<string, Receive> callbacks;
        private Receive statusCallback;
        private Dictionary<string, string> subscribes;
        private WebSocket socket;
        private string url;
        private static int COUNTER = 0;
        private Boolean connected = false;


        public Client(string url, Receive callback)
        {
            this.url = url;
            this.statusCallback = callback;
            this.callbacks = new Dictionary<string, Receive>();
            this.subscribes = new Dictionary<string, string>();
            this.socket = new WebSocket(this.url);
            this.socket.Opened += socket_Opened;
            this.socket.Closed += socket_Closed;
            this.socket.MessageReceived += socket_MessageReceived;
            this.socket.Open();
            this.socket.Error += socket_Error;
            this.socket.EnableAutoSendPing = false;
        }
        public void Connect(Dictionary<string, string> headers, Receive callback)
        {
            if (!this.connected)
            {
                this.callbacks.Add(CID, callback);
                string data = StompCommandEnum.CONNECT.ToString() + LF;
                foreach (string key in headers.Keys)
                {
                    data += key + ":" + headers[key] + LF;
                }
                data += StompPrototypeVersion + LF
                      + HeartBeating + LF + LF + NULL;
                this.socket.Send(data);
            }
        }
        public void Send(string destination, string content)
        {
            this.Send(destination, new Dictionary<string, string>(), content);
        }
        public void Send(string destination, Dictionary<string, string> headers, string content)
        {
            string data = StompCommandEnum.SEND.ToString() + LF;
            foreach (string key in headers.Keys)
            {
                data += key + ":" + headers[key] + LF;
            }
            data += DestinationHeader + ":" + destination + LF
                    + ContentLengthHeader + ":" + GetByteCount(content) + LF + LF
                    + content + NULL;
            this.socket.Send(data);
        }

        public void Subscribe(string destination, Receive callback)
        {
            this.Subscribe(destination, new Dictionary<string, string>(), callback);
        }
        public void Subscribe(string destination, Dictionary<string, string> headers, Receive callback)
        {
            lock (this)
            {
                if (!this.subscribes.ContainsKey(destination))
                {
                    string id = "sub-" + COUNTER++;
                    this.callbacks.Add(id, callback);
                    this.subscribes.Add(destination, id);
                    string data = StompCommandEnum.SUBSCRIBE.ToString() + LF + "id:" + id + LF;
                    foreach (string key in headers.Keys)
                    {
                        data += key + ":" + headers[key] + LF;
                    }
                    data += DestinationHeader + ":" + destination + LF + LF + NULL;
                    this.socket.Send(data);
                }
            }
        }
        public void UnSubscribe(string destination)
        {
            if (this.subscribes.ContainsKey(destination))
            {
                this.socket.Send(StompCommandEnum.UNSUBSCRIBE.ToString() + LF + "id:" + this.subscribes[destination] + LF + LF + NULL);
                this.callbacks.Remove(this.subscribes[destination]);
                this.subscribes.Remove(destination);
            }
        }
        public void DisConnect()
        {
            this.socket.Send(StompCommandEnum.DISCONNECT.ToString() + LF + LF + NULL);
            this.callbacks.Clear();
            this.subscribes.Clear();
            this.connected = false;
        }
        public bool IsSubscribed(string destination)
        {
            return this.subscribes.ContainsKey(destination);
        }

        private int GetByteCount(string content)
        {
            return Regex.Split(Uri.EscapeUriString(content), "%..|.").Length - 1;
        }

        private StompFrame TransformResultFrame(string content)
        {
            lock (this)
            {
                StompFrame frame = new StompFrame();
                string[] matches = Regex.Split(content, "" + NULL + LF + "*");
                foreach (var line in matches)
                {
                    if (line.Length > 0)
                    {
                        this.HandleSingleLine(line, frame);
                    }
                }
                return frame;
            }
        }

        private void HandleSingleLine(string line, StompFrame frame)
        {
            int divider = line.IndexOf("" + LF + LF);
            string[] headerLines = Regex.Split(line.Substring(0, divider), "" + LF);
            frame.Code = (StatusCodeEnum)Enum.Parse(typeof(StatusCodeEnum), headerLines[0]);
            for (int i = 1; i < headerLines.Length; i++)
            {
                int index = headerLines[i].IndexOf(":");
                string key = headerLines[i].Substring(0, index);
                string value = headerLines[i].Substring(index + 1);
                frame.AddHeader(Regex.Replace(key, @"^\s+|\s+$", ""), Regex.Replace(value, @"^\s+|\s+$", ""));
            }
            frame.Content = line.Substring(divider + 2);
        }
        private void socket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            this.connected = false;
            this.statusCallback(new StompFrame(StatusCodeEnum.SERVERERROR, e.Exception.Message));
        }
        void socket_Closed(object sender, EventArgs e)
        {
            this.connected = false;
            this.statusCallback(new StompFrame(StatusCodeEnum.SERVERCLOSED, "与服务器断开连接"));
        }
        private void socket_Opened(object sender, EventArgs e)
        {
            this.statusCallback(new StompFrame(StatusCodeEnum.OPENSERVER, "成功连接到服务器"));
        }
        void socket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
        {
            StompFrame frame = this.TransformResultFrame(e.Message);
            switch (frame.Code)
            {
                case StatusCodeEnum.CONNECTED:
                    connected = true;
                    this.callbacks[CID](frame);
                    break;
                case StatusCodeEnum.MESSAGE:
                    this.callbacks[frame.GetHeader(SubscriptionHeader)](frame);
                    break;
            }

        }
        public string URL
        {
            get { return url; }
            set { this.url = value; }
        }
    }
}
