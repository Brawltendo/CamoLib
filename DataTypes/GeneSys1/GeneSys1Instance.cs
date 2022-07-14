using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using CamoLib.Attributes;
using CamoLib.IO;
using CamoLib.IO.Readers;
using CamoLib.IO.Readers.GeneSys1;
using Zio;
using Zio.FileSystems;

namespace CamoLib.DataTypes.GeneSys1
{
    /// <summary>
    /// A data container that takes its layout from a corresponding GeneSysDefinition.
    /// </summary>
    public class GeneSys1Instance
    {

        /// <summary>
        /// The offset in the file where the fields begin.
        /// </summary>
        [Hidden(true)]
        public int InstFieldsOffset { get; set; }

        /// <summary>
        /// The class ID to search for in order to get the fields' type info.
        /// </summary>
        [Hidden(true)]
        public int InstClass { get; set; }

        /// <summary>
        /// Unique ID for this instance, used for handles (pointers to another instance file).
        /// </summary>
        [Hidden(true)]
        public int InstanceID { get; set; }

        /// <summary>
        /// List of fields generated according to the instance class.
        /// </summary>
        List<object> Fields { get; set; }

        private GeneSys1Definition definition;
        private CamoStreamReader reader;

        public GeneSys1Instance()
        {
            Fields = new List<object>();
            //Fields = new ArrayList();
        }

        public void Read(Stream inStream, MemoryFileSystem fs, Endian endianness = Endian.Little)
        {
            using (reader = new CamoStreamReader(inStream))
            {
                reader.Position += 0x8L;
                InstFieldsOffset = reader.ReadInt(endianness);
                InstClass = reader.ReadInt(endianness);
                InstanceID = reader.ReadInt(endianness);

                reader.BaseStream.Position = InstFieldsOffset;
                GetInstanceBaseResource(fs, endianness);

                var fieldsPos = reader.Position;
                foreach (GeneSys1Field field in definition.Fields)
                {
                    reader.AlignStream(4);
                    Fields.Add(DataTypesReader.GetDataType(field.FieldType, reader, endianness, fs));
                }
            }
        }

        public void Read(MemoryFileSystem fs, bool isChild, Endian endianness = Endian.Little)
        {
            reader.Position += 0x8L;
            InstFieldsOffset = reader.ReadInt(endianness);
            InstClass = reader.ReadInt(endianness);
            if (!isChild) InstanceID = reader.ReadInt(endianness);

            reader.BaseStream.Position = InstFieldsOffset;
            GetInstanceBaseResource(fs, endianness);

            var fieldsPos = reader.Position;
            foreach (GeneSys1Field field in definition.Fields)
            {
                reader.AlignStream(4);
                Fields.Add(DataTypesReader.GetDataType(field.FieldType, reader, endianness, fs));
            }
        }


        /// <summary>
        /// Finds the base class for this instance in order to fill the field data.
        /// </summary>
        /// <param name="fs">Memory file system in which the resource file is located</param>
        /// <param name="endianness"></param>
        void GetInstanceBaseResource(MemoryFileSystem fs, Endian endianness = Endian.Little)
        {
            var files = fs.EnumeratePaths("/Type_12000000/").Select(info => info.GetName()).ToList();
            foreach (UPath file in files)
            {
                Stream st = fs.OpenFile("/Type_12000000/" + file, FileMode.Open, FileAccess.Read, FileShare.Read);
                CamoStreamReader reader = new CamoStreamReader(st);

                if (reader.ReadInt(endianness) == InstClass)
                {
                    reader.Position = 0;
                    definition = new GeneSys1Definition();
                    definition.Read(st, endianness);
                    reader.Dispose();
                    break;
                }
            }
        }
    }
}
