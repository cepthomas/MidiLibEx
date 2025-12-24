
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using NAudio.Midi;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;


namespace Ephemera.MidiLibEx.Test
{
    public partial class MainForm : Form
    {
        #region Types
        public enum PlayState { Stop, Play, Rewind, Complete }
        #endregion

        #region Fields - internal
        /// <summary>All the channels - key is user assigned name.</summary>
        readonly Dictionary<string, Channel> _channels = new();

        /// <summary>Midi output.</summary>
        IOutputDevice _outputDevice = new NullOutputDevice();

        /// <summary>Midi input.</summary>
        IInputDevice? _inputDevice;

        /// <summary>The fast timer.</summary>
        readonly MmTimerEx _mmTimer = new();

        /// <summary>Midi events from the input file.</summary>
        MidiDataFile _mdata = new();

        /// <summary>All the channel play controls.</summary>
        readonly List<ChannelControl> _channelControls = new();

        /// <summary>My logging.</summary>
        readonly Logger _logger = LogManager.CreateLogger("MainForm");

        /// <summary>Test stuff.</summary>
        readonly TestSettings _settings = new();
        #endregion

        #region Fields - adjust to taste
        /// <summary>Cosmetics.</summary>
        readonly Color _controlColor = Color.Aquamarine;

        /// <summary>Where to put things.</summary>
        readonly string _outPath = @"???";
        #endregion

        #region Lifecycle
        /// <summary>
        /// Constructor. No logging allowed yet!
        /// </summary>
        public MainForm()
        {
            // Must do this first before initializing.
            _settings = (TestSettings)SettingsCore.Load(".", typeof(TestSettings));
            MidiSettings.LibSettings = _settings.MidiSettings;

            InitializeComponent();

            toolStrip1.Renderer = new ToolStripCheckBoxRenderer() { SelectedColor = _controlColor };

            // Make sure out path exists.
            _outPath = Path.Join(MiscUtils.GetSourcePath(), "out");
            DirectoryInfo di = new(_outPath);
            di.Create();

            // Logger. Note: you can create this here but don't call any _logger functions until loaded.
            LogManager.MinLevelFile = LogLevel.Trace;
            LogManager.MinLevelNotif = LogLevel.Trace;
            LogManager.LogMessage += LogManager_LogMessage;
            LogManager.Run(Path.Join(_outPath, "log.txt"), 100000);

            // The text output.
            txtViewer.Font = new Font("Cascadia Code", 9);
            txtViewer.WordWrap = true;
            txtViewer.MatchText.Add("ERR", Color.LightPink);
            txtViewer.MatchText.Add("WRN", Color.Plum);

            // UI configs.
            sldVolume.DrawColor = _controlColor;
            sldVolume.Minimum = 0.0;
            sldVolume.Maximum = MidiLibDefs.MAX_VOLUME;
            sldVolume.Resolution = MidiLibDefs.MAX_VOLUME / 50;
            sldVolume.Value = MidiLibDefs.DEFAULT_VOLUME;
            sldVolume.Label = "volume";

            // Time controller.
            MidiSettings.LibSettings.Snap = SnapType.Beat;
            barBar.ProgressColor = _controlColor;
            barBar.CurrentTimeChanged += BarBar_CurrentTimeChanged;

            // Init channel selectors.
            cmbDrumChannel1.Items.Add("NA");
            cmbDrumChannel2.Items.Add("NA");
            for (int i = 1; i <= MidiDefs.NUM_CHANNELS; i++)
            {
                cmbDrumChannel1.Items.Add(i);
                cmbDrumChannel2.Items.Add(i);
            }

            // Hook up some simple UI handlers.
            btnPlay.CheckedChanged += Play_CheckedChanged;
            btnRewind.Click += (_, __) => UpdateState(PlayState.Rewind);
            btnKillMidi.Click += (_, __) =>
            {
                btnPlay.Checked = false;
                _channels.Values.ForEach(ch => ch.Kill());
            };
            btnLogMidi.CheckedChanged += (_, __) => _outputDevice.LogEnable = btnLogMidi.Checked;
            nudTempo.ValueChanged += (_, __) => SetTimer();
        }

        /// <summary>
        /// Window is set up now. OK to log!!
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            bool ok = CreateDevices();
            if(ok)
            {
                // Style file, full info:
                OpenFile(@"C:\Dev\Misc\TestAudioFiles\_LoveSong.S474.sty");

                // Plain midi, full song:
                //OpenFile(@"C:\Dev\Misc\TestAudioFiles\WICKGAME.MID");

                // Plain midi, one instrument, no patch:
                //OpenFile(@"C:\Dev\Misc\TestAudioFiles\_bass_ch2.mid");

                // Plain midi, one instrument, ??? patch:
                //OpenFile(@"C:\Dev\Misc\TestAudioFiles\_drums_ch1.mid");
            }
            base.OnLoad(e);
        }

        /// <summary>
        /// Clean up on shutdown. Dispose() will get the rest.
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            LogManager.Stop();
            Stop();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Resources.
            _mmTimer.Stop();
            _mmTimer.Dispose();

            // Wait a bit in case there are some lingering events.
            System.Threading.Thread.Sleep(100);

            DestroyDevices();

            if (disposing && (components is not null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Devices
        /// <summary>
        /// Create all I/O devices from user settings.
        /// </summary>
        /// <returns>Success</returns>
        bool CreateDevices()
        {
            bool ok = true;

            // First...
            DestroyDevices();

            // Set up input device.
            foreach (var dev in _settings.MidiSettings.InputDevices)
            {
                _inputDevice = new MidiInput(dev.DeviceName);

                if (!_inputDevice.Valid)
                {
                    _logger.Error($"Something wrong with your input device:{dev.DeviceName}");
                    ok = false;
                }
                else
                {
                    _inputDevice.CaptureEnable = true;
                    _inputDevice.InputReceive += Listener_InputReceive;
                }
            }

            // Set up output device.
            foreach (var dev in _settings.MidiSettings.OutputDevices)
            {
                switch (dev.DeviceName)
                {
                    default:
                        // Try midi.
                        _outputDevice = new MidiOutput(dev.DeviceName);
                        if (!_outputDevice.Valid)
                        {
                            _logger.Error($"Something wrong with your output device:{_outputDevice.DeviceName}");
                            ok = false;
                        }
                        else
                        {
                            _outputDevice.LogEnable = btnLogMidi.Checked;
                        }
                        break;
                }
            }

            return ok;
        }

        /// <summary>
        /// Clean up.
        /// </summary>
        void DestroyDevices()
        {
            _inputDevice?.Dispose();
            _outputDevice?.Dispose();
        }
        #endregion

        #region State management
        /// <summary>
        /// General state management. Triggered by play button or the player via mm timer function.
        /// </summary>
        void UpdateState(PlayState state)
        {
            // Suppress recursive updates caused by manually pressing the play button.
            btnPlay.CheckedChanged -= Play_CheckedChanged;

            switch (state)
            {
                case PlayState.Complete:
                    btnPlay.Checked = false;
                    Rewind();
                    Stop();
                    break;

                case PlayState.Play:
                    btnPlay.Checked = true;
                    Play();
                    break;

                case PlayState.Stop:
                    btnPlay.Checked = false;
                    Stop();
                    break;

                case PlayState.Rewind:
                    Rewind();
                    break;
            }

            // Rehook.
            btnPlay.CheckedChanged += Play_CheckedChanged;
        }

        /// <summary>
        /// Handle button presses.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Play_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateState(btnPlay.Checked ? PlayState.Play : PlayState.Stop);
        }

        /// <summary>
        /// The user clicked something in one of the channel controls.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Control_ChannelChange(object? sender, ChannelChangeEventArgs e)
        {
            Channel channel = ((ChannelControl)sender!).BoundChannel;

            if (e.StateChange)
            {
                switch (channel.State)
                {
                    case ChannelState.Normal:
                        break;

                    case ChannelState.Solo:
                        // Mute any other non-solo channels.
                        _channels.Values.ForEach(ch =>
                        {
                            if (channel.ChannelNumber != ch.ChannelNumber && channel.State != ChannelState.Solo)
                            {
                                channel.Kill();
                            }
                        });
                        break;

                    case ChannelState.Mute:
                        channel.Kill();
                        break;
                }
            }

            if (e.PatchChange && channel.Patch >= 0)
            {
                channel.SendPatch();
            }
        }
        #endregion

        #region Transport control
        /// <summary>
        /// Internal handler.
        /// </summary>
        void Play()
        {
            _mmTimer.Start();
        }

        /// <summary>
        /// Internal handler.
        /// </summary>
        void Stop()
        {
            _mmTimer.Stop();
            // Send kill just in case.
            _channels.Values.ForEach(ch => ch.Kill());
        }

        /// <summary>
        /// Go back Jack. Doesn't affect the run state.
        /// </summary>
        void Rewind()
        {
            barBar.Current = new(0);
        }

        /// <summary>
        /// User has changed the time.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BarBar_CurrentTimeChanged(object? sender, EventArgs e)
        {
            //_player.CurrentSub = barBar.Current.TotalSubs;
        }
        #endregion

        #region File handling
        /// <summary>
        /// Common file opener. Initializes pattern list from contents.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        public bool OpenFile(string fn)
        {
            bool ok = true;

            _logger.Info($"Reading file: {fn}");

            if(btnPlay.Checked)
            {
                Stop();
            }

            try
            {
                // Reset stuff.
                cmbDrumChannel1.SelectedIndex = MidiDefs.DEFAULT_DRUM_CHANNEL;
                cmbDrumChannel2.SelectedIndex = 0;
                _mdata = new MidiDataFile();

                // Process the file. Set the default tempo from preferences.
                _mdata.Read(fn, _settings.MidiSettings.DefaultTempo, false);

                // Init new stuff with contents of file/pattern.
                lbPatterns.Items.Clear();
                var pnames = _mdata.GetPatternNames();

                if(pnames.Count > 0)
                {
                    pnames.ForEach(pn => { lbPatterns.Items.Add(pn); });
                }
                else
                {
                    throw new InvalidOperationException($"Something wrong with this file: {fn}");
                }

                Rewind();

                Text = $"Midi Lib Test - {fn}";

                // Pick first.
                lbPatterns.SelectedIndex = 0;

                // Set up timer default.
                nudTempo.Value = 100;

                btnExportMidi.Enabled = _mdata.IsStyleFile;
            }
            catch (Exception ex)
            {
                _logger.Error($"Couldn't open the file: {fn} because: {ex.Message}");
                Text = "Midi Lib";
                ok = false;
            }

            return ok;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Open_Click(object sender, EventArgs e)
        {
            var fileTypes = $"Midi Files|{MidiLibDefs.MIDI_FILE_TYPES}|Style Files|{MidiLibDefs.STYLE_FILE_TYPES}";
            using OpenFileDialog openDlg = new()
            {
                Filter = fileTypes,
                Title = "Select a file",
                InitialDirectory = @"C:\Dev\Apps\TestAudioFiles"
            };

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                OpenFile(openDlg.FileName);
            }
        }
        #endregion

        #region Process pattern info into events
        /// <summary>
        /// Load the requested pattern and create controls.
        /// </summary>
        /// <param name="pinfo"></param>
        void LoadPattern(PatternInfo pinfo)
        {
            Stop();
            Rewind();

            // Clean out our current elements.
            _channelControls.ForEach(c =>
            {
                Controls.Remove(c);
                c.Dispose();
            });
            _channelControls.Clear();
            _channels.Clear();

            // Load the new one.
            // Create the new controls.
            int x = lbPatterns.Right + 5;
            int y = lbPatterns.Top;

            foreach(var (number, patch) in pinfo.GetChannels(true, true))
            {
                // Get events for the channel.
                var chEvents = pinfo.GetFilteredEvents(new List<int>() { number });

                // Is this channel pertinent?
                if (chEvents.Any())
                {
                    // Make new channel.
                    Channel channel = new()
                    {
                        ChannelName = $"chan{number}",
                        ChannelNumber = number,
                        Device = _outputDevice,
                        DeviceId = _outputDevice.DeviceName,
                        Volume = MidiLibDefs.DEFAULT_VOLUME,
                        State = ChannelState.Normal,
                        Patch = patch,
                        IsDrums = number == MidiDefs.DEFAULT_DRUM_CHANNEL,
                        Selected = false,
                    };
                    _channels.Add(channel.ChannelName, channel);
                    channel.SetEvents(chEvents);

                    // Make new control and bind to channel.
                    ChannelControl control = new()
                    {
                        Location = new(x, y),
                        BorderStyle = BorderStyle.FixedSingle,
                        BoundChannel = channel
                    };
                    control.ChannelChange += Control_ChannelChange;
                    Controls.Add(control);
                    _channelControls.Add(control);

                    // Good time to send initial patch.
                    channel.SendPatch();

                    // Adjust positioning.
                    y += control.Height + 5;
                }
            }

            // Add a channel for the input device.
            if(_inputDevice is not null && _inputDevice.Valid)
            {
                int chnum = 16;
                Channel channel = new()
                {
                    ChannelName = $"chan16",
                    ChannelNumber = chnum,
                    Device = _outputDevice,
                    DeviceId = _outputDevice.DeviceName,
                    Volume = MidiLibDefs.DEFAULT_VOLUME,
                    State = ChannelState.Normal,
                    Patch = MidiDefs.GetInstrumentNumber("OrchestralHarp"),
                    IsDrums = false,
                    Selected = false,
                };
                _channels.Add(channel.ChannelName, channel);
                channel.SendPatch();
            }

            // Set timer.
            nudTempo.Value = pinfo.Tempo;

            // Update bar.
            var tot = _channels.TotalSubs();
            barBar.Start = new(0);
            barBar.End = new(tot > 0 ? tot - 1 : 0);
            barBar.Length = new(tot);
            barBar.Current = new(0);

            UpdateDrumChannels();
        }

        /// <summary>
        /// Load pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Patterns_SelectedIndexChanged(object? sender, EventArgs e)
        {
            btnPlay.Checked = false;

            var pname = lbPatterns.SelectedItem.ToString()!;
            var pinfo = _mdata.GetPattern(pname);

            if (pinfo is not null)
            {
                LoadPattern(pinfo);
            }
            else
            {
                _logger.Warn($"Invalid pattern {pname}");
            }

            Rewind();
        }

        /// <summary>
        /// Pattern selection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void AllOrNone_Click(object? sender, EventArgs e)
        {
            bool check = sender == btnAll;
            for(int i = 0; i < lbPatterns.Items.Count; i++)
            {
                lbPatterns.SetItemChecked(i, check);                
            }
        }
        #endregion

        #region Process tick
        /// <summary>
        /// Multimedia timer callback. Synchronously outputs the next midi events.
        /// This is running on the background thread.
        /// </summary>
        void MmTimerCallback(double totalElapsed, double periodElapsed)
        {
            try
            {
                // Check for end of play. Client will take care of transport control.
                if (DoNextStep())
                {
                    // Done playing. Bump over to main thread.
                    this.InvokeIfRequired(_ => UpdateState(PlayState.Complete));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Synchronously outputs the next midi events. Does solo/mute.
        /// This is running on the background thread.
        /// </summary>
        /// <returns>True if sequence completed.</returns>
        public bool DoNextStep()
        {
            bool done;

            // Any soloes?
            bool anySolo = _channels.AnySolo();

            // Process each channel.
            foreach (var ch in _channels.Values)
            {
                // Look for events to send. Any explicit solos?
                if (ch.State == ChannelState.Solo || (!anySolo && ch.State == ChannelState.Normal))
                {
                    // Process any sequence steps.
                    var playEvents = ch.GetEvents(barBar.Current.TotalSubs);
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
                                        Math.Min((int)(evt.Velocity * sldVolume.Value * ch.Volume), MidiDefs.MAX_MIDI),
                                        evt.OffEvent is null ? 0 : evt.NoteLength); // Fix NAudio NoteLength bug.

                                    ch.SendEvent(ne);
                                }
                                break;

                            case NoteEvent evt: // aka NoteOff
                                if (ch.IsDrums)
                                {
                                    // Skip drum noteoffs as windows GM doesn't like them.
                                }
                                else
                                {
                                    ch.SendEvent(evt);
                                }
                                break;

                            default:
                                // Everything else as is.
                                ch.SendEvent(mevt);
                                break;
                        }
                    }
                }
            }

            // Bump time. Check for end of play.
            done = barBar.IncrementCurrent(1);

            return done;
        }
        #endregion

        #region Drum channel
        /// <summary>
        /// User changed the drum channel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void DrumChannel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateDrumChannels();
        }

        /// <summary>
        /// Update all channels based on current UI.
        /// </summary>
        void UpdateDrumChannels()
        {
            _channelControls.ForEach(ctl => ctl.IsDrums =
                (ctl.ChannelNumber == cmbDrumChannel1.SelectedIndex) ||
                (ctl.ChannelNumber == cmbDrumChannel2.SelectedIndex));
        }
        #endregion

        #region Export
        /// <summary>
        /// Export current file to human readable or midi.
        /// </summary>
        void Export_Click(object? sender, EventArgs e)
        {
            try
            {
                // Get selected patterns.
                List<string> patternNames = new();
                if (lbPatterns.Items.Count == 1)
                {
                    patternNames.Add(lbPatterns.Items[0].ToString()!);
                }
                else if (lbPatterns.CheckedItems.Count > 0)
                {
                    foreach (var p in lbPatterns.CheckedItems)
                    {
                        patternNames.Add(p.ToString()!);
                    }
                }
                else
                {
                    _logger.Warn("Please select at least one pattern");
                    return;
                }
                List<PatternInfo> patterns = new();
                patternNames.ForEach(p => patterns.Add(_mdata.GetPattern(p)!));

                // Get selected channels.
                List<Channel> channels = new();
                _channelControls.Where(cc => cc.Selected).ForEach(cc => channels.Add(cc.BoundChannel));
                if (!channels.Any()) // grab them all.
                {
                    _channelControls.ForEach(cc => channels.Add(cc.BoundChannel));
                }

                // Execute the requested export function.
                if (sender == btnExportCsv)
                {
                    var newfn = Tools.MakeExportFileName(_outPath, _mdata.FileName, "all", "csv");
                    MidiExport.ExportCsv(newfn, patterns, channels, _mdata.GetGlobal());
                    _logger.Info($"Exported to {newfn}");
                }
                else if (sender == btnExportMidi)
                {
                    foreach (var pattern in patterns)
                    {
                        var newfn = Tools.MakeExportFileName(_outPath, _mdata.FileName, pattern.PatternName, "mid");
                        MidiExport.ExportMidi(newfn, pattern, channels, _mdata.GetGlobal());
                        _logger.Info($"Export midi to {newfn}");
                    }
                }
                else
                {
                    _logger.Error($"Ooops: {sender}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}");
            }
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Convert tempo to period and set mm timer.
        /// </summary>
        void SetTimer()
        {
            MidiTimeConverter mt = new(_mdata.DeltaTicksPerQuarterNote, (double)nudTempo.Value);
            double period = mt.RoundedInternalPeriod();
            _mmTimer.SetTimer((int)Math.Round(period), MmTimerCallback);
        }

        /// <summary>
        /// Show log events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void LogManager_LogMessage(object? sender, LogMessageEventArgs e)
        {
            // Usually come from a different thread.
            if(IsHandleCreated)
            {
                this.InvokeIfRequired(_ =>
                {
                    txtViewer.AppendLine($"{e.Message}");
                });
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

            _logger.Error($"MidiTimeTest done.");

            void TestClose(double value1, double value2, double tolerance)
            {
                if (Math.Abs(value1 - value2) > tolerance)
                {
                    _logger.Error($"[{value1}] not close enough to [{value2}]");
                }
            }
        }

        /// <summary>
        /// Generate human info.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Docs_Click(object sender, EventArgs e)
        {
            List<string> docs = new()
            {
                "# Midi Input Devices"
            };

            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                docs.Add($"- \"{MidiIn.DeviceInfo(i).ProductName}\"");
            }

            docs.Add("");
            docs.Add("# Midi Output Devices");

            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                docs.Add($"- \"{MidiOut.DeviceInfo(i).ProductName}\"");
            }
            docs.Add("");

            docs.AddRange(MidiDefs.FormatDoc());
            docs.AddRange(MusicDefinitions.FormatDoc());

            Tools.MarkdownToHtml(docs, Tools.MarkdownMode.DarkApi, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Settings_Click(object sender, EventArgs e)
        {
            SettingsEditor.Edit(_settings, "howdy!!!", 400);
            _settings.Save();
            _logger.Warn("You better restart!");
        }
        #endregion

        #region Device input
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Listener_InputReceive(object? sender, InputReceiveEventArgs e)
        {
            _logger.Trace($"Listener:{sender} Note:{e.Note} Controller:{e.Controller} Value:{e.Value}");

            // Translate and pass to output.
            var channel = _channels["chan16"];
            NoteEvent nevt = e.Value > 0 ?
               new NoteOnEvent(0, channel.ChannelNumber, e.Note % MidiDefs.MAX_MIDI, e.Value % MidiDefs.MAX_MIDI, 0) :
               new NoteEvent(0, channel.ChannelNumber, MidiCommandCode.NoteOff, e.Note, 0);
            channel.SendEvent(nevt);
        }
        #endregion
    }

    public class TestSettings : SettingsCore
    {
        // [DisplayName("Background Color")]
        // [Description("The color used for overall background.")]
        // [Browsable(true)]
        // [JsonConverter(typeof(JsonColorConverter))]
        // public Color BackColor { get; set; } = Color.AliceBlue;

        [DisplayName("Ignore Me")]
        [Description("I do nothing.")]
        [Browsable(true)]
        public bool IgnoreMe { get; set; } = true;

        [DisplayName("Midi Settings")]
        [Description("Edit midi settings.")]
        [Browsable(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public MidiSettings MidiSettings { get; set; } = new();
    }
}
