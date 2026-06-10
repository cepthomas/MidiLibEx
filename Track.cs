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
    /// Represents the contents of a midi track.
    /// </summary>
    public class Track
    {
        #region Fields
        /// <summary>All the pattern midi events.</summary>
        readonly List<MidiEvent> _events = [];

        ///// <summary>All the pattern midi events, key is when to play (scaled/internal time).</summary>
        //readonly Dictionary<long, List<MidiEvent>> _eventsByTime = [];

        ///// <summary>For scaling midi ticks to internal.</summary>
        //readonly MidiTimeConverter _mt;

        /// <summary>Collection of all channels in this pattern. Key is channel number, value is associated patch.</summary>
        readonly Dictionary<int, int> _channelPatches = [];

        /// <summary>Channels with real notes.</summary>
        readonly HashSet<int> _hasNotes = [];

        /// <summary>Max length of all sequences in midi ticks.</summary>
        long _maxTick = 0;
        #endregion

        #region Properties
        /// <summary>Track name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Tempo.</summary>
        public int Tempo { get; set; } = 0;

        /// <summary>Key signature.</summary>
        public int SharpsFlats { get; set; } = -1;

        /// <summary>Time signature.</summary>
        public (int num, int denom) TimeSignature { get; set; } = new();

        ///// <summary>Length of all sequences in scaled/internal time.</summary>
        //public int Length { get { return _mt.MidiToInternal(_maxTick, true); } }
        #endregion

        ///// <summary>
        ///// Normal constructor.
        ///// </summary>
        ///// <param name="name">Track name</param>
        ///// <param name="ppq">Resolution</param>
        //public Track(string name, int ppq)
        //{
        //    Name = name;
        //    _mt = new(ppq);
        //}

        /// <summary>
        /// Add an event to the collection.
        /// </summary>
        /// <param name="evt">The event to add.</param>
        public void AddEvent(MidiEvent evt)
        {
            // Capture that this is a valid channel. Patch will get fixed later.
            SetChannelPatch(evt.Channel, -1);

            // Cache channel note info.
            if (evt is NoteOnEvent)
            {
                _hasNotes.Add(evt.Channel);
            }

            // Scale time and add to collections.
            _events.Add(evt); // all

            //int scTime = _mt!.MidiToInternal(evt.AbsoluteTime, true); 
            //_eventsByTime.Add(scTime, evt);

            _maxTick = Math.Max(_maxTick, evt.AbsoluteTime);
        }

        /// <summary>
        /// Get enumerator for events using supplied filters.
        /// </summary>
        /// <param name="channelNumbers">Specific channnels.</param>
        /// <returns>Enumerator sorted by absolute time.</returns>
        public IEnumerable<MidiEvent> GetFilteredEvents(IEnumerable<int> channelNumbers)
        {
            IEnumerable<MidiEvent> descs = _events.Where(e => channelNumbers.Contains(e.Channel)) ?? [];
            return descs.OrderBy(e => e.AbsoluteTime);
        }

        /// <summary>
        /// Get all events at a specific scaled time. <<<<<<<<<<<<<<<????????????????
        /// </summary>
        /// <param name="when"></param>
        /// <returns></returns>
        public IEnumerable<MidiEvent> GetEventsWhen(int when)
        {
            List<MidiEvent> evts = [];// _eventsByTime.ContainsKey(when) ? _eventsByTime[when] : [];
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
            List<(int chnum, int patch)> ps = [];
            // Assemble results from filters.
            bool any = hasNotes ? _events.Where(e => e is NoteOnEvent).Any() : _events.Any();
            if(any)
            {
                _channelPatches
                    .Where(n => !hasPatch || n.Value != -1)
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
            return _channelPatches.TryGetValue(channel, out int value) ? value : -1;
        }

        /// <summary>
        /// Remove a channel from the channel/patches collection.
        /// </summary>
        /// <param name="channel"></param>
        public void RemoveChannel(int channel)
        {
            _channelPatches.Remove(channel);
        }

        /// <summary>
        /// Safely add/update info.
        /// </summary>
        /// <param name="channel">The channel number</param>
        /// <param name="patch">The patch. Can be default -1.</param>
        public void SetChannelPatch(int channel, int patch)
        {
            if (!_channelPatches.TryAdd(channel, patch))
            {
                if (patch != -1)
                {
                    _channelPatches[channel] = patch;
                }
            }
        }

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var pname = Name == "" ? "nameless" : Name;
            var s = $"{pname}";
            //var s = $"{pname} tempo:{Tempo} timesig:{TimeSignature} channels:{_channelPatches.Count}";
            //ValidPatches.ForEach(p => content.Add($"Ch:{p.Key} Patch:{MidiDefs.GetInstrumentName(p.Value)}"));

            return s;
        }
    }
}