using System;
using System.Collections.Generic;

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
        public const short PackageMaxLength = 4 * 1024;
        private bool _headerExtracted;

        private short _packageLen;
        private byte[] _body;
        private short _alreadyExtractedLen;

        public Queue<byte[]> Packages = new Queue<byte[]>();

        public PackerError Process(CircularBuffer buffer, ref short packagesCnt)
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
            if (buffer.ReadableSize < sizeof(short)) return PackerError.Running;

            _packageLen = (short)(buffer.Buffer[buffer.Head] + buffer.Buffer[buffer.Head + 1] * 256);
            if (_packageLen > PackageMaxLength) return PackerError.Failed;
            if (_packageLen < 0) return PackerError.Failed;

            _body = new byte[_packageLen];

            buffer.MoveByRead(sizeof(short));
            _headerExtracted = true;
            return PackerError.Success;
        }

        private PackerError ProcessBody(CircularBuffer buffer)
        {
            var extractLen = Math.Min((short)(_packageLen - _alreadyExtractedLen), buffer.ReadableSize);

            Buffer.BlockCopy(buffer.Buffer, buffer.Head, _body, _alreadyExtractedLen, extractLen);

            buffer.MoveByRead(extractLen);
            _alreadyExtractedLen += extractLen;

            return _alreadyExtractedLen == _packageLen ? PackerError.Success : PackerError.Running;
        }
    }
}
