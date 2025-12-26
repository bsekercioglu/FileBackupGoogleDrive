using System.Collections.Generic;

namespace FileBackupGoogleDrive.Models
{
    public class ConfigModel
    {
        public string SourceFolder { get; set; } = string.Empty;
        public string DriveFolderName { get; set; } = string.Empty;
        public List<string> FileExtensions { get; set; } = new List<string>();
        public string CredentialsJson { get; set; } = string.Empty;
        public string ApplicationName { get; set; } = "FileBackupGoogleDrive";
        public int CheckIntervalMinutes { get; set; } = 60;
        public bool DeleteAfterBackup { get; set; } = false;
    }
}

