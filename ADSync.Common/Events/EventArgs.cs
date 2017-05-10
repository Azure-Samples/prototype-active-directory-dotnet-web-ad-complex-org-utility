using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADSync.Common.Events
{
    public class MessageEventArgs : EventArgs
    {
        public string Message { get; set; }
        public string ConnectionId { get; set; }
        public string ApiKey { get; set; }

    }
}

