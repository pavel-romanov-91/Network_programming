using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace part_2_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {

                IPEndPoint serverPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80);
                Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                serverSocket.Bind(serverPoint);
                serverSocket.Listen(1000);
                serverSocket.BeginAccept(AcceptConnectionCallback, serverSocket);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            Console.ReadLine();
        }
        static void AcceptConnectionCallback(IAsyncResult result)
        {
            try
            {

                if (result.AsyncState != null)
                {
                    byte[] data = new byte[1024];
                    Socket server = (Socket)result.AsyncState;
                    Socket client = server.EndAccept(result);
                    Console.WriteLine($"Произошло подключение: {client.RemoteEndPoint} в {DateTime.Now} ");
                    client.BeginReceive(data, 0, data.Length, SocketFlags.None, ClientReceiveMessageCallback, new ClientMessage(client, data));
                    server.BeginAccept(AcceptConnectionCallback, server);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        static void ClientReceiveMessageCallback(IAsyncResult result)
        {
            if (result.AsyncState != null)
            {
                ClientMessage clientMessage = (ClientMessage)result.AsyncState;
                Socket client = clientMessage.GetClient();
                byte[] data = clientMessage.GetData();
                client.EndReceive(result);
                string message = Encoding.UTF8.GetString(data);
                if (message.StartsWith("Exit"))
                {
                    Console.WriteLine($"Клиент разорвал соединение : {client.RemoteEndPoint} в {DateTime.Now} ");
                    client.Close();
                }
                else
                {

                    string[] currencys = message.Split(' ');
                    string answer = CurrencyList.GetValue(currencys[0], currencys[1]).ToString();
                    byte[] senddata = Encoding.UTF8.GetBytes(answer);

                    Console.WriteLine($"Клиент : {client.RemoteEndPoint} запросил перевод {message.Substring(0, 8)} в {DateTime.Now} ");
                    client.BeginSend(senddata, 0, senddata.Length, SocketFlags.None, ClientSendMessageCallback, client);
                    if (client != null)
                    {
                        client.BeginReceive(data, 0, data.Length, SocketFlags.None, ClientReceiveMessageCallback, new ClientMessage(client, data));
                    }
                }
            }
        }
        static void ClientSendMessageCallback(IAsyncResult result)
        {
            if (result.AsyncState != null)
            {
                Socket client = (Socket)result.AsyncState;
                client.EndSend(result);
            }
        }
        class ClientMessage
        {
            Socket _client;
            byte[] _message = new byte[1024];
            public ClientMessage(Socket client, byte[] message)
            {
                _client = client;
                _message = message;
            }
            public Socket GetClient()
            {
                return _client;
            }
            public byte[] GetData()
            {
                return _message;
            }
        }
        static class CurrencyList
        {
            static double EUR = 82.3736;
            static double USD = 77.2422;
            static double GBP = 93.7720;
            static public double GetValue(string first, string second)
            {
                double result = 0;
                switch (first)
                {
                    case "EUR": if (second == "USD") { result = EURtoUSD(); } else { result = EURtoGBP(); } break;
                    case "USD": if (second == "EUR") { result = USDtoEUR(); } else { result = USDtoGBP(); } break;
                    case "GBP": if (second == "EUR") { result = GBPtoEUR(); } else { result = GBPtoUSD(); } break;
                }
                return result;
            }
            static double USDtoEUR() { return USD / EUR; }
            static double USDtoGBP() { return USD / GBP; }
            static double EURtoUSD() { return EUR / USD; }
            static double EURtoGBP() { return EUR / GBP; }
            static double GBPtoUSD() { return GBP / USD; }
            static double GBPtoEUR() { return GBP / EUR; }

        }
    }
}
