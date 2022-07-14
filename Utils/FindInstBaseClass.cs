using CamoLib.DataTypes;
using CamoLib.IO.Readers;
using System;
using System.IO;
using System.Windows.Forms;

namespace CamoLib.Utils
{
    public class FindInstBaseClass
    {
        public void SearchBase(GeneSys1ReaderOld reader, string path, GeneSys1InstanceBase inst, StreamWriter file)
        {
            var d = new DirectoryInfo(Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "12_00_00_00"));
            foreach (FileInfo fi in d.GetFiles())
            {
                if (reader.baseClassPath != fi.FullName || reader.baseClassPathInst != fi.FullName)
                {
                    Stream f = File.OpenRead(fi.FullName);
                    BinaryReader br = new BinaryReader(f);
                    for (int i = 0; i <= 4; i++)
                    {
                        f.Seek(i, SeekOrigin.Begin);
                        if (br.ReadInt32() == inst.InstanceBaseClass)
                        {
                            //file.WriteLine("\nFound " + br.ReadInt32());
                            file.WriteLine("".PadLeft(12) + "Base class found here: " + fi.FullName);
                            file.WriteLine("\n" + "".PadLeft(12) + "Base class info:");

                            reader.baseClassPath = fi.FullName;
                            br.Close();
                            //ReadResource(fi.FullName, br);
                            //return fi.FullName;
                        }
                    }
                    br.Close();
                }
            }
            //return "";
        }

        public void SearchInst(GeneSys1ReaderOld reader, string path, GeneSys1InstanceBase inst, StreamWriter file)
        {
            var d = new DirectoryInfo(Path.Combine(Directory.GetParent(Path.GetDirectoryName(path)).FullName, "12_00_00_00"));
            foreach (FileInfo fi in d.GetFiles())
            {
                if (reader.baseClassPath != fi.FullName || reader.baseClassPathInst != fi.FullName)
                {
                    Stream f = File.OpenRead(fi.FullName);
                    BinaryReader br = new BinaryReader(f);
                    for (int i = 0; i <= 4; i++)
                    {
                        f.Seek(i, SeekOrigin.Begin);
                        if (br.ReadInt32() == inst.InstanceBaseClass)
                        {
                            //file.WriteLine("\nFound " + br.ReadInt32());
                            file.WriteLine("".PadLeft(12) + "Base class found here: " + fi.FullName);
                            file.WriteLine("\n" + "".PadLeft(12) + "Base class info:");

                            reader.baseClassPathInst = fi.FullName;
                            br.Close();
                            //ReadResource(fi.FullName, br);
                            //return fi.FullName;
                        }
                    }
                    br.Close();
                }
            }
            //return "";
        }
    }
}
