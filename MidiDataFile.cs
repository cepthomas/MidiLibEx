using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio.Midi;
using Ephemera.NBagOfTricks;


namespace Ephemera.MidiLibEx
{
    /// <summary>
    /// Represents one complete collection of midi events from a file - standard midi or yamaha style files.
    /// Writes subsets to various output formats.
    /// </summary>
    public class MidiDataFile
    {
        #region Fields
        /// <summary>All the file pattern sections.</summary>
        List<Pattern> _patterns = [];
        #endregion

        #region Constants
        /// <summary>Supported file types.</summary>
        public const string UNNAMED = "noname";

        /// <summary>Supported file types.</summary>
        public const string MIDI_FILE_TYPES = "*.mid;*.midi";

        /// <summary>Supported file types.</summary>
        public const string STYLE_FILE_TYPES = "*.sty;*.fps;*.pcs;*.sst;*.pst;*.prs;*.bcs;*.yjz";

        /// <summary>System default.</summary>
        public const int DEFAULT_TEMPO = 100;
        #endregion

        #region Properties
        /// <summary>Input file.</summary>
        public string FileName { get; private set; } = "???";

        /// <summary>Style files have a name, plain midi don't.</summary>
        public string StyleName { get; private set; } = "";

        /// <summary>Style file header section.</summary>
        public Header Header { get; private set; } = new();

        /// <summary>Include events like controller changes, pitch wheel, ...</summary>
        public bool IncludeNoisy { get; set; } = false;
        #endregion

        #region Public functions
        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        public void Read(string fn)
        {
            _patterns.Clear();
            FileName = fn;
            bool isStyleFile = STYLE_FILE_TYPES.Contains(Path.GetExtension(fn), StringComparison.CurrentCultureIgnoreCase);
            if (isStyleFile)
            {
                // default
                StyleName = UNNAMED;
            }

            using var br = new BinaryReader(File.OpenRead(fn));
            bool done = false;
            while (!done)
            {
                // Read next section.
                var bytes = br.ReadBytes(4);
                var sectionName = Encoding.UTF8.GetString(bytes);
                Dump($"===== [{sectionName}] =====");

                switch (sectionName)
                {
                    case "MThd":
                        // Read the midi header section.
                        ReadMThd(br);

                        Dump($"{Header}");

                        // Sanity check.
                        if (isStyleFile && (Header.MidiFileType != 0 || Header.NumTracks != 1))
                        {
                            throw new InvalidOperationException("Not a valid style file");
                        }
                        break;

                    case "MTrk":
                        // Read the track section.
                        _patterns = isStyleFile ? ReadMTrkStyle(br) : [ReadMTrkSimple(br)];
                        break;

                    // Style details.
                    case "CASM":
                    case "CSEG":
                    case "Sdec":
                    case "Ctab":
                    case "Cntt":
                    case "OTSc":
                    case "FNRc":
                        // Skip others for now.
                        uint chunkSize2 = ReadStream(br, 4);
                        br.ReadBytes((int)chunkSize2);
                        break;

                    default:
                        // Sometimes there's other stuff at the end of the file - ignore.
                        done = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Get all useful pattern names - those with musical notes.
        /// </summary>
        /// <returns>List of names.</returns>
        public List<string> GetPatternNames()
        {
            var names = _patterns.Select(p => p.Name).ToList();
            return names;
        }

        /// <summary>
        /// Get the pattern by name.
        /// </summary>
        /// <param name="name">Which</param>
        /// <returns>The pattern. Throws if name not found.</returns>
        public Pattern GetPattern(string name)
        {
            var pinfo = _patterns.Where(p => p.Name == name);
            if (pinfo is not null && pinfo.Any())
            {
                return pinfo.First();
            }
            else
            {
                throw new InvalidOperationException($"Invalid pattern name [{name}]");
            }
        }
        #endregion

        #region Section readers
        /// <summary>
        /// Read the midi header section. Same for all input types.
        /// </summary>
        /// <param name="br"></param>
        void ReadMThd(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            if (chunkSize != 6) { throw new InvalidOperationException("Unexpected header chunk length"); }

            Header.MidiFileType = (int)ReadStream(br, 2);
            Header.NumTracks = (int)ReadStream(br, 2);
            Header.DeltaTicksPerQuarterNote = (int)ReadStream(br, 2);
        }

        /// <summary>
        /// Read a plain midi file.
        /// </summary>
        /// <param name="br"></param>
        /// <returns>New tracks</returns>
        Pattern ReadMTrkSimple(BinaryReader br)
        {
            Pattern simplePattern = new();
            Track currentTrack = new(Header.DeltaTicksPerQuarterNote);

            // Stepping through file.
            uint chunkSize;
            long startPos;
            int absoluteTime;
            ResetState();

            // Read all midi events.
            bool done = false;
            MidiEvent? me = null; // current event
            while (!done && br.BaseStream.Position < startPos + chunkSize)
            {
                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;
                Dump($"Next event {me} [{me.DeltaTime}]");

                switch (me)
                {
                    ///// Standard midi events /////
                    case NoteOnEvent evt:
                        currentTrack.AddEvent(evt);
                        break;

                    case NoteEvent evt: // aka NoteOff
                        currentTrack.AddEvent(evt);
                        break;

                    case PatchChangeEvent evt:
                        currentTrack.SetPatch(evt.Channel, evt.Patch);
                        currentTrack.AddEvent(evt);
                        break;

                    case ControlChangeEvent evt when IncludeNoisy:
                        currentTrack.AddEvent(evt);
                        break;

                    case PitchWheelChangeEvent evt when IncludeNoisy:
                        currentTrack.AddEvent(evt);
                        break;

                    //case SysexEvent evt when IncludeNoisy:
                    //    currentTrack.AddEvent(evt);
                    //    break;

                    ///// Meta events /////
                    case TempoEvent evt:
                        currentTrack.AddEvent(evt);
                        break;

                    case TimeSignatureEvent evt:
                        currentTrack.AddEvent(evt);
                        break;

                    case KeySignatureEvent evt:
                        currentTrack.AddEvent(evt);
                        break;

                    ///// Structure events /////
                    case TextEvent evt when evt.MetaEventType == MetaEventType.SequenceTrackName:
                        currentTrack.Name = evt.Text;
                        currentTrack.AddEvent(evt);
                        break;

                    case MetaEvent evt when evt.MetaEventType == MetaEventType.EndTrack:
                        // Indicates end of current midi track.
                        currentTrack.AddEvent(evt);
                        // Add to collection and start a new one.
                        simplePattern.Tracks.Add(currentTrack);

                        // Next section?
                        var sectionName = Encoding.UTF8.GetString(br.ReadBytes(4));
                        if (sectionName == "MTrk")
                        {
                            // One mo time.
                            currentTrack = new(Header.DeltaTicksPerQuarterNote);
                            ResetState();
                        }
                        else
                        {
                            // Assume done reading tracks.
                            done = true;
                        }
                        break;

                    default:
                        // Ignore others.
                        break;
                }
            }

            // Local function.
            void ResetState()
            {
                chunkSize = ReadStream(br, 4);
                startPos = br.BaseStream.Position;
                absoluteTime = 0;
            }

            return simplePattern;
        }

        /// <summary>
        /// Read a style track.
        ///     output will be 1-N patterns, each with 1 track, each with 1-N channels.
        /// </summary>
        /// <param name="br"></param>
        /// <returns>New tracks</returns>
        List<Pattern> ReadMTrkStyle(BinaryReader br)
        {
            List<Pattern> stylePatterns = [];

            // Elements if provided in file track.
            TempoEvent? tempoEvt = null;
            KeySignatureEvent? keySigEvt = null;
            TimeSignatureEvent? timeSigEvt = null;

            string _currentMarker = "";
            Track currentTrack = new(Header.DeltaTicksPerQuarterNote);

            // SInt events for initializing tracks.
            List<MidiEvent> _initEvents = [];

            uint chunkSize = ReadStream(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            bool done = false;
            MidiEvent? me = null; // current event

            while (!done && br.BaseStream.Position < startPos + chunkSize)
            {
                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;
                Dump($"Next event {me}");

                switch (me)
                {
                    ///// Standard midi events /////
                    case NoteOnEvent evt:
                        AddMidiEvent(evt);
                        break;

                    case NoteEvent evt: // aka NoteOff
                        AddMidiEvent(evt);
                        break;

                    case PatchChangeEvent evt:
                        currentTrack.SetPatch(evt.Channel, evt.Patch);
                        AddMidiEvent(evt);
                        break;

                    case ControlChangeEvent evt when IncludeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case PitchWheelChangeEvent evt when IncludeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case SysexEvent evt when IncludeNoisy:
                        //currentTrack.AddEvent(evt);
                        break;

                    ///// Meta events /////
                    case TempoEvent evt:
                        tempoEvt = evt;
                        AddMidiEvent(evt);
                        break;

                    case TimeSignatureEvent evt:
                        timeSigEvt = evt;
                        AddMidiEvent(evt);
                        break;

                    case KeySignatureEvent evt:
                        keySigEvt = evt;
                        AddMidiEvent(evt);
                        break;

                    ///// Structure events /////
                    case TextEvent evt when evt.MetaEventType == MetaEventType.SequenceTrackName:
                        StyleName = evt.Text;
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.Marker:
                        _currentMarker = evt.Text;

                        switch (evt.Text)
                        {
                            case "SFF1":
                            case "SFF2":
                                // Meta stuff. In factory styles, StyleName is generally followed by sysex events
                                // that define the style. The importance of these sysex is not understood.
                            case "SInt":
                                // Common stuff. The SFF and SInt sections are executed when a style section is changed.
                                // This resets the voices and other channel parameters to their initial values.
                                _initEvents.Clear();
                                break;

                            default:
                                TidySection();
                                break;
                        }
                        break;

                    case MetaEvent evt when evt.MetaEventType == MetaEventType.EndTrack:
                        // End of input file processing.
                        TidySection();
                        break;

                    default:
                        break;
                }
            }

            // Local function.
            void AddMidiEvent(MidiEvent evt)
            {
                switch (_currentMarker)
                {
                    case "SFF1":
                    case "SFF2":
                    case "SInt":
                        _initEvents.Add(evt);
                        break;
                    default:
                        currentTrack.AddEvent(evt);
                        break;
                }
            }

            // Local function.
            void TidySection()
            {
                // Start of a style section. Clean up old?
                if (currentTrack.NumStandard > 0)
                {
                    // Finish up.
                    var p = new Pattern() // Header.DeltaTicksPerQuarterNote)
                    {
                        Name = currentTrack.Name,
                        Tempo = tempoEvt is null ? DEFAULT_TEMPO : (int)Math.Round(tempoEvt.Tempo),
                    };

                    var endEvent = new MetaEvent(MetaEventType.EndTrack, 0, absoluteTime);

                    // Insert track 0.
                    var trk0 = new Track(Header.DeltaTicksPerQuarterNote);
                    if (timeSigEvt is not null) { trk0.AddEvent(timeSigEvt); }
                    if (keySigEvt is not null) { trk0.AddEvent(keySigEvt); }
                    if (tempoEvt is not null) { trk0.AddEvent(tempoEvt); }
                    trk0.AddEvent(endEvent);
                    p.Tracks.Add(trk0);

                    // The collected events.
                    currentTrack.AddEvent(endEvent);
                    p.Tracks.Add(currentTrack);

                    stylePatterns.Add(p);
                }

                // Reset, start new.
                currentTrack = new(Header.DeltaTicksPerQuarterNote) { Name = _currentMarker };
                _initEvents.ForEach(evt => currentTrack.AddEvent(evt));
                absoluteTime = 0;
            }

            return stylePatterns;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Read a number from stream and adjust endianess.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        uint ReadStream(BinaryReader br, int size)
        {
            var i = size switch
            {
                2 => MiscUtils.FixEndian(br.ReadUInt16()),
                4 => MiscUtils.FixEndian(br.ReadUInt32()),
                _ => throw new InvalidOperationException("Unsupported read size"),
            };
            return i;
        }

        /// <summary>
        /// Debugging.
        /// </summary>
        /// <param name="msg"></param>
        void Dump(string msg)
        {
            Console.WriteLine(msg);
        }
        #endregion
    }
}
