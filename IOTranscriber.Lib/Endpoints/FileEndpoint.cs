using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Extensions.SerializingEx;
using GCore.Extensions.FileInfoEx;
using System.IO;
using GCore.Logging;
using System.Xml.Serialization;


//Author : KEG
//Datum  : 09.07.2014 15:03:45
//Datei  : FileEndpoint.cs


namespace IOTranscriber.Lib.Endpoints {
    public class FileEndpoint : ComEndpoint {

        public enum Mode {
            Record,
            Play
        }

        [Serializable]
        public struct entry {
            [XmlIgnore]
            public TimeSpan Time;
            public long Ticks {
                get { return this.Time.Ticks; }
                set { this.Time = TimeSpan.FromTicks(value); }
            }
            public string Name;
            public string Value;
        }

        #region Members
        protected List<entry> _entrys = new List<entry>();
        protected DateTime _startTime = DateTime.Now;
        protected Mode _mode = Mode.Record;
        protected string _fileName = "record.bin";
        protected SerializingExtensions.Serializer _serializer = SerializingExtensions.Serializer.Binary;
        protected bool _loop = true;
        protected entry[] _entrysForLoop = null;
        protected bool _loaded = false;
        #endregion

        #region Events
        #endregion

        #region Initialization
        public FileEndpoint() {

        }
        #endregion

        #region Finalization
        ~FileEndpoint() {

        }
        #endregion

        #region Interface
        protected virtual entry[] CollectChanges() {
            TimeSpan now = DateTime.Now - this._startTime;
            entry[] e = (from r in this._entrys where r.Time<now select r).ToArray();
            if (e.Length > 0) {
                //this._entrys.RemoveAll(ent => e.Contains(ent));
                foreach (entry ee in e)
                    this._entrys.Remove(ee);
            }
                
            if (this._entrys.Count == 0 && this._loop && this._entrysForLoop != null) {
                lock (this._entrysForLoop)
                    this._entrys = this._entrysForLoop.ToList();
                this._startTime = DateTime.Now;
            }
                
            return e;
        }

        protected virtual void OutputChanges(entry[] e) {
            foreach (entry ent in e) {
                IIOPipe pipe = this._outputs.Get(ent.Name);
                //pipe.Variable.Value = ent.Value.DeserializeBinary<Object>();
                pipe.Variable.SetValueFromString(ent.Value);
            }
        }
        #endregion

        #region Interface(IMappingObject)
        public override void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            base.ReadMapping(mapping, config);

            this._fileName = mapping.GetOrDef("file", "record_" + this.Name + ".bin");
            this._mode = mapping.GetOrDef("mode", this._mode);
            this._serializer = mapping.GetOrDef("serializer", this._serializer);

            mapping.TryGet(ref this._loop, "loop");

            this.readFile();

        }

        public override void OnShutDown(YamlConfig conf) {
            base.OnShutDown(conf);

            if (this._mode == Mode.Record) {
                try {
                    FileInfo fi = new FileInfo(this._fileName);
                    fi.SetObject(this._entrys.ToArray(), this._serializer);
                } catch (Exception ex) {
                    string msg = string.Format("[{0}] Error writing record '{1}'",
                            this.ConfigURL, this._fileName);
                    Log.Exception(msg , ex);
                }
            }
        }
        #endregion

        #region Interface(ComEndpoint)
        protected override void ReadingTick() {
            try {
                switch (this._mode) {
                    case Mode.Record:
                        this.InputChanges(this._inputs.GetChanges());
                        break;
                    case Mode.Play:
                        entry[] e = this.CollectChanges();
                        if (e.Length > 0)
                            this.OutputChanges(e);
                        break;
                }
            } catch (Exception ex) {
                Log.Exception("Exception while reading", ex);
            }
        }

        protected override void InputChanges(IDictionary<IIOPipe, IVariableChange> changes) {
            foreach(IIOPipe pipe in changes.Keys) {
                entry e = new entry();
                e.Time = DateTime.Now - _startTime;
                e.Name = pipe.Name;
                e.Value = changes[pipe].NewValue.ToString(); //.SerializeBinary();
                this._entrys.Add(e);
            }
        }

        public override bool IsConnected {
            get { return this.Mapping.Config.Active && this._loaded; }
        }
        #endregion

        #region Tools
        protected void readFile() {
            if (this._mode == Mode.Play) {
                try {
                    FileInfo fi = new FileInfo(this._fileName);
                    this._entrys = fi.GetObject<entry[]>(this._serializer).ToList();
                    Log.Success(string.Format("[{0}] Record loaded: {1} entrys",
                            this.ConfigURL, this._entrys.Count));
                    if (this._entrys.Count == 0)
                        this._loop = false;
                    if (this._loop) {
                        this._entrysForLoop = this._entrys.ToArray();
                    }
                    this._loaded = true;
                } catch (Exception ex) {
                    this._loop = false;
                    Log.Exception(string.Format("[{0}] Error reading record '{1}'",
                            this.ConfigURL, this._fileName), ex);
                }
            } else {
                this._loaded = true;
            }
        }
        #endregion

        #region Browsable Properties
        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}