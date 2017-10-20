using GCore.Yaml;
using GCore.Yaml.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;
using IOTranscriber.Lib;

namespace IOTranscriber.Lib.Test
{
    public class ExtensionsTest : IDisposable {
        YamlConfig config;
        MappingWrapper rootMapping;

        ComEndpoint server;
        ComEndpoint client;

        public ExtensionsTest() {
            Startup.StartupApp();

            string result;
            using(Stream stream = typeof(ExtensionsTest).Assembly.GetManifestResourceStream("IOTranscriber.Lib.Test.default.yaml"))
            using(StreamReader reader = new StreamReader(stream)) {
                result = reader.ReadToEnd();
            }
            config = new YamlConfig(new FileInfo("config.yaml"), result);
            rootMapping = new MappingWrapper(config.GetRootNode() as Mapping, config);

            server = config.GetMappingObject<ComEndpoint>("Endpoints.Server");
            client = config.GetMappingObject<ComEndpoint>("Endpoints.Client");
        }


        [Fact]
        public void Test_GetString_Enumerable_IOPipe() {
            Assert.Equal("SharedDouble:0\0SharedBool:True\0SharedString:Hello World", server.Inputs.GetString());
        }

        [Fact]
        public void Test_GetString_IIOPipe_IVariableChange() {

        }

        [Fact]
        public void Test_GetString_IDictionary_IOPipe_IVariableChange() {

        }

        [Fact]
        public void Test_ParseDataString() {
            var res = "test1:1.0\0test2:asdf:1234;+#\0".ParseDataString();

            Assert.Equal("1.0", res["test1"]);
            Assert.Equal("asdf:1234;+#", res["test2"]);
        }

        public void Dispose() {
            config.Shutdown();
        }
    }
}
