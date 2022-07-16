using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CamoLib.IO;

namespace CamoLib
{
    /// <summary>
    /// A basic asset loader that game-specific loaders inherit from
    /// </summary>
    public abstract class BaseAssetLoader
    {

        #region -- Properties --
        /// <summary>
        /// A <see cref="MemoryFs"/> containing all the assets for the loaded bundle
        /// </summary>
        public MemoryFs AssetDb => assetDb;
        /// <summary>
        /// What platform the loaded bundle belongs to
        /// </summary>
        public virtual uint GamePlatform => gamePlatform;
        #endregion

        /// <summary>
        /// Cache of this bundle's types for loading instances faster
        /// </summary>
        public Dictionary<uint, object> TypeCache;
        private MemoryFs assetDb;
        protected uint gamePlatform = 0;

        public BaseAssetLoader(string bundlePath)
        {
            assetDb = new MemoryFs(bundlePath);
            TypeCache = new Dictionary<uint, object>();
        }

        public virtual void PopulateAssetDb()
        {
            throw new Exception($"BaseAssetLoader can't load the bundle located at {assetDb.BundlePath}!");
        }

        public virtual void CacheTypes()
        {
            throw new Exception($"BaseAssetLoader can't cache types!");
        }

    }
}
