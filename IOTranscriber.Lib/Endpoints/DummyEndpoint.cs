using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//Author : KEG
//Datum  : 05.06.2014 17:45:30
//Datei  : DummyEndpoint.cs


namespace IOTranscriber.Lib.Endpoints {
    /// <summary>
    /// Endpoint without a real Connection
    /// </summary>
    public class DummyEndpoint : ComEndpoint {

        #region Members
        #endregion

        #region Events
        #endregion

        #region Initialization
        public DummyEndpoint() {

        }
        #endregion

        #region Finalization
        ~DummyEndpoint() {

        }
        #endregion

        #region Interface
        #endregion

        #region Interface(ComEndpoint)
        protected override void InputChanges(IDictionary<IIOPipe, IVariableChange> changes) {
        }

        public override bool IsConnected {
            get { return true; }
        }
        #endregion

        #region Tools
        #endregion

        #region Browsable Properties
        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}