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
    /// Contents of MThd section.
    /// </summary>
    public class Header
    {
        /// <summary>What midi type is it.</summary>
        public int MidiFileType { get; set; } = 0;

        /// <summary>How many tracks.</summary>
        public int NumTracks { get; set; } = 0;

        /// <summary>Original resolution for all events.</summary>
        public int DeltaTicksPerQuarterNote { get; set; } = 0;
    }

    /// <summary>
    /// Represents one complete collection of midi events from a file - standard midi or yamaha style files.
    /// Writes subsets to various output formats.
    /// </summary>
    public class MidiDataFile
    {
        #region Fields
        /// <summary>All the file pattern sections. Plain midi files will have only one, unnamed.</summary>
        readonly List<Pattern> _patterns = [];
        #endregion

        #region Constants
        /// <summary>Supported file types.</summary>
        public const string MIDI_FILE_TYPES = "*.mid;*.midi";

        /// <summary>Supported file types.</summary>
        public const string STYLE_FILE_TYPES = "*.sty;*.fps;*.pcs;*.sst;*.pst;*.prs;*.bcs;*.yjz";
        #endregion

        #region Properties
        /// <summary>It's a style file.</summary>
        public string FileName { get; private set; } = "???";

        /// <summary>It's a style file.</summary>
        public bool IsStyleFile { get; private set; } = false;

        /// <summary>It's a style file.</summary>
        public Header Header { get; private set; } = new();

        /// <summary>Tempo if provided in file track.</summary>
        public int Tempo { get; set; } = 100;

        /// <summary>Key signature if provided in file track.</summary>
        public int SharpsFlats { get; set; } = 0;

        /// <summary>Time signature if provided in file track. Written is $"{num}/{denom*2}"</summary>
        public (int num, int denom) TimeSignature { get; set; } = new();
        #endregion


        // Currently collecting this pattern.
        Pattern? _currentPattern = null;



        #region Public functions
        /// <summary>
        /// Read a file.
        /// </summary>
        /// <param name="fn">The file to open.</param>
        /// <param name="includeNoisy">Include events like controller changes, pitch wheel, ...</param>
        public void Read(string fn, bool includeNoisy)
        {
            // Sanity checks.
            if (_patterns.Any()) { throw new InvalidOperationException($"Already processed - delete me first"); }

            FileName = fn;

            // Currently collecting this track.
            Track? track;

            IsStyleFile = STYLE_FILE_TYPES.Contains(Path.GetExtension(fn), StringComparison.CurrentCultureIgnoreCase);
            bool done = false;

            // IsStyleFile = false;

            using var br = new BinaryReader(File.OpenRead(fn));

            while (!done)
            {
                var bytes = br.ReadBytes(4);
                if (bytes.Length != 4)
                {
                    done = true;
                    break;
                }

                var sectionName = Encoding.UTF8.GetString(bytes);

                switch (sectionName)
                {
                    case "MThd":
                        ReadMThd(br);
                        // Always at least one pattern. Plain midi has just one, style has multiple.
                        _currentPattern = new("TODO1", Header.DeltaTicksPerQuarterNote);
                        break;

                    case "MTrk": // start a track
                        if (_currentPattern is null) { throw new InvalidOperationException($"Missing MThd section"); }

                        // Do new track.
                        track = ReadMTrk(br, includeNoisy);
                        _currentPattern.Tracks.Add(track);
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
                        uint chunkSize = ReadStream(br, 4);
                        br.ReadBytes((int)chunkSize);
                        break;

                    default:
                        // Sometimes there's other stuff at the end of the file - ignore.
                        done = true;
                        break;
                }
            }

            // Save last one.
            if (_currentPattern is not null)
            {
                _patterns.Add(_currentPattern);
            }
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
                throw new InvalidOperationException($"Invalid pattern name: {name}");
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
        #endregion

        #region Section readers
        /// <summary>
        /// Read the midi header section.
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
        /// Read a midi track chunk.
        /// </summary>
        /// <param name="br"></param>
        /// <param name="includeNoisy">Include events like controller changes, pitch wheel, ...</param>
        /// <returns>New tracks</returns>
        Track ReadMTrk(BinaryReader br, bool includeNoisy)
        {
//Console.WriteLine("===== MTrk =====");
            Track track = new();
            uint chunkSize = ReadStream(br, 4);
            long startPos = br.BaseStream.Position;
            int absoluteTime = 0;

            bool foundEndTrack = false;

            // Read all midi events.
            MidiEvent me = new(0, 16, 0); // current event
            while (br.BaseStream.Position < startPos + chunkSize)
            {
                if (foundEndTrack) { throw new InvalidOperationException("Events past end of track"); }

                me = MidiEvent.ReadNextEvent(br, me);
//Console.WriteLine(me.ToString());
                absoluteTime += me.DeltaTime;
                me.AbsoluteTime = absoluteTime;

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
                        //////// original /////////
                        // // Save the pattern patch.
                        // _currentPattern.SetChannelPatch(evt.Channel, evt.Patch);
                        // if (_currentPattern.PatternName == "SInt")
                        // {
                        //     // Style file section - save to default pattern.
                        //     GetPattern("").SetChannelPatch(evt.Channel, evt.Patch);
                        // }

                        track.SetPatch(evt.Channel, evt.Patch);
                        AddMidiEvent(evt);
                        break;

                    case ControlChangeEvent evt when includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case PitchWheelChangeEvent evt when includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    case SysexEvent evt when includeNoisy:
                        AddMidiEvent(evt);
                        break;

                    ///// Meta events /////
                    case TempoEvent evt:
                        //////// original /////////
                        // var tempo = (int)Math.Round(evt.Tempo);
                        // _currentPattern.Tempo = tempo;
                        // if (_currentPattern.PatternName == "")
                        // {
                        //     Tempo = tempo;
                        // }

                        var tempo = (int)Math.Round(evt.Tempo);
                        Tempo = tempo;
                        AddMidiEvent(evt);
                        break;

                    case TimeSignatureEvent evt:
                        //////// original /////////
                        // _currentPattern.TimeSignature = (evt.Numerator, evt.Denominator);
                        // if (_currentPattern.PatternName == "")
                        // {
                        //     TimeSignature = (evt.Numerator, evt.Denominator);
                        // }

                        // 1:1:0 0 TimeSignature 4 / 4 TicksInClick: 24 32ndsInQuarterNote: 8
                        TimeSignature = (evt.Numerator, evt.Denominator);
                        AddMidiEvent(evt);
                        break;

                    case KeySignatureEvent evt:
                        // 1:1:0 0 KeySignature C major
                        SharpsFlats = evt.SharpsFlats;
                        AddMidiEvent(evt);
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.SequenceTrackName:
                        track.Name = evt.Text;
                        AddMidiEvent(evt);
                        break;

                    case TextEvent evt when evt.MetaEventType == MetaEventType.Marker:
                        // This optional event is used to label points within a sequence, e.g. rehearsal letters,
                        // loop points, or section names (such as 'First verse').
                        // For a format 1 MIDI file, Marker Meta events should only occur within the first MTrk chunk.
                        if (IsStyleFile)
                        {
                            // Indicates start of a new pattern. Save current.
                            if (_currentPattern is null) { throw new InvalidOperationException($"Missing MThd section"); }

                            _patterns.Add(_currentPattern);

                            // Start a new pattern.
                            _currentPattern = new Pattern(evt.Text, Header.DeltaTicksPerQuarterNote) { Tempo = Tempo };
                            absoluteTime = 0;
                            AddMidiEvent(evt);
                        }
                        else
                        {
                            // Simple add if one only pattern.
                            AddMidiEvent(evt);
                        }



                        //////// original /////////
                        // // This optional event is used to label points within a sequence, e.g. rehearsal letters, loop points, or section
                        // // names (such as 'First verse'). For a format 1 MIDI file, Marker Meta events should only occur within the first MTrk chunk.
                        // if (IsStyleFile)
                        // {
                        //     // Indicates start of a new midi pattern. Save current.
                        //     _patterns.Add(_currentPattern);
                        //     // Start a new pattern.
                        //     _currentPattern = new PatternInfo(evt.Text, DeltaTicksPerQuarterNote) {  Tempo = Tempo };
                        //     absoluteTime = 0;
                        //     AddMidiEvent(evt);
                        // }
                        // else
                        // {
                        //     // Simple add if one only pattern.
                        //     AddMidiEvent(evt);
                        // }


                        //////// new /////////
                        if (IsStyleFile && _patterns.Count == 0)
                        {
                            //The midi section is midi type 0, which means that there is one midi track.


                            //In the first measure there is a marker event which informs about the version of the style file
                            //format.Currently there are two different marker values:
                            //• SFF1
                            //• SFF2 New format introduced with the Tyros 3 keyboard(Sept. 2008).
                            //Also named “SFF GE”.
                            //The only difference is the new “Cbt2” sctructure described in chapter 4.6.3.2
                            //SFF1 format files that are loaded into instruments that support SFF2 are automatically
                            //converted to SFF2.

                            //The following midi data has to be completed in the first measure of the midi data.Usually all
                            //events are on measure 1, beat 1, tick 0(1:01:000).It is important that they are located in the
                            //file in the sequence as mentioned below.
                            //Initial data: The first commands after the midi track header are usually time signature, tempo
                            //and copyright(optional). Time Signature is used to determine the metronome behavior and
                            //perhaps the score display; its value does not affect the play back of the note events. This is
                            //determined by the time values associated with the note on-off events.The tempo sets the
                            //default tempo of the instrument.
                            //SFF1 or SFF2: This marker must come before the SInt marker. It is followed by the
                            //StyleName, which is a Meta Event identified by ID = 3(see Table 6).The length of meta text
                            //events(except copyright) usually is limited in practice to a size which fits in a PSR display
                            //field.In factory styles, StyleName is generally followed by sysex events that define the style
                            //(see Table 30).The importance of these sysex is not understood.

                            //SInt: The SInt marker must be after the above data and is generally followed by Midi On,
                            //Controller and Program Change Midi Events necessary to initialize the midi channels and
                            //sysex to set up the DSP

                            //???????????
                            //// Indicates start of a new midi pattern. Save current.
                            //if (_currentTrack is not null)
                            //{
                            //    _currentPattern.AddTrack(_currentTrack);
                            //    _currentTrack = null;
                            //}
                            //_patterns.Add(_currentPattern);

                            //// Start a new pattern.
                            //_currentPattern = new Pattern(evt.Text, DeltaTicksPerQuarterNote);// {  Tempo = Tempo };
                            //absoluteTime = 0;

                            //AddMidiEvent(evt);
                        }
                        else
                        {
                            // Simple add if one only pattern.
                            //AddMidiEvent(evt);
                        }
                        break;

                    case MetaEvent evt when evt.MetaEventType == MetaEventType.EndTrack:
                        // Indicates end of current midi track.
                         AddMidiEvent(evt);
                        foundEndTrack = true;
                        break;

                    default:
                        // Other? Add to taste.
                        break;
                }
            }

            ///// Local function. /////
            void AddMidiEvent(MidiEvent evt)//, bool meta)
            {
                track.AddEvent(evt);
            }

            return track;
        }
        #endregion

        #region Private functions
        /// <summary>
        /// Fill in any missing pattern info using defaults.
        /// </summary>
        void CleanUpPatterns() // TODO1 needed???
        {
            // TODO auto-determine which channel(s) have drums?
            // Drum channels will probably have the most notes. Also durations will be short.
            // Could also remember user's reassignments in the settings file.

            if (IsStyleFile)
            {
               // Get the always present nameless pattern.
               // Delete unneeded stuff.
               List<Pattern> toRemove = [];
               foreach (var p in _patterns)
               {
                   switch(p.Name)
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
                           //// Make sure a patch is supplied.
                           //p.GetChannels(true, false).ForEach(vc =>
                           //{
                           //    if (vc.patch == -1)
                           //    {
                           //        var newp = pdefault.GetPatch(vc.chnum);
                           //        if (newp == -1)
                           //        {
                           //            pdefault.RemoveChannel(vc.chnum);
                           //        }
                           //        else
                           //        {
                           //            p.SetChannelPatch(vc.chnum, newp);
                           //        }
                           //    }
                           //});
                           break;
                   }
               }
               toRemove.ForEach(p => _patterns.Remove(p));
            }
            else
            {
               //// Simple midi file. Handle corner cases.
               //// Some files are missing patch info.
               //pdefault.GetChannels(true, false).ForEach(vc =>
               //{
               //    var newp = pdefault.GetPatch(vc.chnum);
               //    if (newp == -1)
               //    {
               //        // Force to default.
               //        pdefault.SetChannelPatch(vc.chnum, 0);
               //    }
               //});
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
            var i = size switch
            {
                2 => MiscUtils.FixEndian(br.ReadUInt16()),
                4 => MiscUtils.FixEndian(br.ReadUInt32()),
                _ => throw new InvalidOperationException("Unsupported read size"),
            };
            return i;
        }
        #endregion
    }
}
