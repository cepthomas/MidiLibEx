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

        ///// <summary>Max length of all sequences in midi ticks.</summary>
        //long _maxTick = 0;
        #endregion

        #region Properties
        /// <summary>Pattern name. Empty indicates single pattern aka plain midi file.</summary>
        public string Name { get; set; } = "";

        ///// <summary>Tempo, if supplied by file. Default indicates invalid which will be filled in during read.</summary>
        //public int Tempo { get; set; } = 0;

        ///// <summary>Time signature, if supplied by file.</summary>
        //public (int num, int denom) TimeSignature { get; set; } = new();

        /// <summary>All the tracks in the pattern.</summary>
        public List<Track> Tracks { get; set; } = [];

        #endregion

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