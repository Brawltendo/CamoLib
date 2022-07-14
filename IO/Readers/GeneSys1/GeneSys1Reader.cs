using System;
using System.IO;
using System.Reflection;
using CamoCLI.Utils;
using CamoLib.DataTypes;
using CamoLib.Utils;

namespace CamoLib.IO.Readers.GeneSys1
{
    class GeneSys1Reader
    {
        public static GeneSys1FieldDef geneSys1Field = new GeneSys1FieldDef();
        public static GeneSys1InstanceBase geneSys1Inst = new GeneSys1InstanceBase();
        public static GeneSys1InstanceBase geneSys1InstChild = new GeneSys1InstanceBase();
        public static GeneSys1ArrayPointer geneSys1ArrayPtr = new GeneSys1ArrayPointer();
        public FindInstBaseClass instFinder = new FindInstBaseClass();
        public string baseClassPath;
        public string baseClassPathInst;
        //public static int currentField = 0;

        public static void ReadResource(string path, StreamWriter file, BinaryReader br, bool isHPR)
        {
            GeneSys1ResourceBase geneSys1Resource = new GeneSys1ResourceBase();
            PropertyInfo[] resourceBaseProps = typeof(GeneSys1ResourceBase).GetProperties();

            using (BinaryReader reader = new BinaryReader(File.Open(path,
                                                            FileMode.Open,
                                                            FileAccess.Read,
                                                            FileShare.ReadWrite)))
            {
                geneSys1Resource.ResourceType = reader.ReadInt32();
                geneSys1Resource.ResourceID = reader.ReadInt32();
                geneSys1Resource.FieldStartOffset = (int)(isHPR ? reader.ReadInt64() : reader.ReadInt32());
                geneSys1Resource.NumOfFields = reader.ReadUInt32();
                geneSys1Resource.ResourceIndex = reader.ReadByte();
                geneSys1Resource.ResourceClassName = NullTerminatedStringReader.Read(reader);

                foreach (PropertyInfo property in resourceBaseProps)
                {
                    if (property.Name != "NumOfFields" || property.Name != "ResourceIndex" || property.Name != "ResourceClass")
                        file.WriteLine("".PadLeft(4) + property.Name + ": 0x{0:X8}", property.GetValue(geneSys1InstChild));
                    else file.WriteLine("".PadLeft(4) + property.Name + ": " + property.GetValue(geneSys1InstChild)); ;
                }

                PropertyInfo[] resourceFieldProps = typeof(GeneSys1FieldDef).GetProperties();

                file.WriteLine("\n" + "".PadLeft(4) + "ResourceFields: ");
                reader.BaseStream.Seek(geneSys1Resource.FieldStartOffset, SeekOrigin.Begin);

                for (int currentField = 0; currentField < geneSys1Resource.NumOfFields; currentField++)
                {
                    file.WriteLine("\n" + "".PadLeft(8) + "Field" + currentField + ":");

                    geneSys1Field.FieldHash = reader.ReadInt32();
                    //geneSys1Field.FieldType = (EGeneSys1Types)reader.ReadInt32();
                    geneSys1Field.Elements = reader.ReadInt32();
                    geneSys1Field.FieldOffset = reader.ReadInt32();
                    geneSys1Field.FieldLength = reader.ReadInt32();

                    foreach (PropertyInfo property in resourceFieldProps)
                    {
                        if (property.Name == "FieldType")
                            file.WriteLine("".PadLeft(12) + property.Name + ": " + property.GetValue(geneSys1Field));
                        if (property.Name == "Elements" && geneSys1Field.Elements > 1)
                            file.WriteLine("".PadLeft(12) + property.Name + ": " + property.GetValue(geneSys1Field));
                    }
                    for (int i = 0; i < geneSys1Field.Elements; i++)
                    {
                        //GetDataTypeToRead.ReadDataTypeFromRes(geneSys1Field.FieldType, br, file, false, isHPR);
                    }
                }
                reader.Close();
            }
        }

        public void ReadBaseInstance(string path, StreamWriter file, bool isHPR)
        {
            PropertyInfo[] instProps = typeof(GeneSys1InstanceBase).GetProperties();
            using (BinaryReader reader = new BinaryReader(File.Open(path,
                                                             FileMode.Open,
                                                             FileAccess.Read,
                                                             FileShare.ReadWrite)))
            {
                reader.ReadInt64();
                if (isHPR) reader.ReadInt64();
                geneSys1Inst.Offset = (int)(isHPR ? reader.ReadInt64() : reader.ReadInt32());
                geneSys1Inst.InstanceBaseClass = reader.ReadInt32();
                geneSys1Inst.InstanceID = reader.ReadInt32();

                foreach (PropertyInfo property in instProps)
                {
                    file.WriteLine(property.Name + ": 0x{0:X8}", property.GetValue(geneSys1Inst));
                }

                reader.BaseStream.Seek(geneSys1Inst.Offset, SeekOrigin.Begin);
                //instFinder.SearchBase(this, path, geneSys1Inst, file);
                ReadResource(baseClassPath, file, reader, isHPR);
                //reader.Close();
            }
        }

        public void ReadChildInstance(StreamWriter file, BinaryReader br, bool isHPR)
        {
            long origInstOffset = br.BaseStream.Position;

            br.BaseStream.Seek(GetDataTypeToRead.fieldValue + (isHPR ? 16 : 8), SeekOrigin.Begin);
            geneSys1InstChild.Offset = (int)(isHPR ? br.ReadInt64() : br.ReadInt32());
            geneSys1InstChild.InstanceBaseClass = br.ReadInt32();

            PropertyInfo[] instProps = typeof(GeneSys1InstanceBase).GetProperties();
            foreach (PropertyInfo property in instProps)
            {
                if (property.Name != "InstanceID")
                {
                    if (property.Name != "NumOfFields" || property.Name != "ResourceIndex" || property.Name != "ResourceClass")
                        file.WriteLine("".PadLeft(12) + property.Name + ": 0x{0:X8}", property.GetValue(geneSys1InstChild));
                    else file.WriteLine("".PadLeft(12) + property.Name + ": " + property.GetValue(geneSys1InstChild));
                }

            }

            //instFinder.SearchInst(this, baseClassPath, geneSys1InstChild, file);
            br.BaseStream.Seek(geneSys1InstChild.Offset, SeekOrigin.Begin);

            try
            {
                using (BinaryReader reader = new BinaryReader(File.Open(baseClassPathInst,
                                                        FileMode.Open,
                                                        FileAccess.Read,
                                                        FileShare.ReadWrite)))
                {
                    GeneSys1ResourceBase instResource = new GeneSys1ResourceBase();
                    instResource.ResourceType = reader.ReadInt32();
                    instResource.ResourceID = reader.ReadInt32();
                    instResource.FieldStartOffset = reader.ReadInt32();
                    instResource.NumOfFields = reader.ReadUInt32();
                    instResource.ResourceIndex = reader.ReadByte();
                    instResource.ResourceClassName = NullTerminatedStringReader.Read(reader);

                    PropertyInfo[] resourceBaseProps = typeof(GeneSys1ResourceBase).GetProperties();
                    foreach (PropertyInfo property in resourceBaseProps)
                    {
                        if (property.Name != "NumOfFields" || property.Name != "ResourceIndex" || property.Name != "ResourceClass")
                            file.WriteLine("".PadLeft(16) + property.Name + ": 0x{0:X8}", property.GetValue(geneSys1InstChild));
                        else file.WriteLine("".PadLeft(16) + property.Name + ": " + property.GetValue(geneSys1InstChild));
                    }

                    file.WriteLine("\n" + "".PadLeft(20) + "ResourceFields: ");
                    reader.BaseStream.Seek(instResource.FieldStartOffset, SeekOrigin.Begin);

                    GeneSys1FieldDef instFields = new GeneSys1FieldDef();

                    for (int i = 0; i < instResource.NumOfFields; i++)
                    {
                        file.WriteLine("\n" + "".PadLeft(24) + "Field" + i + ":");

                        instFields.FieldHash = reader.ReadInt32();
                        //instFields.FieldType = (EGeneSys1Types)reader.ReadInt32();
                        instFields.Elements = reader.ReadInt32();
                        instFields.FieldOffset = reader.ReadInt32();
                        instFields.FieldLength = reader.ReadInt32();

                        PropertyInfo[] resourceFieldProps = typeof(GeneSys1FieldDef).GetProperties();
                        foreach (PropertyInfo property in resourceFieldProps)
                        {
                            if (property.Name == "FieldType")
                                file.WriteLine("".PadLeft(28) + property.Name + ": " + property.GetValue(instFields));
                        }

                        //GetDataTypeToRead.ReadDataTypeFromRes(instFields.FieldType, br, file, true, isHPR);
                    }
                    reader.Close();
                }
            }
            catch (Exception)
            {

            }
            br.BaseStream.Seek(origInstOffset, SeekOrigin.Begin);
        }

        public void ReadArrayPointer(StreamWriter file, BinaryReader br, bool isHPR)
        {
            try
            {
                long origInstOffset = br.BaseStream.Position;
                br.BaseStream.Seek(geneSys1ArrayPtr.Offset, SeekOrigin.Begin);

                for (int i = 0; i < geneSys1ArrayPtr.Elements; i++)
                {
                    file.WriteLine("\n" + "".PadLeft(12) + "Index" + i + ": ");
                    GetDataTypeToRead.fieldValue = br.ReadInt32();
                    ReadChildInstance(file, br, isHPR);
                }
                br.BaseStream.Seek(origInstOffset, SeekOrigin.Begin);
            }
            catch (Exception)
            {

            }
        }
    }
}