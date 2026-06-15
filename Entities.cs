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
    /// The contents of a midi file pattern.
    /// </summary>
    /// <remarks>
    /// Normal constructor.
    /// </remarks>
    /// <param name="ppq">Resolution</param>
    public class Pattern(int ppq)
    {
        #region Fields
        /// <summary>For scaling midi ticks to internal.</summary>
        readonly MidiTimeConverter _mt = new(ppq);
        #endregion

        #region Properties
        /// <summary>Pattern name. Empty indicates single pattern aka plain midi file.</summary>
        public string Name { get; set; } = MidiDataFile.UNNAMED;

        /// <summary>All the tracks in the pattern.</summary>
        public List<Track> Tracks { get; set; } = [];
        #endregion

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{Name} Tracks: {Tracks.Count}";
        }
    }

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
        public string Name { get; set; } = MidiDataFile.UNNAMED;

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

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var s = $"{Name} Events:{_events.Count}";
            return s;
        }
    }

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
}