# MidiLibEx TODO1 clean up or migrate

This library adds higher level functionality to that provided by [MidiLib](https://github.com/cepthomas/MidiLib).
- Reading and playing midi files.
- Reading and playing the patterns in Yamaha style files.
- Remapping channel patches.
- Various export functions including specific style patterns.

The Test project should be useful. [Midifrier](https://github.com/cepthomas/Midifrier) also uses this extensively.

Requires VS2022 and .NET8.


## Notes
- If midi file type is `1`, all tracks are combined. Because?
- Some midi files (particuarly single instrument) use different drum channel numbers so there are a couple of options for simple remapping.

# Components

- MidiDataFile
  - Processes and contains a massaged version of the midi/style file contents.
  - Represents one complete collection of midi events from a file - standard midi or yamaha style files.
  - Translates from raw file to internal representation.
  - Units are in ticks - scaled version of midi ticks.

- PatternInfo
  - Represents the contents of a midi file pattern.
  - If it is a plain midi file (not style) there will be one only.

- MidiExport
  - Export the original file contents in a csv readable form.
  - Export the original file pattern parts to individual midi files.

# Style Files

http://www.wierzba.homepage.t-online.de/StyleFileDescription_v21.pdf


Style files contain multiple sections, each of which describes a pattern. For the purposes of this application, `section` refers
to a part of a file and `pattern` refers to the internal representation. Patterns are named for their intent (`Intro A`, `Main B`, ...)
with the exception of `""` which contains global stuff in the case of a style file, and the entire contents in the case of
a plain midi file.

There's tons of styles and technical info at https://psrtutorial.com/. An overview taken from `StyleFileDescription_v21.pdf`:

> A style is a special form of a type 0 midi file followed by several information sections.
> Internally, a style starts by specifying the tempo, the time signature and the copyright followed by several sections
> that are defined by marker events.
> The first two sections, SFF1 (or SFF2) and SInt, occupying the first measure of the midi part, include a Midi On
> plus midi commands to setup the default instruments.
> Each of the other markers (Intro A, Main B, etc) defines musical patterns that are triggered by the keying chords.


# External Components

- [NAudio](https://github.com/naudio/NAudio) (MIT).


# ======================= New stuff ============================

- If it is a plain midi file there will be one only pattern, with one or more tracks, each with one or more channels.
- If it is a midi style file there will be one or more patterns, each with a single track, each with one or more channels.

- Plain - file has one or more MTrks (usually but not necessarily one per instrument)
> ===== MTrk =====
0 MidiPort 00
0 SequenceTrackName BASS
0 PatchChange Ch: 1 Electric Bass(finger)
0 ControlChange Ch: 1 Controller MainVolume Value 127
0 NoteOn Ch: 1 A2 Vel:75 Len: ?
0 NoteOn Ch: 1 A2 Vel:0 (Note Off)
0 NoteOn Ch: 1 A2 Vel:75 Len: ?
XXX
0 EndTrack

- Style - one MTrk with one or more patterns identified by Marker
    each pattern contains multipe channels and events starting at 0
    convert each pattern into a track with SequenceTrackName set to Marker text

## From doc
----------------------------------------------
The common order of the sections in the file is at follows:
1. Midi section
2. CASM section
3. OTS (One Touch Setting) section
4. MDB (Music Finder) section
5. MH section

>>>>>>>
Section 1 is always a standard midi file structure of a midi type 0 file (one track). The general structure of this section is a little bit different than the structure of sections 2...4, which share the same common structure

Structure of section 1 (midi section):
Section Id (4 bytes)  "MThd"
Some fix data (14 bytes) file format (0!), num tracks (1!), ppq (SFF1=any SFF2=1920?)  
?? Section Length (4 bytes)
?? Section Data (n bytes)
            Header.MidiFileType = (int)ReadStream(br, 2);
            Header.NumTracks = (int)ReadStream(br, 2);
            Header.DeltaTicksPerQuarterNote = (int)ReadStream(br, 2);



> first/only?? track section
Section Id (4 bytes)  "MTrk"
Section Length (4 bytes)
Section Data (n bytes)

The midi section of a style consists of some initial file related data (key, tempo, time, ...) , then two initializing markers SFF1/2 and SInt used to initialize the PSR/Tyros, set up instrument voices, and the markers used to delineate the midi patterns by the selected sections (e.g. Main A, Ending B).


> other track sections?
Section Id (4 bytes)  "MTrk"
Section Length (4 bytes)
Section Data (n bytes)

etc...


The midi section is midi type 0, which means that there is one midi track. In the first measure there is a marker event which informs about the version of the style file format.
"SFF1" or "SFF2"


> Common structure for sections optional 2 - 4:
Section Id (4 bytes)  "NAME"
Section Length (4 bytes)
Section Data (n bytes)

Optional CASM section (2) contains extended information for the keyboard how to interpret and control playing of the style section. (see 4.6)

