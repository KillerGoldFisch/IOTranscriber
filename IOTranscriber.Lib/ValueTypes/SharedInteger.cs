using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type Int32
    /// </summary>
    public class SharedInterger : SharedVariable<int> {
        public class VariableChange : VariableChange<int> {
            public VariableChange(SharedInterger sharedValue, int oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<int> { }
    }
}
