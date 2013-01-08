using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using NuGet;

namespace NuGetGallery.Helpers
{
    internal class PackageHelper
    {
        /// <summary>
        /// Look for the IPackage instance in the cache first. If it's in the cache, return it.
        /// Otherwise, download the package from the storage service and store it into the cache.
        /// </summary>
        public static async Task<IPackage> GetPackageFromCacheOrDownloadIt(
            Package package,
            IPackageCacheService cacheService,
            IPackageFileService packageFileService)
        {
            Debug.Assert(package != null);
            Debug.Assert(cacheService != null);
            Debug.Assert(packageFileService != null);

            string cacheKey = CreateCacheKey(package.PackageRegistration.Id, package.Version);
            byte[] buffer = cacheService.GetBytes(cacheKey);
            if (buffer == null)
            {
                using (Stream stream = await packageFileService.DownloadPackageFileAsync(package))
                {
                    if (stream == null)
                    {
                        throw new InvalidOperationException("Couldn't download the package from the storage.");
                    }

                    buffer = stream.ReadAllBytes();
                    cacheService.SetBytes(cacheKey, buffer);
                }
            }

            return new ZipPackage(new MemoryStream(buffer));
        }

        private static string CreateCacheKey(string id, string version)
        {
            return id.ToLowerInvariant() + "." + version.ToLowerInvariant();
        }
    }
}