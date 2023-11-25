using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.GlobalManagers
{
    public class FAudioManager
    {
        private readonly IntPtr fileHandle;
        private readonly int channels;
        private readonly int sampleRate;
        private readonly uint sampleCount;
        private readonly bool loopEnabled;
        private readonly uint loopStart;
        private readonly uint loopEnd;
        private bool isDisposed;

        private Stopwatch stopwatch = new Stopwatch();
        private float stopwatchOffset;

        public FAudioManager(string filePath)
        {
            //string filePath = TitleContainerEXT.ResolvePath(fileName);
            //if (!File.Exists(filePath))
            //    throw new FileNotFoundException("File does not exist", filePath);

            // Open file with FAudio
            fileHandle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);
            if (error != 0)
            {
                throw new Exception("Error opening OGG file!");
            }

            // Read metadata
            FAudio.stb_vorbis_info info = FAudio.stb_vorbis_get_info(fileHandle);
            channels = info.channels;
            sampleRate = (int)info.sample_rate;
            sampleCount = FAudio.stb_vorbis_stream_length_in_samples(fileHandle);

            // Look for loop info
            loopStart = DecodeField("LOOPSTART", 0);
            loopEnd = DecodeField("LOOPLENGTH", 0) + loopStart;
            loopEnabled = (loopEnd > 0);
        }

        private uint DecodeField(string desiredFieldName, uint defaultValue)
        {
            string desiredFieldPrefix = desiredFieldName + "=";

            FAudio.stb_vorbis_comment commentInfo = FAudio.stb_vorbis_get_comment(fileHandle);
            for (int i = 0; i < commentInfo.comment_list_length; i++)
            {
                IntPtr pointer = Marshal.ReadIntPtr(commentInfo.comment_list, i * Marshal.SizeOf(typeof(IntPtr)));
                string s = Marshal.PtrToStringAnsi(pointer);
                if (s.StartsWith(desiredFieldPrefix))
                {
                    UInt32.TryParse(s.Substring(desiredFieldPrefix.Length), out defaultValue);
                }
            }

            return defaultValue;
        }

        public bool UsesFloats { get { return true; } }
        public int Channels { get { return channels; } }
        public int SampleRate { get { return sampleRate; } }
        public float Position
        {
            // OGG seems to use samples per channel, not total samples
            get { return stopwatchOffset + (float)stopwatch.Elapsed.TotalSeconds; }
            set { SetPosition((uint)(value * sampleRate)); }
        }

        private void SetPosition(uint desiredSample)
        {
            if (desiredSample < 0)
                desiredSample = 0;
            else if (desiredSample > sampleCount)
                desiredSample = sampleCount;

            FAudio.stb_vorbis_seek(fileHandle, desiredSample);
            stopwatchOffset = desiredSample / (float)sampleRate;
            stopwatch.Reset();
        }

        public unsafe Array Decode(int samplesDesired)
        {
            float[] buffer = new float[samplesDesired * channels];

            int currentSample = FAudio.stb_vorbis_get_sample_offset(fileHandle);

            /* NOTE: this function returns samples per channel, not total samples */
            int samples = FAudio.stb_vorbis_get_samples_float_interleaved(
                fileHandle,
                channels,
                buffer,
                buffer.Length
            );

            if (loopEnabled)
            {
                int samplesLeftUntilLoop = (int)(loopEnd - currentSample);
                if (samplesLeftUntilLoop < 0)
                    samplesLeftUntilLoop = 0;

                if (samplesLeftUntilLoop < samplesDesired)
                {
                    SetPosition(loopStart);
                    int bufferBase = samplesLeftUntilLoop * channels;
                    fixed (float* p = &buffer[bufferBase])
                        samples = FAudio.stb_vorbis_get_samples_float_interleaved(
                            fileHandle,
                            channels,
                            (IntPtr)p,
                            buffer.Length - bufferBase);
                    samples += samplesLeftUntilLoop;
                }
            }

            if (!stopwatch.IsRunning && samples > 0)
                stopwatch.Start();

            return buffer;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                FAudio.stb_vorbis_close(fileHandle);
                isDisposed = true;
            }
        }

        ~FAudioManager()
        {
            Dispose();
        }
    }
}
