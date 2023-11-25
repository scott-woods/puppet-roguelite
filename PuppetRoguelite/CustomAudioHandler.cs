using Nez;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite
{
    public class CustomAudioHandler : GlobalManager
    {
        nint _handler;
        nint _masteringVoice;
        IntPtr _fileHandle;

        public CustomAudioHandler()
        {
            //create handler
            FAudio.FAudioCreate(out _handler, 0, FAudio.FAUDIO_DEFAULT_PROCESSOR);

            //create master voice
            FAudio.FAudio_CreateMasteringVoice(_handler, out _masteringVoice, FAudio.FAUDIO_DEFAULT_CHANNELS, FAudio.FAUDIO_DEFAULT_SAMPLERATE, 0, 0, IntPtr.Zero);
        }

        public unsafe void PlaySound(string filePath)
        {
            //open file
            _fileHandle = FAudio.stb_vorbis_open_filename(filePath, out var error, IntPtr.Zero);

            //get file info
            var info = FAudio.stb_vorbis_get_info(_fileHandle);

            //get format
            var blockAlign = (ushort)((32 / 8) * info.channels);
            var format = new FAudio.FAudioWaveFormatEx
            {
                wFormatTag = 3,
                nChannels = (ushort)info.channels,
                nSamplesPerSec = info.sample_rate,
                wBitsPerSample = 32,
                nBlockAlign = blockAlign,
                nAvgBytesPerSec = blockAlign * info.sample_rate
            };

            //create buffer
            var lengthInFloats = FAudio.stb_vorbis_stream_length_in_samples(_fileHandle) * info.channels;
            var lengthInBytes = lengthInFloats * Marshal.SizeOf<float>();
            var bufferDataPointer = NativeMemory.Alloc((nuint)lengthInBytes);
            FAudio.stb_vorbis_get_samples_float_interleaved(_fileHandle, info.channels, (nint)bufferDataPointer, (int)lengthInFloats);
            FAudio.stb_vorbis_close(_fileHandle);
            var buffer = new FAudio.FAudioBuffer
            {
                Flags = FAudio.FAUDIO_END_OF_STREAM,
                AudioBytes = (uint)lengthInBytes,
                pAudioData = (nint)bufferDataPointer,
                PlayBegin = 0,
                PlayLength = 0,
                LoopBegin = 0,
                LoopLength = 0,
                LoopCount = FAudio.FAUDIO_LOOP_INFINITE
            };

            //create source voice
            IntPtr _sourceVoiceHandle;
            FAudio.FAudio_CreateSourceVoice(_handler, out _sourceVoiceHandle, ref format, FAudio.FAUDIO_VOICE_USEFILTER, FAudio.FAUDIO_DEFAULT_FREQ_RATIO, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

            //FAudio.FAudioSendDescriptor* sendDesc = stackalloc FAudio.FAudioSendDescriptor[1];
            //sendDesc[0].Flags = 0;
            //sendDesc[0].pOutputVoice = _masteringVoice;

            //var sends = new FAudio.FAudioVoiceSends();
            //sends.SendCount = 1;
            //sends.pSends = (nint)sendDesc;

            //FAudio.FAudioVoice_SetOutputVoices(_sourceVoiceHandle, ref sends);

            FAudio.FAudioSourceVoice_SubmitSourceBuffer(_sourceVoiceHandle, ref buffer, IntPtr.Zero);

            FAudio.FAudioSourceVoice_Start(_sourceVoiceHandle, 0, FAudio.FAUDIO_COMMIT_NOW);
        }
    }
}
