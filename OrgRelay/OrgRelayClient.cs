using ADSync.Common.Events;
using Microsoft.Azure.Relay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrgRelay
{
    public class OrgRelayClient
    {
        public static string ConnectionString { get; set; }
        public event EventHandler<MessageEventArgs> IncomingMessage;
        
        private RelayConnectionStringBuilder _conn;
        private StreamReader _reader;
        private TokenProvider _tokenProvider;
        private HybridConnectionStream _relayConnection;
        private HybridConnectionClient _client;
        private bool _runClient;

        protected virtual void OnIncomingMessage(MessageEventArgs e)
        {
            IncomingMessage?.Invoke(this, e);
        }
        public OrgRelayClient()
        {
            if (ConnectionString == "")
            {
                throw new Exception("Missing connection string");
            }

            _conn = new RelayConnectionStringBuilder(ConnectionString);
        }

        public OrgRelayClient(string connectionString)
        {
            ConnectionString = connectionString;
            _conn = new RelayConnectionStringBuilder(connectionString);
        }

        public void Start()
        {
            // Create a new hybrid connection client
            _tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_conn.SharedAccessKeyName, _conn.SharedAccessKey);
            _client = new HybridConnectionClient(_conn.Endpoint, _tokenProvider);

            _runClient = true;
            RunAsync().GetAwaiter().GetResult();
        }

        public void Stop()
        {
            _runClient = false;
        }

        /// <summary>
        /// We run two concurrent loops on the connection. One reads input from the console and writes it to the connection 
        /// with a stream writer. The other reads lines of input from the connection with a stream reader and writes them to the console. 
        /// Entering a blank line will shut down the write task after sending it to the server. The server will then cleanly shut down
        /// the connection which will terminate the read task.
        /// </summary>
        /// <returns></returns>
        protected async Task RunAsync()
        {
            // Initiate the connection
            _relayConnection = await _client.CreateConnectionAsync();

            var reads = GetReads(_relayConnection);
            
            // Wait for both tasks to complete
            await Task.WhenAll(reads);
            await _relayConnection.CloseAsync(CancellationToken.None);
        }

        /// <summary>
        /// Read from the hybrid connection and process
        /// </summary>
        /// <param name="relayConnection"></param>
        /// <returns></returns>
        protected Task GetReads(HybridConnectionStream relayConnection)
        {
            var reads = Task.Run(async () => {
                // Initialize the stream reader over the connection
                _reader = new StreamReader(relayConnection);
                do
                {
                    // Read a full line of UTF-8 text up to newline
                    string line = await _reader.ReadLineAsync();
                    // if the string is empty or null, we are done.
                    if (String.IsNullOrEmpty(line))
                        break;

                    // process message
                    OnIncomingMessage(new MessageEventArgs
                    {
                        Message = line
                    });
                }
                while (_runClient);
            });

            return reads;
        }

        /// <summary>
        /// Write to the hybrid connection
        /// </summary>
        /// <param name="relayConnection"></param>
        /// <returns></returns>
        public Task SendMessageAsync(string message)
        {
            using (var writer = new StreamWriter(_relayConnection))
            {
                // Write the line out, also when it's empty
                var res = writer.WriteLineAsync(message);
                return res;
            }
        }
    }
}
