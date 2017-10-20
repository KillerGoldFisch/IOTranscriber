using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;


//Author : KEG
//Datum  : 15.05.2014 15:55:30
//Datei  : Input.cs


namespace IOTranscriber.Lib {
    /// <summary>
    /// Represents a pipe from an Endpoint to a Variable
    /// </summary>
    public class IOPipe: IIOPipe, IMappingObject {

        #region Members
        // Config object
        protected MappingWrapper _mapping;
        
        protected IVariable _value;
        // Buffer for changes from the variable
        protected IVariableChangeBuffer _buffer;

        protected string _displayName = null;
        #endregion

        #region Events
        #endregion

        #region Initialization
        public IOPipe() {

        }

        public IOPipe(IVariable variable, bool input = false) {
            this._value = variable;
            if(input)
                this._buffer = variable.GetBuffer();
        }
        #endregion

        #region Finalization
        ~IOPipe() {

        }
        #endregion

        #region Interface
        #endregion

        #region Interface(IIOPipe)

        public IVariable Variable {
            get { return this._value; }
        }
        public string Name {
            get {
                string tmp = this._mapping != null ? this._mapping.GetOrDef<string>("name", null) : null;
                if (tmp == null)
                    tmp = this._value.Name;
                return tmp;
            }
        }
        public string DisplayName {
            get { return this._displayName != null ? this._displayName : (this as IIOPipe).Name; }
        }
        public IVariableChangeBuffer ChangeBuffer { get { return this._buffer; } }
        #endregion

        #region Interface(IMappingObject)
        public void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            this._mapping = mapping;

            if (!mapping.Get("value", (IVariable value) => {
                this._value = value;
            })) {
                GCore.Logging.Log.Fatal("Cannot load value");
                return;
            }

            this._buffer = this._mapping.GetOrDef<IVariableChangeBuffer>("changebuffer", null);
            if (this._buffer == null) {
                this._buffer = this._value.GetBuffer();
            }
            if(this._buffer != null) this._buffer.Variable = this._value;

            this._displayName = mapping.GetOrDef("displayname", this._displayName);
        }
        public string ConfigURL {
            get {
                if (this.Mapping == null)
                    return "???";
                return this.Mapping.URL;
            }
        }
        public void OnShutDown(YamlConfig conf) { }
        #endregion

        #region Tools
        #endregion

        #region Browsable Properties
        public MappingWrapper Mapping {
            get { return this._mapping; }
        }

        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}