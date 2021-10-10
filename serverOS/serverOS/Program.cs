using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerOs
{
    public class ClientObject
    {
        public Dictionary<string, Task<byte[]>> TasksAndRequests;
        public int id;

        public TcpClient client;
        public ClientObject(TcpClient tcpClient, Dictionary<string, Task<byte[]>> Spravochnik, int Id)
        {
            client = tcpClient;
            TasksAndRequests = Spravochnik;
            id = Id;
            Console.WriteLine("Клиент #{0} был подключен", Id);
        }
        public void Shpt(byte[] Answer, NetworkStream Stream)
        {
            try
            {
                Stream.Write(Answer, 0, Answer.Length);
            }
            catch
            {

            }
        }

        public byte[] Handling(string numString)
        {
            double numDouble;
            if (!double.TryParse(numString, out numDouble))
            {
                Console.WriteLine("Ошибка");
            }
            Thread.Sleep(8000);
            TasksAndRequests.Remove(numString);
            return Encoding.Unicode.GetBytes((Math.Round(numDouble)).ToString()+"meow");
        }
        public void Act()
        {
            Task<byte[]> ClientTask = null;
            NetworkStream stream = null;

            try
            {
                stream = client.GetStream();
                byte[] data = new byte[64];
                while (true)
                {

                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine("Клиент {0}: {1} => {2}", id, message, DateTime.Now.ToLongTimeString());


                    if (TasksAndRequests.ContainsKey(message))
                    {
                        ClientTask = TasksAndRequests[message];
                    }
                    else
                    {
                        if (ClientTask == null || ClientTask.Status == TaskStatus.RanToCompletion)
                        {
                            TasksAndRequests.Add(message, Task.Factory.StartNew(() => Handling(message)));
                            ClientTask = TasksAndRequests[message];
                        }
                        else
                        {

                            Shpt(Encoding.Unicode.GetBytes("Ожидание"), stream);
                            TasksAndRequests.Add(message, new Task<byte[]>(() => Handling(message)));

                            ClientTask.ContinueWith(Task => {
                                TasksAndRequests[message].Start();
                            });

                            ClientTask = TasksAndRequests[message];
                        }
                    }
                    ClientTask.ContinueWith(Task => {

                        var answer = ClientTask.Result;
                        Shpt(answer, stream);

                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Клиент #{id} отключен");
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }

    }
    class Program
    {
        static int id = 1;
        const int port = 8888;
        static TcpListener listener;
        static public Dictionary<string, Task<byte[]>> TasksAndRequests = new Dictionary<string, Task<byte[]>>();

        static void Main(string[] args)
        {
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client, TasksAndRequests, id++);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Act));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
    }
}
