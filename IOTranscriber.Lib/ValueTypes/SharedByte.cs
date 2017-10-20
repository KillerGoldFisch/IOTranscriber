using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type Byte
    /// </summary>
    public class SharedByte : SharedVariable<Byte> {
        public class VariableChange : VariableChange<Byte> {
            public VariableChange(SharedByte sharedValue, Byte oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<Byte> { }
    }
}
