using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBackupGoogleDrive.Models;

namespace FileBackupGoogleDrive.Services
{
    public static class IniFileReader
    {
        public static ConfigModel LoadConfig(string iniFilePath)
        {
            if (!File.Exists(iniFilePath))
            {
                return null;
            }

            var config = new ConfigModel();
            var lines = File.ReadAllLines(iniFilePath);

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                
                // Boş satırları ve yorumları atla
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                // Key=Value formatını parse et
                var equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex <= 0)
                    continue;

                var key = trimmedLine.Substring(0, equalIndex).Trim();
                var value = trimmedLine.Substring(equalIndex + 1).Trim();

                // Değerleri config nesnesine ata
                switch (key.ToLower())
                {
                    case "sourcefolder":
                        config.SourceFolder = value;
                        break;
                    case "drivefoldername":
                        config.DriveFolderName = value;
                        break;
                    case "fileextensions":
                        config.FileExtensions = value.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(e => e.Trim().ToLower())
                            .ToList();
                        break;
                    case "credentialsjson":
                        config.CredentialsJson = value;
                        break;
                    case "applicationname":
                        config.ApplicationName = value;
                        break;
                    case "checkintervalminutes":
                        if (int.TryParse(value, out int interval))
                            config.CheckIntervalMinutes = interval;
                        break;
                    case "deleteafterbackup":
                        if (bool.TryParse(value, out bool delete))
                            config.DeleteAfterBackup = delete;
                        break;
                }
            }

            // Gerekli alanları kontrol et
            if (string.IsNullOrWhiteSpace(config.SourceFolder) ||
                string.IsNullOrWhiteSpace(config.DriveFolderName) ||
                string.IsNullOrWhiteSpace(config.CredentialsJson) ||
                config.FileExtensions == null || config.FileExtensions.Count == 0)
            {
                throw new Exception("Config dosyasında eksik parametreler var!");
            }

            return config;
        }
    }
}

