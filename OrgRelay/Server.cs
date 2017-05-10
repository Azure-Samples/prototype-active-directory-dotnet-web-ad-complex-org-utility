using ADSync.Common.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrgRelay
{
    public class Server : IServer
    {
        public event EventHandler<MessageReceivedEventArgs> IncomingMessage;

        public Server()
        {

        }
        protected virtual void OnIncomingMessage(MessageReceivedEventArgs e)
        {
            IncomingMessage?.Invoke(this, e);
        }
        public void ProcessMessage(string message)
        {

        }
        public void ProcessMessage<RelayMessage>(RelayMessage data)
        {

        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}
