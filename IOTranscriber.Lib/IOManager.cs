using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Extensions.TypeEx;
using GCore.Logging;
using GCore.Yaml;


//Author : KEG
//Datum  : 15.05.2014 14:58:15
//Datei  : ValueManager.cs


namespace IOTranscriber.Lib {
    /// <summary>
    /// Manages a bunch of input OR output pipes
    /// </summary>
    public class IOManager : List<IIOPipe>{

        #region Members
        /// <summary>
        /// Is this manager for input-pipes?
        /// </summary>
        protected bool _input = false;
        #endregion

        #region Events
        #endregion

        #region Initialization
        public IOManager() {

        }

        public IOManager(Sequence data, YamlConfig config, bool input) {
            this._input = input;
            // Read the input or output pipes
            foreach (DataItem item in data.Enties) {
                Log.TryLog(() => {
                    IMappingObject obj = config.GetMappingObject(item);
                    if (obj.GetType().IsCastableTo2(typeof(IVariable))) {
                        this.Add(new IOPipe(obj as IVariable, input));
                    } else if (obj.GetType().IsCastableTo2(typeof(IIOPipe))) {
                        this.Add(obj as IIOPipe);
                    } else {
                        Log.Error(string.Format("[{0}] Can't cast Object to Variable or Pipe: '{1}'", config.GetURL(item), obj), obj);
                    }
                }, string.Format("[{0}] Error while loading Pipes", config.GetURL(data)));
            }
        }
        #endregion

        #region Finalization
        ~IOManager() {

        }
        #endregion

        #region Interface
        /// <summary>
        /// Returns all changes from pipes
        /// </summary>
        /// <returns></returns>
        public IDictionary<IIOPipe, IVariableChange> GetChanges() {
            IDictionary<IIOPipe, IVariableChange> changes = new Dictionary<IIOPipe, IVariableChange>();
            foreach (IIOPipe pipe in this)
                if (pipe.ChangeBuffer != null) {
                    IVariableChange change = pipe.ChangeBuffer.Pop();
                    if (change != null)
                        changes.Add(pipe, change);
                }
            return changes;
        }

        public IIOPipe Get(string name) {
            foreach (IIOPipe pipe in this)
                if (pipe.Name.ToUpper() == name.ToUpper())
                    return pipe;
            return null;
        }
        #endregion

        #region Interface(Object)
        public override string ToString() {
            return base.ToString() + "(" + (this._input ? "Input" : "Output") + ")";
        }
        #endregion

        #region Tools
        #endregion

        #region Browsable Properties
        public IIOPipe[] Entrys { get { return this.ToArray(); } }
        #endregion

        #region NonBrowsable Properties
        #endregion
    }
}