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
        // This needs DEV_PATH set, or hack to taste.
        public static string InputDir = Path.Join(Environment.GetEnvironmentVariable("DEV_PATH"), "Misc", "TestAudioFiles");
        public static string OutputDir = Path.Join(MiscUtils.GetSourcePath(), "out");

        /// <summary>Test entry.</summary>
        [STAThread]
        static void Main()
        {
            // Ensure paths.
            Directory.CreateDirectory(OutputDir);

            TestRunner runner = new(OutputFormat.Readable);
            //  MLEX_SIMPLE  MLEX_STYLE  MLEX_EXPORT  MLEX_API
            var torun = new[] { "MLEX_STYLE" };
            runner.RunSuites(torun);

            // File.WriteAllLines(Path.Join(MiscUtils.GetSourcePath(), "test.txt"), runner.Context.OutputLines);
        }
    }
}
