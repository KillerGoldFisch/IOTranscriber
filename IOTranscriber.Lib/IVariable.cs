using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;

namespace IOTranscriber.Lib {
    /// <summary>
    /// A Variable wich can be shared between mutliple communication Endpoints
    /// </summary>
    public interface IVariable : IMappingObject {
        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        event Action<IVariableChange> ValueChanged;

        /// <summary>
        /// Informs imediatly about changes of the value.
        /// </summary>
        event Action<IVariableChange> ValueChangedNoGeneric;

        /// <summary>
        /// Content of the Variable
        /// </summary>
        object Value { get; set; }

        /// <summary>
        /// Loads the value from the string.
        /// </summary>
        /// <param name="data"></param>
        void SetValueFromString(string data);

        /// <summary>
        /// The Config-Mapping object.
        /// </summary>
        MappingWrapper Mapping { get; }

        /// <summary>
        /// The Name of the Variable.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Datatype.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Values per secounds
        /// </summary>
        double VPS { get; }


        /// <summary>
        /// Returns the default buffer.
        /// </summary>
        /// <returns></returns>
        IVariableChangeBuffer GetBuffer();

        /// <summary>
        /// Returns the Value as String.
        /// </summary>
        /// <returns></returns>
        string GetStringFromValue();
    }

    /// <summary>
    /// Maintains a change of the Variable.
    /// </summary>
    public interface IVariableChange {
        /// <summary>
        /// Value before the change.
        /// </summary>
        Object OldValue { get; }

        /// <summary>
        /// Value after the change.
        /// </summary>
        Object NewValue { get; }

        /// <summary>
        /// The Variable wich has been changed
        /// </summary>
        IVariable SharedValue { get; }
    }

    /// <summary>
    /// Retains changes of a Variable.
    /// </summary>
    public interface IVariableChangeBuffer : IMappingObject {
        /// <summary>
        /// Get next change.
        /// </summary>
        /// <returns></returns>
        IVariableChange Pop();

        /// <summary>
        /// The Variable wich is bufferd.
        /// </summary>
        IVariable Variable { get; set; }

        /// <summary>
        /// How many changes will be buffers until the oldest will be overwritten.
        /// </summary>
        int BufferSize { get; }
    }
}
