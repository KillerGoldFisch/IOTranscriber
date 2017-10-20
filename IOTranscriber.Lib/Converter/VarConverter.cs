using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Logging;
using GCore.Extensions.TypeEx;
using GCore.Threading;
using System.Threading;


//Author : KEG
//Datum  : 23.06.2014 14:36:11
//Datei  : VarConverter.cs


namespace IOTranscriber.Lib.Converter {
    /// <summary>
    /// Loadable convert-Module,
    /// Converts TSrc IVariable to TDst
    /// </summary>
    public class VarConverter<TSrc, TDst> : IMappingObject {

        #region Members
        // Config-Mapping for IMappingObject
        protected MappingWrapper _mapping;

        // Source and Desteny IVariable
        protected IVariable _variableSrc;
        protected IVariable _variableDst;

        // All configured types OK?
        protected bool _configOK = false;

        // Input buffer
        protected IVariableChangeBuffer _inputBuffer;

        // reader/writer Thread
        protected GThread _workerThread;
        protected int _readingCycle = 50; //50ms
        #endregion

        #region Initialization
        public VarConverter() {

        }
        #endregion

        #region Finalization
        ~VarConverter() {

        }
        #endregion

        #region Interface
        public virtual TDst ValConvert(TSrc val) {
            // Default conversion
            return (TDst)TConverter.ChangeType(typeof(TDst), val);
        }
        #endregion

        #region Interface(IMappingObject)
        public MappingWrapper Mapping {
            get { return this._mapping; }
        }

        public virtual void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            this._mapping = mapping;

            // Sleep time for reader/writer Thread
            this._readingCycle = mapping.GetOrDef("readingcycle", this._readingCycle);

            // Input- and output-Variable
            this._variableSrc = mapping.GetOrDef("src", this._variableSrc);
            this._variableDst = mapping.GetOrDef("dest", this._variableDst);

            // Config for both Variables are OK?
            bool configOKSrc = false;
            bool configOKDst = false;

            if (this._variableSrc != null) {
                // Is the input-Variable from the rigt Type?
                if(this._variableSrc.Type.IsCastableTo(typeof(TSrc))) {
                    configOKSrc=true;
                }else{
                    Log.Error(string.Format("[{0}] Src Variable Type '{1}' is not castable to '{2}'", 
                        this.ConfigURL, this._variableSrc.Type, typeof(TSrc)));
                }
            } else {
                Log.Error(string.Format("[{0}] No 'src' defined", this.ConfigURL));
            }

            if (this._variableDst != null) {
                // Is the output-Variable from the rigt Type?
                if (typeof(TDst).IsCastableTo(this._variableDst.Type)) {
                    configOKDst = true;
                } else {
                    Log.Error(string.Format("[{0}] Dest Variable Type '{1}' is not castable to '{2}'",
                        this.ConfigURL, typeof(TDst), this._variableDst.Type));
                }
            } else {
                Log.Error(string.Format("[{0}] No 'dest' defined", this.ConfigURL));
            }

            this._configOK = configOKSrc && configOKDst;

            // If everything is ok, start the reader/writer Thread
            if (this._configOK) {
                this._inputBuffer = this._variableSrc.GetBuffer();
                this._workerThread = new GThread(new ThreadStart(this._worker), string.Format("[{0}] Worker",this.ConfigURL), true);
                this._workerThread.Start();
            }
        }

        public string ConfigURL {
            get {
                if (this._mapping == null)
                    return "?!?";
                return this._mapping.GetURL();
            }
        }

        public virtual void OnShutDown(YamlConfig conf) {
        }
        #endregion

        #region Tools
        protected void ValueChanged(IVariableChange obj) {
            // A change of the input Variable has been triggert.
            try {
                this._variableDst.Value = this.ValConvert((TSrc)obj.NewValue);
            } catch (Exception ex) {
                Log.Exception(string.Format("[{0}] Value '{1}' is not conertable to '{2}'",
                        this.ConfigURL, obj.NewValue, typeof(TDst)), ex);
            }
        }

        protected virtual void _worker() {
            IVariableChange change = null;
            GThread.CycleSleeper sleeper = new GThread.CycleSleeper(TimeSpan.FromMilliseconds(this._readingCycle));
            while (this._mapping.Config.Active) {
                try {
                    //Thread.Sleep(this._readingCycle);
                    sleeper.Sleep();
                    // check for changes of the input-Variable
                    change = this._inputBuffer.Pop();
                    if (change != null) {
                        this.ValueChanged(change);
                        change = null;
                    }
                    //Thread.Sleep(this._readingCycle);
                } catch (Exception ex) {
                    Log.Exception(string.Format("[{0}] Exception while converting",
                        this.ConfigURL), ex);
                }
            }
        }
        #endregion
    }
}