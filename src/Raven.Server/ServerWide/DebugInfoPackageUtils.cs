﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Raven.Server.Routing;

namespace Raven.Server.ServerWide
{
    public static class DebugInfoPackageUtils
    {
        public static readonly IReadOnlyList<RouteInformation> Routes =
            RouteScanner.Scan(attr => attr.IsDebugInformationEndpoint &&
                                      attr.Path.Contains("info-package") == false).Values.ToList();

        public static string GetOutputPathFromRouteInformation(RouteInformation route, string prefix)
        {
            var path = route.Path;
            if (path.StartsWith("/debug/"))
                path = path.Replace("/debug/", string.Empty);
            else if (path.StartsWith("debug/"))
                path = path.Replace("debug/", string.Empty);

            path = path.Replace("/databases/*/", string.Empty)
                       .Replace("debug/",string.Empty) //if debug/ left in the middle, remove it as well
                       .Replace("/", ".");
            return !string.IsNullOrWhiteSpace(prefix) ? 
                Path.Combine(prefix,$"{path}.json") :
                $"{path}.json";
        }

        public static void WriteExceptionAsZipEntry(Exception e, ZipArchive archive, string entryName)
        {          
            var entry = archive.CreateEntry($"{entryName}.error");
            using (var entryStream = entry.Open())
            using (var sw = new StreamWriter(entryStream))
            {
                sw.Write(e);
                sw.Flush();
            }
        }

    }
}
