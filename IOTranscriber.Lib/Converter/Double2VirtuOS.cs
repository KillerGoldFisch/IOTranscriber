using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;
using GCore.Logging;


//Author : KEG
//Datum  : 23.06.2014 14:23:47
//Datei  : Double2VirtuOS.cs


namespace IOTranscriber.Lib.Converter {
    /// <summary>
    /// Loadable convert-Module,
    /// Converts Double IVariable to Int32 Value for VirtuOS
    /// </summary>
    public class Double2VirtuOS : VarConverter<double, int> {

        #region Members
        // Convert gain
        protected double _factor = 1000.0;
        #endregion

        #region Initialization
        public Double2VirtuOS() {

        }
        #endregion

        #region Finalization
        ~Double2VirtuOS() {

        }
        #endregion

        #region VarConverter
        public override int ValConvert(double val) {
            // Double with gain to Int32
            return (int)(val * this._factor);
        }
        #endregion

        #region Interface(IMappingObject)
        public override void ReadMapping(MappingWrapper mapping, YamlConfig config) {
            base.ReadMapping(mapping, config);
            // Load gain from Config
            this._factor = mapping.GetOrDef("factor", this._factor);
        }
        #endregion

    }
}