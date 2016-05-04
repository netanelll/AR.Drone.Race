using AR.Drone.Infrastructure;
using AR.Drone.Video.Exceptions;
using FFmpeg.AutoGen;
using System;

namespace AR.Drone.Video
{
    public unsafe class VideoConverter : DisposableBase
    {
        private readonly AVPixelFormat _pixelFormat;
        private bool _initialized;

        private sbyte[] _outputData;

        private SwsContext* _pContext;
        private AVFrame* _pCurrentFrame;


        public VideoConverter(AVPixelFormat pixelFormat)
        {
            _pixelFormat = pixelFormat;
        }

        private void Initialize(int width, int height, AVPixelFormat inFormat)
        {
            _initialized = true;
            
            _pContext = ffmpeg.sws_getContext(width, height, inFormat,
                                                    width, height, _pixelFormat,
                                                    ffmpeg.SWS_FAST_BILINEAR, null, null, null);
            if (_pContext == null)
                throw new VideoConverterException("Could not initialize the conversion context.");

            _pCurrentFrame = ffmpeg.av_frame_alloc();

            int outputDataSize = ffmpeg.avpicture_get_size(_pixelFormat, width, height);
            _outputData = new sbyte[outputDataSize];

            fixed (sbyte* pOutputData = &_outputData[0])
            {
                ffmpeg.avpicture_fill((AVPicture*) _pCurrentFrame, pOutputData, _pixelFormat, width, height);
            }
        }

        public sbyte[] ConvertFrame(AVFrame* pFrame)
        {
            if (_initialized == false)
                Initialize(pFrame->width, pFrame->height, (AVPixelFormat)pFrame->format);

            fixed (sbyte* pOutputData = &_outputData[0])
            {
                sbyte** pSrcData = &((pFrame)->data0);
                sbyte** pDstData = &(_pCurrentFrame)->data0;

                _pCurrentFrame->data0 = pOutputData;
                ffmpeg.sws_scale(_pContext, pSrcData, pFrame->linesize, 0, pFrame->height, pDstData, _pCurrentFrame->linesize);
            }
            return _outputData;
        }

        protected override void DisposeOverride()
        {
            if (_initialized == false) return;

            ffmpeg.sws_freeContext(_pContext);
            ffmpeg.av_free(_pCurrentFrame);
        }
    }
}