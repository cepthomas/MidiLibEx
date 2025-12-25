using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Midi;


namespace Ephemera.MidiLibEx
{
    #region Enums
    // /// <summary>Channel state.</summary>
    // public enum ChannelState { Normal = 0, Solo = 1, Mute = 2 }

    // /// <summary>User selection options.</summary>
    // public enum SnapType { Bar, Beat, Sub }
    #endregion

    #region Definitions
    public class MidiLibDefs
    {
        /// <summary>Supported file types.</summary>
        public const string MIDI_FILE_TYPES = "*.mid";

        /// <summary>Supported file types.</summary>
        public const string STYLE_FILE_TYPES = "*.sty;*.pcs;*.sst;*.prs";


        //  TODO1 dupes:
        ///// <summary>Default value.</summary>
        //public const double DEFAULT_VOLUME = 0.8;

        ///// <summary>Allow UI controls some more headroom.</summary>
        //public const double MAX_VOLUME = 2.0;
    }
    #endregion

    // #region Special internal types
    // /// <summary>Custom default type to avoid handling null everywhere.</summary>
    // public class NullMidiEvent : MidiEvent
    // {
    //     /// <summary>Constructor.</summary>
    //     public NullMidiEvent() : base(0, 0, MidiCommandCode.MetaEvent)
    //     {
    //     }

    //     public override string ToString()
    //     {
    //         return $"NullMidiEvent: {base.ToString()}";
    //     }
    // }

    // /// <summary>Custom type to support runtime functions.</summary>
    // public class FunctionMidiEvent : MidiEvent
    // {
    //     /// <summary>The function to call.</summary>
    //     public Action ScriptFunction { get; init; }

    //     /// <summary>
    //     /// Single constructor.
    //     /// </summary>
    //     /// <param name="time"></param>
    //     /// <param name="channel"></param>
    //     /// <param name="scriptFunc"></param>
    //     public FunctionMidiEvent(int time, int channel, Action scriptFunc) : base(time, channel, MidiCommandCode.MetaEvent)
    //     {
    //         ScriptFunction = scriptFunc;
    //     }

    //     /// <summary>For viewing pleasure.</summary>
    //     public override string ToString()
    //     {
    //         return $"FunctionMidiEvent: {base.ToString()} function:{ScriptFunction}";
    //     }
    // }

    // /// <summary>Sink that doesn't do anything.</summary>
    // public sealed class NullOutputDevice : IOutputDevice
    // {
    //     public string DeviceName => "NullOutputDevice";
    //     public bool Valid { get { return false; } }
    //     public bool LogEnable { get; set; }
    //     public void Dispose() { }
    //     public void SendEvent(MidiEvent evt) { }
    // }
    // #endregion

    // #region Event args
    // /// <summary>Notify host of asynchronous changes from user.</summary>
    // public class ChannelChangeEventArgs : EventArgs
    // {
    //     public bool PatchChange { get; set; } = false;
    //     public bool StateChange { get; set; } = false;
    //     public bool ChannelNumberChange { get; set; } = false;
    // }

    // /// <summary>
    // /// Midi (real or sim) has received something. It's up to the client to make sense of it.
    // /// Property value of -1 indicates invalid or not pertinent e.g a controller event doesn't have velocity.
    // /// </summary>
    // public class InputReceiveEventArgs : EventArgs
    // {
    //     /// <summary>Channel number 1-based. Required.</summary>
    //     public int Channel { get; set; } = 0;

    //     /// <summary>The note number to play. NoteOn/Off only.</summary>
    //     public int Note { get; set; } = -1;

    //     /// <summary>Specific controller id.</summary>
    //     public int Controller { get; set; } = -1;

    //     /// <summary>For Note = velocity. For controller = payload.</summary>
    //     public int Value { get; set; } = -1;

    //     /// <summary>Something to tell the client.</summary>
    //     public string ErrorInfo { get; set; } = "";

    //     /// <summary>Special controller id to carry pitch info.</summary>
    //     public const int PITCH_CONTROL = 1000;

    //     /// <summary>Read me.</summary>
    //     public override string ToString()
    //     {
    //         StringBuilder sb = new($"Channel:{Channel} ");

    //         if (ErrorInfo != "")
    //         {
    //             sb.Append($"Error:{ErrorInfo} ");
    //         }
    //         else
    //         {
    //             sb.Append($"Channel:{Channel} Note:{Note} Controller:{Controller} Value:{Value}");
    //         }

    //         return sb.ToString();
    //     }
    // }
    // #endregion
}
