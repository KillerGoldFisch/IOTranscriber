using System;
using System.Collections.Generic;
using GCore.Threading;
using System.Threading;
using GCore.Logging;
using GCore.Yaml.Config;
using GCore.Yaml;
using System.Text;
using System.IO;


//Author : KEG
//Datum  : 15.05.2014 13:48:02
//Datei  : ComEndpoint.cs


namespace IOTranscriber.Lib {
    /// <summary>
    /// Communication Endpoint
    /// </summary>
    public abstract class ComEndpoint : IMappingObject {

        #region Members
        // Manages the input-variables
        protected IOManager _inputs = new IOManager();
        // Manages the output-variables
        protected IOManager _outputs = new IOManager();
        // Name of the Endpoint
        protected string _name;
        // ID of the Endpoint
        protected string _id;
        // Log additional Informations
        protected bool _verbose = false;

        protected int _readingCycle = 40; //40ms
        protected GThread _readingThread;
        // Configuration
        protected MappingWrapper _mapping;
        #endregion

        #region Events
        #endregion

        #region Initialization
        public ComEndpoint() {
            
        }
        #endregion

        #region Finalization
        ~ComEndpoint() {
            if (this._readingThread != null)
                this._readingThread.Abort();
        }
        #endregion

        #region Abstract Interface
        protected abstract void InputChanges(IDictionary<IIOPipe, IVariableChange> changes);
        #endregion

        #region Interface
        protected virtual void ReadingTick() {
            // get all changes
            IDictionary<IIOPipe, IVariableChange> changes = this._inputs.GetChanges();
            if (changes.Count > 0) {
                try {
                    this.InputChanges(changes);
                } catch (Exception ex) {
                    Log.Exception(string.Format("[{0}] Exception while processing Changes", this.ConfigURL), ex);
                }
            }
            //this.InputChanges(this._inputs.GetChanges());
        }

        public String Dump() {
            MemoryStream ms = new MemoryStream();
            StreamWriter sw = new StreamWriter(ms);

            void PrintTable(IOManager iOManager) {
                string[,] tbl = new string[iOManager.Count, 2];

                int i = 0;
                foreach(var v in iOManager) {
                    tbl[i, 0] = v.Name;
                    tbl[i, 1] = v.Variable.Value.ToString();

                    i++;
                }


                GCore.XConsole.XConsole.ArrayPrinter.PrintToStream(tbl, sw);
            }

            sw.WriteLine(this.ConfigURL + ".Inputs");
            PrintTable(this.Inputs);

            sw.WriteLine("\n"+ this.ConfigURL + ".Outputs");
            PrintTable(this.Outputs);
            sw.Flush();

            ms.Position = 0;
            return new StreamReader(ms).ReadToEnd();
        }


        #endregion

        #region Interface(Object)
        public override string ToString() {
            return base.ToString() +"[" + this.ConfigURL + "]";
        }
        #endregion

        #region Interface(IMappingObject)
        public virtual void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            this._mapping = mapping;

#if DEBUG
            Log.Debug(string.Format("[{0}] Loading ComEndpoint({1})", this.ConfigURL, this.GetType().Name));
#endif
            // Get input-variable from config
            Sequence input = mapping.Get("input") as Sequence;
            // Get output-variable from config
            Sequence output = mapping.Get("output") as Sequence;
            if (input != null) {
                this._inputs = new IOManager(input, config, true);
            }
            if (output != null) {
                this._outputs = new IOManager(output, config, false);
            }
            // Get data from config
            this._name = mapping.GetOrDef("name", this._name);
            this._readingCycle = mapping.GetOrDef("readingcycle", this._readingCycle);
            this._verbose = mapping.GetOrDef("verbose", this._verbose);
            this._id = mapping.Mapping.GetAnchor();

            this.initReadingCycle();
        }

        public virtual void OnShutDown(YamlConfig conf) { }
        #endregion

        #region Tools
        protected void initReadingCycle() {
            if (this._inputs != null) {
                this._readingThread = new GThread(new ThreadStart(this.threadLoop), string.Format("[{0}] Reading Loop", this.ConfigURL), true);
                this._readingThread.Start();
            }
        }

        private void threadLoop() {
            // while application is active
            GThread.CycleSleeper sleeper = new GThread.CycleSleeper(TimeSpan.FromMilliseconds(this._readingCycle));
            while (this._mapping != null ? this._mapping.Config.Active : true) {
                //Thread.Sleep(this._readingCycle);
                sleeper.Sleep();

                if (!this.IsConnected)
                    continue;
                this.ReadingTick();
            }
        }
        #endregion

        #region Browsable Properties
        public abstract bool IsConnected {
            get;
        }
        public MappingWrapper Mapping { get { return this._mapping; } }
        public string ConfigURL {
            get {
                if (this.Mapping == null)
                    return "???";
                return this.Mapping.URL;
            }
        }

        public IOManager Inputs { get { return this._inputs; } }
        public IOManager Outputs { get { return this._outputs; } }
        #endregion

        #region NonBrowsable Properties
        public string Name { get { return this._name; } }
        #endregion
    }
}