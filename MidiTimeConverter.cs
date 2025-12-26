using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ephemera.MidiLib;


namespace Ephemera.MidiLibEx
{
    /// <summary>Helpers to translate between midi standard and arbtrary internal representation.</summary>
    public class MidiTimeConverter
    {
        /// <summary>Resolution for midi file events aka DeltaTicksPerQuarterNote.</summary>
        readonly int _midiPpq;

        /// <summary>Tempo aka BPM.</summary>
        readonly double _tempo;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="midiPpq">The file resolution.</param>
        /// <param name="tempo">BPM may be needed for some calcs.</param>
        public MidiTimeConverter(int midiPpq, double tempo)
        {
            _midiPpq = midiPpq;
            _tempo = tempo;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public long InternalToMidi(int t)
        {
            long mtime = t * _midiPpq / MusicTime.TicksPerBeat;
            return mtime;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int MidiToInternal(long t)
        {
            long itime = t * MusicTime.TicksPerBeat / _midiPpq;
            return (int)itime;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double InternalToMsec(int t)
        {
            double msec = InternalPeriod() * t;
            return msec;
        }

        /// <summary>
        /// Conversion function.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public double MidiToSec(int t)
        {
            double msec = MidiPeriod() * t / 1000.0;
            return msec;
        }

        /// <summary>
        /// Exact time between events.
        /// </summary>
        /// <returns></returns>
        public double MidiPeriod()
        {
            double secPerBeat = 60.0 / _tempo;
            double msecPerT = 1000 * secPerBeat / _midiPpq;
            return msecPerT;
        }

        /// <summary>
        /// Exact time between events.
        /// </summary>
        /// <returns></returns>
        public double InternalPeriod()
        {
            double secPerBeat = 60.0 / _tempo;
            double msecPerT = 1000 * secPerBeat / MusicTime.TicksPerBeat;
            return msecPerT;
        }

        /// <summary>
        /// Integer time between events.
        /// </summary>
        /// <returns></returns>
        public int RoundedInternalPeriod()
        {
            double msecPerT = InternalPeriod();
            int period = msecPerT > 1.0 ? (int)Math.Round(msecPerT) : 1;
            return period;
        }
    }
}
