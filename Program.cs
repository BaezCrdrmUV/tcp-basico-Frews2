using System;

namespace tcp_com
{
    class Program
    {
         static void Main(string[] args)
        {
            string ip = "127.0.0.1";
            int port = 8090;

            if(args.Length > 1)
            {
                // IP:puerto
                string[] tempArray = args[1].Split(':');
                try
                {
                    ip = tempArray[tempArray.Length - 2];
                    ip = ip.Replace("//", "");
                    port = Int32.Parse(tempArray[tempArray.Length - 1]);
                }
                catch (System.Exception) { }
            }

            if(args.Length == 0 || String.IsNullOrEmpty(args[0]) || args[0] == "client")
            {
                string name = "Cliente";
                try
                {
                    name = args[2];
                }
                catch (Exception) { }

                // Ejecución de cliente
                TCPClient client = new TCPClient(ip, port, name);
                client.Chat();
            }
            else if(args[0] == "server")
            {
                TCPServer server = new TCPServer(ip, port, true);
                server.Listen();
            }
        }
    }
}