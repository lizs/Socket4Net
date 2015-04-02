using System;
using System.Collections.Generic;
using socket4net.Log;

namespace socket4net.Net.TCP
{
    public enum PackerError
    {
        Success,
        Running,
        Failed,
    }

    /// <summary>
    /// 拆包
    /// 非线程安全，需要上层来确保始终在某个线程运行
    /// </summary>
    public class Packer
    {
        public Packer(ushort packMaxSize)
        {
            PackageMaxSize = packMaxSize;
        }

        public ushort PackageMaxSize { get; private set; }
        private bool _headerExtracted;

        private ushort _packageLen;
        private byte[] _body;
        private ushort _alreadyExtractedLen;

        public Queue<byte[]> Packages = new Queue<byte[]>();

        public PackerError Process(CircularBuffer buffer, ref ushort packagesCnt)
        {
            PackerError error;
            if (!_headerExtracted)
            {
                error = ProcessHeader(buffer);
                switch (error)
                {
                    case PackerError.Running:
                    case PackerError.Failed:
                        return error;
                }
            }

            error = ProcessBody(buffer);
            switch (error)
            {
                case PackerError.Success:
                    {
                        packagesCnt++;
                        Packages.Enqueue(_body);
                        Reset();

                        return Process(buffer, ref packagesCnt);
                    }

                default:
                    return error;
            }
        }

        private void Reset()
        {
            _body = null;
            _headerExtracted = false;
            _packageLen = 0;
            _alreadyExtractedLen = 0;
        }

        private PackerError ProcessHeader(CircularBuffer buffer)
        {
            if (buffer.ReadableSize < sizeof(ushort)) return PackerError.Running;

            _packageLen = (ushort)(buffer.Buffer[buffer.Head] + buffer.Buffer[buffer.Head + 1] * 256);
            if (_packageLen > PackageMaxSize)
            {
                Logger.Instance.WarnFormat("Processing buffer size : {0} bytes,  bigger than {1} bytes!", _packageLen, PackageMaxSize);
                return PackerError.Failed;
            }

            _body = new byte[_packageLen];

            buffer.MoveByRead(sizeof(ushort));
            _headerExtracted = true;
            return PackerError.Success;
        }

        private PackerError ProcessBody(CircularBuffer buffer)
        {
            var extractLen = Math.Min((ushort)(_packageLen - _alreadyExtractedLen), buffer.ReadableSize);

            Buffer.BlockCopy(buffer.Buffer, buffer.Head, _body, _alreadyExtractedLen, extractLen);

            buffer.MoveByRead(extractLen);
            _alreadyExtractedLen += extractLen;

            return _alreadyExtractedLen == _packageLen ? PackerError.Success : PackerError.Running;
        }
    }
}
