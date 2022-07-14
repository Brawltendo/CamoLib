using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamoLib.DataTypes.GeneSys1
{
    public class GeneSys1Field
    {

        /// <summary>
        /// CRC32 hash of the field's name
        /// </summary>
        public uint FieldHash { get; set; }

        /// <summary>
        /// The type of this field
        /// </summary>
        public EValueType FieldType { get; set; }

        /// <summary>
        /// The number of elements this field stores if it's an array
        /// </summary>
        public int Elements { get; set; }
        public int FieldOffset { get; set; }
        public int FieldSize { get; set; }
    }
}
