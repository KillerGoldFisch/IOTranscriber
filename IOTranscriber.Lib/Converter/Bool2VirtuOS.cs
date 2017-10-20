using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IOTranscriber.Lib.Converter {
    /// <summary>
    /// Loadable convert-Module,
    /// Converts Bool IVariable to Int32 Value for VirtuOS
    /// </summary>
    public class Bool2VirtuOS : VarConverter<bool, int> {
        public override int ValConvert(bool val) {
            // true = 1, false = 0
            return val ? 1 : 0;
        }
    }
}
