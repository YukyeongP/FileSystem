using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Runtime.InteropServices.ComTypes;

namespace MD.IO.Buffer
{
    // sector
    // - 512 (1~512)
    // - atomic (all or nothing) unit of HD i/o
    // - camelCase, snake_case, lint
    //
    /// <summary>
    /// The purpose of this class is different from ByteBuffer(1).
    /// Previous ByteBuffer(1) is dynamic and self contained container 
    /// whereas ByteBuffer2 is based on shallow-copied buffer.
    /// 
    /// So, the size of this class is fixed once created and the contents of this buffer 
    /// will be changed as the contents of base byte[] change.
    /// 
    /// </summary>
    public class ByteBuffer2
    {
        protected int begin, count;
        protected int limit;
        protected byte[] data;

        // private static readonly object locked = new object();
        // private static SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();

        public byte[] Data { get { return data; } }

        public ByteBuffer2(): this(null, 0, 0)
        {
        }

        public ByteBuffer2(byte[] buffer): this(buffer, 0, buffer.Length)
        {
        }

        public ByteBuffer2(byte[] buffer, int offset, int count)
        {
            this.begin  = offset;
            this.Offset = offset;
            this.limit  = offset + count;
            this.data   = buffer; // shallow copy, deep copy
            this.count  = count;
        }

        public int Size         { get { return this.count; } }

        public int RemainedSize { get { return this.limit - this.Offset; } }

        public int Offset       { get; set; }

        public int End          { get { return limit; } }

        public bool Empty()
        {
            return this.Offset >= this.limit;
        }

        public Byte Peek()
        {
            return this.data[this.Offset];
        }

        public ByteBuffer2 Flip()
        {
            this.Offset = this.begin;

            return this;
        }

        public ByteBuffer2 Reset()
        {
            this.Offset = this.begin;

            return this;
        }

        public bool HasRemaining()
        {
            return this.Offset < this.limit;
        }

        public bool CanRead(int toRead)
        {
            return toRead > 0 && toRead <= RemainedSize;
        }

        #region string
        // defualt Deleimeter : 0x00
        private int getStringLength(byte delimiter = byte.MinValue)
        {
            for (int index = 0; this.Offset + index < this.limit; index++)
            {
                if (delimiter != byte.MinValue && this.data[this.Offset + index] == delimiter)
                    return index;
              
                if (this.data[this.Offset + index] == 0)
                    return index;
            }

            return (delimiter == byte.MinValue)
                ? this.limit - this.Offset
                : -1;
        }

        # region default
        public string GetDefaultString(byte delimiter)
        {
            var index = getStringLength(delimiter);
            return (index == -1) ? "" : GetDefaultString(index);
        }

        public string GetDefaultString()
        {
            var index = getStringLength();
            return (index == -1) ? "" : GetDefaultString(index);
        }

        public string GetDefaultString(int size, int at = -1, byte delimiter = byte.MinValue)
        {
            var pos = at >= 0 ? at : this.Offset;
            if (pos + size > this.data.Length)
            {
                size = this.data.Length - pos;
                if (size <= 0)
                    return "";
            }

            var here = advance(at, size);
            return Encoding.Default.GetString(this.data, here, size);
        }
        #endregion

        # region Ascii
        public string GetAscii(byte delimiter)
        {
            // Python에서 사용할 때는 아래와 같이 사용합니다.
            // from System import Byte
            // file.Read(0, 16).GetAscii(Byte(255))
            
            var index = getStringLength(delimiter);
            return (index  == -1) ? "" : GetAscii(index);
        }

        public string GetAscii()
        {
            var len = getStringLength();

            if (len == 0)
            {
                this.Offset += 1;
                return "";
            }

            if (len < 0)
                return "";

            var ascii = Encoding.ASCII.GetString(data, Offset, len);
            this.Offset += ascii.Length + 1;

            return ascii;
        }

        public string GetAscii(int size, int at = -1)
        {
            var pos = at >= 0 ? at : this.Offset;
            if (pos + size > this.data.Length)
            {
                size = this.data.Length - pos;
                if (size <= 0)
                    return "";
            }

            var here = advance(at, size);
            return Encoding.ASCII.GetString(this.data, here, size);
        }
        #endregion

        #region UTF8

        public string GetUTF8(int size, int at = -1)
        {
            var pos = at >= 0 ? at : this.Offset;
            if (pos + size > this.data.Length)
            {
                size = this.data.Length - pos;
                if (size <= 0)
                    return "";
            }

            var here = advance(at, size);
            return Encoding.UTF8.GetString(this.data, here, size);
        }

        #endregion

        private static string[] HexTbl = Enumerable.Range(0, 256).Select(v => v.ToString("X2")).ToArray();

        public string ToHex()
        {
            var hexRepr = new StringBuilder((this.limit - this.Offset) * 2);
            for (var i = this.Offset; i < this.limit; i++)
                hexRepr.Append(HexTbl[this.data[i]]);

            return hexRepr.ToString();
        }

        public string ToHexSwapNibble(bool hasPadding = false)
        {
            var hexcode = ToHex();
                       
            var str = "";
            for (var i = 0; i < hexcode.Length; i +=2 )
            {
                str += hexcode[i + 1];
                str += hexcode[i];
            }

            if (!string.IsNullOrEmpty(str) && hasPadding)
            {
                while (str[str.Length -1] == 'F')
                {
                    str = str.Substring(0, str.Length - 1);
                }
            }

            return str;
        }

        #region Unicode
        public string GetUnicode()
        {
            int pos = data.Length;
            for (int i=this.Offset; i<data.Length-1; i+=2)
            {
                if (data[i] == 0 && data[i+1] == 0)
                {
                    pos = i;
                    break;
                }
            }

            int sz = pos - this.Offset;
            var unicode = Encoding.Unicode.GetString(data, this.Offset, sz);
            this.Offset = pos + 2;

            return unicode;
        }

		public string GetUnicodeBE()
		{
			int pos = data.Length;
			for (int i = this.Offset; i < data.Length - 1; i += 2)
			{
				if (data[i] == 0 && data[i + 1] == 0)
				{
					pos = i;
					break;
				}
			}

			int sz = pos - this.Offset;
			var unicode = Encoding.BigEndianUnicode.GetString(data, this.Offset, sz);
			this.Offset = pos + 2;

			return unicode;
		}


		public string GetUnicode(int size)
        {
            if (this.Offset + size*2 > this.data.Length)
                throw new IndexOutOfRangeException(
                    "ByteBuffer2.GetUnicde: The size exceeds current buffer range");

            var res = Encoding.Unicode.GetString(this.data, this.Offset, size*2);
            this.Offset += size*2;

            return res;
        }
        #endregion

        #endregion

        #region numeric

        public long GetIntLE(int size)
        {
            if (size <= 0 || size > 8)
                throw new Exception("Invalid integer size");

            long res = -1;
            switch (size)
            {
                case 1: res = (long)this.GetSByte(); break;
                case 2: res = (long)this.GetInt16LE(); break;
                case 3: res = (long)this.GetInt24LE(); break;
                case 4: res = (long)this.GetInt32LE(); break;
                case 5: res = (long)this.GetInt40LE(); break;
                case 6: res = (long)this.GetInt48LE(); break;
                case 7: res = (long)this.GetInt56LE(); break;
                case 8: res = (long)this.GetInt64LE(); break;
            }

            return res;
        }

        public sbyte GetInt8(int at = -1)
        {
            if (this.Offset + 1 > this.data.Length)
                throw new IndexOutOfRangeException("The (index, size) pair is not valid");

            var here = advance(at, 1);
            return (sbyte)this.data[here];
        }

        public ushort GetUInt16BE(int at = -1)
        {
            var here = advance(at, 2);

            var r = (UInt16) this.data[here]; r <<= 8;
            r |= this.data[here+1];

            return (UInt16)r;
        }

        public short GetInt16BE(int at = -1)
        {
            return (short)this.GetUInt16BE(at);
        }

        public ushort GetUInt16LE(int at = -1)
        {
            var here = advance(at, 2);

            var r = (UInt16)this.data[here + 1]; r <<= 8;
            r |= this.data[here];

            return r;
        }

        public float GetFloat(int at = -1)
        {
            if (at >= this.data.Length)
                return 0.0f;

            var f = BitConverter.ToSingle(this.data, at);

            return f;
        }

        public short GetInt16LE(int at = -1)
        {
            return (short)this.GetUInt16LE(at);
        }

        public uint GetUInt24BE(int at = -1)
        {
            var here = advance(at, 3);

            var r = (uint)this.data[here]; r <<= 8;
            r |= this.data[here + 1]; r <<= 8;
            r |= this.data[here + 2]; 

            return r;
        }

        public int GetInt24BE(int at = -1)
        {
            var here = advance(at, dist: 3);

            var first = this.data[here]; 
            var l = (int) leadingByte(first);
            var r = (l << 8) | first; r <<= 8;         // complete first 2
            r |= this.data[here + 1]; r <<= 8;         // complete first 3
            r |= this.data[here + 2];                  // complete first 4

            return (int)r;
        }

        public uint GetUInt24LE(int at = -1)
        {
            var here = advance(at, 3);

            var r = (uint)this.data[here+2]; r <<= 8;
            r |= this.data[here+1]; r <<= 8;
            r |= this.data[here];

            return r;
        }

        public int GetInt24LE(int at = -1)
        {
            var here = advance(at, dist: 3);

            var first = this.data[here+2]; 
            var l = (int) leadingByte(first);
            var r = (l << 8) | first; r <<= 8;         // complete first 2
            r |= this.data[here + 1]; r <<= 8;         // complete first 3
            r |= this.data[here];                      // complete first 4

            return (int)r;
        }

        public uint GetUInt32BE(int at = -1)
        {
            var here = advance(at, 4);

            var r = (uint)this.data[here]; r <<= 8;
            r |= this.data[here + 1]; r <<= 8;
            r |= this.data[here + 2]; r <<= 8;
            r |= this.data[here + 3];

            return r;
        }

        public int GetInt32BE(int at = -1)
        {
            return (int)this.GetUInt32BE(at);
        }

        public uint GetUInt32LE(int at = -1)
        {
            var here = advance(at, 4);

            var r = (uint)this.data[here+3]; r <<= 8;
            r |= this.data[here + 2]; r <<= 8;
            r |= this.data[here + 1]; r <<= 8;
            r |= this.data[here];

            return r;
        }

        public int GetInt32LE(int at = -1)
        {
            return (int)this.GetUInt32LE(at);
        }

        public ulong GetUInt40BE(int at = -1)
        {
            var here = advance(at, 5);

            var r = (ulong)this.data[here]; r <<= 8;
            for (int i=1; i<4; i++) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here + 4]; 

            return r;
        }

        public long GetInt40BE(int at = -1)
        {
            var here = advance(at, dist: 5);

            var first = this.data[here]; 
            var l = (long) leadingByte(first);
            var r = (l << 24) | (l << 16) | (l << 8) | first; r <<= 8; // completed first 4

            for (int i=1; i<=3; i++) { r |= this.data[here + i]; r <<= 8; } // 5, 6, 7 
            r |= this.data[here + 4];                                       // complete first 8

            return r;
        }

        public ulong GetUInt40LE(int at = -1)
        {
            var here = advance(at, 5);

            var r = (ulong)this.data[here+4]; r <<= 8;
            for (int i=3; i>0; i--) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here];

            return r;
        }

        public long GetInt40LE(int at = -1)
        {
            var here = advance(at, dist: 5);

            var first = this.data[here+4]; 
            var l = (long) leadingByte(first);
            var r = (l << 24) | (l << 16) | (l << 8) | first; r <<= 8;
                                                 // completed first 4
            for (int i=3; i>0; i--) { r |= this.data[here + i]; r <<= 8; } // 4, 5, 6, 7
            r |= this.data[here + 0];            // complete first 8

            return r;
        }

        public ulong GetUInt48BE(int at = -1)
        { 
            var here = advance(at, 6);

            var r = (ulong)this.data[here]; r <<= 8;
            for (int i=1; i<=4; i++) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here + 5]; 

            return r;
        }

        public long GetInt48BE(int at = -1)
        { 
            var here = advance(at, dist: 6);

            var first = this.data[here]; 
            var l = (long) leadingByte(first);
            var r = (l << 16) | (l << 8) | first; r <<= 8; // completed first 3

            for (int i=1; i<=4; i++) { r |= this.data[here + i]; r <<= 8; } // 4, 5, 6, 7
            r |= this.data[here + 5];                                       // complete first 8

            return r;
        }

        public ulong GetUInt48LE(int at = -1)
        { 
            var here = advance(at, 6);

            var r = (ulong)this.data[here+5]; r <<= 8;
            for (int i=4; i>0; i--) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here];

            return r;
        }

        public long GetInt48LE(int at = -1)
        { 
            var here = advance(at, dist: 6);

            var first = this.data[here+5]; 
            var l = (long) leadingByte(first);
            var r = (l << 16) | (l << 8) | first; r <<= 8;                 // completed first 3
            for (int i=4; i>0; i--) { r |= this.data[here + i]; r <<= 8; } // 4, 5, 6, 7
            r |= this.data[here + 0];                                      // complete first 8

            return r;
        }

        public ulong GetUInt56BE(int at = -1)
        { 
            var here = advance(at, 7);

            var r = (ulong)this.data[here]; r <<= 8;
            for (int i=1; i<=5; i++) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here + 6]; 

            return r;
        }

        public long GetInt56BE(int at = -1)
        { 
            var here = advance(at, dist: 7);

            var first = this.data[here]; 
            var l = (long) leadingByte(first);
            var r = (l << 8) | first; r <<= 8; // completed first 2

            for (int i=1; i<=5; i++) { r |= this.data[here + i]; r <<= 8; } // 3, 4, 5, 6, 7
            r |= this.data[here + 6];                                       // complete first 8

            return r;
        }

        public ulong GetUInt56LE(int at = -1)
        { 
            var here = advance(at, 7);

            var r = (ulong)this.data[here+6]; r <<= 8;
            for (int i=5; i>0; i--) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here];

            return r;
        }

        public long GetInt56LE(int at = -1)
        { 
            var here = advance(at, dist: 7);

            var first = this.data[here+6]; 
            var l = (long) leadingByte(first);
            var r = (l << 8) | first; r <<= 8;                             // completed first 2
            for (int i=5; i>0; i--) { r |= this.data[here + i]; r <<= 8; } // 3, 4, 5, 6, 7
            r |= this.data[here + 0];                                      // complete first 8

            return r;
        }

        public ulong GetUInt64BE(int at = -1)
        { 
            var here = advance(at, 8);

            var r = (ulong)this.data[here]; r <<= 8;
            for (int i=1; i<7; i++) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here + 7]; 

            return r;
        }

        public long GetInt64BE(int at = -1)
        { 
            return (long)this.GetUInt64BE(at);
        }

        public ulong GetUInt64LE(int at = -1)
        { 
            var here = advance(at, 8);

            var r = (ulong)this.data[here+7]; r <<= 8;
            for (int i=6; i>0; i--) { r |= this.data[here + i]; r <<= 8; }
            r |= this.data[here];

            return r;
        }

        public long GetInt64LE(int at = -1)
        { 
            return (long)this.GetUInt64LE(at);
        }

        public double GetDouble(int at = -1)
        {
            var here = advance(at, 8);

            var arr = new byte[8];
            System.Buffer.BlockCopy(this.data, here, arr, 0, 8);
            Array.Reverse(arr);
            return BitConverter.ToDouble(arr, 0);
        }

        public double GetDoubleLE(int at = -1)
        {
            var here = advance(at, 8);

            var arr = new byte[8];
            System.Buffer.BlockCopy(this.data, here, arr, 0, 8);

            return BitConverter.ToDouble(arr, 0);
        }

       
        #endregion


        public Tuple<int, int> GetNibbles(int at=-1)
        {
            var tmp = this.GetByte(at);
            var hi = (tmp & 0xF0) >> 4;
            var lo = (tmp & 0x0F);

            return Tuple.Create(hi, lo);
        }

        public byte GetByte(int at = -1)
        {
            if (this.Offset + 1 > this.data.Length)
                throw new IndexOutOfRangeException("The (index, size) pair is not valid");

            var here = advance(at, 1);
            return this.data[here];
        }

        public sbyte GetSByte(int at = -1)
        {
            if (this.Offset + 1 > this.data.Length)
                throw new IndexOutOfRangeException("The (index, size) pair is not valid");

            var here = advance(at, 1);
            return (sbyte)this.data[here];
        }

        public byte[] GetBytes(int size, int at = -1)
        {
            if (this.Offset + size > this.data.Length)
                throw new IndexOutOfRangeException("The (index, size) pair is not valid");

            var bytes = new byte[size];
            var here = advance(at, size);
            System.Buffer.BlockCopy(this.data, here, bytes, 0, size);
            return bytes;
        }

        public byte[] GetBytesUntil(byte value)
        {
            int index = -1;
            for (int i=this.Offset; i<this.data.Length; i++)
            {
                if (this.data[i] == value)
                {
                    index = i;
                    break;
                }
            }

            int count = index - this.Offset;
            if (count <= 0) return null;

            var bytes = new byte[count]; 
            System.Buffer.BlockCopy(this.data, this.Offset, bytes, 0, count);

            return bytes;
        }


        public string GetStringUTF8(int size)
        {
            return Encoding.UTF8.GetString(GetBytes(size));
        }

        public ByteBuffer2 SetUnicode(string s, bool addNullAtEmpty = false)
        {
            if (!string.IsNullOrEmpty(s))
            {
                var bytes = Encoding.Unicode.GetBytes(s);
                System.Buffer.BlockCopy(bytes, 0, this.data, this.Offset, bytes.Length);
                this.Offset += 2 * s.Length;

                addNullAtEmpty = true;
            }

            if (addNullAtEmpty)
            {
                this.data[this.Offset] = 0; this.Offset++;
                this.data[this.Offset] = 0; this.Offset++;
            }

            return this;
        }

        public ByteBuffer2 Unget(int count)
        {
            this.Offset -= count;
            return this;
        }

        public ByteBuffer2 Skip(int count)
        {
            this.Offset += count;

            return this;
        }

        public ByteBuffer2 Append(byte b)
        {
            var bytes = new byte[] { b };

            return this.Append(bytes, 0, 1);
        }

        public ByteBuffer2 Append(ByteBuffer2 buffer)
        {
            var data   = buffer.data;
            var offset = buffer.Offset;
            var count  = buffer.RemainedSize;
            return this.Append(data, offset, count);
        }
        

        public ByteBuffer2 Append(byte[] buffer, int offset, int count)
        {
            if (buffer != null && count > 0)
            { 
                var len = (this.data == null) ? 0 : this.data.Length;

                var newBuf = new byte[len + count];
                if (this.data != null)
                { 
                    System.Buffer.BlockCopy(this.data, 0, newBuf, 0, len);   // copy previous buffer
                }

                System.Buffer.BlockCopy(buffer, offset, newBuf, len, count); // append new buffer
                this.data  = newBuf;
                this.count += count;
                this.limit += count;
            }

            return this;
        }

        public ByteBuffer2 Slice(int offset, int count)
        {
            return new ByteBuffer2(this.CopyRange(offset, count));
        }

        public bool CompareRange(int offset, int count, byte value)
        {
            for (int i=offset; i<offset+count; i++)
                if (this.data[i] != value)
                    return false;

            return true;
        }

        public bool CompareRange(int offset, int count, byte[] value)
        {
            if (this.data.Length < offset + count || value.Length < count)
                return false;

            for (int i = 0; i < count; i++)
                if (this.data[i + offset] != value[i])
                    return false;

            return true;
        }

        public byte[] CopyRange(int from, int count)
        {
            /*
            if (from + count > this.limit)
                throw new IndexOutOfRangeException("ByteBuffer2.CopyRange: buffer out of range");
            */

            var result = new byte[count];
            System.Buffer.BlockCopy(this.data, from, result, 0, count);
            return result;
        }

        

        private byte leadingByte(byte b)
        {
            return ((int)b & 0x80) == 0 ? (byte)0 : (byte)0xFF;
        }

        private int advance(int at, int dist)
        {
            var here = (at == -1) ? this.Offset : this.begin + at;
            if (at == -1)
                this.Offset += dist;

            return here;
        }

        public override string ToString()
        {
            return string.Format("begin: 0x{0:x}, offset: 0x{1:x}, remained: 0x{2:x}, limit: 0x{3:x}", 
                begin, Offset, RemainedSize, limit);
        }
    }
}
