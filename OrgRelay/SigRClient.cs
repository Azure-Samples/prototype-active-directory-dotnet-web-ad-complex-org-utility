using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using ADSync.Common.Models;
using System.Timers;
using System.Threading;
using System.Diagnostics;
using Common;
using ADSync.Common.Enums;
using Newtonsoft.Json;
using ADSync.Common.Events;

namespace OrgRelay
{
    public class SigRClient
    {
        #region Events
        public event EventHandler<StatusEvent> StatusUpdate;
        protected void OnStatusUpdate(StatusEvent e)
        {
            StatusUpdate?.Invoke(this, e);
        }

        public event EventHandler<ErrorEvent> ErrorEvent;
        protected void OnErrorEvent(ErrorEvent e)
        {
            ErrorEvent?.Invoke(this, e);
        }

        public event EventHandler<PingEvent> PingEvent;
        public void OnPingEvent(PingEvent e)
        {
            PingEvent?.Invoke(this, e);
        }
        #endregion

        #region Declarations
        private int _errorCount;
        private HubConnection _hubConnection;
        private string _apiKey;
        private IHubProxy _siteHubProxy;
        private bool _isRunning;
        private string _hubUrl;
        private string _hubUri;
        private string _proxyHub;
        private ValidationWaiter validationWaiter;
        private RelayWaiter relayWaiter;

        public string ConnectionId
        {
            get
            {
                return (_hubConnection != null && _hubConnection.State == ConnectionState.Connected) ? _hubConnection.ConnectionId : null;
            }
        }
        #endregion

        public SigRClient(string url, string apiKey, string proxyHub)
        {
            validationWaiter = new ValidationWaiter();
            relayWaiter = new RelayWaiter();
            _apiKey = apiKey;
            _proxyHub = proxyHub;
            _hubUri = string.Format("{0}{1}{2}", url, "sitelink/", _proxyHub);
            _hubUrl = string.Format("{0}{1}", url, "sitelink");
        }

        private void InitHubConnection()
        {
            try { _hubConnection.Dispose(); }
            catch (Exception) { }

            _hubConnection = new HubConnection(_hubUrl);

            _hubConnection.Error += _hubConnection_Error;
            _hubConnection.Closed += _hubConnection_Closed;
            _hubConnection.StateChanged += _hubConnection_StateChanged;

            _hubConnection.Headers.Add("apikey", _apiKey);
            _siteHubProxy = _hubConnection.CreateHubProxy(_proxyHub);

        }
        private void SetupListeners()
        {
            //Calls to Client
            _siteHubProxy.On<RelayMessage>("Send", msg => {
                var response = SiteOp.ProcessMessage(msg);
                if (response.Exception != null)
                {
                    //a fatal error occured while processing - will trigger the error event and return the failed response object
                    OnErrorEvent(new ErrorEvent("Error processing RelayMessage.", EventLogEntryType.Error, 100, response.Exception));
                }

                switch (msg.Operation)
                {
                    case SiteOperation.Ping:
                        OnPingEvent(new PingEvent(msg.Data, DateTime.Now));
                        break;

                    case SiteOperation.AddLogEntry:
                        var data = JsonConvert.DeserializeObject<ErrorEvent>(msg.Data);
                        OnErrorEvent(data);
                        break;

                    case SiteOperation.GetUserStatus:
                    case SiteOperation.DisableUser:
                    case SiteOperation.EnableUser:
                    case SiteOperation.ValidateUser:
                    case SiteOperation.ResetPW:
                    case SiteOperation.SetUserStatus:
                    case SiteOperation.UpdateUser:
                        ForwardRelayResponse(response);
                        break;

                    default:
                        ForwardRelayResponse(response);
                        break;
                }

                var status = string.Format("Send responded for {0} from {1} with success: {2}", response.Operation.ToString(), response.OriginSiteId, response.Success);
                SendStatus(status);
            });
            _siteHubProxy.On<RelayResponse>("ProcessRelayResponse", response => {
                //setting the relay response, releasing the waiter
                if (response.Identifier == relayWaiter.Identifier)
                {
                    relayWaiter.Awaiter.SetResult(response);
                }
            });

            //this one will only be sent to HQ
            _siteHubProxy.On<RelayMessage>("TriggerPoll", message => {
                SiteOp.ActivatePoll(message);
            });

            //STS Login/Validation listeners
            _siteHubProxy.On<STSCredential>("Validate", cred => {
                var res = SiteOp.GetValidationResponse(cred);
                ForwardValidationResponse(res);
            });
            _siteHubProxy.On<ValidationResponse>("ProcessValidationResponse", response => {
                //setting the validation response, releasing the waiter
                if (response.UserName == validationWaiter.UserName)
                {
                    validationWaiter.Awaiter.SetResult(response);
                }
            });
        }

        #region Events
        private void _hubConnection_StateChanged(StateChange obj)
        {
            switch (obj.NewState)
            {
                case ConnectionState.Connected:
                    _errorCount = 0;
                    SendStatus("Connected to {0}", _hubUri);
                    break;
            }
        }
        private void _hubConnection_Closed()
        {
            SendStatus("Connection closed");
            Reconnect();
        }

        private void _hubConnection_Error(Exception ex)
        {
            _errorCount++;
            SendStatus("An error occured, please check the application log ({0})", ex.Message);
            OnErrorEvent(new ErrorEvent("Error in client hub relay connection", EventLogEntryType.Error, 0, ex));
            //Reconnect();
        }
        #endregion

        #region Helpers
        private void SendStatus(string message)
        {
            var status = string.Format("{0}: {1}{2}", DateTime.Now, message, Environment.NewLine);
            OnStatusUpdate(new StatusEvent(status));
        }

        private void SendStatus(string message, params object[] args)
        {
            message = string.Format(message, args);
            SendStatus(message);
        }

        private void Reconnect()
        {
            if (!_isRunning) return;

            //_hubConnection.EnsureReconnecting();

            SendStatus("Connection closed, reconnecting...");

            //var delay = new Random().Next(2000, 4000);
            //delay ensures all the disconnected sites don't flood the server at the same time with
            //reconnection attempts
            //var t = Task.Delay(delay).ContinueWith((arg) =>
            //{
                StartAsync().Wait();
            //});

            //t.Wait();
        }
        #endregion

        public Task StartAsync()
        {
            InitHubConnection();
            SetupListeners();
            _isRunning = true;

            if (_hubConnection.State == ConnectionState.Disconnected)
            {
                return _hubConnection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error opening the connection: {0}",
                                          task.Exception.GetBaseException());
                        Reconnect();
                    }
                });
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public void Stop()
        {
            SendStatus("Service stopping");
            _isRunning = false;
            _hubConnection.Stop();
            SendStatus("Service stopped");
        }

        #region CallsToServer
        /// <summary>
        /// Called by the site to inform the HQ site to check the queue for a site update
        /// </summary>
        public void TriggerPoll()
        {
            SendStatus("Calling Trigger Poll...");
            _siteHubProxy.Invoke<RelayMessage>("TriggerPoll");
        }

        /// <summary>
        /// Called by the responding site to send an STS validation back to the hub for forwarding to the STS
        /// </summary>
        /// <param name="response"></param>
        public void ForwardValidationResponse(ValidationResponse response)
        {
            SendStatus("Calling forward validation response for \"{0}\", validation {1}...", response.UserName, (response.IsValid) ? "successful": "unsuccessful");
            _siteHubProxy.Invoke<ValidationResponse>("ForwardValidationResponse", response);
        }

        /// <summary>
        /// Called by the STS to send a validation request down to the appropriate site. Uses a semaphore to wait
        /// for the response
        /// </summary>
        /// <param name="credential"></param>
        /// <returns></returns>
        public async Task<ValidationResponse> ProcessSTSValidationRequest(STSCredential credential)
        {
            SendStatus("Processing STS validation request for \"{0}\"...", credential.UserName);
            validationWaiter.UserName = credential.UserName;

            await _siteHubProxy.Invoke<STSCredential>("ProcessSTSValidationRequest", credential);

            return await validationWaiter.Awaiter.Task;
        }

        /// <summary>
        /// Called by the responding site to send relay response back to the hub for forwarding to the requester
        /// </summary>
        /// <param name="res"></param>
        private void ForwardRelayResponse(RelayResponse response)
        {
            SendStatus("Calling forward relay response for {0} from {1}...", response.Operation.ToString(), response.OriginSiteId);
            _siteHubProxy.Invoke<RelayResponse>("ForwardRelayResponse", response);
        }

        /// <summary>
        /// Called by the site to send a generic request message up to the hub for forwarding to the destination, using the SiteOperation enum
        /// to specify the command to execute
        /// </summary>
        /// <param name="message"></param>
        public async Task<RelayResponse> ProcessRelayMessage(RelayMessage message)
        {
            SendStatus("Calling generic Send and awaiting a response");
            relayWaiter.Identifier = message.Identifier;
            await _siteHubProxy.Invoke<RelayMessage>("ProcessRelayMessage", message);

            return await relayWaiter.Awaiter.Task;
        }

        /// <summary>
        /// Called by the site to send a generic message up to the hub for further processing, using the SiteOperation enum
        /// to specify the command to execute
        /// </summary>
        /// <param name="message"></param>
        public void Send(RelayMessage message)
        {
            SendStatus("Calling generic Send");
            _siteHubProxy.Invoke<RelayMessage>("ProcessMessage", message);
        }
        #endregion
    }

    /// <summary>
    /// Contains the validation signal semaphore and ValidationResponse. Used to allow the STS request to wait while the async call is routed 
    /// onsite and back through the relay hub to the STS
    /// </summary>
    public class ValidationWaiter
    {
        public TaskCompletionSource<ValidationResponse> Awaiter { get; set; }
        public string UserName { get; set; }
        //public ValidationResponse Response { get; set; }

        public ValidationWaiter()
        {
            Awaiter = new TaskCompletionSource<ValidationResponse>();
        }
    }
    public class RelayWaiter
    {
        public TaskCompletionSource<RelayResponse> Awaiter { get; set; }
        public string Identifier { get; set; }

        public RelayWaiter()
        {
            Awaiter = new TaskCompletionSource<RelayResponse>();
        }
    }

}
