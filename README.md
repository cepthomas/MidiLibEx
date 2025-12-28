# MidiLibEx

This library contains a bunch of higher level components to complement the MidiLib project.
- Reading and playing midi files.
- Reading and playing the patterns in Yamaha style files.
- Remapping channel patches.
- Various export functions including specific style patterns.
- Requires VS2022 and .NET8.


## Notes
- Since midi files and NAudio use 1-based channel numbers, so does this application, except when used internally as an array index.
- Time is represented by `bar.beat.tick ` but 0-based, unlike typical music representation.
- If midi file type is `1`, all tracks are combined. Because?
- NAudio `NoteEvent` is used to represent Note Off and Key After Touch messages. It is also the base class for `NoteOnEvent`. Not sure why it was done that way.
- Some midi files (particuarly single instrument) use different drum channel numbers so there are a couple of options for simple remapping.

# Components

MidiDataFile, PatternInfo, MidiExport
- Processes and contains a massaged version of the midi/style file contents.
- Translates from raw file to MidiData internal representation.
- Units are in ticks - scaled version of midi ticks.
- MidiTimeConverter: Used for mapping between data sets using different resolutions.
- Lots of utility and export functions too.

# Style Files

Style files contain multiple sections, each of which describes a pattern. For the purposes of this application, `section` refers
to a part of a file and `pattern` refers to the internal representation. Patterns are named for their intent (`Intro A`, `Main B`, ...)
with the exception of `""` which contains global stuff in the case of a style file, and the entire contents in the case of
a plain midi file.

There's tons of styles and technical info at https://psrtutorial.com/. An overview taken from `StyleFileDescription_v21.pdf`:

> A style is a special form of a type 0 midi file followed by several information sections.
> 
> Internally, a style starts by specifying the tempo, the time signature and the copyright followed by several sections that are defined
> by marker events.
> 
> The first two sections, SFF1 (or SFF2) and SInt, occupying the first measure of the midi part, include a Midi On plus midi commands to
> setup the default instruments.
> 
> Each of the other markers (Intro A, Main B, etc) defines musical patterns that are triggered by the keying chords.

# Example

The Test project should be useful. [Midifrier](https://github.com/cepthomas/Midifrier) also uses this extensively.

# External Components

- [NAudio](https://github.com/naudio/NAudio) (MIT).

