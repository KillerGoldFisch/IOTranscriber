using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Networking.Socket;
using GCore.Logging;


//Author : KEG
//Datum  : 16.05.2014 11:45:22
//Datei  : TCPServer.cs


namespace IOTranscriber.Lib.Endpoints {
    /// <summary>
    /// Endpoint as a TCP-Server with Listener.
    /// Sends and receaves data in following format:
    /// "[key1]:[value1];[key2]:[value2];..."
    /// </summary>
    public class TCPServer : TCPClient {

        #region Members
        // Listener for incoming Connections
        GSocketListener _listener;
        #endregion

        #region Events
        #endregion

        #region Initialization
        public TCPServer() {

        }
        #endregion

        #region Finalization
        ~TCPServer() {

        }
        #endregion

        #region Interface
        #endregion

        #region Interface(IMappingObject)
        public override void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            base._connectServer = false;
            base.ReadMapping(mapping, config);

            this._listener = new GSocketListener(this._port, new GSocketListener.ClientArrivedHandler(this.ClientArrived));
            Log.Info(string.Format("[{0}] Start listening on port {1}", this.ConfigURL, this._port));
        }

        public override void OnShutDown(YamlConfig conf) {
            base.OnShutDown(conf);
            try {
                this._listener.Kill();
            }catch(Exception ex) {
                Log.Debug("Exception while Killing TCP Listener", ex);
            }
        }
        #endregion

        #region Tools
        protected virtual void ClientArrived(GSocketListener listener, GSocket client) {
            if (client == this._socket)
                return;
            // Chack it there is alredy a valid client
            if (this._socket != null && this._socket.isActive) {
                Log.Warn(string.Format("[{0}] Second TCPClient connected from {1}:{2}, only one client supported",
                    this.ConfigURL,
                    ((System.Net.IPEndPoint)client.TCP_Client.Client.RemoteEndPoint).Address.ToString(),
                    this._port
                ));
                client.Kill();
                return;
            }

            Log.Success(string.Format("[{0}] New TCPClient connected from {1}:{2}",
                this.ConfigURL,
                ((System.Net.IPEndPoint)client.TCP_Client.Client.RemoteEndPoint).Address.ToString(),
                this._port
             ));

            this.AttachSocket(client);
        }
        #endregion

        #region Browsable Properties
        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}