using System.IO;

namespace CamoLib.IO.Readers
{
    public static class NullTerminatedStringReader
    {
        public static string Read(this BinaryReader stream)
        {
            string str = "";
            char ch;
            while ((int)(ch = stream.ReadChar()) != 0)
                str = str + ch;
            return str;
        }

        public static string Read(this CamoStreamReader stream)
        {
            string str = "";
            char ch;
            while ((int)(ch = (char)stream.ReadSByte()) != 0)
                str = str + ch;
            return str;
        }
    }
}
