using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ADSync.Common.Events;

namespace OrgRelay
{
    public class SigRServer : IServer
    {
        public event EventHandler<MessageReceivedEventArgs> IncomingMessage;

        public void Send(string message)
        {
            throw new NotImplementedException();
        }

        public void Send<T>(T data)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
