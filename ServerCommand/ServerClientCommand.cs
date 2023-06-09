using System.Net.Sockets;
using System.Text;
using Contacts;

namespace ServerCommand
{
   public class ServerClientCommand
    {
        public List<Socket> clientSockets = new List<Socket>();
        public List<ClientContacts> contacts = new List<ClientContacts>();
        public string _eror = "";

        public bool GetClientSocket(Socket server)
        {
            try
            {
                server.BeginAccept(ServerAcceptDelegate, server);
            }
            catch (Exception ex)
            {
                _eror = ex.Message;
            }
            return _eror.Length > 0 ? false : true;
        }
        void ServerAcceptDelegate(IAsyncResult result)
        {
            if (result != null)
            {
                Socket serv = (Socket)result.AsyncState;
                if (serv != null)
                {
                    Socket clientsocket = serv.EndAccept(result);
                    clientSockets.Add(clientsocket);
                    byte[] buffer = Encoding.UTF8.GetBytes("Успешное подключение!servclientcomm");
                    ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
                    clientsocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendMessageDelegate, clientsocket);
                    //clientsocket.SendAsync(segment, SocketFlags.None);

                }
            }
        }
        void SendMessageDelegate(IAsyncResult result) 
        {
            Socket client = (Socket)result.AsyncState;
            client.EndSend(result);
        }
        public string ReciveMessage(Socket client)
        {
            string text = "";
            byte[] buffer = new byte[1024];
            ArraySegment<byte> segment = new ArraySegment<byte>(buffer, 0, buffer.Length);
            Task<int> answer = client.ReceiveAsync(segment, SocketFlags.None);
            if (answer.IsCompleted)
            {
                text = Encoding.UTF8.GetString(segment.ToArray());
            }
            return text;
        }
        public void CommandManage(string text, Socket client)
        {
            if (text.StartsWith("Contact"))
                AddNewContact(text.Split('|'), client);
        }

        void AddNewContact(string[] data, Socket client) => contacts.Add(new ClientContacts(client, data[1], data[3], data[2], data[4]));
    }
}