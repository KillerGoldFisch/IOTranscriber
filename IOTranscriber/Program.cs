using GCore.Logging;
using GCore.Logging.Logger;
using GCore.XConsole;
using GCore.Yaml;
using GCore.Yaml.Config;
using IOTranscriber.Lib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;

namespace IOTranscriber
{
    public class Program
    {
        YamlConfig config;
        MappingWrapper rootMapping;

        static void Main(string[] args)
        {
            Console.BufferWidth = 180;
            Log.LoggingHandler.Add(new ConsoleLogger(
#if DEBUG
                LogEntry.LogTypes.All
#endif
                ));
            new Program().Run(args);

            Console.WriteLine("\n-- END --");
            Console.ReadLine();
        }

        public void Run(string[] args) {
            string result;
            using(Stream stream = typeof(Program).Assembly.GetManifestResourceStream("IOTranscriber.default.yaml"))
            using(StreamReader reader = new StreamReader(stream)) {
                result = reader.ReadToEnd();
            }
            config = new YamlConfig(new FileInfo("config.yaml"), result);
            rootMapping = new MappingWrapper(config.GetRootNode() as Mapping, config);

            Shell().Wait();
        }

        void PrintTable(IOManager iOManager) {
            string[,] tbl = new string[iOManager.Count, 2];

            int i = 0;
            foreach(var v in iOManager) {
                tbl[i, 0] = v.Name;
                tbl[i, 1] = v.Variable.Value.ToString();

                i++;
            }

            XConsole.ArrayPrinter.PrintToConsole(tbl);
        }

        async Task Shell() {



            ExpandoObject expando = new ExpandoObject();

            MappingWrapper endpointMap = new MappingWrapper(rootMapping.Get("Endpoints") as Mapping, config);
            foreach(string key in endpointMap.GetKeys()) {
                Log.Info("Start loading Endpoint '" + key + "'");
                ComEndpoint endpint = endpointMap.GetMO(key) as ComEndpoint;
                if(endpint == null) {
                    Log.Warn("Key '" + key + "' can't Mapped to ComEndpoint");
                    continue;
                }
                Log.Success("Added Endpoint data." + key);
                expando.TryAdd(key, endpint);
            }


            var refs = new List<MetadataReference>{
            MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location)};

            var state = await CSharpScript.RunAsync("", globals: new Globals() {
                data = expando
            },
            options: ScriptOptions.Default.AddReferences(refs));

            Console.WriteLine(state.ReturnValue);
            while(config.Active) {
                Console.Write(">>> ");
                try {
                    state = await state.ContinueWithAsync(Console.ReadLine());
                    if(state.ReturnValue != null)
                        Console.WriteLine(state.ReturnValue);
                } catch(Exception ex) {
                    Log.Exception("REPL Exception", ex);
                }
            }
        }

        public class Globals {
            public dynamic data;
            public void Exit() {
                data.config.Shutdown();
            }
        }

    }
}
