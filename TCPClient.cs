using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace tcp_com
{
    public class TCPClient
    {
        TcpClient client;
        string IP;
        int Port;
        string Username;

        public TCPClient(string ip, int port, string username)
        {
            try
            {
                client = new TcpClient();
                this.IP = ip;
                this.Port = port;
                this.Username = username;
            }
            catch (System.Exception)
            {
                
            }
        }

        public void Chat()
        {
            client.Connect(IP, Port);  
            int messageId = 0; 
            Console.WriteLine("Conectado");
            Console.WriteLine("Ingrese su nombre de usuario");
            string user = Console.ReadLine();
            string msg = "";

            while(!msg.StartsWith("bye"))
            {
                try
                {
                    Console.WriteLine("\nEnvía \"Mostrar\" para ver la lista de mensajes" +
                                "\n Envía \"Actualizar\" para cambiar un texto " + 
                                "\n Envía \"Eliminar\" para eliminar un texto" + 
                                "\nEnvía \"bye\" para terminar \n o escriba un mensaje a guardar en el servidor");
                    msg = Console.ReadLine();
                    
                    if(msg.StartsWith("Mostrar"))
                    {
                        Message newMessage = new Message(messageId, msg, user,1);
                        string jsonMessage = JsonConvert.SerializeObject(newMessage);

                        // Envío de datos
                        var stream = client.GetStream();
                        byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                        Console.WriteLine("Comando: Mostrar lista de mensajes.\n");
                        stream.Write(data, 0, data.Length);

                        // Recepción de mensajes
                        byte[] package = new byte[1024];
                        stream.Read(package);
                        string serverMessage = Encoding.UTF8.GetString(package);
                        List<Message> serverMessageList = JsonConvert.DeserializeObject<List<Message>>(serverMessage);
                        
                        foreach (Message message in serverMessageList)
                        {
                            Console.WriteLine(message.User + ": " + message.MessageString + "\t Mandado - " + message.CreationTime.ToString("HH:mm"));
                        }
                    }
                    else
                    {
                        if(msg.StartsWith("Actualizar"))
                        {
                            // Envío de comandos
                            Console.WriteLine("Enviando comando Actualizar...");
                            Message updateMessage = new Message(messageId, "Comando Actualizar", user, 2);
                            string jsonUpdateMessage = JsonConvert.SerializeObject(updateMessage);
                            var stream = client.GetStream();
                            byte[] data = Encoding.UTF8.GetBytes(jsonUpdateMessage);
                            stream.Write(data, 0, data.Length);

                            Console.WriteLine("Ingrese la id del mensaje a actualizar: ");
                            int id = Int32.Parse(Console.ReadLine());
                            Console.WriteLine("Ingrese el texto de mensaje nuevo: ");
                            string texto = Console.ReadLine();
                            // Envío de mensaje cambiado
                            updateMessage = new Message(id, texto, user, 2);
                            jsonUpdateMessage = JsonConvert.SerializeObject(updateMessage);
                            stream = client.GetStream();
                            data = Encoding.UTF8.GetBytes(jsonUpdateMessage);
                            
                            stream.Write(data, 0, data.Length);

                            // Recepción de mensajes
                            byte[] package = new byte[1024];
                            stream.Read(package);
                            string serverMessage = Encoding.UTF8.GetString(package);
                            Console.WriteLine(serverMessage);
                        }
                        else
                        {
                            if(msg.StartsWith("Eliminar"))
                            {
                                // Envío de comandos
                                Console.WriteLine("Enviando comando Eliminar...");
                                Message deleteMessage = new Message(messageId, "Comando Eliminar", user,3);
                                string jsonDeleteMessage = JsonConvert.SerializeObject(deleteMessage);
                                var stream = client.GetStream();
                                byte[] data = Encoding.UTF8.GetBytes(jsonDeleteMessage);
                                stream.Write(data, 0, data.Length);

                                // Envío de mensaje cambiado
                                Console.WriteLine("Ingrese la id del mensaje a eliminar: ");
                                int id = Int32.Parse(Console.ReadLine());
                                deleteMessage = new Message(id, "Eliminado", user,3);
                                jsonDeleteMessage = JsonConvert.SerializeObject(deleteMessage);
                                stream = client.GetStream();
                                data = Encoding.UTF8.GetBytes(jsonDeleteMessage);
                                Console.WriteLine("Enviando datos para eliminar...");
                                stream.Write(data, 0, data.Length);

                                // Recepción de mensajes
                                byte[] package = new byte[1024];
                                stream.Read(package);
                                string serverMessage = Encoding.UTF8.GetString(package);
                                Console.WriteLine(serverMessage);   
                            }
                            else
                            {
                                Message newMessage = new Message(messageId, msg, user,0);
                                string jsonMessage = JsonConvert.SerializeObject(newMessage);
                                messageId = messageId + 1;

                                // Envío de datos
                                var stream = client.GetStream();
                                byte[] data = Encoding.UTF8.GetBytes(jsonMessage);
                                Console.WriteLine("Enviando mensaje a servidor...");
                                stream.Write(data, 0, data.Length);

                                // Recepción de mensajes
                                byte[] package = new byte[1024];
                                stream.Read(package);
                                string serverMessage = Encoding.UTF8.GetString(package);
                                Console.WriteLine(serverMessage);
                            }
                        }
                    }   
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error {0}", ex.Message);
                }
            }
            Console.WriteLine("Cerrando cliente...");
        }
    }
}