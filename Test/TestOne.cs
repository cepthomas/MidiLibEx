using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfTricks.PNUT;
using Ephemera.MidiLibEx;
using System.Linq;
using Ephemera.MidiLib;
using NAudio.Midi;


namespace Ephemera.MidiLibEx.Test
{
    /// <summary>Test the simpler functions.</summary>
    public class MLEX_ONE : TestSuite // TODO1
    {
        public override void RunSuite()
        {
            bool bret;
            UT_STOP_ON_FAIL(true);

            //var st = State.Instance;
            //st.ValueChangeEvent += (sender, e) => { };

            //MockConsole console = new();
            //var cli = new Cli("none", console);

            //// Set some test settings defaults.
            //UserSettings.Current.MonitorSnd = false;
            //UserSettings.Current.MonitorRcv = false;

            //string prompt = ">";

            /////// Fat fingers.
            //console.Reset();
            //console.NextReadLine = "bbbbb";
            //bret = cli.DoCommand();
            //UT_TRUE(bret);
            //UT_EQUAL(console.Capture.Count, 2);
            //UT_EQUAL(console.Capture[0], $"Invalid command");
            //UT_EQUAL(console.Capture[1], prompt);

            //// Wait for logger to stop.
            //Thread.Sleep(100);

            //bool bret;
            //UT_STOP_ON_FAIL(true);

            //var st = State.Instance;
            //st.ValueChangeEvent += (sender, e) => { };

            //MockConsole console = new();
            //var cli = new Cli("none", console);
            //string prompt = ">";

            /////// Fake valid loaded script.
            //Dictionary<int, string> sectionInfo = new() { [0] = "start", [200] = "middle", [300] = "end", [400] = "LENGTH" };
            //State.Instance.InitSectionInfo(sectionInfo);
            //UT_EQUAL(State.Instance.SectionInfo.Count, 4);

            //// Wait for logger to stop.
            //Thread.Sleep(100);

        }
    }




    public class ToTest
    {
        /// <summary>Midi events from the input file.</summary>
        MidiDataFile _mdata = new();

        /// <summary>Where to put things.</summary>
        readonly string _outPath = @"???";


        #region Lifecycle
        /// <summary>
        /// Constructor.
        /// </summary>
        public ToTest()
        {
            // Make sure out path exists.
            _outPath = Path.Join(MiscUtils.GetSourcePath(), "out");
            DirectoryInfo di = new(_outPath);
            di.Create();

            // // Init channel selectors.
            // cmbDrumChannel1.Items.Add("NA");
            // cmbDrumChannel2.Items.Add("NA");
            // for (int i = 1; i <= MidiDefs.NUM_CHANNELS; i++)
            // {
            //     cmbDrumChannel1.Items.Add(i);
            //     cmbDrumChannel2.Items.Add(i);
            // }

            // Style file, full info:
            OpenFile(@"C:\Dev\Misc\TestAudioFiles\_LoveSong.S474.sty");

            // Plain midi, full song:
            //OpenFile(@"C:\Dev\Misc\TestAudioFiles\WICKGAME.MID");

            // Plain midi, one instrument, no patch:
            //OpenFile(@"C:\Dev\Misc\TestAudioFiles\_bass_ch2.mid");

            // Plain midi, one instrument, ??? patch:
            //OpenFile(@"C:\Dev\Misc\TestAudioFiles\_drums_ch1.mid");

        }

        #endregion


        #region File handling
        /// <summary>
        /// Common file opener. Initializes pattern list from contents.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        public void OpenFile(string fn)
        {
            try
            {
                // Reset stuff.
                //cmbDrumChannel1.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
                //cmbDrumChannel2.SelectedIndex = 0;
                _mdata = new MidiDataFile();

                // Process the file. Set the default tempo from ???.
                _mdata.Read(fn, 100, false);

                // Init new stuff with contents of file/pattern.
                //lbPatterns.Items.Clear();
                var pnames = _mdata.GetPatternNames();

                if(pnames.Count > 0)
                {
                    //pnames.ForEach(pn => { lbPatterns.Items.Add(pn); });
                }
                else
                {
                    throw new InvalidOperationException($"Something wrong with this file: {fn}");
                }

                // Pick first.
                //lbPatterns.SelectedIndex = 0;

                // Set up timer default.
                //sldTempo.Value = 100;

            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Open_Click(object sender, EventArgs e)
        {
            //var fileTypes = $"Midi Files|{MidiLibDefs.MIDI_FILE_TYPES}|Style Files|{MidiLibDefs.STYLE_FILE_TYPES}";
            //using OpenFileDialog openDlg = new()
            //{
            //    Filter = fileTypes,
            //    Title = "Select a file",
            //    InitialDirectory = @"C:\Dev\Apps\TestAudioFiles"
            //};

            //if (openDlg.ShowDialog() == DialogResult.OK)
            //{
            //    OpenFile(openDlg.FileName);
            //}
        }
        #endregion

        #region Process pattern info into events
        /// <summary>
        /// Load the requested pattern and create controls.
        /// </summary>
        /// <param name="pinfo"></param>
        void LoadPattern(PatternInfo pinfo)
        {
            // Load the new one.
            int maxTick = 0;

            foreach(var (chnum, patch) in pinfo.GetChannels(true, true))
            {
                // Get events for the channel.
                var chEvents = pinfo.GetFilteredEvents([chnum]);
                if (chEvents.Any())
                {
                    maxTick = Math.Max(chEvents.Last().ScaledTime, maxTick);

                    //// Create channel.
                    //var ch = _mgr.OpenMidiOutput(_settings.OutputDeviceName, chnum, $"out_chan{chnum}", patch);
                    //ch.Volume = Defs.DEFAULT_VOLUME;
                    //ch.IsDrums = chnum == MidiDefs.DEFAULT_DRUM_CHANNEL;
                    //ch.SetEvents(chEvents);
                    //_channels.Add(ch.ChannelName, ch);

                    //// Make new control and bind to channel.
                    //ChannelControl control = new()
                    //{
                    //    Location = new(x, y),
                    //    BorderStyle = BorderStyle.FixedSingle,
                    //    BoundChannel = ch
                    //};
                    //control.ChannelChange += Control_ChannelChange;
                    //Controls.Add(control);
                    //_channelControls.Add(control);

                    //// Adjust positioning.
                    //y += control.Height + 5;
                }
            }

            UpdateDrumChannels();
        }

        /// <summary>
        /// Load pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patterns_SelectedIndexChanged(object? sender, EventArgs e)
        {
            var pname = "???"; // lbPatterns.SelectedItem.ToString()!;
            var pinfo = _mdata.GetPattern(pname);

            if (pinfo is not null)
            {
                LoadPattern(pinfo);
            }
            else
            {
                //_logger.Warn($"Invalid pattern {pname}");
            }
        }

        #endregion

        #region Process tick
        /// <summary>
        /// Synchronously outputs the next midi events. Does solo/mute.
        /// This is running on the background thread.
        /// </summary>
        /// <returns>True if sequence completed.</returns>
        public void DoNextStep()
        {
            bool done;

            // Any soloes?
            bool anySolo = false; // _channelControls.Where(c => c.State == ChannelState.Solo).Any();

            // Process each channel.
            List<OutputChannel> channels = new();

            foreach (var ch in channels)
            {
                // Look for events to send. Any explicit solos?
                //if (cc.State == ChannelState.Solo || (!anySolo && cc.State == ChannelState.Normal))
                {
                    // Process any sequence steps.
                    int now = 999;

                    /// <summary>All the events defined in the script.</summary>
                    //internal List<MidiEventDesc> _scriptEvents = [];

                    List<MidiEvent> events = [];
                    var playEvents = events; // ch.GetEvents(now);// timeBar.Current.Tick);

                    foreach (var mevt in playEvents)
                    {
                        switch (mevt)
                        {
                            case NoteOnEvent evt:
                                if (ch.IsDrums && evt.Velocity == 0)
                                {
                                    // Skip drum noteoffs as windows GM doesn't like them.
                                }
                                else
                                {
                                    // Adjust volume. Redirect drum channel to default.
                                    NoteOnEvent ne = new(
                                        evt.AbsoluteTime,
                                        ch.IsDrums ? MidiDefs.DEFAULT_DRUM_CHANNEL : evt.Channel,
                                        evt.NoteNumber,
                                        Math.Min((int)(evt.Velocity /* sldVolume.Value*/ * ch.Volume), MidiDefs.MAX_MIDI),
                                        evt.OffEvent is null ? 0 : evt.NoteLength); // Fix NAudio NoteLength bug.

                                    //ch.Device.SendEvent(ne);
                                }
                                break;

                            case NoteEvent evt: // aka NoteOff
                                if (ch.IsDrums)
                                {
                                    // Skip drum noteoffs as windows GM doesn't like them.
                                }
                                else
                                {
                                   // ch.Device.SendEvent(evt);
                                }
                                break;

                            default:
                                // Everything else as is.
                               // ch.Device.SendEvent(mevt);
                                break;
                        }
                    }
                }
            }
        }
        #endregion

        #region Drum channel
        /// <summary>
        /// Update all channels based on current UI.
        /// </summary>
        void UpdateDrumChannels()
        {
           //_channelControls.ForEach(ctl => ctl.IsDrums =
           //    (ctl.ChannelNumber == cmbDrumChannel1.SelectedIndex) ||
           //    (ctl.ChannelNumber == cmbDrumChannel2.SelectedIndex));
        }
        #endregion

        #region Export
        /// <summary>
        /// Export current file to human readable or midi.
        /// </summary>
        void Export_Click(object? sender, EventArgs e)
        {
            // Get selected patterns.
            List<string> patternNames = new();
            patternNames.Add("XXX 1");
            patternNames.Add("XXX 2");

            List<PatternInfo> patterns = new();
            patternNames.ForEach(p => patterns.Add(_mdata.GetPattern(p)!));

            // Get selected channels.
            List<OutputChannel> channels = new();
            //_channelControls.Where(cc => cc.Selected).ForEach(cc => channels.Add(cc.BoundChannel));
            //if (!channels.Any()) // grab them all.
            //{
            //    _channelControls.ForEach(cc => channels.Add(cc.BoundChannel));
            //}

            // Execute the requested export function.
            // "btnExportCsv"
            var newfn = Tools.MakeExportFileName(_outPath, _mdata.FileName, "all", "csv");
            MidiExport.ExportCsv(newfn, patterns, channels, _mdata.GetGlobal());

            // "btnExportMidi")
            foreach (var pattern in patterns)
            {
                var newfn2 = Tools.MakeExportFileName(_outPath, _mdata.FileName, pattern.PatternName, "mid");
                MidiExport.ExportMidi(newfn2, pattern, channels, _mdata.GetGlobal());
            }
        }
        #endregion


        #region Debug stuff
        /// <summary>
        /// Mainly for debug.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Stuff_Click(object sender, EventArgs e)
        {
            // A unit test.

            // If we use ppq of 8 (32nd notes):
            // 100 bpm = 800 ticks/min = 13.33 ticks/sec = 0.01333 ticks/msec = 75.0 msec/tick
            //  99 bpm = 792 ticks/min = 13.20 ticks/sec = 0.0132 ticks/msec  = 75.757 msec/tick

            MidiTimeConverter mt = new(0, 100);
            TestClose(mt.InternalPeriod(), 75.0, 0.001);

            mt = new(0, 90);
            TestClose(mt.InternalPeriod(), 75.757, 0.001);

            mt = new(384, 100);
            TestClose(mt.MidiToSec(144000) / 60.0, 3.75, 0.001);

            mt = new(96, 100);
            TestClose(mt.MidiPeriod(), 6.25, 0.001);

            void TestClose(double value1, double value2, double tolerance)
            {
                if (Math.Abs(value1 - value2) > tolerance)
                {
                   // _logger.Error($"[{value1}] not close enough to [{value2}]");
                }
            }
        }
        #endregion

    }





}



# if API_to_test
    /// <summary>
    /// Represents one complete collection of midi events from a file - standard midi or yamaha style files.
    /// Writes subsets to various output formats.
    /// </summary>
    public class MidiDataFile
    {
        #region Properties
        /// <summary>Current file.</summary>
        public string FileName { get; private set; } = "";

        /// <summary>It's a style file.</summary>
        public bool IsStyleFile { get; private set; } = false;

        /// <summary>What midi type is it.</summary>
        public int MidiFileType { get; private set; } = 0;

        /// <summary>How many tracks.</summary>
        public int NumTracks { get; private set; } = 0;// TODO Properly handle tracks from original files?

        /// <summary>Original resolution for all events.</summary>
        public int DeltaTicksPerQuarterNote { get; private set; } = 0;

        /// <summary>Tempo supplied by file.</summary>
        public int Tempo { get; private set; } = 0;

        /// <summary>Time signature, if supplied by file. Default is 4/4.</summary>
        public (int num, int denom) TimeSignature { get; set; } = (4, 2);
        #endregion


        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <param name="defaultTempo">Specified by client in case not in the file.</param>
        /// <param name="includeNoisy"></param>
        public void Read(string fn, int defaultTempo, bool includeNoisy)

        /// <summary>
        /// Get the pattern by name.
        /// </summary>
        /// <param name="name">Which</param>
        /// <returns>The pattern. Throws if name not found.</returns>
        public PatternInfo GetPattern(string name)

        /// <summary>
        /// Get all useful pattern names - those with musical notes.
        /// </summary>
        /// <returns>List of names.</returns>
        public List<string> GetPatternNames()

        /// <summary>
        /// Get common midi file meta info.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetGlobal()
    }

    /// <summary>
    /// Internal representation of one midi event.
    /// </summary>
    public class MidiEventDesc
    {
        /// <summary>One-based channel number.</summary>
        public int ChannelNumber { get { return RawEvent.Channel; } }

        /// <summary>Associated channel name.</summary>
        public string ChannelName { get; }

        /// <summary>Time (ticks) from original file.</summary>
        public long AbsoluteTime { get { return RawEvent.AbsoluteTime; } }

        /// <summary>Time (ticks) scaled to internal units using send PPQ.</summary>
        public int ScaledTime { get; set; } = -1;

        /// <summary>The raw midi event.</summary>
        public MidiEvent RawEvent { get; init; }

        /// <summary>Normal constructor from NAudio event.</summary>
        public MidiEventDesc(MidiEvent evt, string channelName)

        /// <summary>Read me.</summary>
        public override string ToString()
    }


    /// <summary>Represents the contents of a midi file pattern. If it is a plain midi file (not style) there will be one only.</summary>
    public class PatternInfo
    {
        /// <summary>Pattern name. Empty indicates single pattern aka plain midi file.</summary>
        public string PatternName { get; init; } = "";

        /// <summary>Tempo, if supplied by file. Default indicates invalid which will be filled in during read.</summary>
        public int Tempo { get; set; } = 0;

        /// <summary>Time signature, if supplied by file.</summary>
        public (int num, int denom) TimeSignature { get; set; } = new();

        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="name">Pattern name</param>
        /// <param name="tempo">Defalt tempo</param>
        /// <param name="ppq">Resolution</param>
        public PatternInfo(string name, int tempo, int ppq) : this()

        /// <summary>
        /// Constructor from existing data. 
        /// </summary>
        /// <param name="name">Pattern name</param>
        /// <param name="ppq">Resolution</param>
        /// <param name="events">All the events</param>
        /// <param name="channels">Channels of interest</param>
        /// <param name="tempo">Defalt tempo</param>
        public PatternInfo(string name, int ppq, IEnumerable<MidiEventDesc> events, IEnumerable<OutputChannel> channels, int tempo) : this(name, tempo, ppq)

        /// <summary>
        /// Add an event to the collection. This function does the scaling.
        /// </summary>
        /// <param name="evt">The event to add.</param>
        public void AddEvent(MidiEventDesc evt)


        /// <summary>
        /// Get enumerator for events using supplied filters.
        /// </summary>
        /// <param name="channels">Specific channnels.</param>
        /// <returns>Enumerator sorted by scaled time.</returns>
        public IEnumerable<MidiEventDesc> GetFilteredEvents(IEnumerable<int> channels)

        /// <summary>
        /// Get all events at a specific scaled time.
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        public IEnumerable<MidiEventDesc> GetEventsWhen(int when)

        /// <summary>
        /// Get an ordered list of channels and their patches.
        /// </summary>
        /// <param name="hasNotes">Must have noteons.</param>
        /// <param name="hasPatch">Must have valid patch.</param>
        /// <returns></returns>
        public IEnumerable<(int chnum, int patch)> GetChannels(bool hasNotes, bool hasPatch)

        /// <summary>
        /// Get the patch associated with the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The patch or -1 if invalid channel</returns>
        public int GetPatch(int channel)

        /// <summary>
        /// Remove a channel from the channel/patches collection.
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveChannel(int channel)

        /// <summary>
        /// Safely add/update info.
        /// </summary>
        /// <param name="channel">The channel number</param>
        /// <param name="patch">The patch. Can be default -1.</param>
        public void SetChannelPatch(int channel, int patch)

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
    }


    /// <summary>Helpers to translate between midi standard and arbtrary internal representation.</summary>
    public class MidiTimeConverter
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="midiPpq">The file resolution.</param>
        /// <param name="tempo">BPM may be needed for some calcs.</param>
        public MidiTimeConverter(int midiPpq, double tempo)

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public long InternalToMidi(int t)

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int MidiToInternal(long t)

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double InternalToMsec(int t)

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double MidiToSec(int t)

        /// <summary>
        /// Exact time between events.
        /// </summary>
        /// <returns></returns>
        public double MidiPeriod()

        /// <summary>
        /// Exact time between events.
        /// </summary>
        /// <returns></returns>
        public double InternalPeriod()

        /// <summary>
        /// Integer time between events.
        /// </summary>
        /// <returns></returns>
        public int RoundedInternalPeriod()
    }

    public class MidiExport
    {
        /// <summary>
        /// Export the contents in a csv readable form. This is as the events appear in the original file.
        /// </summary>
        /// <param name="outFileName">Where to boss?</param>
        /// <param name="patterns">Specific patterns.</param>
        /// <param name="channels">Specific channnels or all if empty.</param>
        /// <param name="global">File meta data to include.</param>
        public static void ExportCsv(string outFileName, IEnumerable<PatternInfo> patterns, IEnumerable<OutputChannel> channels, Dictionary<string, int> global)
        {
        }

        /// <summary>
        /// Export pattern parts to individual midi files. This is as the events appear in the original file.
        /// </summary>
        /// <param name="outFileName">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels or all if empty.</param>
        /// <param name="global">File meta data to include.</param>
        public static void ExportMidi(string outFileName, PatternInfo pattern, IEnumerable<OutputChannel> channels, Dictionary<string, int> global)
        {
        }

        /// <summary>
        /// Format event for export.
        /// </summary>
        /// <param name="evtDesc">Event to format</param>
        /// <param name="isDrums"></param>
        /// <returns></returns>
        static string Format(MidiEventDesc evtDesc, bool isDrums)
        {
        }
    }

#endif
