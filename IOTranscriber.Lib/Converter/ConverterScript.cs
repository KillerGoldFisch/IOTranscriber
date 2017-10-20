using System;
using System.Threading;
using GCore.Threading;
using GCore.Logging;
using GCore.Yaml.Config;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace IOTranscriber.Lib.Converter {
    public class ConverterScript : IMappingObject {

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

        protected Script _scriptEngine;
        #endregion

        #region Initialization
        public ConverterScript() {

        }
        #endregion

        #region Finalization
        ~ConverterScript() {

        }
        #endregion

        #region Interface

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

            string code = mapping.GetOrDef("code", "Result=value;");

            this._scriptEngine = CSharpScript.Create(
                code, 
                globalsType: typeof(ScriptContext)
                );

            try {
                this._scriptEngine.Compile();
                this._configOK = true;
            }catch(Exception ex) {
                this._configOK = false;
                Log.Exception($"Exception while compiling Script '{code}'", ex);
            }

            // If everything is ok, start the reader/writer Thread
            if (this._configOK) {
                this._inputBuffer = this._variableSrc.GetBuffer();
                this._workerThread = new GThread(new ThreadStart(this._worker), string.Format("[{0}] Worker", this.ConfigURL), true);
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
                var task = this._scriptEngine.RunAsync(new ScriptContext() { value = obj.NewValue });
                this._variableDst.Value = task.Result.GetVariable("Result").Value;
            } catch (Exception ex) {
                Log.Exception(string.Format("[{0}] Can't convert Value '{1}' to {2}",
                        this.ConfigURL, obj.NewValue, this._variableDst.Type), ex);
            }
        }

        protected virtual void _worker() {
            IVariableChange change = null;
            while (this._mapping.Config.Active) {
                // check for changes of the input-Variable
                change = this._inputBuffer.Pop();
                if (change != null) {
                    this.ValueChanged(change);
                    change =null;
                }
                Thread.Sleep(this._readingCycle);
            }
        }
        #endregion

        public class ScriptContext {
            public dynamic value;
            public dynamic Result;
        }
    }
}
