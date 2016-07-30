using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dist.SpringWebsocket
{
    public class StompFrame
    {
        private StatusCodeEnum code;
        private string content;
        private Dictionary<string,string> headers;
        public StompFrame() {
        }
        public StompFrame(StatusCodeEnum code):this()
        {
            this.code = code;
        }
        public StompFrame(StatusCodeEnum code, string content) :this(code)
        {
            this.content = content;
        }
        public StompFrame(StatusCodeEnum code, string content, Dictionary<string, string> headers): this(code, content)
        {
            this.headers = headers;
        }

        public void AddHeader(string key, string value)
        {
            if (this.headers == null)
            {
                this.headers = new Dictionary<string, string>();
            }
            this.headers.Add(key, value);
        }
        public string GetHeader(string key)
        {
            return this.headers[key];
        }
        public bool ContainsHeader(string key)
        {
            return this.headers.ContainsKey(key);
        }
        public StatusCodeEnum Code
        {
            get { return code; }
            set { code = value; }
        }
       
        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        public Dictionary<string, string> Headers
        {
            get { return headers; }
            set { headers = value; }
        }
    }
}
