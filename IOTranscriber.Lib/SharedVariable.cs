using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Logging;
using GCore.Diagnostics;

//Author : KEG
//Datum  : 14.05.2014 17:17:30
//Datei  : SharedValue.cs


namespace IOTranscriber.Lib {
    /// <summary>
    /// A Variable wich can be shared between mutliple communication Endpoints
    /// </summary>
    /// <typeparam name="T">Datatype of the shared value</typeparam>
    public class SharedVariable<T> : IMappingObject, IVariable {

        #region Members
        // The value of the Variable
        protected T _value;
        protected string _id;
        protected string _name;
        protected MappingWrapper _mapping;

        // Values per secound counter
        protected CPSCounter _vps = new CPSCounter(20);
        #endregion

        #region Events
        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        public event Action<VariableChange<T>> ValueChanged;

        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        public event Action<IVariableChange> ValueChangedNoGeneric;
        #endregion

        #region Initialization
        public SharedVariable() {

        }

        public SharedVariable(string id = null, string name = null, T value = default(T)) {
            this._id = id;
            this._name = name;
            this._value = value;
        }
        #endregion

        #region Finalization
        ~SharedVariable() {

        }
        #endregion

        #region Interface
        #endregion

        #region Interface(Object)
        public override string ToString() {
            return base.ToString() + "[" + this.ConfigURL + "]";
        }
        #endregion

        #region Interface(IMappingObject)
        public virtual void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            // Becouse a Variable is a shared by references
            this._id = mapping.Mapping.GetAnchor();

            // Load from config
            this._name = mapping.GetOrDef("name", this._name);
            this._value = mapping.GetOrDef("value", this._value);
            this._mapping = mapping;
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

        #region Interface(IVariable)
        /// <summary>
        /// Content of the Variable
        /// </summary>
        object IVariable.Value {
            get {
                return this.Value;
            }
            set {
                this.Value = (T)value;
            }
        }


        /// <summary>
        /// Values per secounds
        /// </summary>
        double IVariable.VPS {
            get { return _vps.CPS; }
        }

        /// <summary>
        /// Loads the value from the string.
        /// </summary>
        /// <param name="data"></param>
        void IVariable.SetValueFromString(string data) {
            this.Value = TConverter.ChangeType<T>(data);
        }

        string IVariable.GetStringFromValue() {
            return this.Value.ToString();
        }

        /// <summary>
        /// The Config-Mapping object.
        /// </summary>
        public MappingWrapper Mapping {
            get { return this._mapping; }
        }

        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        event Action<IVariableChange> IVariable.ValueChanged {
            add { this.ValueChanged += value; }
            remove { this.ValueChanged -= value; }
        }

        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        event Action<IVariableChange> IVariable.ValueChangedNoGeneric {
            add { this.ValueChangedNoGeneric += value; }
            remove { this.ValueChangedNoGeneric -= value; }
        }

        /// <summary>
        /// The Datatype.
        /// </summary>
        Type IVariable.Type {
            get { return typeof(T); }
        }

        /// <summary>
        /// Returns the default buffer.
        /// </summary>
        /// <returns></returns>
        public virtual IVariableChangeBuffer GetBuffer() {
            return new VariableChangeBuffer<T>(this, 1);
        }
        #endregion

        #region Tools
        protected void RaiseValueChanged(T oldValue) {
# if DEBUG
            Log.Debug(string.Format("[{0}] ({3}) Value changed from '{1}' to '{2}'", this.ConfigURL, oldValue, this.Value, this.Name));
#endif
            // Values per secound counter
            this._vps.Call();

            VariableChange<T> c = new VariableChange<T>(this, oldValue);
            if (this.ValueChanged != null) {
                this.ValueChanged(c);
            }

            if (this.ValueChangedNoGeneric != null) {
                this.ValueChangedNoGeneric(c);
            }
        }
        #endregion

        #region Browsable Properties
        public string ID { get { return this._id; } }
        public string Name { get { return this._name; } }

        public T Value {
            get { return this._value; }
            set {
                if (!this.IsEqual(value, this._value)) {
                    T old = this._value;
                    this._value = value;
                    this.RaiseValueChanged(old);
                }
            }
        }

        public virtual bool IsEqual<T>(T v1, T v2) {
            return EqualityComparer<T>.Default.Equals(v1, v2);
        }
        #endregion

        #region NonBrowsable Properties
        #endregion

        /// <summary>
        /// Maintains a change of the Variable.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        public class VariableChange<T1> : IVariableChange {
            protected T1 _oldValue;
            protected T1 _newValue;
            protected SharedVariable<T1> _sValue;

            public VariableChange(SharedVariable<T1> sharedValue, T1 oldValue) {
                this._newValue = sharedValue.Value;
                this._oldValue = oldValue;
                this._sValue = sharedValue;
            }

            /// <summary>
            /// Value before the change.
            /// </summary>
            public T1 OldValue { get { return this._oldValue; } }

            /// <summary>
            /// Value after the change.
            /// </summary>
            public T1 NewValue { get { return this._newValue; } }

            /// <summary>
            /// The Variable wich has been changed
            /// </summary>
            public SharedVariable<T1> SharedValue { get { return this._sValue; } }

            /// <summary>
            /// Value before the change.
            /// </summary>
            object IVariableChange.OldValue {
                get { return this._oldValue; }
            }

            /// <summary>
            /// Value after the change.
            /// </summary>
            object IVariableChange.NewValue {
                get { return this._newValue; }
            }

            /// <summary>
            /// The Variable wich has been changed
            /// </summary>
            IVariable IVariableChange.SharedValue {
                get { return this._sValue; }
            }
        }

        /// <summary>
        /// Retains changes of a Variable.
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        public class VariableChangeBuffer<T2> : IDisposable, IMappingObject, IVariableChangeBuffer {

            #region Members
            protected SharedVariable<T2> _sValue;
            protected IList<SharedVariable<T2>.VariableChange<T2>> _changes;
            protected int _bufferSize = 1;
            protected MappingWrapper _mapping;
            #endregion

            #region Initialization
            public VariableChangeBuffer() {
                this.new_changes();
            }

            public VariableChangeBuffer(SharedVariable<T2> sharedValue, uint buffersize = 1) {
                this._bufferSize = (int)buffersize;
                this._sValue = sharedValue;
                this.init();
            }

            ~VariableChangeBuffer() {
                this.Dispose();
            }
            #endregion

            #region Tools
            protected virtual void SharedValue_ValueChanged(SharedVariable<T2>.VariableChange<T2> obj) {
                this._changes.Add(obj);
                while (this._changes.Count > this._bufferSize && this._changes.Count > 0)
                    this._changes.RemoveAt(0);
            }

            protected virtual void new_changes() {
                this._changes = new List<SharedVariable<T2>.VariableChange<T2>>();
            }

            protected virtual void init() {
                if (this._sValue != null) {
                    this.new_changes();
                    this._sValue.ValueChanged += new Action<SharedVariable<T2>.VariableChange<T2>>(SharedValue_ValueChanged);
                }
            }

            protected virtual void AttachValue(SharedVariable<T2> value) {
                if(this._sValue != null)
                    this.DetachValue();
                this._sValue = value;
                this.init();
            }
            protected virtual void DetachValue() {
                if(this._sValue == null)
                    return;
                this._sValue.ValueChanged -= new Action<SharedVariable<T2>.VariableChange<T2>>(SharedValue_ValueChanged);
                this._sValue = null;
            }
            #endregion

            #region Interface
            /// <summary>
            /// Get next change.
            /// </summary>
            /// <returns></returns>
            public virtual SharedVariable<T2>.VariableChange<T2> Pop() {
                if (this._changes.Count == 0)
                    return null;
                SharedVariable<T2>.VariableChange<T2> tmp = this._changes[0];
                this._changes.RemoveAt(0);
                return tmp;
            }

            /// <summary>
            /// If a change is ready, the callback function will be executed.
            /// </summary>
            /// <returns></returns>
            public virtual bool Pop(Action<SharedVariable<T2>.VariableChange<T2>> callback) {
                SharedVariable<T2>.VariableChange<T2> tmp = this.Pop();
                if (tmp != null) {
                    callback(tmp);
                    return true;
                }
                return false;
            }
            #endregion

            #region Interface(IMappingObject)
            public virtual void ReadMapping(MappingWrapper mapping, YamlConfig config) {
                if(!mapping.Get<SharedVariable<T2>>("value", (SharedVariable<T2> value)=>{
                    this._sValue = value;
                })) {
                    //throw new Exception(GCore.Diagnostics.StackTraceInfo.Inject("{stacktrace} - Cannot load SharedValue"));
                }
                this._bufferSize = mapping.GetOrDef("buffersize", this._bufferSize);
                this.init();
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

            #region Interface(IDisposable)
            public virtual void Dispose() {
                if(this._sValue != null)
                    this._sValue.ValueChanged -= new Action<SharedVariable<T2>.VariableChange<T2>>(SharedValue_ValueChanged);
            }
            #endregion

            #region Interface(IValue.IValueChangeBuffer)
            /// <summary>
            /// Get next change.
            /// </summary>
            /// <returns></returns>
            IVariableChange IVariableChangeBuffer.Pop() {
                return this.Pop();
            }

            /// <summary>
            /// If a change is ready, the callback function will be executed.
            /// </summary>
            /// <returns></returns>
            IVariable IVariableChangeBuffer.Variable {
                get { return this.Value; }
                set { if (this._sValue != value) this.AttachValue(value as SharedVariable<T2>); }
            }
            #endregion

            #region Browsable Properties
            public SharedVariable<T2> Value { get { return this._sValue; } }

            /// <summary>
            /// How many changes will be buffers until the oldest will be overwritten.
            /// </summary>
            public int BufferSize { get { return this._bufferSize; } }
            public MappingWrapper Mapping { get { return this._mapping; } }
            #endregion
        }
    }
}