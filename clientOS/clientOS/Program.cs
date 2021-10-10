using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientOs
{
    class Program
    {
        const int port = 8888;
        const string address = "127.0.0.1";
        static NetworkStream stream;
        public static string UserName;

        static public void SendtoServer()
        {
            while (true)
                try
                {
                    double number = Math.Round(-1d + 1.3d * new Random().NextDouble(), 1);// Формирование числа согласно заданным параметрам
                    Console.WriteLine(UserName + ": " + number);
                    string message = number.ToString();// Запись числа
                    byte[] data = Encoding.Unicode.GetBytes(message);// Преобразование числа в массив
                    stream.Write(data, 0, data.Length);
                    Thread.Sleep(1000);// ожидание 1 секунда
                }
                catch
                {
                    Console.WriteLine("Ошибка формирования сообщения");
                    break;
                }
        }
        static void Main(string[] args)
        {
            Console.Write("Введите имя клиента:");
            UserName = Console.ReadLine();
            TcpClient client = new TcpClient(address, port);
            stream = client.GetStream();

            try
            {


                new Thread(new ThreadStart(SendtoServer)).Start();


                while (true)
                {

                    var data = new byte[256];
                    StringBuilder Strbuilder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);// Чтение данных от сервера
                        Strbuilder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    var message = Strbuilder.ToString();
                    Console.WriteLine("Сервер: {0}", message);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Close();
            }

        }
               
    }
}
