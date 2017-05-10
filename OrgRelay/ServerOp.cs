using ADSync.Common.Enums;
using ADSync.Common.Events;
using ADSync.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelayMonitor
{
    public class ServerOp
    {
        public event EventHandler<MessageEventArgs> IncomingMessage;
        protected virtual void OnIncomingMessage(MessageEventArgs e)
        {
            if (IncomingMessage != null)
            {
                IncomingMessage?.Invoke(this, e);
            }
        }
        private void MessageIncoming(string message)
        {
            OnIncomingMessage(new MessageEventArgs
            {
                Message = message
            });
        }

        public event EventHandler<MessageEventArgs> OutgoingMessage;
        protected virtual void OnOutgoingMessage(MessageEventArgs e)
        {
            if (OutgoingMessage != null)
            {
                OutgoingMessage?.Invoke(this, e);
            }
        }
        private void MessageOutgoing(string message)
        {
            OnOutgoingMessage(new MessageEventArgs
            {
                Message = message
            });
        }

        /// <summary>
        /// Process incoming messages
        /// </summary>
        /// <param name="message"></param>
        public RelayMessage ProcessMessage(RelayMessage message)
        {
            switch (message.Operation)
            {
                case SiteOperation.TriggerPoll:

                    break;
            }
            return message;
        }
    }
}
