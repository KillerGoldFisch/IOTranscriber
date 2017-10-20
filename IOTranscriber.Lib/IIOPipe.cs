using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GCore.Yaml.Config;

namespace IOTranscriber.Lib {
    public interface IIOPipe : IMappingObject {
        MappingWrapper Mapping { get; }
        IVariable Variable { get; }
        IVariableChangeBuffer ChangeBuffer { get; }
        string Name { get; }
        string DisplayName { get; }
    }
}
