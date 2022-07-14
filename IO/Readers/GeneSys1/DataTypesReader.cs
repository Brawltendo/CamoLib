using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CamoLib.DataTypes;
using CamoLib.DataTypes.GeneSys1;
using Zio.FileSystems;

namespace CamoLib.IO.Readers.GeneSys1
{
    class DataTypesReader
    {
        public static object GetDataType(EValueType type, CamoStreamReader reader, Endian endianness = Endian.Little, MemoryFileSystem fs = null)
        {
            switch (type)
            {
                case EValueType.Int32:
                    reader.AlignStream(4);
                    return reader.ReadInt(endianness);

                case EValueType.Float32:
                    reader.AlignStream(4);
                    return reader.ReadFloat(endianness);

                case EValueType.Bool:
                    return reader.ReadBoolean();

                case EValueType.String:
                    reader.AlignStream(4);
                    var value = reader.ReadInt(endianness);
                    var originalPos = reader.Position;

                    reader.Position = originalPos - 4 + value;
                    var str = NullTerminatedStringReader.Read(reader);

                    reader.Position = originalPos + 4;
                    return str;

                case EValueType.ResourceHandle:
                    reader.AlignStream(4);
                    return reader.ReadLong(endianness);

                case EValueType.Instance:
                    var offset = reader.ReadInt(endianness);
                    var oldPos = reader.Position;
                    GeneSys1Instance inst = new GeneSys1Instance();

                    reader.Position = offset;
                    inst.Read(fs, true, endianness);
                    reader.Position = oldPos;
                    return inst;

                case EValueType.RwVector2:
                    reader.AlignStream(4);
                    return reader.ReadVector2(endianness);

                case EValueType.RwVector3:
                    reader.AlignStream(4);
                    return reader.ReadVector3(endianness);

                case EValueType.RwVector4:
                    reader.AlignStream(4);
                    return reader.ReadVector4(endianness);

                case EValueType.RwMatrix44:
                    reader.AlignStream(4);
                    return reader.ReadMatrix4x4(endianness);

                default:
                    if ((type & EValueType.VariableArray) == EValueType.VariableArray)
                    {
                        reader.AlignStream(4);
                        VariableArrayType array = new VariableArrayType(type);
                        array.Offset = reader.ReadInt();
                        array.NumElements = reader.ReadInt();
                        array.Unk = reader.ReadInt();

                        long origPos = reader.Position;
                        reader.Position = array.Offset;

                        for (int i = 0; i < array.NumElements; ++i)
                        {
                            array.Members.Add(GetDataType(array.MemberType, reader, endianness, fs));
                        }
                        reader.Position = origPos;
                        return array;
                    }
                    return 0;
            }
        }
    }
}
