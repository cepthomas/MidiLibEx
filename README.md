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
