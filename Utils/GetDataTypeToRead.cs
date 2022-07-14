using CamoLib.DataTypes;
using CamoLib.IO.Readers;
using System;
using System.IO;
using System.Reflection;

namespace CamoCLI.Utils
{
    public class GetDataTypeToRead
    {
        public static dynamic fieldValue;
        public static GeneSys1ReaderOld geneSys1Reader = new GeneSys1ReaderOld();
        public static void ReadDataTypeFromRes(EValueType typeData, BinaryReader br, StreamWriter file, bool isInst, bool isHPR)
        {
            switch(typeData)
            {
                case EValueType.Int32:
                    fieldValue = br.ReadInt32();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    break;

                case EValueType.Float32:
                    fieldValue = br.ReadSingle();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    break;

                case EValueType.Bool:
                    fieldValue = br.ReadBoolean();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    break;

                case EValueType.String:
                    fieldValue = br.ReadInt32();
                    long originalPos = br.BaseStream.Position;

                    br.BaseStream.Seek(originalPos - 4 + fieldValue, SeekOrigin.Begin);
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + NullTerminatedStringReader.Read(br));
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + NullTerminatedStringReader.Read(br));

                    br.BaseStream.Position = originalPos + 4;
                    break;

                case EValueType.ResourceHandle:
                    fieldValue = br.ReadInt64();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    break;

                case EValueType.Instance:
                    fieldValue = br.ReadInt32();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    geneSys1Reader.ReadChildInstance(file, br, isHPR);
                    break;

                case EValueType.RwVector2:
                    PropertyInfo[] vec2Props = typeof(Vector2).GetProperties();

                    foreach (PropertyInfo property in vec2Props)
                    {
                        fieldValue = br.ReadSingle();
                        if (isInst)
                            file.WriteLine("".PadLeft(28) + "FieldValue." + property.Name + ": " + fieldValue);
                        else
                            file.WriteLine("".PadLeft(12) + "FieldValue." + property.Name + ": " + fieldValue);
                    }
                    br.ReadInt64();
                    break;

                case EValueType.RwVector3:
                    PropertyInfo[] vec3Props = typeof(Vector3).GetProperties();

                    foreach (PropertyInfo property in vec3Props)
                    {
                        fieldValue = br.ReadSingle();
                        if (isInst)
                            file.WriteLine("".PadLeft(28) + "FieldValue." + property.Name + ": " + fieldValue);
                        else
                            file.WriteLine("".PadLeft(12) + "FieldValue." + property.Name + ": " + fieldValue);
                    }
                    br.ReadInt32();
                    break;

                case EValueType.RwVector4:
                    PropertyInfo[] vec4Props = typeof(Vector4).GetProperties();

                    foreach (PropertyInfo property in vec4Props)
                    {
                        fieldValue = br.ReadSingle();
                        if (isInst)
                            file.WriteLine("".PadLeft(28) + "FieldValue." + property.Name + ": " + fieldValue);
                        else
                            file.WriteLine("".PadLeft(12) + "FieldValue." + property.Name + ": " + fieldValue);
                    }
                    break;

                case EValueType.RwMatrix44:
                    PropertyInfo[] matrix44Props = typeof(Vector4).GetProperties();
                    for (int i = 0; i < 4; i++)
                    {
                        foreach (PropertyInfo property in matrix44Props)
                        {
                            fieldValue = br.ReadSingle();
                            if (isInst)
                                file.WriteLine("".PadLeft(28) + "FieldValue." + property.Name + ": " + fieldValue);
                            else
                                file.WriteLine("".PadLeft(12) + "FieldValue." + property.Name + ": " + fieldValue);
                        }
                    }
                    break;

                case EValueType.Array:
                    GeneSys1ReaderOld.geneSys1ArrayPtr.Offset = br.ReadInt32();
                    GeneSys1ReaderOld.geneSys1ArrayPtr.Elements = br.ReadInt32();
                    GeneSys1ReaderOld.geneSys1ArrayPtr.unk = br.ReadInt32();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + GeneSys1ReaderOld.geneSys1ArrayPtr.Elements);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + GeneSys1ReaderOld.geneSys1ArrayPtr.Elements);
                    try
                    {
                        geneSys1Reader.ReadArrayPointer(file, br, isHPR);
                    }
                    catch (Exception)
                    {

                    }
                    break;

                default:
                    fieldValue = br.ReadInt32();
                    if (isInst)
                        file.WriteLine("".PadLeft(28) + "FieldValue: " + fieldValue);
                    else
                        file.WriteLine("".PadLeft(12) + "FieldValue: " + fieldValue);
                    break;
            }
        }
    }
}
