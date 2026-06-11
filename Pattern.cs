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
    public class Pattern
    {
        #region Fields
        /// <summary>For scaling midi ticks to internal.</summary>
        readonly MidiTimeConverter _mt;

        ///// <summary>Max length of all sequences in midi ticks.</summary>
        //long _maxTick = 0;
        #endregion

        #region Properties
        /// <summary>Pattern name. Empty indicates single pattern aka plain midi file.</summary>
        public string Name { get; init; } = "";

        /// <summary>All the tracks.</summary>
        public List<Track> Tracks { get; init; } = [];

        // /// <summary>Length of all sequences in scaled/internal time.</summary>
        // public int Length { get { return _mt.MidiToInternal(_maxTick, true); } }
        #endregion

        /// <summary>
        /// Normal constructor.
        /// </summary>
        /// <param name="name">Pattern name</param>
        /// <param name="ppq">Resolution</param>
        public Pattern(string name, int ppq)
        {
            Name = name;
            _mt = new(ppq);
        }

        public void AddTrack(Track trk)
        {
            Tracks.Add(trk);
        }

        /// <summary>
        /// Readable version.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var pname = Name == "" ? "noname" : Name;
            var s = $"{pname}";
            //var s = $"{pname} tempo:{Tempo} timesig:{TimeSignature} channels:{_channelPatches.Count}";
            //ValidPatches.ForEach(p => content.Add($"Ch:{p.Key} Patch:{MidiDefs.GetInstrumentName(p.Value)}"));

            return s;
        }
    }
}