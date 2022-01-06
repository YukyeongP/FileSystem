using System;
using System.Collections.Generic;
using System.IO;

namespace FileSystem
{
    /// <summary>
    /// 데이터 블럭의 시작 Offset과 길이를 저장합니다.
    /// </summary>
    public struct Extent
    {
        public long Start;
        public long Size;

        /// <param name="start">데이터 블럭의 시작 Offset 입니다.</param>
        /// <param name="size">데이터 블럭의 byte 수입니다.</param>
        public Extent(long start, long size)
        {
            Start = start;
            Size = size;
        }

        public override string ToString()
        {
            return string.Format($"Start: {Start}, Size: {Size}");
        }
    }

    static class ExtentExtension
    {
        /// <summary>
        /// 모든 요소의 <see cref="Extent.Size"/>의 합을 가져옵니다.
        /// </summary>
        /// <param name="extents">Size의 합을 가져올 <see cref="List{Extent}"/> 입니다.</param>
        /// <returns></returns>
        public static long GetTotalSize(this List<Extent> extents)
        {
            long result = 0;
            extents.ForEach(e => result += e.Size);
            return result;
        }
    }

    public class NodeStream : Stream
    {
        /// <summary>
        /// Stream의 전체 데이터 위치를 표시하는 <see cref="Extent"/> 목록입니다.
        /// </summary>
        public List<Extent> DataExtent;

        /// <summary>
        /// Stream에서 현재 위치를 가져오거나 설정합니다.
        /// </summary>
        public override long Position { get => Offset; set => Offset = value; }


        /// <summary>
        /// <value>Node stream의 전체 크기를 가져옵니다. Extent의 길이의 합으로, 실제 데이터의 크기와는 다를 수 있습니다.
        /// </summary>
        public override long Length { get => Size; }

        private long Size;
        private long Offset;
        private Stream stream;

        /// <param name="stream">Node의 데이터를 읽을 Stream</param>
        /// <param name="extents">Node의 데이터 Extent List</param>
        public NodeStream(Stream stream, List<Extent> extents)
        {
            DataExtent = extents;
            DataExtent.ForEach(e => Size += e.Size);
            Offset = 0;
            this.stream = stream;
        }

        /// <param name="buffer">읽은 데이터를 저장할 byte 배열입니다.</param>
        /// <param name="offset">읽은 데이터를 저장하기 시작하는 <paramref name="buffer"/>의 Offset 입니다.</param>
        /// <param name="count">읽을 최대 byte 개수입니다.</param>
        /// <returns>Stream에서 읽은 총 byte 수입니다. Stream에 남은 byte가 충분하지 않을 경우 <paramref name="count"/>보다 작을 수 있습니다.</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalSize = 0;
            long prevSize = 0;
            foreach (var extent in DataExtent)
            {
                if (count == 0)
                    break;

                prevSize += extent.Size;
                if (Offset >= prevSize - 1)
                    continue;

                setRealOffset();
                int readSize = (int)(extent.Start + extent.Size - stream.Position);
                if (readSize > count)
                    readSize = count;

                var tmp = new byte[readSize];
                totalSize = stream.Read(tmp, 0, readSize);
                Buffer.BlockCopy(tmp, 0, buffer, offset, readSize);

                count -= readSize;
                Offset += readSize;
                offset += readSize;
            }

            return totalSize;
        }

        /// <summary>
        /// 현재 Stream에서 위치를 설정합니다.
        /// </summary>
        /// <param name="offset"><paramref name="origin"/>에 대한 상대 Offset 입니다.</param>
        /// <param name="origin"><paramref name="offset"/>에 대한 기준점을 나타내는  <see cref="SeekOrigin"/>형식의 값입니다.</param>
        /// <returns>설정된 Offset 입니다.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Offset = offset;
                    break;
                case SeekOrigin.Current:
                    Offset += offset;
                    break;
                case SeekOrigin.End:
                    Offset = Size - offset - 1;
                    break;
            }

            return Offset;
        }

        /// <summary>
        /// Stream의 최대 byte 수를 설정합니다.
        /// </summary>
        /// <param name="value">설정할 Stream의 최대 byte 수입니다.</param>
        public override void SetLength(long value) => Size = value;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        private void setRealOffset()
        {
            long prevSize = 0;
            foreach (var e in DataExtent)
            {
                if (Offset < prevSize + e.Size)
                {
                    stream.Seek(e.Start + Offset - prevSize, SeekOrigin.Begin);
                    break;
                }
                prevSize += e.Size;
            }
        }

        public override bool CanWrite => false;

        #region Not Use
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new System.NotImplementedException();
        }
        
        public override void Flush() => throw new System.NotImplementedException();
        #endregion
    }
}