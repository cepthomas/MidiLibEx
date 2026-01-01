using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using NAudio.Midi;
using Ephemera.NBagOfTricks;
using Ephemera.MidiLib;


namespace Ephemera.MidiLibEx
{
    /// <summary>
    /// Represents the contents of a midi file pattern.
    /// If it is a plain midi file (not style) there will be one only.
    /// </summary>
    public class PatternInfo
    {
        #region Fields
        /// <summary>All the pattern midi events.</summary>
        readonly List<MidiEvent> _events = [];

        /// <summary>All the pattern midi events, key is when to play (scaled time).</summary>
        readonly Dictionary<long, List<MidiEvent>> _eventsByTime = [];

        /// <summary>For scaling subs to internal.</summary>
        readonly MidiTimeConverter? _mt = null;

        /// <summary>Collection of all channels in this pattern. Key is number, value is associated patch.</summary>
        readonly Dictionary<int, int> _channelPatches = [];

        /// <summary>Channels with real notes.</summary>
        HashSet<int> _hasNotes = [];
        #endregion

        #region Properties
        /// <summary>Pattern name. Empty indicates single pattern aka plain midi file.</summary>
        public string PatternName { get; init; } = "";

        /// <summary>Tempo, if supplied by file. Default indicates invalid which will be filled in during read.</summary>
        public int Tempo { get; set; } = 0;

        /// <summary>Time signature, if supplied by file.</summary>
        public (int num, int denom) TimeSignature { get; set; } = new();
        #endregion

        /// <summary>
        /// Default constructor. Use only for initialization!
        /// </summary>
        public PatternInfo()
        {
        }

        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="name">Pattern name</param>
        /// <param name="ppq">Resolution</param>
        public PatternInfo(string name, int ppq) : this()
        {
            PatternName = name;
            _mt = new(ppq);
        }

        /// <summary>
        /// Add an event to the collection.
        /// Note!! this replaces the original file AbsoluteTime with the value scaled for internal use.
        /// </summary>
        /// <param name="evt">The event to add.</param>
        public void AddEvent(MidiEvent evt)
        {
            // Capture that this is a valid channel. Patch will get fixed later.
            SetChannelPatch(evt.Channel, -1);

            // Cache info.
            if(evt is NoteOnEvent)
            {
                _hasNotes.Add(evt.Channel);
            }

            // Scale time.
            evt.AbsoluteTime = _mt!.MidiToInternal(evt.AbsoluteTime); 
            _events.Add(evt);

            if(!_eventsByTime.ContainsKey(evt.AbsoluteTime))
            {
                _eventsByTime.Add(evt.AbsoluteTime, new() { evt });
            }
            else
            {
                _eventsByTime[evt.AbsoluteTime].Add(evt);
            }
        }

        /// <summary>
        /// Get enumerator for events using supplied filters.
        /// </summary>
        /// <param name="channels">Specific channnels.</param>
        /// <returns>Enumerator sorted by absolute time.</returns>
        public IEnumerable<MidiEvent> GetFilteredEvents(IEnumerable<int> channels)
        {
            IEnumerable<MidiEvent> descs = _events.Where(e => channels.Contains(e.Channel)) ?? Enumerable.Empty<MidiEvent>();
            return descs.OrderBy(e => e.AbsoluteTime);
        }

        /// <summary>
        /// Get all events at a specific scaled time.
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        public IEnumerable<MidiEvent> GetEventsWhen(int when)
        {
            var evts = _eventsByTime.ContainsKey(when) ? _eventsByTime[when] : new();
            return evts;
        }

        /// <summary>
        /// Get an ordered list of channels and their patches.
        /// </summary>
        /// <param name="hasNotes">Must have noteons.</param>
        /// <param name="hasPatch">Must have valid patch.</param>
        /// <returns></returns>
        public IEnumerable<(int chnum, int patch)> GetChannels(bool hasNotes, bool hasPatch)
        {
            List<(int chnum, int patch)> ps = new();
            // Assemble results from filters.
            bool any = hasNotes ? _events.Where(e => e is NoteOnEvent).Any() : _events.Any();
            if(any)
            {
                _channelPatches
                    .Where(n => hasPatch ? n.Value != -1 : true)
                    .Where(n => _hasNotes.Contains(n.Key))
                    .OrderBy(n => n.Key)
                    .ForEach(n => { ps.Add((n.Key, n.Value)); });
            }

            return ps;
        }

        /// <summary>
        /// Get the patch associated with the channel.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>The patch or -1 if invalid channel</returns>
        public int GetPatch(int channel)
        {
            return _channelPatches.ContainsKey(channel) ? _channelPatches[channel] : -1;
        }

        /// <summary>
        /// Remove a channel from the channel/patches collection.
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveChannel(int channel)
        {
            if(_channelPatches.ContainsKey(channel))
            {
                _channelPatches.Remove(channel);
            }
        }

        /// <summary>
        /// Safely add/update info.
        /// </summary>
        /// <param name="channel">The channel number</param>
        /// <param name="patch">The patch. Can be default -1.</param>
        public void SetChannelPatch(int channel, int patch)
        {
            if (!_channelPatches.ContainsKey(channel))
            {
                _channelPatches.Add(channel, patch);
            }
            else if (patch != -1)
            {
                _channelPatches[channel] = patch;
            }
        }

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var pname = PatternName == "" ? "nameless" : PatternName;
            var s = $"{pname} tempo:{Tempo} timesig:{TimeSignature} channels:{_channelPatches.Count}";
            //ValidPatches.ForEach(p => content.Add($"Ch:{p.Key} Patch:{MidiDefs.GetInstrumentName(p.Value)}"));

            return s;
        }
    }
}