using CamoLib.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamoLib.DataTypes.GeneSys1
{
    public class VariableArrayType
    {
        #region Constructors
        public VariableArrayType(EValueType type)
        {
            Members = new List<object>();
            _fullType = type;
        }
        #endregion

        #region Properties
        /// <summary>
        /// The data type that array members should use.
        /// </summary>
        public EValueType MemberType
        {
            get
            {
                return _fullType ^ EValueType.VariableArray;
            }
        } private EValueType _fullType;

        /// <summary>
        /// The offset in the file at which this array starts.
        /// </summary>
        [Hidden(true)]
        public int Offset { get; set; }

        /// <summary>
        /// The amount of elements this array contains.
        /// </summary>
        [Hidden(true)]
        public int NumElements { get; set; }

        [Hidden(true)]
        public int Unk { get; set; }

        /// <summary>
        /// All members of the array.
        /// </summary>
        public List<object> Members { get; set; }
        #endregion

    }
}
