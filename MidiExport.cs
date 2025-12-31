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
using Ephemera.MusicLib;


namespace Ephemera.MidiLibEx
{
    /// <summary>
    /// Writes to various output formats.
    /// </summary>
    public class MidiExport
    {
        /// <summary>
        /// Export the contents in a csv readable form. This is as the events appear in the original file.
        /// </summary>
        /// <param name="outFileName">Where to boss?</param>
        /// <param name="patterns">Specific patterns.</param>
        /// <param name="channelNumbers">Specific channnel numbers.</param>
        /// <param name="drumChannelNumbers">Drum channel numbers.</param>
        /// <param name="global">File meta data to include.</param>
        public static void ExportCsv(string outFileName, IEnumerable<PatternInfo> patterns,
            List<int> channelNumbers, List<int> drumChannelNumbers, Dictionary<string, int> global)
        {
            // Init output file contents.
            int ppq = global["DeltaTicksPerQuarterNote"];

            // Header
            List<string> contentText = new()
            {
                "ScaledTime,AbsoluteTime,DeltaTime,Event,Channel,Content1,Content2"
            };

            // Any globals.
            global.ForEach(m => contentText.Add($"-1,0,0,Global,0,{m.Key}:{m.Value},"));

            // Midi events.
            foreach (PatternInfo pi in patterns)
            {
                MidiTimeConverter mt = new(ppq, pi.Tempo);

                contentText.Add($"0,0,0,Pattern,0,{pi.PatternName},tempo:{pi.Tempo}");
                contentText.Add($"0,0,0,Pattern,0,{pi.PatternName},timesig:{pi.TimeSignature}");

                pi.GetChannels(false, false).ForEach(p =>
                {
                    var pname = drumChannelNumbers.Contains(p.chnum) ?
                        MidiDefs.Instance.GetDrumKitName(p.patch) :
                        MidiDefs.Instance.GetInstrumentName(p.patch);
                    contentText.Add($"0,0,0,Patch,{p.chnum}:{pname},,");
                });

                var events = pi.GetFilteredEvents(channelNumbers);
                foreach (var mevt in events)
                {
                    string ret = "???";

                    // Boilerplate.
                    string ntype = mevt.CommandCode == MidiCommandCode.MetaEvent ? (mevt as MetaEvent)!.MetaEventType.ToString() : mevt.CommandCode.ToString();
                    string sc = $"{mevt.AbsoluteTime},{mt.InternalToMidi((int)mevt.AbsoluteTime)},{mevt.DeltaTime},{ntype},{mevt.Channel}";

                    bool isDrums = drumChannelNumbers.Contains(mevt.Channel);

                    string NoteName(int nnum)
                    {
                        return isDrums ? MidiDefs.Instance.GetDrumName(nnum) : MusicDefs.Instance.NoteNumberToName(nnum);
                    }

                    string PatchName(int pnum)
                    {
                        return isDrums ? MidiDefs.Instance.GetDrumKitName(pnum) : MidiDefs.Instance.GetInstrumentName(pnum);
                    }

                    switch (mevt)
                    {
                        case NoteOnEvent evt:
                            //string slen = evt.OffEvent is null ? "?" : evt.NoteLength.ToString(); // NAudio NoteLength bug?
                            ret = $"{sc},{evt.NoteNumber}:{NoteName(evt.NoteNumber)},vel:{evt.Velocity}";
                            break;

                        case NoteEvent evt: // used for NoteOff
                            ret = $"{sc},{evt.NoteNumber}:{NoteName(evt.NoteNumber)},";
                            break;

                        case TempoEvent evt:
                            ret = $"{sc},tempo:{evt.Tempo},mspqn:{evt.MicrosecondsPerQuarterNote}";
                            break;

                        case TimeSignatureEvent evt:
                            ret = $"{sc},timesig:{evt.TimeSignature},";
                            break;

                        case KeySignatureEvent evt:
                            ret = $"{sc},sharpsflats:{evt.SharpsFlats},majorminor:{evt.MajorMinor}";
                            break;

                        case PatchChangeEvent evt:
                            ret = $"{sc},{evt.Patch}:{PatchName(evt.Patch)},";
                            break;

                        case ControlChangeEvent evt:
                            ret = $"{sc},{(int)evt.Controller}:{MidiDefs.Instance.GetControllerName((int)evt.Controller)},value:{evt.ControllerValue}";
                            break;

                        case PitchWheelChangeEvent evt:
                            //ret = $"{sc},pitch:{evt.Pitch},"; too busy?
                            break;

                        case TextEvent evt:
                            ret = $"{sc},text:{evt.Text},datalen:{evt.Data.Length}";
                            break;

                        case TrackSequenceNumberEvent evt:
                            ret = $"{sc},seq:{evt},";
                            break;

                        //Others as needed:
                        //case ChannelAfterTouchEvent:
                        //case SysexEvent:
                        //case MetaEvent:
                        //case RawMetaEvent:
                        //case SequencerSpecificEvent:
                        //case SmpteOffsetEvent:
                        default:
                            ret = $"{sc},other:???,,";
                            break;
                    }
                    contentText.Add(ret);
                }
            }

            File.WriteAllLines(outFileName, contentText);
        }

        /// <summary>
        /// Export pattern parts to individual midi files. This is as the events appear in the original file.
        /// </summary>
        /// <param name="outFileName">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channelNumbers">Specific channnel numbers.</param>
        /// <param name="global">File meta data to include.</param>
        public static void ExportMidi(string outFileName, PatternInfo pattern, List<int> channelNumbers, Dictionary<string, int> global)
        {
            // Init output file contents.
            int ppq = global["DeltaTicksPerQuarterNote"];
            MidiEventCollection outColl = new(1, ppq);
            IList<MidiEvent> outEvents = outColl.AddTrack();

            // Build the event collection.
            outEvents.Add(new TempoEvent(0, 0) { Tempo = pattern.Tempo });
            outEvents.Add(new TextEvent($"Export {pattern.PatternName}", MetaEventType.TextEvent, 0));

            if (pattern.TimeSignature == (0, 0))
            {
                outEvents.Add(new TimeSignatureEvent(0, pattern.TimeSignature.num, pattern.TimeSignature.denom, 24, 8));
            }

            // Patches.
            pattern.GetChannels(true, true).ForEach(p => { outEvents.Add(new PatchChangeEvent(0, p.chnum, p.patch)); });

            // Gather the midi events for the pattern ordered by time.
            var events = pattern.GetFilteredEvents(channelNumbers);
            events?.ForEach(e => { outEvents.Add(e); });

            // Add end track.
            long ltime = outEvents.Last().AbsoluteTime;
            var endt = new MetaEvent(MetaEventType.EndTrack, 0, ltime);
            outEvents.Add(endt);

            // Use NAudio function to create out file.
            MidiFile.Export(outFileName, outColl);
        }
    }
}
