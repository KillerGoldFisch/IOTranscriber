using GCore.Logging;
using GCore.Logging.Logger;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOTranscriber.Lib.Test
{
    public class Startup
    {
        private static bool _isStarted = false;
        public static void StartupApp() {
            if(!_isStarted) {
                _isStarted = true;
                startupApp();
            }
        }

        private static void startupApp() {
            Log.LoggingHandler.Add(new ConsoleLogger());
            Log.LoggingHandler.Add(new DebugLogger());

            Log.Info("Startup");
        }
    }
}
