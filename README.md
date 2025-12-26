# Google Drive Dosya Yedekleme Servisi

Bu proje, belirtilen klasördeki dosyaları Google Drive hesabınıza güvenli bir şekilde yedeklemek için geliştirilmiştir.

## Özellikler

- ✅ INI dosyasından yapılandırma
- ✅ Belirli dosya türlerini filtreleme
- ✅ Google Drive API ile güvenli yedekleme
- ✅ Otomatik klasör oluşturma
- ✅ Yedekleme sonrası dosya silme seçeneği
- ✅ Detaylı log çıktıları

## Gereksinimler

- .NET Framework 4.8.1
- Google Drive API Credentials (JSON dosyası)

## Kurulum

### 1. Google Drive API Credentials Alma

1. [Google Cloud Console](https://console.cloud.google.com/) adresine gidin
2. Yeni bir proje oluşturun veya mevcut projeyi seçin
3. "APIs & Services" > "Library" bölümüne gidin
4. "Google Drive API" arayın ve etkinleştirin
5. "APIs & Services" > "Credentials" bölümüne gidin
6. "Create Credentials" > "OAuth client ID" seçin
7. Application type olarak "Desktop app" seçin
8. Oluşturulan credentials'ı JSON formatında indirin
9. İndirilen JSON dosyasını proje klasörüne `credentials.json` olarak kaydedin

### 2. Yapılandırma

`config.ini` dosyasını düzenleyin:

```ini
SourceFolder=D:\Backup\Source
DriveFolderName=Yedekler
FileExtensions=.pdf,.doc,.docx,.xls,.xlsx,.txt,.jpg,.png,.zip
CredentialsJson=credentials.json
ApplicationName=FileBackupGoogleDrive
CheckIntervalMinutes=60
DeleteAfterBackup=false
```

### 3. Derleme ve Çalıştırma

```bash
dotnet restore
dotnet build
dotnet run
```

## Kullanım

1. `config.ini` dosyasını ihtiyacınıza göre düzenleyin
2. `credentials.json` dosyasını proje klasörüne yerleştirin
3. Uygulamayı çalıştırın
4. İlk çalıştırmada tarayıcı açılacak ve Google hesabınızla giriş yapmanız istenecek
5. İzinleri onayladıktan sonra yedekleme işlemi başlayacaktır

## Yapılandırma Parametreleri

- **SourceFolder**: Yedeklenecek dosyaların bulunduğu klasör yolu
- **DriveFolderName**: Google Drive'da oluşturulacak klasör adı
- **FileExtensions**: Yedeklenecek dosya uzantıları (virgül veya noktalı virgül ile ayrılmış)
- **CredentialsJson**: Google Drive API credentials JSON dosyasının yolu
- **ApplicationName**: Uygulama adı
- **CheckIntervalMinutes**: Kontrol aralığı (dakika) - Gelecekte kullanılabilir
- **DeleteAfterBackup**: Yedekleme sonrası dosyaları sil (true/false)

## Notlar

- İlk çalıştırmada Google hesabınızla giriş yapmanız gerekecektir
- İzinler `DriveApiAuth` klasöründe saklanır (tekrar giriş yapmanız gerekmez)
- Büyük dosyalar için yükleme süresi uzun olabilir
- `DeleteAfterBackup=true` yaparsanız, yedeklenen dosyalar kaynak klasörden silinir

## Lisans

Bu proje özel kullanım içindir.

