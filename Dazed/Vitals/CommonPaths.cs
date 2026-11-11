using Dazed.Vitals.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dazed.Vitals
{
    /// <summary>
    /// All the common paths and file extensions that the game may use
    /// </summary>
    public static class CommonPaths
    {
        public const string LocalizationFolderName = "localization";
        public const string ShadersFolderName = "shaders";
        public const string ResourcesFolderName = "resources";
        public const string FontsFolderName = "fonts";
        public const string MapDataFolderName = "maps";
        public const string ExposedDataFile = "expDat.data";
        public const string LogsFolderName = "logs";
        public const string MaterialFolderName = "materials";
        /// <summary>
        /// Includes the dot
        /// </summary>
        public const string VertexShaderFileExtension = ".vert";
        /// <summary>
        /// Includes the dot
        /// </summary>
        public const string FragmentShaderFileExtension = ".frag";
        /// <summary>
        /// Includes dot
        /// </summary>
        public const string MaterialExtension = ".material";

        public static string RelativeFontsFolder { get; } = Path.Combine(ResourcesFolderName, FontsFolderName);
        public static string RelativeMapFolder { get; } = Path.Combine(ResourcesFolderName, MapDataFolderName);
        public static string RelativeMaterialsFolder {get;} = Path.Combine(ResourcesFolderName, MaterialFolderName);
        public static string MissingTexturePath { get; } = Path.Combine(ResourcesFolderName, "missingtexture.png");
    }
}
