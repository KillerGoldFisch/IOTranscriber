using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Globalization;
using GCore.Logging;
using GCore.Yaml.Config;
using GCore.Extensions.TypeEx;
using GCore.Extensions.ArrayEx;

namespace IOTranscriber.Lib {
    public static class Extensions {
        /// <summary>
        /// Returns the data-string from a set of IOPipes
        /// </summary>
        /// <param name="this_"></param>
        /// <returns></returns>
        public static string GetString(this IEnumerable<IIOPipe> this_) {
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = TConverter.Culture;
            string tmp = String.Join("\0", from pipe in this_ select pipe.Name+":"+pipe.Variable.Value.ToString());
            Thread.CurrentThread.CurrentCulture = culture;
            return tmp;
        }

        /// <summary>
        /// Returns the "[key]:[value]" pair from
        /// value and pipe
        /// </summary>
        /// <param name="this_">Key</param>
        /// <param name="change">Value</param>
        /// <returns></returns>
        public static string GetString(this IIOPipe this_, IVariableChange change) {
            // Replaces the culture to get points for deciamls
            CultureInfo culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = TConverter.Culture;
            string tmp = string.Format("{0}:{1}", this_.Name, change.NewValue.ToString());
            Thread.CurrentThread.CurrentCulture = culture;
            return tmp;
        }

        /// <summary>
        /// Returns the data-string from changes and pipes
        /// </summary>
        /// <param name="this_"></param>
        /// <returns></returns>
        public static string GetString(this IDictionary<IIOPipe, IVariableChange> this_) {
            return string.Join("\0", from c in this_ select c.Key.GetString(c.Value))+"\0";
        }

        /// <summary>
        /// Returns the splitted data from string.
        /// Expected format is: "[key1]:[val1];[key2]:[val2];..."
        /// </summary>
        /// <param name="this_"></param>
        /// <returns></returns>
        public static IDictionary<string, string> ParseDataString(this string this_) {
            IEnumerable<string[]> data = (from s in this_.Split('\0') where s.Length > 3 && s.Contains(':') select s.Split(':'));
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (string[] dataentry in data) {
                string value = String.Join(':', dataentry.Slice(1, -1));
                if(dict.ContainsKey(dataentry[0]))
                    dict[dataentry[0]] = value;
                else
                    dict.Add(dataentry[0], value);
            }
            return dict;
        }

        /// <summary>
        /// Distributes the data to the pipes
        /// </summary>
        /// <param name="this_"></param>
        /// <param name="data"></param>
        public static void SetDataString(this IEnumerable<IIOPipe> this_, string data) {
            IDictionary<string, string> entrys = data.ParseDataString();

            foreach (IIOPipe pipe in this_)
                if (entrys.ContainsKey(pipe.Name))
                    try {
                        pipe.Variable.SetValueFromString(entrys[pipe.Name]);
                    } catch (Exception ex) {
                        Log.Exception(string.Format("[{0}] Exception while processing new value '{1}'", pipe.ConfigURL, entrys[pipe.Name]), ex);
                    }
        }

        public static IIOPipe GetPipeByName(this IEnumerable<IIOPipe> this_, string name) {
            foreach (IIOPipe pipe in this_)
                if (pipe.Name == name)
                    return pipe;
            return null;
        }

        public static IIOPipe GetPipe(this IMappingObject this_, bool input = false) {
            if (this_.GetType().IsCastableTo2(typeof(IVariable))) {
                return new IOPipe(this_ as IVariable, input);
            } else if (this_.GetType().IsCastableTo2(typeof(IIOPipe))) {
                return this_ as IIOPipe;
            } else {
                Log.Error(string.Format("[{0}] Can't cast Object to Variable or Pipe: '{1}'", this_.ConfigURL, this_));
                return null;
            }
        }
    }
}
