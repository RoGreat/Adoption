using System.IO;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace Adoption.Settings
{
    internal static class ModUtilities
    {
        private static string ConfigDirectory
        {
            get
            {
                return System.IO.Path.Combine(EngineFilePaths.ConfigsPath.Path, "ModSettings", SubModule.ModId);
            }
        }

        private static PlatformFilePath ConfigFullPath
        {
            get
            {
                return new PlatformFilePath(new PlatformDirectoryPath(PlatformFileType.User, ConfigDirectory), SubModule.ModId + "Config.txt");
            }
        }

        public static string LoadConfigFile()
        {
            PlatformFilePath configFullPath = ConfigFullPath;
            if (!FileHelper.FileExists(configFullPath))
            {
                return "";
            }
            return FileHelper.GetFileContentString(configFullPath);
        }

        public static SaveResult SaveConfigFile(string configProperties)
        {
            // Create mod config directory if not already created
            string configDirectoryPath = ConfigDirectory;
            if (!Directory.Exists(configDirectoryPath))
            {
                Directory.CreateDirectory(configDirectoryPath);
            }

            PlatformFilePath configFullPath = ConfigFullPath;
            SaveResult result;
            try
            {
                string data = configProperties.Substring(0, configProperties.Length - 1);
                FileHelper.SaveFileString(configFullPath, data);
                result = SaveResult.Success;
            }
            catch
            {
                TaleWorlds.Library.Debug.Print("Could not create " + SubModule.ModTitle + " Config file", 0, TaleWorlds.Library.Debug.DebugColor.White);
                result = SaveResult.ConfigFileFailure;
            }
            return result;
        }
    }
}
