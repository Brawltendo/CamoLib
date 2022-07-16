using CamoLib.IO;
using CamoLib.Utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamoLib
{
    public class GeneSys1Loader : BaseAssetLoader
    {
        public enum PlatformType : uint
        {
            Platform_PC = 0x01000000,
            Platform_X360 = 0x00000002,
            Platform_PS3 = 0x00000003
        }
        public new PlatformType GamePlatform => (PlatformType)base.GamePlatform;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneSys1Loader"/> class. Inherits from <see cref="BaseAssetLoader"/>
        /// </summary>
        /// <param name="bundlePath">Path to the bundle to load</param>
        public GeneSys1Loader(string bundlePath) : base(bundlePath)
        {
            PopulateAssetDb();
        }

        public override void PopulateAssetDb()
        {
            using (var reader = new BinaryReader(File.Open(AssetDb.BundlePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                byte[] magic = reader.ReadBytes(4);
                // validate header magic
                if (magic[0] != 'b'
                ||  magic[1] != 'n'
                ||  magic[2] != 'd'
                ||  magic[3] != '2')
                {
                    MessageBox.Show("This is not a valid bundle!");
                    reader.Close();
                    return;
                }

                reader.ReadInt32(); // unknown, seems to always be 3 across platforms
                gamePlatform = BinaryPrimitives.ReverseEndianness(reader.ReadUInt32());
                switch (GamePlatform)
                {
                    case PlatformType.Platform_PC:
                        MessageBox.Show("Platform: PC");
                        UnpackBundlePC(reader);
                        break;
                    //case PlatformType.Platform_X360:
                    //    MessageBox.Show("Platform: X360");
                    //    break;
                    //case PlatformType.Platform_PS3:
                    //    MessageBox.Show("Platform: PS3");
                    //    break;
                    default:
                        MessageBox.Show("Other platforms are not implemented yet!");
                        break;
                }
            }
        }

        private void UnpackBundlePC(BinaryReader reader)
        {
            if (AssetDb == null || !AssetDb.IsInitialized)
            {
                throw new Exception("Global MemoryFs was uninitialized");
            }

            // offset to the ResourceStringTable if present
            // debug builds contain this data, along with the final version of Hot Pursuit Remastered
            int stringTableOffset = reader.ReadInt32();

            //
            // start unpacking, derived from DGIorio's Noesis scripts
            //
            (int IdCount, int IdTableOffset, int Block1Offset, int Block2Offset, int BundleSize1, int BundleSize2, int Compression) bndlInfo;
            bndlInfo.IdCount = reader.ReadInt32();
            bndlInfo.IdTableOffset = reader.ReadInt32();
            bndlInfo.Block1Offset = reader.ReadInt32();
            bndlInfo.Block2Offset = reader.ReadInt32();
            bndlInfo.BundleSize1 = reader.ReadInt32();
            bndlInfo.BundleSize2 = reader.ReadInt32();
            bndlInfo.Compression = reader.ReadInt32();

            //MessageBox.Show("Unpacking " + bndlInfo.IdCount + "files.");
            //MessageBox.Show("Compression Type: " + bndlInfo.Compression.ToString("X8"));

            List<string> idList = new List<string>();

            // Loop through all IDs and unpack their respective files
            for (int i = 0; i < bndlInfo.IdCount; i++)
            {
                const int fileInfoSize = 0x50;
                //Console.WriteLine("Unpacking file " + (i + 1) + "\n");
                reader.BaseStream.Position = bndlInfo.IdTableOffset + (i * fileInfoSize);

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

                reader.BaseStream.Seek(bndlInfo.Block1Offset + posInfo.position1, SeekOrigin.Begin);

                byte[] dataOut;
                string id1Name = id1.ToString("X8") + (count > 0 ? ("_Subtype " + count.ToString()) : "");
                var outFolder = MemoryFs.RootDirName + "/" + "Type_" + dataType.ToString("X8") + "/";
                AssetDb.CreateDirectory(outFolder);
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
                        dataOut = Decompression.DecompressDataZlib(compData);
                        //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                        break;
                }

                AssetDb.CreateFile(outName + ".dat", dataOut);
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
                    reader.BaseStream.Seek(bndlInfo.Block2Offset + posInfo.position2, SeekOrigin.Begin);
                    switch (bndlInfo.Compression)
                    {
                        case 0x8:
                            dataOut = reader.ReadBytes((int)decompSizes.decompSize2);
                            break;
                        default:
                            byte[] compData = reader.ReadBytes(compSizes.compSize2);
                            dataOut = Decompression.DecompressDataZlib(compData);
                            //Console.WriteLine("Decompressed: " + fileInfo + "\n");
                            break;
                    }
                }

                AssetDb.CreateFile(outName + ".dat", dataOut);
                //unpackedFile = fs.OpenFile(outName + ".dat", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                //unpackedFile.Write(dataOut, 0, dataOut.Length);
                //unpackedFile.Flush();
                //unpackedFile.Close();

                //Console.WriteLine("File ID 1: " + id1Name + "\n");
                //Console.WriteLine("File ID 2: " + id2.ToString("X8") + "\n");
            }
            reader.Close();
        }

    }
}
