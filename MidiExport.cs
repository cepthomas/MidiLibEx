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
    /// Writes to various output formats.
    /// </summary>
    public class MidiExport
    {
        /// <summary>
        /// Export the contents in a csv readable form. This is as the events appear in the original file.
        /// </summary>
        /// <param name="fn">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels.</param>
        /// <param name="meta">File meta data to include.</param>
        public static void ExportCsv(string fn, PatternInfo pattern, List<int> channels, Dictionary<string, int> meta)
        {
            // Collect output text.
            List<string> contentText = ["AbsoluteTime,DeltaTime,Event,Channel,Content1,Content2"];

            // Any globals. TODO1 client puts patch info in meta??
            meta.ForEach(m => contentText.Add($"0,0,Global,0,{m.Key}:{m.Value},"));

            // // Selections.
            // List<int> channelNumbers = [.. channels.Select(cc => cc.ChannelNumber)];

            // Midi events.
            var pname = pattern.PatternName == "" ? "NoName" : pattern.PatternName;
            contentText.Add($"0,0,Pattern,0,name:{pname},tempo:{pattern.Tempo}");
            contentText.Add($"0,0,Pattern,0,name:{pname},timesig:{pattern.TimeSignature}");

            // channels.ForEach(ch => { contentText.Add($"0,0,Patch,0,{ch.Patch},{ch.PatchName}"); });

            foreach (var mevt in pattern.GetFilteredEvents(channels))
            {
                // Boilerplate.
                List<object> parts =
                [
                    mevt.AbsoluteTime,
                    mevt.DeltaTime,
                    mevt.CommandCode == MidiCommandCode.MetaEvent ? (mevt as MetaEvent)!.MetaEventType : mevt.CommandCode,
                    mevt.Channel
                ];

                switch (mevt)
                {
                    case NoteOnEvent evt: parts.AddRange([evt.NoteNumber, evt.Velocity]); break;
                    case NoteEvent evt: parts.AddRange([evt.NoteNumber, ""]); break; // used for NoteOff
                    case TempoEvent evt: parts.AddRange([evt.Tempo, evt.MicrosecondsPerQuarterNote]); break;
                    case TimeSignatureEvent evt: parts.AddRange([evt.TimeSignature, ""]); break;
                    case KeySignatureEvent evt: parts.AddRange([evt.SharpsFlats, evt.MajorMinor]); break;
                    case PatchChangeEvent evt: parts.AddRange([evt.Patch, "???"]); break; // TODO1 get patch name from channel
                    case ControlChangeEvent evt: parts.AddRange([$"{(int)evt.Controller}:{MidiDefs.Controllers.GetName((int)evt.Controller)}", $"value:{evt.ControllerValue}"]); break;
                    case PitchWheelChangeEvent evt: /*parts.AddRange([evt.Pitch, ""]);*/ break;
                    case TextEvent evt: parts.AddRange([evt.Text, evt.Data.Length]); break;
                    case TrackSequenceNumberEvent evt: parts.AddRange([evt, ""]); break;
                    //Others as needed:
                    //case ChannelAfterTouchEvent:
                    //case SysexEvent:
                    //case MetaEvent:
                    //case RawMetaEvent:
                    //case SequencerSpecificEvent:
                    //case SmpteOffsetEvent:
                    default: parts.AddRange(["other", ""]); break;
                }
                var sparts = string.Join(",", parts);
                contentText.Add(sparts);
            }

            File.WriteAllLines(fn, contentText);
        }

        /// <summary>
        /// Export pattern parts to individual midi files. This is as the events appear in the original file.
        /// </summary>
        /// <param name="fn">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels.</param>
        /// <param name="meta">File meta data to include.</param>
        public static void ExportMidi(string fn, PatternInfo pattern, List<int> channels, Dictionary<string, int> meta)
        {
            // Init output file contents.
            int ppq = meta["DeltaTicksPerQuarterNote"];
            MidiEventCollection outColl = new(1, ppq);
            IList<MidiEvent> outEvents = outColl.AddTrack();

            // List<int> channelNumbers = [.. channels.Select(cc => cc.ChannelNumber)];

            // Build the event collection.
            outEvents.Add(new TempoEvent(0, 0) { Tempo = pattern.Tempo });
            outEvents.Add(new TextEvent($"Export {pattern.PatternName}", MetaEventType.TextEvent, 0));

            if (pattern.TimeSignature != (0, 0))
            {
                outEvents.Add(new TimeSignatureEvent(0, pattern.TimeSignature.num, pattern.TimeSignature.denom, 24, 8));
            }

            // // Patches.
            // pattern.GetChannels(true, true).ForEach(p => { outEvents.Add(new PatchChangeEvent(0, p.chnum, p.patch)); });

            // Gather the midi events for the pattern ordered by time.
            var events = pattern.GetFilteredEvents(channels);
            events?.ForEach(e => { outEvents.Add(e); });

            // Add end track.
            long ltime = outEvents.Last().AbsoluteTime;
            var endt = new MetaEvent(MetaEventType.EndTrack, 0, ltime);
            outEvents.Add(endt);

            // Use NAudio function to create out file.
            MidiFile.Export(fn, outColl);
        }

        /// <summary>
        /// Export the contents as a text piano roll.
        /// </summary>
        /// <param name="fn">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels.</param>
        /// <param name="meta">File meta data to include.</param>
        public static void ExportPianoRoll(string fn, PatternInfo pattern, List<int> channels, Dictionary<string, int> meta)
        {

            // /// Get all events at a specific scaled time.
            // public IEnumerable<MidiEvent> GetEventsWhen(int when)
            // {
            //     List<MidiEvent> evts = _eventsByTime.ContainsKey(when) ? _eventsByTime[when] : [];
            //     return evts;
            // }






            /***********************************************

            local example_seq =
            {
                -- | beat 0 | beat 1 | beat 2 | beat 3 | beat 4 | beat 5 | beat 6 | beat 7 |,  WHAT_TO_PLAY
                -- |........|........|........|........|........|........|........|........|
                { "|6-------|--      |        |        |7-------|--      |        |        |", "G4.m7" },
                { "|7-------|--      |        |        |7-------|--      |        |        |",  84 },
                { "|        |        |        |5---    |        |        |        |5-8---  |", "D6" },
            }

            local drums_verse =
            {
                -- |........|........|........|........|........|........|........|........|
                { "|8       |        |8       |        |8       |        |8       |        |", bdrum },
                { "|    8   |        |    8   |    8   |    8   |        |    8   |    8   |", snare },
                { "|        |     8 8|        |     8 8|        |     8 8|        |     8 8|", hhcl }
            }

            ===>>>

            -- channel music1 i.e.  G4.m7
            -- | beat 0 | beat 1 | beat 2 | beat 3 | beat 4 | beat 5 | beat 6 | beat 7 |
            41 |........|..      |        |        |........|..      |        |        | 
            42 |........|..      |        |        |........|..      |        |        | 
            43 |........|..      |        |        |........|..      |        |        | 
            44 |........|..      |        |        |........|..      |        |        | 


            -- channel drums
            10 |.       |        |.       |        |.       |        |.       |        |
            11 |    .   |        |    .   |    .   |    .   |        |    .   |    .   |
            12 |        |     . .|        |     . .|        |     . .|        |     . .|




AbsoluteTime,DeltaTime,Event ,Channel,Content1,Content2
1152        ,0        ,NoteOn,3      ,66      ,91
1536        ,1536     ,NoteOn,2      ,62      ,100
1536        ,0        ,NoteOn,2      ,59      ,100
1536        ,0        ,NoteOn,2      ,66      ,100
3072        ,1536     ,NoteOn,2      ,66      ,0
3072        ,0        ,NoteOn,2      ,59      ,0
3072        ,0        ,NoteOn,2      ,62      ,0
3072        ,0        ,NoteOn,2      ,64      ,100
3072        ,0        ,NoteOn,2      ,61      ,100
3072        ,0        ,NoteOn,2      ,57      ,100
4032        ,820      ,NoteOn,3      ,66      ,0
4224        ,0        ,NoteOn,3      ,66      ,91
4608        ,1536     ,NoteOn,2      ,57      ,0
4608        ,0        ,NoteOn,2      ,61      ,0
4608        ,0        ,NoteOn,2      ,64      ,0
4608        ,0        ,NoteOn,2      ,59      ,100
4608        ,0        ,NoteOn,2      ,52      ,100
4608        ,0        ,NoteOn,2      ,56      ,100
7296        ,0        ,NoteOn,3      ,66      ,91
7680        ,3072     ,NoteOn,2      ,56      ,0
7680        ,0        ,NoteOn,2      ,52      ,0
7680        ,0        ,NoteOn,2      ,59      ,0
7680        ,0        ,NoteOn,2      ,62      ,100
7680        ,0        ,NoteOn,2      ,59      ,100
7680        ,0        ,NoteOn,2      ,66      ,100
7680        ,4        ,NoteOn,3      ,66      ,0


            */



            // // Selections.
            // List<int> channelNumbers = [.. channels.Select(cc => cc.ChannelNumber)];
            // //channels.ForEach(ch => { contentText.Add($"0,0,Patch,0,{ch.Patch},{ch.PatchName}"); });

            List<string> contentText = [];


            // Midi events.
            foreach (var mevt in pattern.GetFilteredEvents(channels))
            {
                // Boilerplate.
                List<object> parts =
                [
                    mevt.AbsoluteTime,
                        mevt.DeltaTime,
                        mevt.CommandCode == MidiCommandCode.MetaEvent ? (mevt as MetaEvent)!.MetaEventType : mevt.CommandCode,
                        mevt.Channel
                ];

                switch (mevt)
                {
                    case NoteOnEvent evt: parts.AddRange([evt.NoteNumber, evt.Velocity]); break;
                    case NoteEvent evt: parts.AddRange([evt.NoteNumber, ""]); break; // used for NoteOff
                    case TempoEvent evt: parts.AddRange([evt.Tempo, evt.MicrosecondsPerQuarterNote]); break;
                    case TimeSignatureEvent evt: parts.AddRange([evt.TimeSignature, ""]); break;
                    case KeySignatureEvent evt: parts.AddRange([evt.SharpsFlats, evt.MajorMinor]); break;
                    case PatchChangeEvent evt: parts.AddRange([evt.Patch, "???"]); break; // TODO1 get patch name from channel
                    case ControlChangeEvent evt: parts.AddRange([$"{(int)evt.Controller}:{MidiDefs.Controllers.GetName((int)evt.Controller)}", $"value:{evt.ControllerValue}"]); break;
                    case PitchWheelChangeEvent evt: /*parts.AddRange([evt.Pitch, ""]);*/ break;
                    case TextEvent evt: parts.AddRange([evt.Text, evt.Data.Length]); break;
                    case TrackSequenceNumberEvent evt: parts.AddRange([evt, ""]); break;
                    //Others as needed:
                    //case ChannelAfterTouchEvent:
                    //case SysexEvent:
                    //case MetaEvent:
                    //case RawMetaEvent:
                    //case SequencerSpecificEvent:
                    //case SmpteOffsetEvent:
                    default: parts.AddRange(["other", ""]); break;
                }
                var sparts = string.Join(",", parts);
                contentText.Add(sparts);
            }

            File.WriteAllLines(fn, contentText);
        }
    }
}
