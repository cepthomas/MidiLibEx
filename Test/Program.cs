using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;


namespace Ephemera.MidiLibEx.Test
{
    static class Program
    {
        /// <summary>Test entry.</summary>
        [STAThread]
        static void Main(string[] _)
        {
            TestRunner runner = new(OutputFormat.Readable);
            // "INTEROP"  "CLI"  "MISC"
            var cases = new[] { "INTEROP", "CLI", "MISC" };
            runner.RunSuites(cases);
            File.WriteAllLines(@"_test.txt", runner.Context.OutputLines);
        }


        // /// <summary> orig
        // ///  The main entry point for the application.
        // /// </summary>
        // [STAThread]
        // static void Main()
        // {
        //     Application.SetHighDpiMode(HighDpiMode.SystemAware);
        //     Application.EnableVisualStyles();
        //     Application.SetCompatibleTextRenderingDefault(false);

        //     //var f = new MainForm();
        //     //Application.Run(f);
        // }
    }
}
