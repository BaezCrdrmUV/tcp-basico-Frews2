using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace tcp_com
{
    public class TCPServer
    {
        public TcpListener listener { get; set; }
        public bool acceptFlag { get; set; }
        public List<Message> messageList { get; set; }
        public List<int> threadsIds { get; set; }
        public bool hasOpenedThreads;

        public TCPServer(string ip, int port, bool start = false)
        {
            messageList = new List<Message>();
            threadsIds = new List<int>();
            hasOpenedThreads = false;

            IPAddress address = IPAddress.Parse(ip);
            this.listener = new TcpListener(address, port);

            if(start == true)
            {
                listener.Start();
                Console.WriteLine("Servidor iniciado en la dirección {0}:{1}",
                    address.MapToIPv4().ToString(), port.ToString());
                acceptFlag = true;
            }
        }

        public void Listen()
        {
            if(listener != null && acceptFlag == true)
            {
                int id = 0;
                Thread watch = new Thread(new ThreadStart(watchOpenedThreads));
                watch.Start();

                while(true)
                {
                    Console.WriteLine("Esperando conexión de clientes...");

                    if(hasOpenedThreads == true && threadsIds.Count == 0) break;

                    try
                    {
                        var clientSocket = listener.AcceptSocket();
                        Console.WriteLine("Cliente aceptado");

                        Thread thread = new Thread(new ParameterizedThreadStart(HandleCommunication));
                        thread.Start(new ThreadParams(clientSocket, id));
                        threadsIds.Add(id);
                        id++;
                        hasOpenedThreads = true;
                    }
                    catch (System.Exception)
                    {
                        
                    }
                }

                watch.Interrupt();
                return;
            }
        }

        public void HandleCommunication(Object obj)
        {
            ThreadParams param = (ThreadParams)obj;
            Socket client = param.obj;

            if(client != null)
            {
                Console.WriteLine("Cliente conectado. Esperando datos");
                string msg = "";
                Message newMessage = new Message();

                while(newMessage != null && !newMessage.MessageString.Equals("bye"))
                {
                    try
                    {              
                        switch(msg)
                        {
                            case "0":
                                // Envia un mensaje al cliente
                                byte[] data = Encoding.UTF8.GetBytes("Mensaje guardado en servidor");
                                client.Send(data);
                                break;
                            case "1":
                                // Muestra los mensajes en el servidor Operación Read 
                                string jsonMessage  = JsonConvert.SerializeObject(messageList);
                                byte[] listBuffer = Encoding.UTF8.GetBytes(jsonMessage);
                                client.Send(listBuffer);
                                break;
                            case "2":
                                // Actualiza el texto de un mensaje en el servidor Operación Update
                                byte[] updateBuffer = new byte[1024];
                                client.Receive(updateBuffer);
                                msg = Encoding.UTF8.GetString(updateBuffer);
                                newMessage = JsonConvert.DeserializeObject<Message>(msg);

                                bool messageUpdated = ChangeMessage(newMessage);

                                if(messageUpdated)
                                {
                                    msg = "Mensaje con id: " + newMessage.MessageID + " actualizado exitosamente";
                                }
                                else
                                {
                                    msg = "ERROR: Mensaje con texto: " + newMessage.MessageString + " no fue actualizado.";    
                                }

                                byte[] updateResultBuffer = Encoding.UTF8.GetBytes(msg);
                                client.Send(updateResultBuffer);

                                break;
                            
                            case "3":
                                // Elimina un mensaje en el servidor Operación Delete
                                byte[] deleteBuffer = new byte[1024];
                                client.Receive(deleteBuffer);
                                msg = Encoding.UTF8.GetString(deleteBuffer);
                                newMessage = JsonConvert.DeserializeObject<Message>(msg);

                                bool messageDeleted = ChangeMessage(newMessage);

                                if(messageDeleted)
                                {
                                    msg = "Mensaje con id: " + newMessage.MessageID + " eliminado exitosamente";
                                }
                                else
                                {
                                    msg = "ERROR: El Mensaje con id: " + newMessage.MessageID + " no fue eliminado";    
                                }

                                byte[] deleteResultBuffer = Encoding.UTF8.GetBytes(msg);
                                client.Send(deleteResultBuffer);
                                break;

                            default:
                                Console.WriteLine("Esperando datos...");
                                break;
                        }

                        // Escucha por nuevos mensajes
                        byte[] buffer = new byte[1024];
                        client.Receive(buffer);

                        msg = Encoding.UTF8.GetString(buffer);
                        newMessage = JsonConvert.DeserializeObject<Message>(msg);

                        Console.WriteLine(newMessage.User + ": " + newMessage.MessageString + "\t Mandado - " + newMessage.CreationTime.ToString("HH:mm"));
                        msg = newMessage.Type.ToString();

                        // Guarda un mensaje en el servidor Operación Create
                        if(newMessage.Type == 0)
                        {
                            messageList.Add(newMessage);
                        }
                        
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Exception", msg, ex.Message);
                    }
                }
                Console.WriteLine("Cerrando conexión");
                client.Dispose();
                foreach (var item in threadsIds)
                {
                    Console.WriteLine(item);
                }
                Console.WriteLine("------------");
                threadsIds.Remove(param.id);
                foreach (var item in threadsIds)
                {
                    Console.WriteLine(item);
                }
                Thread.CurrentThread.Join();
            }
        }
        public void watchOpenedThreads()
        {
            while(true)
            {
                if(hasOpenedThreads == true && threadsIds.Count == 0)
                {
                    Console.WriteLine("Conexión terminada");
                    listener.Stop();
                    listener = null;
                    break;
                }
            }
            Console.WriteLine("Opened messages");
            displayMessages();
            Thread.CurrentThread.Join();
        }

        public void displayMessages()
        {
            Console.WriteLine("Mensajes en la colección");
            foreach (Message msg in messageList)
            {
                Console.WriteLine("{0} >> {1}", msg.User, msg.MessageString);
            }
        }

        public bool ChangeMessage(Message change)
        {
            bool wasUpdated = false;
            if(change.Type == 2 || change.Type == 3)
            {
                foreach (Message query in messageList)
                {
                    if (query.MessageID == change.MessageID)
                    {
                        if(change.Type == 2)
                        {
                            query.MessageString = change.MessageString;
                        }
                        else 
                        {
                            if (change.Type == 3)
                            {
                                messageList.RemoveAt(query.MessageID);
                            }
                        }
                        
                        wasUpdated = true;
                        break;
                    }
                }
            }

            return wasUpdated;
        }
    }
    public class ThreadParams
    {
        public Socket obj { get; set; }
        public int id { get; set; }

        public ThreadParams(Socket obj, int id)
        {
            this.obj = obj;
            this.id = id;
        }
    }
}