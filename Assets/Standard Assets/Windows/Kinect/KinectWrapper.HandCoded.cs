using RootSystem = System;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class MonoPInvokeCallbackAttribute : System.Attribute
{
    public System.Type RootType { get; set; }
    public MonoPInvokeCallbackAttribute(System.Type t) { RootType = t; }
}

namespace Windows.Foundation
{
    public struct Point
    {
    }
}

namespace Windows.Storage.Streams
{
    public partial class IBuffer : RootSystem.IDisposable
    {
        protected internal RootSystem.IntPtr _pNative;
        
        // Constructors and Finalizers
        internal IBuffer(RootSystem.IntPtr pNative)
        {
            _pNative = pNative;
            Windows_Storage_Streams_IBuffer_AddRefObject(ref _pNative);
        }
        
        ~IBuffer()
        {
            Dispose(false);
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern RootSystem.IntPtr Windows_Storage_Streams_IBuffer_get_UnderlyingBuffer(RootSystem.IntPtr pNative);
        public IntPtr UnderlyingBuffer
        {
            get
            {
                if (_pNative == RootSystem.IntPtr.Zero)
                {
                    throw new RootSystem.ObjectDisposedException("IBuffer");
                }
                
                return Windows_Storage_Streams_IBuffer_get_UnderlyingBuffer(_pNative);
            }
        }
        
        public void Dispose()
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("IBuffer");
            }
            
            Dispose(true);
            RootSystem.GC.SuppressFinalize(this);
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Storage_Streams_IBuffer_ReleaseObject(ref RootSystem.IntPtr pNative);
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Storage_Streams_IBuffer_AddRefObject(ref RootSystem.IntPtr pNative);
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Storage_Streams_IBuffer_Dispose(RootSystem.IntPtr pNative);
        protected virtual void Dispose(bool disposing)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                return;
            }
            
            Helper.NativeObjectCache.RemoveObject<IBuffer>(_pNative);
            Windows_Storage_Streams_IBuffer_ReleaseObject(ref _pNative);
            
            if (disposing)
            {
                Windows_Storage_Streams_IBuffer_Dispose(_pNative);
            }
            
            _pNative = RootSystem.IntPtr.Zero;
        }
        
        public static implicit operator RootSystem.IntPtr(IBuffer other)
        {
            if(other != null)
            {
                return other._pNative;
            }
            return RootSystem.IntPtr.Zero;
        }
        
        public static explicit operator IBuffer(RootSystem.IntPtr other)
        {
            if(other == RootSystem.IntPtr.Zero)
            {
                return null;
            }
            other = Helper.NativeObjectCache.MapToIUnknown(other);
            var obj = Helper.NativeObjectCache.GetObject<IBuffer>(other);
            if(obj == null)
            {
                obj = new IBuffer(other);
                Helper.NativeObjectCache.AddObject<IBuffer>(other, obj);
            }
            return obj;
        }
        
        // Public Properties
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern uint Windows_Storage_Streams_IBuffer_get_Capacity(RootSystem.IntPtr pNative);
        public  uint Capacity
        {
            get
            {
                if (_pNative == RootSystem.IntPtr.Zero)
                {
                    throw new RootSystem.ObjectDisposedException("IBuffer");
                }
                
                return Windows_Storage_Streams_IBuffer_get_Capacity(_pNative);
            }
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern uint Windows_Storage_Streams_IBuffer_get_Length(RootSystem.IntPtr pNative);
        public  uint Length
        {
            get
            {
                if (_pNative == RootSystem.IntPtr.Zero)
                {
                    throw new RootSystem.ObjectDisposedException("IBuffer");
                }
                
                return Windows_Storage_Streams_IBuffer_get_Length(_pNative);
            }
        }
    }
}

namespace Windows.Kinect
{
    internal partial class AudioStream : System.IO.Stream
    {
        protected internal RootSystem.IntPtr _pNative;
        
        // Constructors and Finalizers
        internal AudioStream(RootSystem.IntPtr pNative)
        {
            _pNative = pNative;
            Windows_Kinect_IStream_AddRefObject(ref _pNative);
        }
        
        ~AudioStream()
        {
            Dispose(false);
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_IStream_ReleaseObject(ref RootSystem.IntPtr pNative);
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_IStream_AddRefObject(ref RootSystem.IntPtr pNative);
        protected override void Dispose(bool disposing)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                return;
            }
            
            Helper.NativeObjectCache.RemoveObject<AudioStream>(_pNative);
            Windows_Kinect_IStream_ReleaseObject(ref _pNative);
            
            if (disposing)
            {
                Windows_Kinect_IStream_Dispose(_pNative);
            }
            
            _pNative = RootSystem.IntPtr.Zero;
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_IStream_Dispose(RootSystem.IntPtr pNative);
        public override void Close()
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                return;
            }
            
            Dispose(true);
            RootSystem.GC.SuppressFinalize(this);
        }
        
        public static implicit operator RootSystem.IntPtr(AudioStream other)
        {
            if(other != null)
            {
                return other._pNative;
            }
            return RootSystem.IntPtr.Zero;
        }
        
        public static explicit operator AudioStream(RootSystem.IntPtr other)
        {
            if(other == RootSystem.IntPtr.Zero)
            {
                return null;
            }
            other = Helper.NativeObjectCache.MapToIUnknown(other);
            var obj = Helper.NativeObjectCache.GetObject<AudioStream>(other);
            if(obj == null)
            {
                obj = new AudioStream(other);
                Helper.NativeObjectCache.AddObject<AudioStream>(other, obj);
            }
            return obj;
        }

        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_IStream_Read(
            RootSystem.IntPtr pNative,
            byte[] pBuffer,
            int offset,
            int bufferLength,
            RootSystem.IntPtr pcbRead);
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("AudioStream");
            }

            IntPtr readPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(int)));

            Windows_Kinect_IStream_Read(_pNative, buffer, offset, count, readPtr);

            int read = Marshal.ReadInt32(readPtr);
            Marshal.FreeCoTaskMem(readPtr);

            return read;
        }

        public override bool CanRead
        {
            get { return true; }
        }
        
        public override bool CanSeek
        {
            get { return false; }
        }
        
        public override bool CanWrite
        {
            get { return false; }
        }
        
        public override void Flush()
        {
            throw new NotImplementedException();
        }
        
        public override long Length
        {
            get { throw new NotImplementedException(); }
        }
        
        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        
        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotImplementedException();
        }
        
        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }
        
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
    
    public partial class AudioBeam
    {
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern RootSystem.IntPtr Windows_Kinect_AudioBeam_OpenInputStream(RootSystem.IntPtr pNative);
        public System.IO.Stream OpenInputStream()
        {
            RootSystem.IntPtr objectPointer = Windows_Kinect_AudioBeam_OpenInputStream(_pNative);
            if (objectPointer == RootSystem.IntPtr.Zero)
            {
                return null;
            }
            
            objectPointer = Helper.NativeObjectCache.MapToIUnknown(objectPointer);
            var obj = Helper.NativeObjectCache.GetObject<Windows.Kinect.AudioStream>(objectPointer);
            if (obj == null)
            {
                obj = new Windows.Kinect.AudioStream(objectPointer);
                Helper.NativeObjectCache.AddObject<Windows.Kinect.AudioStream>(objectPointer, obj);
            }
            
            return obj;
        }
    }

    public partial class AudioBeamFrame
    {
        private Windows.Kinect.AudioBeamSubFrame[] _subFrames = null;

        protected virtual void Dispose(bool disposing)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                return;
            }

            if (_subFrames != null)
            {
                foreach(var subFrame in _subFrames)
                {
                    subFrame.Dispose();
                }
                
                _subFrames = null;
            }
            
            Helper.NativeObjectCache.RemoveObject<AudioBeamFrame>(_pNative);
            Windows_Kinect_AudioBeamFrame_ReleaseObject(ref _pNative);
            
            if (disposing)
            {
                Windows_Kinect_AudioBeamFrame_Dispose(_pNative);
            }
            
            _pNative = RootSystem.IntPtr.Zero;
        }

        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_AudioBeamFrame_Dispose(RootSystem.IntPtr pNative);
        public void Dispose()
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                return;
            }
            
            Dispose(true);
            RootSystem.GC.SuppressFinalize(this);
        }
                
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int Windows_Kinect_AudioBeamFrame_get_SubFrames(RootSystem.IntPtr pNative, [RootSystem.Runtime.InteropServices.Out] RootSystem.IntPtr[] outCollection, int collectionSize);
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern int Windows_Kinect_AudioBeamFrame_get_SubFrames_Length(RootSystem.IntPtr pNative);
        public  Windows.Kinect.AudioBeamSubFrame[] SubFrames
        {
            get
            {
                if (_pNative == RootSystem.IntPtr.Zero)
                {
                    throw new RootSystem.ObjectDisposedException("AudioBeamFrame");
                }
                
                if (_subFrames == null)
                {
                    int collectionSize = Windows_Kinect_AudioBeamFrame_get_SubFrames_Length(_pNative);
                    var outCollection = new RootSystem.IntPtr[collectionSize];
                    _subFrames = new Windows.Kinect.AudioBeamSubFrame[collectionSize];
                    
                    collectionSize = Windows_Kinect_AudioBeamFrame_get_SubFrames(_pNative, outCollection, collectionSize);
                    for(int i=0;i<collectionSize;i++)
                    {
                        if(outCollection[i] == RootSystem.IntPtr.Zero)
                        {
                            continue;
                        }
                        
                        outCollection[i] = Helper.NativeObjectCache.MapToIUnknown(outCollection[i]);
                        var obj = Helper.NativeObjectCache.GetObject<Windows.Kinect.AudioBeamSubFrame>(outCollection[i]);
                        if (obj == null)
                        {
                            obj = new Windows.Kinect.AudioBeamSubFrame(outCollection[i]);
                            Helper.NativeObjectCache.AddObject<Windows.Kinect.AudioBeamSubFrame>(outCollection[i], obj);
                        }
                        
                        _subFrames[i] = obj;
                    }
                }
                
                return _subFrames;
            }
        }
    }

    public partial class CoordinateMapper
    {
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_CoordinateMapper_MapColorFrameToDepthSpace(RootSystem.IntPtr pNative, RootSystem.IntPtr depthFrameData, uint depthFrameDataSize, [RootSystem.Runtime.InteropServices.Out] Windows.Kinect.DepthSpacePoint[] depthSpacePoints, int depthSpacePointsSize);
        public void MapColorFrameToDepthSpace(Windows.Storage.Streams.IBuffer depthFrameData, Windows.Kinect.DepthSpacePoint[] depthSpacePoints)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("CoordinateMapper");
            }
            
            uint length = depthFrameData.Length / sizeof(UInt16);
            Windows_Kinect_CoordinateMapper_MapColorFrameToDepthSpace(_pNative, depthFrameData, length, depthSpacePoints, depthSpacePoints.Length);
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_CoordinateMapper_MapColorFrameToCameraSpace(RootSystem.IntPtr pNative, RootSystem.IntPtr depthFrameData, uint depthFrameDataSize, [RootSystem.Runtime.InteropServices.Out] Windows.Kinect.CameraSpacePoint[] cameraSpacePoints, int cameraSpacePointsSize);
        public void MapColorFrameToCameraSpace(Windows.Storage.Streams.IBuffer depthFrameData, Windows.Kinect.CameraSpacePoint[] cameraSpacePoints)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("CoordinateMapper");
            }
            
            uint length = depthFrameData.Length / sizeof(UInt16);
            Windows_Kinect_CoordinateMapper_MapColorFrameToCameraSpace(_pNative, depthFrameData, length, cameraSpacePoints, cameraSpacePoints.Length);
        }
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_CoordinateMapper_MapDepthFrameToColorSpace(RootSystem.IntPtr pNative, RootSystem.IntPtr depthFrameData, uint depthFrameDataSize, [RootSystem.Runtime.InteropServices.Out] Windows.Kinect.ColorSpacePoint[] colorSpacePoints, int colorSpacePointsSize);
        public void MapDepthFrameToColorSpace(Windows.Storage.Streams.IBuffer depthFrameData, Windows.Kinect.ColorSpacePoint[] colorSpacePoints)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("CoordinateMapper");
            }
            
            uint length = depthFrameData.Length / sizeof(UInt16);
            Windows_Kinect_CoordinateMapper_MapDepthFrameToColorSpace(_pNative, depthFrameData, length, colorSpacePoints, colorSpacePoints.Length);
        }
       
        
        [RootSystem.Runtime.InteropServices.DllImport("KinectForUnity", CallingConvention=RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
        private static extern void Windows_Kinect_CoordinateMapper_MapDepthFrameToCameraSpace(RootSystem.IntPtr pNative, IntPtr depthFrameData, uint depthFrameDataSize, [RootSystem.Runtime.InteropServices.Out] Windows.Kinect.CameraSpacePoint[] cameraSpacePoints, int cameraSpacePointsSize);
        public void MapDepthFrameToCameraSpace(Windows.Storage.Streams.IBuffer depthFrameData, Windows.Kinect.CameraSpacePoint[] cameraSpacePoints)
        {
            if (_pNative == RootSystem.IntPtr.Zero)
            {
                throw new RootSystem.ObjectDisposedException("CoordinateMapper");
            }
            
            uint length = depthFrameData.Length / sizeof(UInt16);
            Windows_Kinect_CoordinateMapper_MapDepthFrameToCameraSpace(_pNative, depthFrameData.UnderlyingBuffer, length, cameraSpacePoints, cameraSpacePoints.Length);
        }
    }
}

namespace Helper
{
    public static class NativeObjectCache
    {
        private static object _lock = new object();
        private static Dictionary<Type, Dictionary<IntPtr, WeakReference>> _objectCache = new Dictionary<Type, Dictionary<IntPtr, WeakReference>>();
        
        public static IntPtr MapToIUnknown(IntPtr nativePtr)
        {
            //private static Guid _guidIUnknown = new Guid("00000000-0000-0000-C000-000000000046");
            // NOTE: The IntPtr needs to use the IUnknown identity
            return nativePtr;
        }
        
        public static void AddObject<T>(IntPtr nativePtr, T obj) where T : class
        {
            lock (_lock)
            {
                Dictionary<IntPtr, WeakReference> objCache = null;
                
                if (!_objectCache.TryGetValue(typeof(T), out objCache) || objCache == null)
                {
                    objCache = new Dictionary<IntPtr, WeakReference>();
                    _objectCache[typeof(T)] = objCache;
                }
                
                objCache[nativePtr] = new WeakReference (obj);
            }
        }
        
        public static void RemoveObject<T>(IntPtr nativePtr)
        {
            lock (_lock)
            {
                Dictionary<IntPtr, WeakReference> objCache = null;
                
                if (!_objectCache.TryGetValue(typeof(T), out objCache) || objCache == null)
                {
                    objCache = new Dictionary<IntPtr, WeakReference>();
                    _objectCache[typeof(T)] = objCache;
                }
                
                if (objCache.ContainsKey(nativePtr))
                {
                    objCache.Remove(nativePtr);
                }
            }
        }
        
        public static T GetObject<T>(IntPtr nativePtr) where T : class
        {
            lock (_lock) 
            {
                Dictionary<IntPtr, WeakReference> objCache = null;
                
                if (!_objectCache.TryGetValue(typeof(T), out objCache) || objCache == null)
                {
                    objCache = new Dictionary<IntPtr, WeakReference>();
                    _objectCache[typeof(T)] = objCache;
                }
                
                WeakReference reference = null;
                if (objCache.TryGetValue(nativePtr, out reference)) 
                {
                    if(reference != null)
                    {
                        T obj = reference.Target as T; 
                        if(obj != null)
                        {
                            return (T)obj;
                        }
                    }
                }
                
                return null;
            }
        }
    }
}
