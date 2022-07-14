using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO.Compression;
using System.Buffers.Binary;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using ICSharpCode.SharpZipLib.Lzw;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.BZip2;
using CamoLib.Utils;
using CamoLib.IO;
using Zio.FileSystems;
using Zio;

namespace CamoLib.IO.Unpackers
{
    public class GeneSys1Unpacker
    {
        enum PlatformType : uint
        {
            Platform_PC = 0x01000000,
            Platform_X360 = 0x00000002,
            Platform_PS3 = 0x00000003
        }

        private static PlatformType gamePlatform;

        public static void Unpack(string fileToUnpack, string saveDir)
        {
            GetGamePlatform(fileToUnpack);
            switch (gamePlatform)
            {
                case PlatformType.Platform_PC:
                    UnpackPC(fileToUnpack, saveDir);
                    break;
                default:
                    MessageBox.Show("Other platforms are not implemented yet!");
                    break;
            }
        }

        static void GetGamePlatform(string fileToUnpack)
        {
            using (var reader = new BinaryReader(File.Open(fileToUnpack, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                reader.ReadBytes(4); // magic
                reader.BaseStream.Seek(4, SeekOrigin.Current);
                gamePlatform = (PlatformType)BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
                switch (gamePlatform)
                {
                    case PlatformType.Platform_PC:
                        MessageBox.Show("Platform: PC");
                        break;
                    case PlatformType.Platform_X360:
                        MessageBox.Show("Platform: X360");
                        break;
                    case PlatformType.Platform_PS3:
                        MessageBox.Show("Platform: PS3");
                        break;
                }
                reader.Close();
            }    
        }

        static void UnpackPC(string fileToUnpack, string saveDir)
        {
            using (var reader = new BinaryReader(File.Open(fileToUnpack, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                reader.ReadBytes(4); // magic
                reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
                (int NumIDs, int IDsTab_start, int Block1_start, int Block2_start, int Fullsize, int Fullsize2, int Compression) bndlInfo;

                bndlInfo.NumIDs = reader.ReadInt32();
                bndlInfo.IDsTab_start = reader.ReadInt32();
                bndlInfo.Block1_start = reader.ReadInt32();
                bndlInfo.Block2_start = reader.ReadInt32();
                bndlInfo.Fullsize = reader.ReadInt32();
                bndlInfo.Fullsize2 = reader.ReadInt32();
                bndlInfo.Compression = reader.ReadInt32();

                MessageBox.Show("Unpacking " + bndlInfo.NumIDs + "files.");
                MessageBox.Show("Compression Type: " + bndlInfo.Compression.ToString("X8"));

                List<string> idList = new List<string>();

                // Loop through all IDs and unpack their respective files
                for (int i = 0; i < bndlInfo.NumIDs; i++)
                {
                    Console.WriteLine("Unpacking file " + (i + 1) + "\n");
                    reader.BaseStream.Seek(bndlInfo.IDsTab_start + i * 0x50, SeekOrigin.Begin);

                    uint id1 = reader.ReadUInt32();
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    byte count = reader.ReadByte();
                    reader.BaseStream.Seek(-3, SeekOrigin.Current);
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    uint id2 = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    (int decompSize1, uint decompSize2) decompSizes = (reader.ReadInt32(), reader.ReadUInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int compSize1, int compSize2) compSizes = (reader.ReadInt32(), reader.ReadInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int position1, int position2) posInfo = (reader.ReadInt32(), reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    int called_block = BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
                    uint dataType = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
                    Console.WriteLine("File Type: " + dataType.ToString("X8") + "\n");

                    string fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + (count > 0 ? (", Subtype " + count.ToString()) : "");
                    if (idList.Contains(fileInfo))
                    {
                        count = 1;
                        fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        while (idList.Contains(fileInfo))
                        {
                            count++;
                            fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        }
                    }
                    idList.Add(fileInfo);
                    //MessageBox.Show("Current file: " + idList[i]);
                    //MessageBox.Show("Decomp Sizes: " + decompSizes.decompSize1.ToString("X8") + ", " + decompSizes.decompSize2.ToString("X8"));

                    ushort numIntIds = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());
                    /*if (decompSizes.decompSize1 >= 0x90000000)
                    {
                        decompSizes.decompSize1 = (int)(decompSizes.decompSize1 - 0x90000000);
                    }
                    else if (decompSizes.decompSize1 >= 0x80000000)
                    {
                        decompSizes.decompSize1 = (int)(decompSizes.decompSize1 - 0x80000000);
                    }
                    else if (decompSizes.decompSize1 >= 0x70000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x70000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x60000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x60000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x50000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x50000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x40000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x40000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x30000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x30000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x20000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x20000000;
                    }
                    else if (decompSizes.decompSize1 >= 0x10000000)
                    {
                        decompSizes.decompSize1 = decompSizes.decompSize1 - 0x10000000;
                    }*/

                    var decompVal = 0x90000000;
                    while (decompSizes.decompSize1 >= decompVal)
                    {
                        decompSizes.decompSize1 -= (int)decompVal;
                        decompVal -= 0x10000000;
                    }

                    decompVal = 0xC0000000;
                    while (decompSizes.decompSize2 >= decompVal)
                    {
                        decompSizes.decompSize2 -= decompVal;
                        decompVal -= 0x10000000;
                    }

                    /*if (decompSizes.decompSize2 >= 0xC0000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0xC0000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x70000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x70000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x60000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x60000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x50000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x50000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x40000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x40000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x30000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x30000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x20000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x20000000;
                    }
                    else if (decompSizes.decompSize2 >= 0x10000000)
                    {
                        decompSizes.decompSize2 = decompSizes.decompSize2 - 0x10000000;
                    }*/

                    reader.BaseStream.Seek(bndlInfo.Block1_start + posInfo.position1, SeekOrigin.Begin);

                    byte[] dataOut;
                    string id1Name = id1.ToString("X8") + (count > 0 ? ("_Subtype " + count.ToString()) : "");
                    string outFolder = saveDir + "\\" + "Type_" + dataType.ToString("X8");
                    if (!File.Exists(outFolder))
                    {
                        Directory.CreateDirectory(outFolder);
                    }
                    string outName = outFolder + "\\" + "Resource_" + id1Name;

                    switch (bndlInfo.Compression)
                    {
                        case 0x8:
                            dataOut = reader.ReadBytes(decompSizes.decompSize1);
                            break;
                        default:
                            byte[] compData = reader.ReadBytes(compSizes.compSize1);
                            dataOut = DecompressionUtils.DecompressData(compData);
                            Console.WriteLine("Decompressed: " + fileInfo + "\n");
                            break;
                    }
                    

                    FileStream exportFile = new FileStream(outName + ".dat", FileMode.OpenOrCreate);
                    exportFile.Write(dataOut, 0, dataOut.Length);
                    exportFile.Flush();
                    exportFile.Close();

                    if (decompSizes.decompSize2 != 0)
                    {
                        switch (dataType)
                        {
                            case 0x05000000:
                                outName = outName + "_model";
                                break;
                            case 0x01000000:
                                outName = outName + "_texture";
                                break;
                            default:
                                outName = outName + "_unknown";
                                break;
                        }
                        reader.BaseStream.Seek(bndlInfo.Block2_start + posInfo.position2, SeekOrigin.Begin);
                        switch (bndlInfo.Compression)
                        {
                            case 0x8:
                                dataOut = reader.ReadBytes((int)decompSizes.decompSize2);
                                break;
                            default:
                                byte[] compData = reader.ReadBytes(compSizes.compSize2);
                                dataOut = DecompressionUtils.DecompressData(compData);
                                Console.WriteLine("Decompressed: " + fileInfo + "\n");
                                break;
                        }
                    }
                    exportFile = new FileStream(outName + ".dat", FileMode.OpenOrCreate);
                    exportFile.Write(dataOut, 0, dataOut.Length);
                    exportFile.Flush();
                    exportFile.Close();

                    Console.WriteLine("File ID 1: " + id1Name + "\n");
                    Console.WriteLine("File ID 2: " + id2.ToString("X8") + "\n");
                }
                reader.Close();
            }
        }

        public static MemoryFileSystem LoadFileSystemPC(string fileToUnpack)
        {
            var fs = new MemoryFileSystem();
            using (var reader = new BinaryReader(File.Open(fileToUnpack, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                reader.ReadBytes(4); // magic
                reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
                (int NumIDs, int IDsTab_start, int Block1_start, int Block2_start, int Fullsize, int Fullsize2, int Compression) bndlInfo;

                bndlInfo.NumIDs = reader.ReadInt32();
                bndlInfo.IDsTab_start = reader.ReadInt32();
                bndlInfo.Block1_start = reader.ReadInt32();
                bndlInfo.Block2_start = reader.ReadInt32();
                bndlInfo.Fullsize = reader.ReadInt32();
                bndlInfo.Fullsize2 = reader.ReadInt32();
                bndlInfo.Compression = reader.ReadInt32();

                //MessageBox.Show("Unpacking " + bndlInfo.NumIDs + "files.");
                //MessageBox.Show("Compression Type: " + bndlInfo.Compression.ToString("X8"));

                List<string> idList = new List<string>();

                // Loop through all IDs and unpack their respective files
                for (int i = 0; i < bndlInfo.NumIDs; i++)
                {
                    //Console.WriteLine("Unpacking file " + (i + 1) + "\n");
                    reader.BaseStream.Seek(bndlInfo.IDsTab_start + i * 0x50, SeekOrigin.Begin);

                    uint id1 = reader.ReadUInt32();
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    byte count = reader.ReadByte();
                    reader.BaseStream.Seek(-3, SeekOrigin.Current);
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    uint id2 = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    (int decompSize1, uint decompSize2) decompSizes = (reader.ReadInt32(), reader.ReadUInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int compSize1, int compSize2) compSizes = (reader.ReadInt32(), reader.ReadInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int position1, int position2) posInfo = (reader.ReadInt32(), reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    int called_block = BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
                    uint dataType = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
                    //Console.WriteLine("File Type: " + dataType.ToString("X8") + "\n");

                    string fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + (count > 0 ? (", Subtype " + count.ToString()) : "");
                    if (idList.Contains(fileInfo))
                    {
                        count = 1;
                        fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        while (idList.Contains(fileInfo))
                        {
                            count++;
                            fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        }
                    }
                    idList.Add(fileInfo);
                    ushort numIntIds = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());

                    var decompVal = 0x90000000;
                    while (decompSizes.decompSize1 >= decompVal)
                    {
                        decompSizes.decompSize1 -= (int)decompVal;
                        decompVal -= 0x10000000;
                    }

                    decompVal = 0xC0000000;
                    while (decompSizes.decompSize2 >= decompVal)
                    {
                        decompSizes.decompSize2 -= decompVal;
                        decompVal -= 0x10000000;
                    }

                    reader.BaseStream.Seek(bndlInfo.Block1_start + posInfo.position1, SeekOrigin.Begin);

                    byte[] dataOut;
                    string id1Name = id1.ToString("X8") + (count > 0 ? ("_Subtype " + count.ToString()) : "");
                    var outFolder = "/" + "Type_" + dataType.ToString("X8") + "/";
                    if (!fs.FileExists(outFolder))
                    {
                        fs.CreateDirectory(outFolder);
                    }
                    var outName = outFolder + "Resource_" + id1Name;

                    if (decompSizes.decompSize1 != 0)
                    {
                        switch (dataType)
                        {
                            case 0x05000000:
                                outName = outName + "_modelHeader";
                                break;
                            case 0x01000000:
                                outName = outName + "_textureHeader";
                                break;
                            default:
                                //outName = outName + "_unknownHeader";
                                break;
                        }
                    }

                    switch (bndlInfo.Compression)
                    {
                        case 0x8:
                            dataOut = reader.ReadBytes(decompSizes.decompSize1);
                            break;
                        default:
                            byte[] compData = reader.ReadBytes(compSizes.compSize1);
                            dataOut = DecompressionUtils.DecompressData(compData);
                            //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                            break;
                    }

                    Stream unpackedFile = fs.OpenFile(outName + ".dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    unpackedFile.Write(dataOut, 0, dataOut.Length);
                    unpackedFile.Flush();
                    unpackedFile.Close();

                    outName = outFolder + "Resource_" + id1Name;
                    if (decompSizes.decompSize2 != 0)
                    {
                        switch (dataType)
                        {
                            case 0x05000000:
                                outName = outName + "_model";
                                break;
                            case 0x01000000:
                                outName = outName + "_texture";
                                break;
                            default:
                                outName = outName + "_unknown";
                                break;
                        }
                        reader.BaseStream.Seek(bndlInfo.Block2_start + posInfo.position2, SeekOrigin.Begin);
                        switch (bndlInfo.Compression)
                        {
                            case 0x8:
                                dataOut = reader.ReadBytes((int)decompSizes.decompSize2);
                                break;
                            default:
                                byte[] compData = reader.ReadBytes(compSizes.compSize2);
                                dataOut = DecompressionUtils.DecompressData(compData);
                                //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                                break;
                        }
                    }
                    unpackedFile = fs.OpenFile(outName + ".dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    unpackedFile.Write(dataOut, 0, dataOut.Length);
                    unpackedFile.Flush();
                    unpackedFile.Close();

                    //Console.WriteLine("File ID 1: " + id1Name + "\n");
                    //Console.WriteLine("File ID 2: " + id2.ToString("X8") + "\n");
                }
                reader.Close();
                return fs;
            }
        }

        /// <summary>
        /// Unpacks a GeneSys1 bundle into memory for use with <see cref="MemoryFs"/>
        /// </summary>
        /// <param name="fileToUnpack">Path to the bundle to unpack</param>
        /// <param name="rootDir">The root directory for other files to reside under</param>
        public static void LoadFileSystemPCNew()
        {
            if (MemoryFs.AppFs == null || !MemoryFs.AppFs.IsInitialized)
            {
                throw new Exception("Global MemoryFs was uninitialized");
            }

            using (var reader = new BinaryReader(File.Open(MemoryFs.AppFs.BundlePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                reader.ReadBytes(4); // magic
                reader.BaseStream.Seek(0x10, SeekOrigin.Begin);
                (int NumIDs, int IDsTab_start, int Block1_start, int Block2_start, int Fullsize, int Fullsize2, int Compression) bndlInfo;

                bndlInfo.NumIDs = reader.ReadInt32();
                bndlInfo.IDsTab_start = reader.ReadInt32();
                bndlInfo.Block1_start = reader.ReadInt32();
                bndlInfo.Block2_start = reader.ReadInt32();
                bndlInfo.Fullsize = reader.ReadInt32();
                bndlInfo.Fullsize2 = reader.ReadInt32();
                bndlInfo.Compression = reader.ReadInt32();

                //MessageBox.Show("Unpacking " + bndlInfo.NumIDs + "files.");
                //MessageBox.Show("Compression Type: " + bndlInfo.Compression.ToString("X8"));

                List<string> idList = new List<string>();

                // Loop through all IDs and unpack their respective files
                for (int i = 0; i < bndlInfo.NumIDs; i++)
                {
                    //Console.WriteLine("Unpacking file " + (i + 1) + "\n");
                    reader.BaseStream.Seek(bndlInfo.IDsTab_start + i * 0x50, SeekOrigin.Begin);

                    uint id1 = reader.ReadUInt32();
                    reader.BaseStream.Seek(2, SeekOrigin.Current);
                    byte count = reader.ReadByte();
                    reader.BaseStream.Seek(-3, SeekOrigin.Current);
                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    uint id2 = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());

                    reader.BaseStream.Seek(4, SeekOrigin.Current);
                    (int decompSize1, uint decompSize2) decompSizes = (reader.ReadInt32(), reader.ReadUInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int compSize1, int compSize2) compSizes = (reader.ReadInt32(), reader.ReadInt32());

                    reader.BaseStream.Seek(8, SeekOrigin.Current);
                    (int position1, int position2) posInfo = (reader.ReadInt32(), reader.ReadInt32());
                    reader.BaseStream.Seek(8, SeekOrigin.Current);

                    int called_block = BinaryPrimitives.ReverseEndianness(reader.ReadInt32());
                    uint dataType = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
                    //Console.WriteLine("File Type: " + dataType.ToString("X8") + "\n");

                    string fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + (count > 0 ? (", Subtype " + count.ToString()) : "");
                    if (idList.Contains(fileInfo))
                    {
                        count = 1;
                        fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        while (idList.Contains(fileInfo))
                        {
                            count++;
                            fileInfo = "Type_" + dataType.ToString("X8") + "-ID_" + id1.ToString("X8") + "-Subtype_" + count.ToString();
                        }
                    }
                    idList.Add(fileInfo);
                    ushort numIntIds = BinaryPrimitives.ReverseEndianness(reader.ReadUInt16());

                    var decompVal = 0x90000000;
                    while (decompSizes.decompSize1 >= decompVal)
                    {
                        decompSizes.decompSize1 -= (int)decompVal;
                        decompVal -= 0x10000000;
                    }

                    decompVal = 0xC0000000;
                    while (decompSizes.decompSize2 >= decompVal)
                    {
                        decompSizes.decompSize2 -= decompVal;
                        decompVal -= 0x10000000;
                    }

                    reader.BaseStream.Seek(bndlInfo.Block1_start + posInfo.position1, SeekOrigin.Begin);

                    byte[] dataOut;
                    string id1Name = id1.ToString("X8") + (count > 0 ? ("_Subtype " + count.ToString()) : "");
                    var outFolder = MemoryFs.RootDirName + "/" + "Type_" + dataType.ToString("X8") + "/";
                    MemoryFs.AppFs.CreateDirectory(outFolder);
                    var outName = outFolder + "Resource_" + id1Name;

                    if (decompSizes.decompSize1 != 0)
                    {
                        switch (dataType)
                        {
                            case 0x05000000:
                                outName += "_modelHeader";
                                break;
                            case 0x01000000:
                                outName += "_textureHeader";
                                break;
                            default:
                                //outName = outName + "_unknownHeader";
                                break;
                        }
                    }

                    switch (bndlInfo.Compression)
                    {
                        case 0x8:
                            dataOut = reader.ReadBytes(decompSizes.decompSize1);
                            break;
                        default:
                            byte[] compData = reader.ReadBytes(compSizes.compSize1);
                            dataOut = DecompressionUtils.DecompressData(compData);
                            //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                            break;
                    }

                    MemoryFs.AppFs.CreateFile(outName + ".dat", dataOut);
                    //Stream unpackedFile = fs.OpenFile(, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //unpackedFile.Write(dataOut, 0, dataOut.Length);
                    //unpackedFile.Flush();
                    //unpackedFile.Close();

                    outName = outFolder + "Resource_" + id1Name;
                    if (decompSizes.decompSize2 != 0)
                    {
                        switch (dataType)
                        {
                            case 0x05000000:
                                outName += "_model";
                                break;
                            case 0x01000000:
                                outName += "_texture";
                                break;
                            default:
                                outName += "_unknown";
                                break;
                        }
                        reader.BaseStream.Seek(bndlInfo.Block2_start + posInfo.position2, SeekOrigin.Begin);
                        switch (bndlInfo.Compression)
                        {
                            case 0x8:
                                dataOut = reader.ReadBytes((int)decompSizes.decompSize2);
                                break;
                            default:
                                byte[] compData = reader.ReadBytes(compSizes.compSize2);
                                dataOut = DecompressionUtils.DecompressData(compData);
                                //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                                break;
                        }
                    }

                    MemoryFs.AppFs.CreateFile(outName + ".dat", dataOut);
                    //unpackedFile = fs.OpenFile(outName + ".dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    //unpackedFile.Write(dataOut, 0, dataOut.Length);
                    //unpackedFile.Flush();
                    //unpackedFile.Close();

                    //Console.WriteLine("File ID 1: " + id1Name + "\n");
                    //Console.WriteLine("File ID 2: " + id2.ToString("X8") + "\n");
                }
                reader.Close();
                //return fs;
            }
        }
    }
}
