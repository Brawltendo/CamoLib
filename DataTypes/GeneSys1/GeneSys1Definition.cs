using CamoLib.IO;
using CamoLib.IO.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamoLib.DataTypes.GeneSys1
{
    /// <summary>
    /// Defines the layout of a class for use in an Instance.
    /// </summary>
    public class GeneSys1Definition// : Asset
    {
        /// <summary>
        /// Unique ID for this resource.
        /// </summary>
        public uint ResourceID { get; set; }

        /// <summary>
        /// Unique ID for this resource's type.
        /// </summary>
        public uint BaseTypeID { get; set; }

        /// <summary>
        /// Offset in the file where the field type info begins.
        /// </summary>
        public int FieldsOffset { get; set; }

        /// <summary>
        /// Number of fields to read.
        /// </summary>
        public int NumFields { get; set; }

        /// <summary>
        /// Greater than zero if this resource is a subtype of the base type.
        /// </summary>
        public byte Subtype { get; set; }

        /// <summary>
        /// Name of this resource's type/subtype.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// List of all the field data in this resource.
        /// </summary>
        public List<GeneSys1Field> Fields { get; set; }

        public GeneSys1Definition()
        {
            Fields = new List<GeneSys1Field>();
        }

        public void Read(Stream inStream, Endian endianness = Endian.Little)
        {
            using (CamoStreamReader reader = new CamoStreamReader(inStream))
            {
                ResourceID = reader.ReadUInt(endianness);
                BaseTypeID = reader.ReadUInt(endianness);
                FieldsOffset = reader.ReadInt(endianness);
                NumFields = reader.ReadInt(endianness);
                Subtype = reader.ReadByte();
                Name = reader.ReadNullTerminatedString();

                reader.Position = FieldsOffset;

                for (var i = 0; i < NumFields; ++i)
                {
                    GeneSys1Field field = new GeneSys1Field();
                    field.FieldHash = reader.ReadUInt(endianness);
                    field.FieldType = (EValueType)reader.ReadUInt(endianness);
                    field.Elements = reader.ReadInt(endianness);
                    field.FieldOffset = reader.ReadInt(endianness);
                    field.FieldSize = reader.ReadInt(endianness);
                    Fields.Add(field);
                }
            }
        }
    }
}
