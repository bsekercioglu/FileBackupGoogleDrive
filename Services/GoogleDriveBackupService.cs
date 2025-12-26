using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileBackupGoogleDrive.Models;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using DriveFile = Google.Apis.Drive.v3.Data.File;

namespace FileBackupGoogleDrive.Services
{
    public class GoogleDriveBackupService
    {
        private readonly ConfigModel _config;
        private DriveService _driveService;
        private string _driveFolderId;

        public GoogleDriveBackupService(ConfigModel config)
        {
            _config = config;
        }

        public async Task StartBackupAsync()
        {
            try
            {
                // Google Drive servisini başlat
                await InitializeDriveServiceAsync();

                // Hedef klasörü oluştur veya bul
                _driveFolderId = await GetOrCreateFolderAsync(_config.DriveFolderName);

                // Kaynak klasörü kontrol et
                if (!Directory.Exists(_config.SourceFolder))
                {
                    Console.WriteLine($"HATA: Kaynak klasör bulunamadı: {_config.SourceFolder}");
                    return;
                }

                // Yedeklenecek dosyaları bul
                var filesToBackup = GetFilesToBackup();

                if (filesToBackup.Count == 0)
                {
                    Console.WriteLine("Yedeklenecek dosya bulunamadı.");
                    return;
                }

                Console.WriteLine($"{filesToBackup.Count} dosya bulundu. Yedekleme başlatılıyor...");
                Console.WriteLine();

                // Dosyaları yedekle
                int successCount = 0;
                int failCount = 0;

                foreach (var filePath in filesToBackup)
                {
                    try
                    {
                        var fileName = Path.GetFileName(filePath);
                        Console.Write($"Yedekleniyor: {fileName}... ");

                        await UploadFileAsync(filePath, fileName);

                        Console.WriteLine("✓ Başarılı");

                        // Yedekleme sonrası silme seçeneği
                        if (_config.DeleteAfterBackup)
                        {
                            System.IO.File.Delete(filePath);
                            Console.WriteLine($"  → Dosya silindi: {fileName}");
                        }

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"✗ HATA: {ex.Message}");
                        failCount++;
                    }
                }

                Console.WriteLine();
                Console.WriteLine($"Yedekleme Özeti:");
                Console.WriteLine($"  Başarılı: {successCount}");
                Console.WriteLine($"  Başarısız: {failCount}");
            }
            catch
            {
                // Hata mesajını yukarıya fırlat, Program.cs'de gösterilecek
                throw;
            }
        }

        private async Task InitializeDriveServiceAsync()
        {
            try
            {
                Console.WriteLine("Google Drive servisi başlatılıyor...");

                if (!System.IO.File.Exists(_config.CredentialsJson))
                {
                    var fullPath = Path.GetFullPath(_config.CredentialsJson);
                    throw new Exception($"Credentials JSON dosyası bulunamadı!\nDosya yolu: {fullPath}\nLütfen config.ini dosyasındaki 'CredentialsJson' parametresini kontrol edin.");
                }

                // JSON credentials dosyasını oku
                string[] scopes = { DriveService.Scope.DriveFile };
                var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromFile(_config.CredentialsJson).Secrets,
                    scopes,
                    "user",
                    System.Threading.CancellationToken.None,
                    new FileDataStore("DriveApiAuth", true));

                // Drive servisini oluştur
                _driveService = new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _config.ApplicationName
                });

                Console.WriteLine("Google Drive servisi başarıyla başlatıldı.");
            }
            catch (FileNotFoundException)
            {
                throw new Exception("Credentials JSON dosyası bulunamadı! Lütfen config.ini dosyasındaki 'CredentialsJson' parametresini kontrol edin.");
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception("Google hesabınıza erişim izni verilemedi. Lütfen credentials.json dosyasını kontrol edin.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("credentials") || ex.Message.Contains("Credential"))
                {
                    throw new Exception($"Google Drive kimlik doğrulama hatası: {ex.Message}");
                }
                throw new Exception($"Google Drive servisi başlatılamadı: {ex.Message}");
            }
        }

        private async Task<string> GetOrCreateFolderAsync(string folderName)
        {
            try
            {
                if (_driveService == null)
                    throw new InvalidOperationException("Drive servisi başlatılmamış!");

                // Mevcut klasörleri ara
                var listRequest = _driveService.Files.List();
                listRequest.Q = $"mimeType='application/vnd.google-apps.folder' and name='{folderName}' and trashed=false";
                listRequest.Fields = "files(id, name)";

                var folders = await listRequest.ExecuteAsync();

                if (folders.Files != null && folders.Files.Count > 0)
                {
                    Console.WriteLine($"Klasör bulundu: {folderName} (ID: {folders.Files[0].Id})");
                    return folders.Files[0].Id;
                }

                // Klasör yoksa oluştur
                var folderMetadata = new DriveFile()
                {
                    Name = folderName,
                    MimeType = "application/vnd.google-apps.folder"
                };

                var createRequest = _driveService.Files.Create(folderMetadata);
                createRequest.Fields = "id";
                var folder = await createRequest.ExecuteAsync();

                Console.WriteLine($"Yeni klasör oluşturuldu: {folderName} (ID: {folder.Id})");
                return folder.Id;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("network") || ex.Message.Contains("timeout") || ex.Message.Contains("connection"))
                {
                    throw new Exception($"Google Drive'a bağlanılamadı. İnternet bağlantınızı kontrol edin: {ex.Message}");
                }
                throw new Exception($"Google Drive klasörü oluşturulamadı veya bulunamadı: {ex.Message}");
            }
        }

        private List<string> GetFilesToBackup()
        {
            var files = new List<string>();

            try
            {
                var allFiles = Directory.GetFiles(_config.SourceFolder, "*.*", SearchOption.AllDirectories);

                foreach (var file in allFiles)
                {
                    var extension = Path.GetExtension(file).ToLower();
                    if (_config.FileExtensions.Contains(extension))
                    {
                        files.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Dosya listesi alınırken hata: {ex.Message}");
            }

            return files;
        }

        private async Task UploadFileAsync(string filePath, string fileName)
        {
            try
            {
                if (_driveService == null)
                    throw new InvalidOperationException("Drive servisi başlatılmamış!");
                
                if (_driveFolderId == null)
                    throw new InvalidOperationException("Drive klasör ID'si belirlenmemiş!");

                var fileMetadata = new DriveFile()
                {
                    Name = fileName,
                    Parents = new List<string> { _driveFolderId }
                };

                using (var stream = new FileStream(filePath, FileMode.Open))
                {
                    var request = _driveService.Files.Create(fileMetadata, stream, GetMimeType(filePath));
                    request.Fields = "id, name, size";
                    await request.UploadAsync();
                }
            }
            catch (FileNotFoundException)
            {
                throw new Exception($"Dosya bulunamadı: {fileName}");
            }
            catch (UnauthorizedAccessException)
            {
                throw new Exception($"Dosyaya erişim izni yok: {fileName}");
            }
            catch (IOException ex)
            {
                throw new Exception($"Dosya okuma hatası: {ex.Message}");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("quota") || ex.Message.Contains("storage"))
                {
                    throw new Exception($"Google Drive depolama alanı dolu! Lütfen Drive'ınızda yer açın.");
                }
                if (ex.Message.Contains("network") || ex.Message.Contains("timeout"))
                {
                    throw new Exception($"Ağ hatası: Dosya yüklenirken bağlantı kesildi.");
                }
                throw new Exception($"Dosya yüklenemedi: {ex.Message}");
            }
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLower();
            
            var mimeTypes = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".txt", "text/plain" },
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".zip", "application/zip" },
                { ".rar", "application/x-rar-compressed" },
                { ".7z", "application/x-7z-compressed" }
            };

            return mimeTypes.ContainsKey(extension) 
                ? mimeTypes[extension] 
                : "application/octet-stream";
        }
    }
}

