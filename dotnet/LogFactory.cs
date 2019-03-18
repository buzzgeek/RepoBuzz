using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;

namespace BuzzNet
{
    public class LogFactory
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(LogFactory));

        public static void EnableLogging()
        {
            if (!log4net.LogManager.GetRepository().Configured)
            {
                byte[] data = Encoding.UTF8.GetBytes(Properties.Resources.log4net);
                log4net.Config.XmlConfigurator.Configure(new MemoryStream(data));
                log.Debug("Logging enabled...");
            }
        }
    }
}
