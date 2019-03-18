using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace BuzzNet
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                LogFactory.EnableLogging();
            }
            catch (Exception ex)
            {
                Console.Error.Write(ex);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
