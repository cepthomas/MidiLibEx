# MidiLibEx TODO1 update doc ----------------------------------------------------------

This library contains a bunch of components and controls accumulated over the years. It supports:
- Reading and playing midi files.
- Reading and playing the patterns in Yamaha style files.
- Remapping channel patches.
- Various export functions including specific style patterns.
- Midi input handler.
- Requires VS2022 and .NET6.


## Notes
- Since midi files and NAudio use 1-based channel numbers, so does this application, except when used internally as an array index.
- Time is represented by `bar.beat.sub ` but 0-based, unlike typical music representation.
- Because the windows multimedia timer has inadequate accuracy for midi notes, resolution is limited to 32nd notes.
- If midi file type is `1`, all tracks are combined. Because.
- NAudio `NoteEvent` is used to represent Note Off and Key After Touch messages. It is also the base class for `NoteOnEvent`. Not sure why it was done that way.
- Midi devices are limited to the ones available on your box. (Hint - try VirtualMidiSynth).
- Some midi files (particuarly single instrument) use different drum channel numbers so there are a couple of options for simple remapping.

# Components

## Core

MidiOutput
- The top level component for sending midi data.
- Translates from MidiData to the wire.

MidiInput
- A simple midi input component.
- You supply the handler.

MidiOsc
- Implementation of midi over [OSC](https://opensoundcontrol.stanford.edu).

Channel
- Represents a physical output channel in a way usable by ChannelControl UI and MidiOutput.

MidiDataFile, PatternInfo, MidiExport
- Processes and contains a massaged version of the midi/style file contents.
- Translates from raw file to MidiData internal representation.
- Units are in subs - essentially midi ticks.
- Lots of utility and export functions too.

## UI

ChannelControl
- Bound to a Channel object.
- Provides volume, mute, solo.
- Patch selection.

BarBar, BarTime
- Shows progress in musical bars and beats.
- User can select time.

PatchPicker
- Select from the standard GM list.

DevicesEditor
- Used for selecting inputs and outputs in settings editing.

## Other

- MidiDefs: The GM definitions plus conversion functions.
- MidiTimeConverter: Used for mapping between data sets using different resolutions.
- MidiSettings container/editor for use by clients.
- MidiCommon: All the other stuff.

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

The Test project contains a fairly complete demo application.

[Midifrier](https://github.com/cepthomas/Midifrier) also uses this extensively.

# External Components

- [NAudio](https://github.com/naudio/NAudio) (Microsoft Public License).

