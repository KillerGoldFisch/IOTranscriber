using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type UInt32
    /// </summary>
    public class SharedUInt32 : SharedVariable<UInt32> {
        public class VariableChange : VariableChange<UInt32> {
            public VariableChange(SharedUInt32 sharedValue, UInt32 oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<UInt32> { }
    }
}
