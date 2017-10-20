using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.ValueTypes {
    /// <summary>
    /// Shared-Variable from type Double
    /// </summary>
    public class SharedDouble : SharedVariable<double>, IVariable {

        /// <summary>
        /// Loads the value from the string.
        /// </summary>
        /// <param name="data"></param>
        void IVariable.SetValueFromString(string data) {
            using (new GCore.Globalisation.Culture()) {
                this.Value = Double.Parse(data.Replace(',', '.'));
            }
        }

        string IVariable.GetStringFromValue() {
            return this.Value.ToString().Replace(',', '.');
        }

        public class VariableChange : VariableChange<double> {
            public VariableChange(SharedDouble sharedValue, double oldValue) : base(sharedValue, oldValue) { }
        }
        public class VariableChangeBuffer : VariableChangeBuffer<double> { }
    }
}
