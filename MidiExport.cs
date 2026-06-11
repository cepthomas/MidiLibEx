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
        /// <param name="header">File header data to include.</param>
        public static void ExportCsv(string fn, Pattern pattern, List<int> channels, Header header)
        {
            // Collect output text.
            List<string> contentText =
            [
                $"AbsoluteTime,DeltaTime,Event,Channel,Content1,Content2",
                $"0,0,Header,0,MidiFileType,{header.MidiFileType}",
                $"0,0,Header,0,NumTracks,{header.NumTracks}",
                $"0,0,Header,0,DeltaTicksPerQuarterNote,{header.DeltaTicksPerQuarterNote}"
            ];

            // Midi events.
            for (int i = 0; i < pattern.Tracks.Count; i++)
            {
                var track = pattern.Tracks[i];

                // Don't include only-meta tracks except first.
                if (track.NumStandard == 0 && i != 0) { continue; }

                foreach (var mevt in track.GetFilteredEvents(channels))
                {
                    // Boilerplate.
                    var isMeta = mevt.CommandCode == MidiCommandCode.MetaEvent;
                    List<object> parts =
                    [
                        mevt.AbsoluteTime,
                        mevt.DeltaTime,
                        isMeta ? (mevt as MetaEvent)!.MetaEventType : mevt.CommandCode,
                        isMeta ? 0 : mevt.Channel
                    ];

                    switch (mevt)
                    {
                        case NoteOnEvent evt: parts.AddRange([evt.NoteNumber, evt.Velocity]); break;
                        case NoteEvent evt: parts.AddRange([evt.NoteNumber, ""]); break;
                        case TempoEvent evt: parts.AddRange([evt.Tempo, evt.MicrosecondsPerQuarterNote]); break;
                        case TimeSignatureEvent evt: parts.AddRange([evt.TimeSignature, ""]); break;
                        case KeySignatureEvent evt: parts.AddRange([evt.SharpsFlats, evt.MajorMinor]); break;
                        case PatchChangeEvent evt: parts.AddRange([evt.Patch, MidiDefs.Instruments.GetName(evt.Patch)]); break; // TODO doesn't handle alternate banks
                        case ControlChangeEvent evt: parts.AddRange([evt.Controller, MidiDefs.Controllers.GetName((int)evt.Controller)]); break;
                        case PitchWheelChangeEvent evt: /*parts.AddRange([evt.Pitch, ""]);*/ break;
                        case TextEvent evt: parts.AddRange([evt.Text, ""]); break;
                        // Others as needed: TrackSequenceNumberEvent, ChannelAfterTouchEvent, SysexEvent,
                        // MetaEvent, RawMetaEvent, SequencerSpecificEvent, SmpteOffsetEvent
                        default: parts.AddRange(["", ""]); break;
                    }
                    var sparts = string.Join(",", parts);
                    contentText.Add(sparts);
                }
            }

            File.WriteAllLines(fn, contentText);
        }

        /// <summary>
        /// Export pattern parts to individual midi files. This is as the events appear in the original file.
        /// </summary>
        /// <param name="fn">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels.</param>
        /// <param name="header">File header data to include.</param>
        public static void ExportMidi(string fn, Pattern pattern, List<int> channels, Header header)
        {
            // Init output file contents.
            int ppq = header.DeltaTicksPerQuarterNote;
            MidiEventCollection outColl = new(1, ppq);

            for (int i = 0; i < pattern.Tracks.Count; i++)
            {
                var track = pattern.Tracks[i];

                // Don't include only-meta tracks except first.
                if (track.NumStandard == 0 && i != 0) { continue; }

                // Build the event collection.
                IList<MidiEvent> outEvents = outColl.AddTrack();
                var events = track.GetFilteredEvents(channels);
                events?.ForEach(e => { outEvents.Add(e); });
                //// Add end track.
                //long ltime = outEvents.Last().AbsoluteTime;
                //var endt = new MetaEvent(MetaEventType.EndTrack, 0, ltime);
                //outEvents.Add(endt);
            }

            // Use NAudio function to create out file.
            MidiFile.Export(fn, outColl);
        }

        /// <summary>
        /// Export the contents as a text piano roll. TODO1
        /// </summary>
        /// <param name="fn">Where to boss?</param>
        /// <param name="pattern">Specific pattern.</param>
        /// <param name="channels">Specific channnels.</param>
        /// <param name="header">File header data to include.</param>
        public static void ExportPianoRoll(string fn, Pattern pattern, List<int> channels, Header header)
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

            */

            // // Selections.
            // List<int> channelNumbers = [.. channels.Select(cc => cc.ChannelNumber)];
            // //channels.ForEach(ch => { contentText.Add($"0,0,Patch,0,{ch.Patch},{ch.PatchName}"); });
        }
    }
}
