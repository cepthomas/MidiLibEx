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
        /// <summary>All the track midi events.</summary>
        readonly List<MidiEvent> _events = [];

        /// <summary>Max length of all sequences in midi ticks.</summary>
        long _maxTick = 0;
        #endregion

        #region Properties
        /// <summary>Track name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Standard events - not meta.</summary>
        public int NumStandard { get; private set; } = 0;

        /// <summary>Channels and patches in this track.</summary>
        public ChannelState[] ChannelStates { get; set; } = new ChannelState[MidiDefs.NUM_CHANNELS];
        public record struct ChannelState(bool HasNotes, int Patch);
        #endregion



        /// <summary>
        /// Standard constructor.
        /// </summary>
        public Track()
        {
            ChannelStates.ForEach(state => { state.HasNotes = false; state.Patch = -1; });
        }

        /// <summary>
        /// Add an event to the collection.
        /// </summary>
        /// <param name="evt">The event to add.</param>
        public void AddEvent(MidiEvent evt)
        {
            if (evt is not MetaEvent)
            {
                NumStandard++;
            }
            
            // Cache channel note info.
            if (evt is NoteOnEvent)
            {
                ChannelStates[evt.Channel - 1].HasNotes = true;
            }

            // Scale time and add to collections.
            _events.Add(evt); // all

            //int scTime = _mt!.MidiToInternal(evt.AbsoluteTime, true); 
            //_eventsByTime.Add(scTime, evt);

            _maxTick = Math.Max(_maxTick, evt.AbsoluteTime);
        }

        /// <summary>
        /// Get events using supplied filters.
        /// </summary>
        /// <param name="channelNumbers">Specific channnels.</param>
        /// <returns>Enumerator sorted by absolute time.</returns>
        public IEnumerable<MidiEvent> GetFilteredEvents(IEnumerable<int> channelNumbers)
        {
            IEnumerable<MidiEvent> descs = _events.Where(e => channelNumbers.Contains(e.Channel)) ?? [];
            return descs.OrderBy(e => e.AbsoluteTime);
        }

        /// <summary>
        /// Safely add/update info.
        /// </summary>
        /// <param name="channel">The channel number</param>
        /// <param name="patch">The patch. Can be default -1.</param>
        public void SetPatch(int channel, int patch)
        {
            ChannelStates[channel - 1].Patch = patch;
        }

        ///// <summary>
        ///// Get all events at a specific scaled time. TODO1 - client?
        ///// </summary>
        ///// <param name="when"></param>
        ///// <returns></returns>
        //public IEnumerable<MidiEvent> GetEventsWhen_X(int when)
        //{
        //    List<MidiEvent> evts = [];// _eventsByTime.ContainsKey(when) ? _eventsByTime[when] : [];
        //    return evts;
        //}

        ///// <summary>
        ///// Get an ordered list of channels and their patches. TODO1
        ///// </summary>
        ///// <param name="hasNotes">Must have noteons.</param>
        ///// <param name="hasPatch">Must have valid patch.</param>
        ///// <returns></returns>
        //public IEnumerable<(int chnum, int patch)> GetChannels_X(bool hasNotes, bool hasPatch)
        //{
        //    List<(int chnum, int patch)> ps = [];
        //    // Assemble results from filters.
        //    bool any = hasNotes ? _events.Where(e => e is NoteOnEvent).Any() : _events.Any();
        //    if (any)
        //    {
        //        _channelPatches_X
        //            .Where(n => !hasPatch || n.Value != -1)
        //            .Where(n => _channelsWithNotes.Contains(n.Key))
        //            .OrderBy(n => n.Key)
        //            .ForEach(n => { ps.Add((n.Key, n.Value)); });
        //    }

        //    return ps;
        //}

        ///// <summary>
        ///// Get the patch associated with the channel.
        ///// </summary>
        ///// <param name="channel"></param>
        ///// <returns>The patch or -1 if invalid channel</returns>
        //public int GetPatch_X(int channel)
        //{
        //    return _channelPatches_X.TryGetValue(channel, out int value) ? value : -1;
        //}

        ///// <summary>
        ///// Remove a channel from the channel/patches collection.
        ///// </summary>
        ///// <param name="channel"></param>
        //public void RemoveChannel_X(int channel)
        //{
        //    _channelPatches_X.Remove(channel);
        //}

        ///// <summary>
        ///// Safely add/update info.
        ///// </summary>
        ///// <param name="channel">The channel number</param>
        ///// <param name="patch">The patch. Can be default -1.</param>
        //public void SetChannelPatch_X(int channel, int patch)
        //{
        //    if (!_channelPatches_X.TryAdd(channel, patch))
        //    {
        //        if (patch != -1)
        //        {
        //            _channelPatches_X[channel] = patch;
        //        }
        //    }
        //}

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var pname = Name;// == "" ? "nameless" : Name;
            var s = $"{pname}";
            //var s = $"{pname} tempo:{Tempo} timesig:{TimeSignature} channels:{_channelPatches.Count}";
            //ValidPatches.ForEach(p => content.Add($"Ch:{p.Key} Patch:{MidiDefs.GetInstrumentName(p.Value)}"));

            return s;
        }
    }
}