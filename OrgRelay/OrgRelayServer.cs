using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using Microsoft.Azure.Relay;
using ADSync.Common.Events;
using OrgRelay;

namespace RelayMonitor
{
    public class OrgRelayServer : IServer
    {
        public static string ConnectionString { get; set; }

        private RelayConnectionStringBuilder _conn;
        private CancellationTokenSource _cts;

        public event EventHandler<MessageReceivedEventArgs> IncomingMessage;

        protected virtual void OnIncomingMessage(MessageReceivedEventArgs e)
        {
            if (IncomingMessage != null)
            {
                IncomingMessage?.Invoke(this, e);
            }
        }
        private void MessageIncoming(string message)
        {
            OnIncomingMessage(new MessageReceivedEventArgs
            {
                Message = message
            });
        }

        public OrgRelayServer()
        {
            if (ConnectionString.Length == 0)
            {
                throw new Exception("Please set the connection string property or call the parameterized constructor.");
            }
            _conn = new RelayConnectionStringBuilder(ConnectionString);
        }

        public OrgRelayServer(string connectionString)
        {
            ConnectionString = connectionString;
            _conn = new RelayConnectionStringBuilder(connectionString);
        }

        public void Start()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        // Method is used to initiate connection
        private async void ProcessMessagesOnConnection(HybridConnectionStream relayConnection, CancellationTokenSource cts)
        {
            Console.WriteLine("New session");

            // The connection is a fully bidirectional stream. 
            // We put a stream reader and a stream writer over it 
            // which allows us to read UTF-8 text that comes from 
            // the sender and to write text replies back.
            var reader = new StreamReader(relayConnection);
            var writer = new StreamWriter(relayConnection) { AutoFlush = true };
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    // Read a line of input until a newline is encountered
                    var line = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(line))
                    {
                        // If there's no input data, we will signal that 
                        // we will no longer send data on this connection
                        // and then break out of the processing loop.
                        await relayConnection.ShutdownAsync(cts.Token);
                        break;
                    }

                    MessageIncoming(line);

                    // Write the line back to the client, prepending "Echo:"
                    //await writer.WriteLineAsync($"Echo: {line}");
                }
                catch (IOException)
                {
                    // Catch an IO exception that is likely caused because
                    // the client disconnected.
                    Console.WriteLine("Client closed connection");
                    break;
                }
            }

            Console.WriteLine("End session");

            // Closing the connection
            await relayConnection.CloseAsync(cts.Token);
        }

        private async Task RunAsync()
        {
            _cts = new CancellationTokenSource();

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(_conn.SharedAccessKeyName, _conn.SharedAccessKey);
            var listener = new HybridConnectionListener(_conn.Endpoint, tokenProvider);

            // Subscribe to the status events
            listener.Connecting += Listener_Connecting;
            listener.Offline += Listener_Offline;
            listener.Online += Listener_Online;

            // Opening the listener will establish the control channel to
            // the Azure Relay service. The control channel will be continuously 
            // maintained and reestablished when connectivity is disrupted.
            await listener.OpenAsync(_cts.Token);
            Console.WriteLine("Server listening");

            // Providing callback for cancellation token that will close the listener.
            _cts.Token.Register(() => listener.CloseAsync(CancellationToken.None));

            // Start a new thread that will continuously read the console.
            new Task(() => Console.In.ReadLineAsync().ContinueWith((s) => {
                _cts.Cancel();
            })).Start();

            // Accept the next available, pending connection request. 
            // Shutting down the listener will allow a clean exit with 
            // this method returning null
            while (true)
            {
                var relayConnection = await listener.AcceptConnectionAsync();
                if (relayConnection == null)
                {
                    break;
                }

                ProcessMessagesOnConnection(relayConnection, _cts);
            }

            // Close the listener after we exit the processing loop
            await listener.CloseAsync(_cts.Token);
        }

        private void Listener_Online(object sender, EventArgs e)
        {
            Console.WriteLine("Listener online");
        }

        private void Listener_Offline(object sender, EventArgs e)
        {
            Console.WriteLine("Listener offline");
        }

        private void Listener_Connecting(object sender, EventArgs e)
        {
            Console.WriteLine("Listener Connecting");
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
