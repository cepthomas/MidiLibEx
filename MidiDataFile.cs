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
        /// <summary>Include events like controller changes, pitch wheel, ...</summary>
        bool _includeNoisy = false;

        /// <summary>All the file pattern sections. Plain midi files will have only one, unnamed.</summary>
        readonly List<PatternInfo> _patterns = new();

        /// <summary>Currently collecting this pattern.</summary>
        PatternInfo _currentPattern = new();
        #endregion

        #region Constants
        /// <summary>Supported file types.</summary>
        public const string MIDI_FILE_TYPES = "*.mid";

        /// <summary>Supported file types.</summary>
        public const string STYLE_FILE_TYPES = "*.sty;*.pcs;*.sst;*.prs";
        #endregion

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

        #region Public functions
        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <param name="defaultTempo">Specified by client in case not in the file.</param>
        /// <param name="includeNoisy"></param>
        public void Read(string fn, int defaultTempo, bool includeNoisy)
        {
            if(_patterns.Any())
            {
                throw new InvalidOperationException($"Already processed - delete me first");
            }

            FileName = fn;
            Tempo = defaultTempo; 
            _includeNoisy = includeNoisy;
            IsStyleFile = STYLE_FILE_TYPES.Contains(Path.GetExtension(fn).ToLower());

            using var br = new BinaryReader(File.OpenRead(fn));
            bool done = false;

            while (!done)
            {
                var sectionName = Encoding.UTF8.GetString(br.ReadBytes(4));

                switch (sectionName)
                {
                    case "MThd":
                        ReadMThd(br);
                        // Always at least one pattern. Plain midi has just one, style has multiple.
                        _currentPattern = new("", DeltaTicksPerQuarterNote);
                        break;
                    case "MTrk":
                        ReadMTrk(br);
                        break;
                    case "CASM":
                        ReadCASM(br);
                        break;
                    case "CSEG":
                        ReadCSEG(br);
                        break;
                    case "Sdec":
                        ReadSdec(br);
                        break;
                    case "Ctab":
                        ReadCtab(br);
                        break;
                    case "Cntt":
                        ReadCntt(br);
                        break;
                    case "OTSc":
                        // One Touch Setting section
                        ReadOTSc(br);
                        break;
                    case "FNRc":
                        // MDB (Music Finder) section
                        ReadFNRc(br);
                        break;
                    default:
                        done = true;
                        break;
                }
            }

            // Save last one.
            _patterns.Add(_currentPattern);

            // Fix up gaps.
            CleanUpPatterns();
        }

        /// <summary>
        /// Get the pattern by name.
        /// </summary>
        /// <param name="name">Which</param>
        /// <returns>The pattern. Throws if name not found.</returns>
        public PatternInfo GetPattern(string name)
        {
            var pinfo = _patterns.Where(p => p.PatternName == name);
            if (pinfo is not null && pinfo.Any())
            {
                return pinfo.First();
            }
            else
            {
                throw new InvalidOperationException($"Invalid pattern name: {name}");
            }
        }

        /// <summary>
        /// Get all useful pattern names - those with musical notes.
        /// </summary>
        /// <returns>List of names.</returns>
        public List<string> GetPatternNames()
        {
            var names = _patterns.Select(p => p.PatternName).ToList();
            return names;
        }

        /// <summary>
        /// Get common midi file meta info.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> GetGlobal()
        {
            Dictionary<string, int> global = new()
            {
                { nameof(MidiFileType), MidiFileType },
                { nameof(DeltaTicksPerQuarterNote), DeltaTicksPerQuarterNote },
                { nameof(NumTracks), NumTracks }
            };

            return global;
        }
        #endregion

        #region Section readers
        /// <summary>
        /// Read the midi header section.
        /// </summary>
        /// <param name="br"></param>
        void ReadMThd(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);

            if (chunkSize != 6)
            {
                throw new FormatException("Unexpected header chunk length");
            }

            MidiFileType = (int)ReadStream(br, 2);
            NumTracks = (int)ReadStream(br, 2);
            DeltaTicksPerQuarterNote = (int)ReadStream(br, 2);
        }

        /// <summary>
        /// Read a midi track chunk.
        /// </summary>
        /// <param name="br"></param>
        /// <returns></returns>
        int ReadMTrk(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            // Read all midi events.
            MidiEvent? me = null; // current event
            while (br.BaseStream.Position < startPos + chunkSize)
            {
                //_lastStreamPos = br.BaseStream.Position;

                me = MidiEvent.ReadNextEvent(br, me);
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;

                // Style file patterns:
                // - nameless has: tempo, time signature, copyright. 
                // - SFF1/SFF2 has: SequenceTrackName, ???.
                // - SInt has: default patches, ???.
                // Plain midi file has one only pattern - nameless.

                switch (me)
                {
                    ///// Standard midi events /////
                    case NoteOnEvent evt:
                        AddMidiEvent(evt);
                        break;

                    case NoteEvent evt: // usually NoteOff
                        AddMidiEvent(evt);
                        break;

                    case PatchChangeEvent evt:
                        // Save the pattern patch.
                        _currentPattern.SetChannelPatch(evt.Channel, evt.Patch);
                        if (_currentPattern.PatternName == "SInt")
                        {
                            // Style file section - save to default pattern.
                            GetPattern("").SetChannelPatch(evt.Channel, evt.Patch);
                        }
                        AddMidiEvent(evt);
                        break;

                    case ControlChangeEvent evt when _includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case PitchWheelChangeEvent evt when _includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case SysexEvent evt when _includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    ///// Meta events /////
                    case TrackSequenceNumberEvent evt:
                        AddMidiEvent(evt);
                        break;

                    case TempoEvent evt:
                        var tempo = (int)Math.Round(evt.Tempo);
                        _currentPattern.Tempo = tempo;
                        if (_currentPattern.PatternName == "")
                        {
                            Tempo = tempo;
                        }
                        AddMidiEvent(evt);
                        break;

                    case TimeSignatureEvent evt:
                        _currentPattern.TimeSignature = (evt.Numerator, evt.Denominator);
                        if (_currentPattern.PatternName == "")
                        {
                            TimeSignature = (evt.Numerator, evt.Denominator);
                        }
                        AddMidiEvent(evt);
                        break;

                    case KeySignatureEvent evt:
                        AddMidiEvent(evt);
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.SequenceTrackName:
                        AddMidiEvent(evt);
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.Marker:
                        // This optional event is used to label points within a sequence, e.g. rehearsal letters, loop points, or section
                        // names (such as 'First verse'). For a format 1 MIDI file, Marker Meta events should only occur within the first MTrk chunk.

                        if (IsStyleFile)
                        {
                            // Indicates start of a new midi pattern. Save current.
                            _patterns.Add(_currentPattern);

                            // Start a new pattern.
                            _currentPattern = new PatternInfo(evt.Text, DeltaTicksPerQuarterNote) {  Tempo = Tempo };
                            absoluteTime = 0;
                            AddMidiEvent(evt);
                        }
                        else
                        {
                            // Simple add if one only pattern.
                            AddMidiEvent(evt);
                        }
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.TextEvent:
                        AddMidiEvent(evt);
                        break;

                    case MetaEvent evt when evt.MetaEventType == MetaEventType.EndTrack:
                        // Indicates end of current midi track.
                        AddMidiEvent(evt);
                        break;

                    default:
                        // Add to taste.
                        break;
                }
            }

            ///// Local function. /////
            void AddMidiEvent(MidiEvent evt)
            {
                _currentPattern.AddEvent(evt);
            }

            return absoluteTime;
        }

        /// <summary>
        /// Read the CASM section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCASM(BinaryReader br)
        {
            /*uint chunkSize =*/ ReadStream(br, 4);
        }

        /// <summary>
        /// Read the CSEG section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCSEG(BinaryReader br)
        {
            /*uint chunkSize =*/ ReadStream(br, 4);
        }

        /// <summary>
        /// Read the Sdec section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadSdec(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the Ctab section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCtab(BinaryReader br)
        {
            // Has some key and chord info.
            uint chunkSize = ReadStream(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the Cntt section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadCntt(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the OTSc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadOTSc(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            br.ReadBytes((int)chunkSize);
        }

        /// <summary>
        /// Read the FNRc section of a style file.
        /// </summary>
        /// <param name="br"></param>
        void ReadFNRc(BinaryReader br)
        {
            uint chunkSize = ReadStream(br, 4);
            br.ReadBytes((int)chunkSize);
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Fill in any missing pattern info using defaults.
        /// </summary>
        void CleanUpPatterns()
        {
            // TODO auto-determine which channel(s) have drums?
            // Drum channels will probably have the most notes. Also durations will be short.
            // Could also remember user's reassignments in the settings file.

            var pdefault = GetPattern("");

            if (IsStyleFile)
            {
                // Get the always present nameless pattern.

                // Delete unneeded stuff.
                List<PatternInfo> toRemove = new();

                foreach (var p in _patterns)
                {
                    switch(p.PatternName)
                    {
                        case "SFF1":
                        case "SFF2":
                        case "SInt":
                        case "":
                            toRemove.Add(p);
                            break;

                        default:
                            // Update missing properties.
                            if (p.Tempo == 0) // not specified.
                            {
                                p.Tempo = Tempo;
                            }

                            if (p.TimeSignature == (0, 0)) // not specified.
                            {
                                p.TimeSignature = TimeSignature;
                            }

                            // Make sure a patch is supplied.
                            p.GetChannels(true, false).ForEach(vc =>
                            {
                                if (vc.patch == -1)
                                {
                                    var newp = pdefault.GetPatch(vc.chnum);
                                    if(newp == -1)
                                    {
                                        pdefault.RemoveChannel(vc.chnum);
                                    }
                                    else
                                    {
                                        p.SetChannelPatch(vc.chnum, newp);
                                    }
                                }
                            });
                            break;
                    }
                }

                toRemove.ForEach(p => _patterns.Remove(p));
            }
            else
            {
                // Simple midi file. Handle corner cases.

                // Some files are missing patch info.
                pdefault.GetChannels(true, false).ForEach(vc =>
                {
                    var newp = pdefault.GetPatch(vc.chnum);
                    if (newp == -1)
                    {
                        // Force to default.
                        pdefault.SetChannelPatch(vc.chnum, 0);
                    }
                });
            }
        }

        /// <summary>
        /// Read a number from stream and adjust endianess.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        uint ReadStream(BinaryReader br, int size)
        {
            //_lastStreamPos = br.BaseStream.Position;

            var i = size switch
            {
                2 => MiscUtils.FixEndian(br.ReadUInt16()),
                4 => MiscUtils.FixEndian(br.ReadUInt32()),
                _ => throw new FormatException("Unsupported read size"),
            };
            return i;
        }
        #endregion
    }
}
