using System;
using System.Collections.Generic;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes
{
    public class SharedString : SharedVariable<String>, IVariable {
        public class VariableChange : VariableChange<String> {
            public VariableChange(SharedString sharedValue, String oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<String> { }
    }
}
