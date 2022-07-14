using CamoLib.DataTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamoLib.IO.Readers
{
    public class CamoStreamReader : IDisposable
    {

		public Stream BaseStream
		{
			get
			{
				return stream;
			}
		}
		protected Stream stream;

		protected byte[] buffer;
		protected char[] charBuffer;
		
		protected Encoding wideDecoder;

		public virtual long Position
		{
			get
			{
				if (stream == null)
				{
					return 0L;
				}
				return stream.Position;
			}
			set
			{
				stream.Position = value;
			}
		}

		public virtual long Length
		{
			get
			{
				return streamLength;
			}
		}
		protected long streamLength;

		public CamoStreamReader(Stream inStream)
		{
			stream = inStream;
			if (stream != null)
			{
				streamLength = stream.Length;
			}
			wideDecoder = new UnicodeEncoding();
			buffer = new byte[20];
			charBuffer = new char[2];
		}

		public string ReadNullTerminatedString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (; ; )
			{
				char c = (char)ReadByte();
				if (c == '\0')
				{
					break;
				}
				stringBuilder.Append(c);
			}
			return stringBuilder.ToString();
		}

		public bool ReadBoolean()
		{
			return ReadByte() == 1;
		}

		public byte ReadByte()
		{
			FillBuffer(1);
			return this.buffer[0];
		}

		public sbyte ReadSByte()
		{
			FillBuffer(1);
			return (sbyte)this.buffer[0];
		}

		public short ReadShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (short)((int)buffer[0] | (int)buffer[1] << 8);
			}
			return (short)((int)buffer[1] | (int)buffer[0] << 8);
		}

		public ushort ReadUShort(Endian inEndian = Endian.Little)
		{
			FillBuffer(2);
			if (inEndian == Endian.Little)
			{
				return (ushort)((int)buffer[0] | (int)buffer[1] << 8);
			}
			return (ushort)((int)buffer[1] | (int)buffer[0] << 8);
		}

		public int ReadInt(Endian inEndian = Endian.Little)
		{
			FillBuffer(4);
			if (inEndian == Endian.Little)
			{
				return (int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24;
			}
			return (int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24;
		}

		public uint ReadUInt(Endian inEndian = Endian.Little)
		{
			FillBuffer(4);
			if (inEndian == Endian.Little)
			{
				return (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
			}
			return (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
		}

		public long ReadLong(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			if (inEndian == Endian.Little)
			{
				return (long)((ulong)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24) << 32 | (ulong)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24));
			}
			return (long)((ulong)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24) << 32 | (ulong)((int)buffer[7] | (int)buffer[6] << 8 | (int)buffer[5] << 16 | (int)buffer[4] << 24));
		}

		public ulong ReadULong(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			if (inEndian == Endian.Little)
			{
				return (ulong)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24) << 32 | (ulong)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
			}
			return (ulong)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24) << 32 | (ulong)((int)buffer[7] | (int)buffer[6] << 8 | (int)buffer[5] << 16 | (int)buffer[4] << 24);
		}

		public unsafe float ReadFloat(Endian inEndian = Endian.Little)
		{
			FillBuffer(4);
			uint num = 0U;
			if (inEndian == Endian.Little)
			{
				num = (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
			}
			else
			{
				num = (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
			}
			return *(float*)(&num);
		}

		public unsafe double ReadDouble(Endian inEndian = Endian.Little)
		{
			FillBuffer(8);
			uint num;
			uint num2;
			if (inEndian == Endian.Little)
			{
				num = (uint)((int)buffer[0] | (int)buffer[1] << 8 | (int)buffer[2] << 16 | (int)buffer[3] << 24);
				num2 = (uint)((int)buffer[4] | (int)buffer[5] << 8 | (int)buffer[6] << 16 | (int)buffer[7] << 24);
			}
			else
			{
				num = (uint)((int)buffer[3] | (int)buffer[2] << 8 | (int)buffer[1] << 16 | (int)buffer[0] << 24);
				num2 = (uint)((int)buffer[7] | (int)buffer[6] << 8 | (int)buffer[5] << 16 | (int)buffer[4] << 24);
			}
			ulong num3 = (ulong)num2 << 32 | (ulong)num;
			return *(double*)(&num3);
		}

		public Vector2 ReadVector2(Endian inEndian = Endian.Little)
        {
            var vec = new Vector2
            {
                X = ReadFloat(inEndian),
                Y = ReadFloat(inEndian)
            };
            return vec;
		}

		public Vector3 ReadVector3(Endian inEndian = Endian.Little)
		{
            var vec = new Vector3
            {
                X = ReadFloat(inEndian),
                Y = ReadFloat(inEndian),
                Z = ReadFloat(inEndian)
            };
            return vec;
		}

		public Vector4 ReadVector4(Endian inEndian = Endian.Little)
		{
            var vec = new Vector4
            {
                X = ReadFloat(inEndian),
                Y = ReadFloat(inEndian),
                Z = ReadFloat(inEndian),
                W = ReadFloat(inEndian)
            };
            return vec;
		}

		public Matrix4x4 ReadMatrix4x4(Endian inEndian = Endian.Little)
		{
            var matrix = new Matrix4x4
            {
                V0 = ReadVector4(inEndian),
                V1 = ReadVector4(inEndian),
                V2 = ReadVector4(inEndian),
                V3 = ReadVector4(inEndian)
            };
            return matrix;
		}

		public void AlignStream(int alignment)
        {
			while (Position % alignment != 0L)
			{
				Position++;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void FillBuffer(int numBytes)
		{
			stream.Read(buffer, 0, numBytes);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stream stream = this.stream;
				this.stream = null;
				if (stream != null)
				{
					stream.Close();
				}
			}
			stream = null;
			buffer = null;
		}

	}
}
