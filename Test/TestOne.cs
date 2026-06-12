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
    public class MLEX_STYLE : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);

            // Style file, full info:
            var mdata = new MidiDataFile();
            mdata.Read(Path.Join(Program.InputDir, "_LoveSong.S474.sty"), false);
            Assert(mdata is not null);

            // Load the new one.
            // long maxTick = 0;
            var pnames = mdata!.GetPatternNames();
            var pattern = mdata!.GetPattern("Main C");
            Assert(pattern is not null);

            //TODO1 these:::
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

    //----------------------------------------------------------------
    /// <summary>Test export functions.</summary>
    public class MLEX_EXPORT : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);

            // Simple midi file:
            var mdata = new MidiDataFile();
            mdata.Read(Path.Join(Program.InputDir, "WICKGAME.MID"), false);

            //var numtr = mdata!.NumTracks; // 10
            var pnames = mdata.GetPatternNames(); // one: ""
            var pinfo = mdata.GetPattern("");

            // Execute the export function.
            var hdr = mdata.Header;

            var fn1 = Path.Join(Program.OutputDir, "simple_midi_all");
            List<int> chs1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
            MidiExport.ExportCsv($"{fn1}.csv", pinfo, chs1, hdr);
            MidiExport.ExportMidi($"{fn1}.mid", pinfo, chs1, hdr);

            //var fn2 = Path.Join(Program.OutputDir, "simple_midi_some");
            //List<int> chs2 = [1, 2, 3];
            //MidiExport.ExportCsv($"{fn2}.csv", pinfo, chs2, hdr);
            //MidiExport.ExportMidi($"{fn2}.mid", pinfo, chs2, hdr);
         }
    }

    //----------------------------------------------------------------
    /// <summary>Test all api.</summary>
    public class MLEX_API : TestSuite // more tests as needed
    {
        public override void RunSuite()
        {
            StopOnFail(true);
        }
    }
}
