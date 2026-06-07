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
        static void Main()
        {
            TestRunner runner = new(OutputFormat.Readable);
            //  MLEX_STYLE  MLEX_EXPORT  MLEX_API
            var torun = new[] { "MLEX_EXPORT" };
            runner.RunSuites(torun);

            // File.WriteAllLines(Path.Join(MiscUtils.GetSourcePath(), "test.txt"), runner.Context.OutputLines);
        }
    }
}
