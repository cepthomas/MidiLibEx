using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.MidiLibEx;
using Ephemera.MidiLib;
using System.Runtime.CompilerServices;


// Useful files - from https://github.com/cepthomas/NTerm/TestAudioFiles:
// Style file, full info: _LoveSong.S474.sty
// Plain midi, full song: WICKGAME.MID
// Plain midi, one instrument, no patch: bass_ch2.mid
// Plain midi, drums on different channel: _drums_ch1.mid


namespace Ephemera.MidiLibEx.Test
{
    //----------------------------------------------------------------
    internal class Common
    {
        //public static string OutPath { get { return MiscUtils.GetSourcePath(); } }

        ///// <summary>Common file opener.</summary>
        ///// <param name="fn">The TestAudioFiles file to open.</param>
        //internal static MidiDataFile OpenFile(string fn, int tempo)
        //{
        //    //string fnPath = Path.Join(MiscUtils.GetSourcePath(), "Files", fn);
        //    //// This needs DEV_PATH set, or hack to taste.
        //    //var devPath = Environment.GetEnvironmentVariable("DEV_PATH");
        //    //string fnPath = Path.Join(devPath, "Misc", "TestAudioFiles", fn);

        //    var mdata = new MidiDataFile();
        //    mdata.Read(fnPath, tempo, false);

        //    var pnames = mdata.GetPatternNames();
        //    if (pnames is null || pnames.Count == 0)
        //    {
        //        throw new InvalidOperationException($"Something wrong with this file: {fnPath}");
        //    }

        //    return mdata;
        //}
    }

    //----------------------------------------------------------------
    public class MLEX_STYLE : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);
            int tempo = 100;

            // Style file, full info:
            var mdata = new MidiDataFile();
            mdata.Read(Path.Join(Program.InputDir, "_LoveSong.S474.sty"), tempo, false);
            Assert(mdata is not null);

            // Load the new one.
            long maxTick = 0;
            var pnames = mdata!.GetPatternNames();
            var pinfo = mdata!.GetPattern("Main C");
            Assert(pinfo is not null);

            foreach (var (chnum, patch) in pinfo!.GetChannels(true, true))
            {
                // Get events for the channel.
                var channelEvents = pinfo.GetFilteredEvents([chnum]);
                maxTick = Math.Max(channelEvents.Last().AbsoluteTime, maxTick);

                Info($"chnum:{chnum} patch:{patch} events:{channelEvents.Count()}");
            }

            Info($"maxTick:{maxTick}");

            int now = 22;
            var events = pinfo.GetEventsWhen(now);

            foreach (var mevt in events)
            {
                // tests???...
            }
        }
    }

    //----------------------------------------------------------------
    /// <summary>Test export functions.</summary>
    public class MLEX_EXPORT : TestSuite
    {
        public override void RunSuite()
        {
            StopOnFail(true);
            int tempo = 100;

            // Simple midi file:
            var mdata = new MidiDataFile();
            mdata.Read(Path.Join(Program.InputDir, "WICKGAME.MID"), tempo, false);

            var numtr = mdata!.NumTracks; // 10
            var pnames = mdata.GetPatternNames(); // one: ""
            var pinfo = mdata.GetPattern("");

            // Get selected channels. 

            // TODO1 mini copy script for midi

            // Execute the export function.
            var glob = mdata.GetGlobal();

            var fn1 = Path.Join(Program.OutputDir, "simple_midi_all");
            List<int> chs1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16];
            MidiExport.ExportCsv($"{fn1}.csv", pinfo, chs1, glob);
            MidiExport.ExportMidi($"{fn1}.mid", pinfo, chs1, glob);

            //var fn2 = Path.Join(Program.OutputDir, "simple_midi_some");
            //List<int> chs2 = [1, 2, 3];
            //MidiExport.ExportCsv($"{fn2}.csv", pinfo, chs2, glob);
            //MidiExport.ExportMidi($"{fn2}.mid", pinfo, chs2, glob);

        }
    }

    //----------------------------------------------------------------
    /// <summary>Test all api.</summary>
    public class MLEX_API : TestSuite // more tests as needed
    {
        public override void RunSuite()
        {
            StopOnFail(true);

            // public class MidiDataFile
            //     public string FileName { get; private set; } = "";
            //     public bool IsStyleFile { get; private set; } = false;
            //     public int MidiFileType { get; private set; } = 0;
            //     public int NumTracks { get; private set; } = 0;// Properly handle tracks from original files?
            //     public int DeltaTicksPerQuarterNote { get; private set; } = 0;
            //     public int Tempo { get; private set; } = 0;
            //     public (int num, int denom) TimeSignature { get; set; } = (4, 2);
            //
            //     public void Read(string fn, int defaultTempo, bool includeNoisy)
            //     public PatternInfo GetPattern(string name)
            //     public List<string> GetPatternNames()
            //     public Dictionary<string, int> GetGlobal()
            //
            // public class MidiEventDesc
            //     public int ChannelNumber { get { return RawEvent.Channel; } }
            //     public string ChannelName { get; }
            //     public long AbsoluteTime { get { return RawEvent.AbsoluteTime; } }
            //     public int ScaledTime { get; set; } = -1;
            //     public MidiEvent RawEvent { get; init; }
            //     public MidiEventDesc(MidiEvent evt, string channelName)
            //     public override string ToString()
            //
            // public class PatternInfo
            //     public string PatternName { get; init; } = "";
            //     public int Tempo { get; set; } = 0;
            //     public (int num, int denom) TimeSignature { get; set; } = new();
            //     public PatternInfo(string name, int tempo, int ppq) : this()
            //     public void AddEvent(MidiEventDesc evt)
            //     public IEnumerable<MidiEventDesc> GetFilteredEvents(IEnumerable<int> channels)
            //     public IEnumerable<MidiEventDesc> GetEventsWhen(int when)
            //     public IEnumerable<(int chnum, int patch)> GetChannels(bool hasNotes, bool hasPatch)
            //     public int GetPatch(int channel)
            //     public void RemoveChannel(int channel)
            //     public void SetChannelPatch(int channel, int patch)
            //     public override string ToString()
            //
            // public class MidiTimeConverter
            //     public MidiTimeConverter(int midiPpq, double tempo)
            //     public long InternalToMidi(int t)
            //     public int MidiToInternal(long t)
            //     public double InternalToMsec(int t)
            //     public double MidiToSec(int t)
            //     public double MidiPeriod()
            //     public double InternalPeriod()
            //     public int RoundedInternalPeriod()
            //
            // public class MidiExport
            //     public static void ExportCsv(string outFileName, IEnumerable<PatternInfo> patterns, IEnumerable<OutputChannel> channels, Dictionary<string, int> global)
            //     public static void ExportMidi(string outFileName, PatternInfo pattern, IEnumerable<OutputChannel> channels, Dictionary<string, int> global)
            //     static string Format(MidiEventDesc evtDesc, bool is-Drums)
        }
    }
}
