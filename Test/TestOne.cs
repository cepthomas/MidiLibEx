using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Midi;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.MidiLibEx;
using Ephemera.MidiLib;


// Useful files:
// Style file, full info:
//var mdata = Common.OpenFile("_LoveSong.S474.sty");
// Plain midi, full song:
// var mdata = Common.OpenFile("WICKGAME.MID");
// Plain midi, one instrument, no patch:
// var mdata = Common.OpenFile("_bass_ch2.mid");
// Plain midi, one instrument, ??? patch:
// var mdata = Common.OpenFile("_drums_ch1.mid");


namespace Ephemera.MidiLibEx.Test
{
    //----------------------------------------------------------------
    internal class Common
    {
        public static string OutPath
        {
            get
            {
                if (_outPath is null)
                {
                    _outPath = Path.Join(MiscUtils.GetSourcePath(), "out");
                    DirectoryInfo di = new(_outPath);
                    di.Create();
                }
                return _outPath;
            }
        }
        static string? _outPath = null;

        /// <summary>Common file opener.</summary>
        /// <param name="fn">The TestAudioFiles file to open.</param>
        internal static MidiDataFile OpenFile(string fn, int tempo)
        {
            string fnPath = Path.Join(MiscUtils.GetSourcePath(), "..", "..", "..", "Misc", "TestAudioFiles", fn);
            var mdata = new MidiDataFile();

            mdata.Read(fnPath, tempo, false);

            var pnames = mdata.GetPatternNames();
            if (pnames.Count == 0)
            {
                throw new InvalidOperationException($"Something wrong with this file: {fnPath}");
            }

            return mdata;
        }
    }

    //----------------------------------------------------------------
    public class MLEX_BASIC : TestSuite
    {
        /// <summary>Midi events from the input file.</summary>
        MidiDataFile _mdata = new();

        public override void RunSuite()
        {
            UT_STOP_ON_FAIL(true);
            int tempo = 100;

            // Style file, full info:
            var mdata = Common.OpenFile("_LoveSong.S474.sty", tempo);
            UT_NOT_NULL(mdata);

            // Load the new one.
            long maxTick = 0;

            //var pnames = mdata.GetPatternNames();

            var pinfo = mdata.GetPattern("Main C");
            UT_NOT_NULL(pinfo);

            foreach (var (chnum, patch) in pinfo.GetChannels(true, true))
            {
                // Get events for the channel.
                var channelEvents = pinfo.GetFilteredEvents([chnum]);
                maxTick = Math.Max(channelEvents.Last().AbsoluteTime, maxTick);

                UT_INFO($"chnum:{chnum} patch:{patch} events:{channelEvents.Count()}");
            }

            UT_INFO($"maxTick:{maxTick}");

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
            UT_STOP_ON_FAIL(true);
            int tempo = 100;

            var mdata = Common.OpenFile("WICKGAME.MID", tempo);
            UT_NOT_NULL(mdata);

            var numtr = mdata.NumTracks; // 10
            var pnames = mdata.GetPatternNames(); // one: ""
            var pinfo = mdata.GetPattern("");
            List<PatternInfo> patterns = [pinfo];

            // Get selected channels.
            List<int> channels = [1, 2, 3, 4, 10];
            List<int> drums = [10];

            // Execute the requested export function.
            var newfn = Tools.MakeExportFileName(Common.OutPath, mdata.FileName, "all", "csv");
            MidiExport.ExportCsv(newfn, patterns, channels, drums, mdata.GetGlobal());

            newfn = Tools.MakeExportFileName(Common.OutPath, mdata.FileName, "", "mid");
            MidiExport.ExportMidi(newfn, patterns[0], channels, mdata.GetGlobal());
        }
    }

    //----------------------------------------------------------------
    /// <summary>Test MidiTimeConverter.</summary>
    public class MLEX_TCONV : TestSuite
    {
        public override void RunSuite()
        {
            UT_STOP_ON_FAIL(false);

            //////////////////////////////////////////////////////////
            // A unit test. If we use ppq of 8 (32nd notes):
            // 100 bpm = 800 ticks/min = 13.33 ticks/sec = 0.01333 ticks/msec = 75.0 msec/tick
            //  99 bpm = 792 ticks/min = 13.20 ticks/sec = 0.0132 ticks/msec  = 75.757 msec/tick

            MidiTimeConverter mt = new(0, 100);
            UT_CLOSE(mt.InternalPeriod(), 75.0, 0.001);

            mt = new(0, 99);
            UT_CLOSE(mt.InternalPeriod(), 75.757, 0.001);

            mt = new(384, 100);
            UT_CLOSE(mt.MidiToSec(144000) / 60.0, 3.75, 0.001);

            mt = new(96, 100);
            UT_CLOSE(mt.MidiPeriod(), 6.25, 0.001);
        }
    }

    //----------------------------------------------------------------
    /// <summary>Test all api.</summary>
    public class MLEX_API : TestSuite // TODO more tests as needed
    {
        public override void RunSuite()
        {
            UT_STOP_ON_FAIL(true);

            // public class MidiDataFile
            //     public string FileName { get; private set; } = "";
            //     public bool IsStyleFile { get; private set; } = false;
            //     public int MidiFileType { get; private set; } = 0;
            //     public int NumTracks { get; private set; } = 0;// TODO Properly handle tracks from original files?
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
            //     static string Format(MidiEventDesc evtDesc, bool isDrums)

        }
    }
}
