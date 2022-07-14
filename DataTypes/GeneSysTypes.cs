using System;
using System.Collections.Generic;


namespace CamoLib
{
    public enum GeneSysVersion
    {
        /// <summary>
        /// Need for Speed Hot Pursuit
        /// </summary>
        GeneSys1,
        /// <summary>
        /// Need for Speed Hot Pursuit Remastered
        /// </summary>
        GeneSys1R,
        /// <summary>
        /// Need for Speed Most Wanted
        /// </summary>
        GeneSys2
    }
}


namespace CamoLib.DataTypes
{
    public class GeneSys1ResourceBase
    {
        public int ResourceType { get; set; }
        public int ResourceID { get; set; }
        public int FieldStartOffset { get; set; }
        public uint NumOfFields { get; set; }
        public byte ResourceIndex { get; set; }
        public string ResourceClassName { get; set; }
        public List<GeneSys1FieldDef> Fields { get; set; }
    }

    public class GeneSys1FieldDef 
    { 
        public int FieldHash { get; set; }
        public EValueType FieldType { get; set; }
        public int Elements { get; set; }
        public int FieldOffset { get; set; }
        public int FieldLength { get; set; }
    }

    public class GeneSys1InstanceBase
    {
        public int Offset { get; set; }
        public int InstanceBaseClass { get; set; }
        public int InstanceID { get; set; }
    }

    public class GeneSys1InstanceChild
    {
        public int Offset { get; set; }
        public int InstanceBaseClass { get; set; }
    }

    public class GeneSys1ArrayPointer
    {
        public int Offset { get; set; }
        public int Elements { get; set; }
        public int unk { get; set; }

        public static implicit operator GeneSys1ArrayPointer(GeneSys1InstanceChild v)
        {
            throw new NotImplementedException();
        }
    }

    public class GeneSysString
    {
        public int Offset { get; set; }
        public int Length { get; set; }
    }

    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Vector4
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float W { get; set; }
    }

    public class Matrix4x4
    {
        public Vector4 V0 { get; set; }
        public Vector4 V1 { get; set; }
        public Vector4 V2 { get; set; }
        public Vector4 V3 { get; set; }
    }

    public enum EValueType
    {
        Int32 = 0x0,
        Float32 = 0x1,
        Bool = 0x2,
        unk1 = 0x3,
        String = 0x4,
        WideString = 0x5,
        ResourceHandle = 0x6,
        ResourceID = 0x7,
        Instance = 0x8,
        Enumeration = 0x9,
        RwVector2 = 0xA,
        RwVector3 = 0xB,
        RwVector4 = 0xC,
        RwMatrix44 = 0xD,
        RwMatrix44Affine = 0xE,
        VariableArray = 0x1000,
        unk = 4102,
        Array = 4104
    }

}
