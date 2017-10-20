using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type Int64
    /// </summary>
    public class SharedInt64 : SharedVariable<Int64> {
        public class VariableChange : VariableChange<Int64> {
            public VariableChange(SharedInt64 sharedValue, Int64 oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<Int64> { }
    }
}
