using System;
using System.IO;
using System.Threading.Tasks;
using FileBackupGoogleDrive.Services;

namespace FileBackupGoogleDrive
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Google Drive Yedekleme Servisi Başlatılıyor...");
                Console.WriteLine("================================================");

                // INI dosyasından yapılandırmayı yükle
                var config = IniFileReader.LoadConfig("config.ini");
                
                if (config == null)
                {
                    Console.WriteLine("HATA: config.ini dosyası bulunamadı veya okunamadı!");
                    Console.WriteLine("Lütfen config.ini dosyasını kontrol edin.");
                    Console.ReadKey();
                    return;
                }

                // Yedekleme servisini başlat
                var backupService = new GoogleDriveBackupService(config);
                
                Console.WriteLine($"Kaynak Klasör: {config.SourceFolder}");
                Console.WriteLine($"Hedef Klasör (Drive): {config.DriveFolderName}");
                Console.WriteLine($"Dosya Uzantıları: {string.Join(", ", config.FileExtensions)}");
                Console.WriteLine("================================================");
                Console.WriteLine();

                // Yedeklemeyi başlat
                await backupService.StartBackupAsync();

                Console.WriteLine();
                Console.WriteLine("Yedekleme işlemi tamamlandı!");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.WriteLine("                    HATA OLUŞTU");
                Console.WriteLine("═══════════════════════════════════════════════════");
                Console.WriteLine();
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                Console.WriteLine("═══════════════════════════════════════════════════");
            }

            Console.WriteLine();
            Console.WriteLine("Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}

