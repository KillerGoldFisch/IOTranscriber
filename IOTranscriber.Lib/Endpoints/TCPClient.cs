using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Networking.Socket;
using GCore.Extensions.ArrayEx;
using GCore.Yaml.Config;
using GCore.Logging;
using GCore.Extensions.StringEx;
using GCore.Threading;
using System.Threading;


//Author : KEG
//Datum  : 16.05.2014 10:17:40
//Datei  : TCPServer.cs


namespace IOTranscriber.Lib.Endpoints {
    /// <summary>
    /// Endpoint as a TCP-Client.
    /// Sends and receaves data in following format:
    /// "[key1]:[value1];[key2]:[value2];..."
    /// </summary>
    public class TCPClient : ComEndpoint, IDisposable {

        #region Members
        // TCP-Client
        protected GSocket _socket;
        // True while application is active
        protected bool _connectServer = true;
        // TCP-Port
        protected int _port = 1234;
        // IP-Address
        protected string _host = "127.0.0.1";
        #endregion

        #region Events
        #endregion

        #region Initialization
        public TCPClient() {

        }
        #endregion

        #region Finalization
        ~TCPClient() {
            this.Dispose();
        }
        #endregion

        #region Interface
        #endregion

        #region Interface(ComEndpoint)
        protected override void InputChanges(IDictionary<IIOPipe, IVariableChange> changes) {
            // An input-Variable has changed
            if(this._socket == null)
                return;
            if(!this._socket.isActive)
                return;
            // Converts all changes to a data-string
            string msg = changes.GetString();
#if DEBUG
            Log.Debug(string.Format("[{0}] Sending Data '{1}'", this.ConfigURL, msg));
#endif
            // Send data-string to TCP-Server

            this._socket.Send(msg);
        }
        #endregion

        #region Interface(IMappingObject)
        public override void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            base.ReadMapping(mapping, config);

            // Read port-numer from config
            this._port = mapping.GetOrDef("port", this._port);
            // Read PI-Address from config
            this._host = mapping.GetOrDef("host", this._host);


            this.ConnectSocket();
        }

        public override void OnShutDown(YamlConfig conf) {
            base.OnShutDown(conf);
            this._connectServer = false;
        }
        #endregion

        #region Interface(IDisposable)
        public void Dispose() {
            this._connectServer = false;
        }
        #endregion

        #region Tools
        protected virtual void AttachSocket(GSocket socket) {
            if(this._socket != null)
                this.DetachSocket();
            this._socket = socket;
            this._socket.DataArrived += new GSocket.DataArrivedHandler(Socket_DataArrived);
            this._socket.Closed += new GSocket.ClosedHandler(Socket_Closed);

            if (this._inputs != null) {
                // If there is a Pipe named "SendAll", set it to true
                IIOPipe pipe = this._inputs.Get("SendAll");
                if (pipe != null)
                    pipe.Variable.Value = true;

                // Send data from current values
                string msg = this._inputs.GetString();
//#if DEBUG
//                Log.Debug(string.Format("[{0}] Sending Data {1}", this.ConfigURL, msg.ToLiteral()));
//#endif
                this._socket.Send(msg);
            }
        }

        protected void Socket_Closed(BaseSocket sender) {
            Thread.Sleep(500);
            Log.Warn(string.Format("[{2}] TCPClient connection lost {0}:{1}", this._host, this._port, this.ConfigURL));
            this.ConnectSocket();
        }

        protected virtual void ConnectSocket() {
            if (this._connectServer) {
                new GThread(() => {
                    // Try to connect as long as the application active is.
                    while (this._connectServer) {
                        try {
                            System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
                            client.Connect(this._host, this._port);
                            GSocket socket = new GSocket(client);
                            this.AttachSocket(socket);
                            Log.Success(string.Format("[{2}] TCPClient connected to {0}:{1}", this._host, this._port, this.ConfigURL));
                        } catch (Exception ex) {
                            Log.Exception(string.Format("[{2}] Cannot connect to {0}:{1}", this._host, this._port, this.ConfigURL), ex);
                        }
                        Thread.Sleep(5000);
                        while (this._socket != null && this._socket.isActive && this._connectServer)
                            Thread.Sleep(200);
                    }
                }, string.Format("[{0}] - Connect-thread", this.ConfigURL), true).Start();
            }
        }

        protected virtual void Socket_DataArrived(BaseSocket sender, byte[] Data)
        {
            GCore.Logging.Log.TryLog(() => {
                string msg = Data.ToUTF8String();
#if DEBUG
                Log.Debug(string.Format("[{0}] Data arrived: '{1}'", this.ConfigURL, msg));
#endif
                // Distributes the data to the output variables
                this._outputs.SetDataString(msg);
            }, string.Format("[{0}] Exception while processing message", this.ConfigURL));
        }

        protected virtual void DetachSocket() {
            if(this._socket == null)
                return;
            this._socket.DataArrived -= new GSocket.DataArrivedHandler(Socket_DataArrived);
            this._socket = null;
        }
        #endregion

        #region Browsable Properties
        public override bool IsConnected {
            get {
                if (this._socket == null)
                    return false;
                return this._socket.isActive;
            }
        }
        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}