using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using zlib;

namespace CamoLib.Utils
{
    public class DecompressionUtils
    {
        public static byte[] DecompressData(byte[] buffer)
        {
            MemoryStream memOutput = new MemoryStream();
            ZOutputStream zipOut = new ZOutputStream(memOutput);

            zipOut.Write(buffer, 0, buffer.Length);
            zipOut.finish();

            memOutput.Seek(0, SeekOrigin.Begin);
            return memOutput.ToArray();
        }
    }
}
