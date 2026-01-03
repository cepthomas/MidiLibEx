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
            // MLEX_BASIC  MLEX_EXPORT  MLEX_TCONV
            var cases = new[] { "MLEX" };
            runner.RunSuites(cases);

            // File.WriteAllLines(Path.Join(MiscUtils.GetSourcePath(), "_test.txt"), runner.Context.OutputLines);
        }
    }
}
