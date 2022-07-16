using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CamoLib.IO
{
    /// <summary>
    /// Stores the file layout for a Chameleon bundle in memory
    /// </summary>
    public class MemoryFs
    {

        public class FsFile
        {
            //public string Path => filePath;
            /// <summary>
            /// The name of this file/directory
            /// </summary>
            public string Name => name;
            /// <summary>
            /// Is this item a folder?
            /// </summary>
            public bool IsFolder => isFolder;

            private byte[] itemData;
            private bool isFolder;
            //private string filePath;
            private string name;
            private Dictionary<string, FsFile> children;

            /// <summary>
            /// Creates a folder
            /// </summary>
            /// <param name="inName">The folder name to use</param>
            public FsFile(string inName)
            {
                isFolder = true;
                name = inName;
                children = new Dictionary<string, FsFile>();
            }

            /// /// <summary>
            /// Creates a file
            /// </summary>
            /// <param name="inName">The filename to use/></param>
            /// <param name="inData">Raw bytes for this file</param>
            public FsFile(string inName, byte[] inData)
            {
                isFolder = false;
                name = inName;
                if (inData != null)
                    itemData = inData;
                else
                    itemData = new byte[0];
            }

            /// <summary>
            /// Retrives a child file/folder given a name
            /// </summary>
            /// <param name="inName">The name of the child to search for</param>
            /// <returns></returns>
            public FsFile GetChild(string inName)
            {
                return children[inName];
            }

            public IEnumerable<string> GetChildPaths()
            {
                if (!isFolder || children == null)
                    throw new Exception("Files don't have children!");

                return children.Keys;
            }

            /// <summary>
            /// Checks if this item has a child with the given name
            /// </summary>
            /// <param name="inName">The name of the child to search for</param>
            /// <returns>Returns true if this item is a folder and has succesfully found a child, returns false otherwise</returns>
            public bool HasChild(string inName)
            {
                return isFolder && children.ContainsKey(inName);
            }

            /// <summary>
            /// Adds a child file/folder
            /// </summary>
            /// <param name="name">The name of the file to add</param>
            /// <param name="inFile">The child file to add</param>
            /// <returns>True if path and file are valid, false otherwise</returns>
            public bool AddChild(string name, FsFile inFile)
            {
                // not a folder, or file/path is invalid
                if (inFile == null || name == null || name == "" || !isFolder)
                    return false;

                children.Add(name, inFile);
                return true;
            }

            /*
            /// <summary>
            /// Retrieves the directory that contains this item
            /// </summary>
            /// <returns>The parent path string</returns>
            public string GetParentDir()
            {
                int splitPos = filePath.LastIndexOf('/');
                // parent path was invalid so default to root
                if (splitPos == -1)
                    return RootDirName;

                string parent = filePath.Substring(0, splitPos);
                if (parent == RootDirName.TrimEnd("/".ToCharArray()))
                    parent = RootDirName;
                return parent;
            }*/

            /// <summary>
            /// Create a <see cref="Stream"/> using this file's data
            /// </summary>
            /// <returns>Returns a <see cref="Stream"/> if this file is valid, otherwise an exception is thrown</returns>
            public Stream GetStream()
            {
                if (isFolder)
                    throw new Exception("Can't get the file stream of a folder!");
                else if (itemData == null)
                    throw new Exception("Raw item data can't be null!");
                else
                    return new MemoryStream(itemData);
            }
        }

        #region -- Properties --
        public static string RootDirName => "gamedb:";
        /// <summary>
        /// The root directory of this filesystem
        /// </summary>
        public FsFile Root => rootDir;
        public bool IsInitialized => initialized;
        /// <summary>
        /// Hard path to the bundle loaded into this filesystem
        /// </summary>
        public string BundlePath => fsObjPath;
        #endregion

        #region -- Fields --
        private string fsObjPath;
        private bool initialized = false;
        private FsFile rootDir;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryFs"/> class
        /// </summary>
        /// <param name="filepath">The path on the hard drive to a GeneSys bundle</param>
        /// <param name="ver">The engine version, used for determining how to unpack and read files</param>
        public MemoryFs(string filepath)
        {
            if (filepath == null || filepath == "")
            {
                MessageBox.Show($"No file path was provided. Try again!");
                return;
            }

            fsObjPath = filepath;
            rootDir = new FsFile(RootDirName);
            initialized = true;
            //switch (ver)
            //{
            //    case GeneSysVersion.GeneSys1:
            //    case GeneSysVersion.GeneSys1R:
            //        Unpackers.GeneSys1Unpacker.LoadFileSystemPC(filepath);
            //        break;
            //    default:
            //        MessageBox.Show($"{ver} is not implemented yet!");
            //        break;
            //}
        }

        /// <summary>
        /// Checks to see if a file or folder already exists in memory
        /// </summary>
        /// <param name="path">Path to a file/folder in memory</param>
        /// <returns>Returns true if found, false if not</returns>
        public bool FileExists(string path)
        {
            // remove trailing slashes if present
            path = path.TrimEnd("/".ToCharArray());
            string[] split = path.Split("/".ToCharArray());
            var file = rootDir;
            bool exists = false;
            for (int i = 1; i < split.Length; ++i)
            {
                if (file.HasChild(split[i]))
                    file = file.GetChild(split[i]);
                if (file.Name != split[i] || file == null)
                {
                    exists = false;
                    break; // break early as there's nothing else to find
                }
                else
                {
                    exists = true;
                }
            }
            return exists;
        }

        /// <summary>
        /// Finds a virtual file/folder from the given path
        /// </summary>
        /// <param name="path">The path to the file/folder</param>
        /// <returns></returns>
        public FsFile GetFile(string path)
        {
            // remove trailing slashes if present
            path = path.TrimEnd("/".ToCharArray());
            string[] split = path.Split("/".ToCharArray());
            var file = rootDir;
            for (int i = 1; i < split.Length; ++i)
            {
                if (file.HasChild(split[i]))
                    file = file.GetChild(split[i]);
                if (file.Name != split[i] || file == null)
                    break; // break early as there's nothing else to find
            }
            return file;
        }

        /// <summary>
        /// Creates a virtual folder at the specified path.
        /// Will also create any missing directories leading up to it
        /// </summary>
        /// <param name="path">The path to the new folder</param>
        public void CreateDirectory(string path)
        {
            // remove trailing slashes if present
            path = path.TrimEnd("/".ToCharArray());
            string[] split = path.Split("/".ToCharArray());
            var file = rootDir;
            // create any missing directories
            for (int i = 1; i < split.Length; ++i)
            {
                if (!file.HasChild(split[i]))
                    file.AddChild(split[i], new FsFile(split[i]));
                file = file.GetChild(split[i]);
            }
        }

        /// <summary>
        /// Creates a virtual file at the specified path.
        /// Will also create any missing directories leading up to it
        /// </summary>
        /// <param name="path">The path to the new file</param>
        /// <param name="data">The file's data</param>
        public void CreateFile(string path, byte[] data)
        {
            // remove trailing slashes if present
            path = path.TrimEnd("/".ToCharArray());
            string[] split = path.Split("/".ToCharArray());
            string fname = split[split.Length - 1];
            var file = rootDir;
            // create any missing directories
            for (int i = 1; i < split.Length - 1; ++i)
            {
                if (!file.HasChild(split[i]))
                    file.AddChild(split[i], new FsFile(split[i]));
                file = file.GetChild(split[i]);
            }

            // create file if it doesn't already exist
            if (!file.HasChild(fname))
                file.AddChild(fname, new FsFile(fname, data));
        }

        /// <summary>
        /// Opens a file from memory
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>Returns a <see cref="Stream"/> if the file is valid, otherwise an exception is thrown</returns>
        public Stream OpenFile(string path)
        {
            // remove trailing slashes if present
            path = path.TrimEnd("/".ToCharArray());

            FsFile file = GetFile(path);
            if (file == null)
                throw new Exception("Can't open a null file!");
            else
                return file.GetStream();
        }

        public IEnumerable<string> GetPaths(string searchDir)
        {
            // remove trailing slashes if present
            searchDir = searchDir.TrimEnd("/".ToCharArray());
            FsFile file = GetFile(searchDir);
            return file.GetChildPaths();
        }

        /// <summary>
        /// Gets only the name of the file/folder pointed to by the given path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileName(string path)
        {
            int splitPos = path.LastIndexOf('/');
            // already a filename so just return as is
            if (splitPos == -1)
                return path;
            else
                return path.Substring(splitPos + 1, path.Length - 1 - splitPos);
        }

        /// <summary>
        /// Gets only the name of the file pointed to by the given path without its extension
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileNameNoExtension(string path)
        {
            string fname = GetFileName(path);
            int splitPos = path.LastIndexOf('.');
            // file has no extension, or it's a folder so just return the regular name
            if (splitPos == -1)
                return fname;
            else
                return fname.Substring(0, splitPos);
        }
    }
}
