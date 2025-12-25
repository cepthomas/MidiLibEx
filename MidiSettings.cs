using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using NAudio.Midi;
using Ephemera.NBagOfTricks;
using Ephemera.MidiLib;


namespace Ephemera.MidiLibEx
{
    [Serializable]
    public class MidiSettings
    {
        /// <summary>Current midi settings. Client must set this before accessing!</summary>
        [Browsable(false)]
        public static MidiSettings LibSettings
        {
            get { if (_settings is null) throw new InvalidOperationException("Client must set this property before accessing"); return _settings; }
            set { _settings = value; }
        }
        static MidiSettings? _settings = null;

        #region Properties - persisted editable
        [DisplayName("Input Devices")]
        [Description("Valid devices if handling input.")]
        [Browsable(true)]
//TODO1        [Editor(typeof(DevicesTypeEditor), typeof(UITypeEditor))]
        public List<DeviceSpec> InputDevices { get; set; } = new();

        [DisplayName("Output Devices")]
        [Description("Valid devices if sending output.")]
        [Browsable(true)]
//TODO1        [Editor(typeof(DevicesTypeEditor), typeof(UITypeEditor))]
        public List<DeviceSpec> OutputDevices { get; set; } = new();

        [DisplayName("Default Tempo")]
        [Description("Use this tempo if it's not in the file.")]
        [Browsable(true)]
        public int DefaultTempo { get; set; } = 100;

        /// <summary>How to snap.</summary>
        [DisplayName("Snap Type")]
        [Description("How to snap to grid.")]
        [Browsable(true)]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SnapType Snap { get; set; } = SnapType.Beat;

        [DisplayName("Internal Time Resolution")]
        [Description("aka DeltaTicksPerQuarterNote or subdivisions per beat.")]
        [Browsable(false)] // TODO Implement user selectable later maybe.
        [JsonIgnore()]
        public int InternalPPQ { get; set; } = 32;
        #endregion

        #region Properties - internal
        /// <summary>Only 4/4 time supported.</summary>
        [Browsable(false)]
        [JsonIgnore()]
        public int BeatsPerBar { get { return 4; } }

        /// <summary>Convenience.</summary>
        [Browsable(false)]
        [JsonIgnore()]
        public int SubsPerBeat { get { return InternalPPQ; } }

        /// <summary>Convenience.</summary>
        [Browsable(false)]
        [JsonIgnore()]
        public int SubsPerBar { get { return InternalPPQ * BeatsPerBar; } }
        #endregion
    }

    [Serializable]
    public class DeviceSpec
    {
        [DisplayName("Device Id")]
        [Description("User supplied id for use in client.")]
        [Browsable(true)]
        public string DeviceId { get; set; } = "";

        [DisplayName("Device Name")]
        [Description("System device name.")]
        [Browsable(true)]
        public string DeviceName { get; set; } = "";
    }
}
