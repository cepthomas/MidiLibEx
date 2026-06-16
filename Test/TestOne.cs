using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.MidiLibEx;


// Useful files - from https://github.com/cepthomas/TestAudioFiles:
// Style file, full info: _LoveSong.S474.sty
// Plain midi, full song: WICKGAME.MID (has other stuff after the last track)
// Plain midi, one instrument, no patch: bass_ch2.mid
// Plain midi, drums on different channel: _drums_ch1.mid


namespace Ephemera.MidiLibEx.Test
{
    //----------------------------------------------------------------
    /// <summary>Test export functions.</summary>
    public class MLEX_SIMPLE : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);

            MidiDataFile mfd = new();
            mfd.Read(Path.Join(Program.InputDir, "WICKGAME.MID"));

            //var numtr = mfd!.NumTracks; // 10
            var pnames = mfd.GetPatternNames();
            Assert(pnames.Count == 1);

            // Execute the export functions.
            var exThrown = ThrowsNot(() =>
            {
                var pattern = mfd.GetPattern(MidiDataFile.UNNAMED);

                var hdr = mfd.Header;

                var fn1 = Path.Join(Program.OutputDir, "simple_midi_all");
                List<int> chs1 = [];
                MidiExport.ExportCsv($"{fn1}.csv", pattern, chs1, hdr);
                MidiExport.ExportMidi($"{fn1}.mid", pattern, chs1, hdr);

                var fn2 = Path.Join(Program.OutputDir, "simple_midi_some");
                List<int> chs2 = [1, 2, 3];
                MidiExport.ExportCsv($"{fn2}.csv", pattern, chs2, hdr);
                MidiExport.ExportMidi($"{fn2}.mid", pattern, chs2, hdr);
            });
            Assert(exThrown == null);

        }
    }

    //----------------------------------------------------------------
    public class MLEX_STYLE : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);

            // Style file, full info:
            var mfd = new MidiDataFile();
            mfd.Read(Path.Join(Program.InputDir, "_LoveSong.S474.sty"));;
            Assert(mfd is not null);

            // Load the new one.
            // long maxTick = 0;
            var pnames = mfd!.GetPatternNames();
            Assert(pnames.Count == 15);

            // Execute the export functions.
            var exThrown = ThrowsNot(() =>
            {
                var pattern = mfd.GetPattern("Main C");

                var hdr = mfd.Header;

                var fn1 = Path.Join(Program.OutputDir, "style_Main_C");
                List<int> chs1 = [];
                MidiExport.ExportCsv($"{fn1}.csv", pattern, chs1, hdr);
                MidiExport.ExportMidi($"{fn1}.mid", pattern, chs1, hdr);
            });
            Assert(exThrown == null);

            //TODO? these:::
            //foreach (var (chnum, patch) in pattern!.GetChannels(true, true))
            //{
            //    // Get events for the channel.
            //    var channelEvents = pattern.GetFilteredEvents([chnum]);
            //    maxTick = Math.Max(channelEvents.Last().AbsoluteTime, maxTick);
            //    Info($"chnum:{chnum} patch:{patch} events:{channelEvents.Count()}");
            //}
            //Info($"maxTick:{maxTick}");

            //int now = 22;
            //var events = pattern.GetEventsWhen(now);
            //foreach (var mevt in events)
            //{
            //    // tests???...
            //}
        }
    }
}
