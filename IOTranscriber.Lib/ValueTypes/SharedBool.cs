using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type Bool
    /// </summary>
    public class SharedBool : SharedVariable<Boolean> {
        public class VariableChange : VariableChange<Boolean> {
            public VariableChange(SharedBool sharedValue, Boolean oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<Boolean> { }
    }
}
