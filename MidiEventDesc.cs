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
    /// Internal representation of one midi event.
    /// </summary>
    public class MidiEventDesc
    {
        /// <summary>One-based channel number.</summary>
        public int ChannelNumber { get { return RawEvent.Channel; } }

        /// <summary>Associated channel name.</summary>
        public string ChannelName { get; }

        /// <summary>Time (ticks) from original file.</summary>
        public long AbsoluteTime { get { return RawEvent.AbsoluteTime; } }

        /// <summary>Time (ticks) scaled to internal units using send PPQ.</summary>
        public int ScaledTime { get; set; } = -1;

        /// <summary>The raw midi event.</summary>
        public MidiEvent RawEvent { get; init; }

        /// <summary>Normal constructor from NAudio event.</summary>
        public MidiEventDesc(MidiEvent evt, string channelName)
        {
            RawEvent = evt;
            ChannelName = channelName;
        }

        /// <summary>Read me.</summary>
        public override string ToString()
        {
            string ntype = RawEvent.CommandCode == MidiCommandCode.MetaEvent ? (RawEvent as MetaEvent)!.MetaEventType.ToString() : RawEvent.CommandCode.ToString();
            string ret = $"Ch {ChannelNumber}:{ChannelName} TAbs:{AbsoluteTime} TScld:{ScaledTime} Event:{RawEvent}";
            return ret;
        }
    }
}
