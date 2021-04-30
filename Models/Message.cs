using System;
using System.Globalization;

namespace tcp_com
{
    public class Message
    {
        public int MessageID {get; set;}
        public string MessageString { get; set; }
        public string User { get; set; }

        public DateTime CreationTime { get; set; }

        public int Type { get; set; }

        public Message()
        {
            MessageString = "";
            User = "Default";
        }

        public Message( int id, string messageString, string user)
        {
            this.MessageID = id;
            this.MessageString = messageString;
            this.User = user;
            this.CreationTime = DateTime.Now;
            this.Type = -1;  
        }

        public Message( int id, string messageString, string user, int type)
        {
            this.MessageID = id;
            this.MessageString = messageString;
            this.User = user;
            this.CreationTime = DateTime.Now;
            this.Type = type;  
        }
    }
}