using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MyApp.Business.Services
{
    public interface IBackblazeStorageService
    {
        Task<string> UploadFileAsync(string localFilePath, string remotePath);
    }

    public class BackblazeStorageService : IBackblazeStorageService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accountId;
        private readonly string _applicationKey;
        private readonly string _bucketId;
        private readonly string _bucketName;

        public BackblazeStorageService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _accountId = configuration["Backblaze:AccountId"] ?? throw new ArgumentNullException("Backblaze:AccountId");
            _applicationKey = configuration["Backblaze:ApplicationKey"] ?? throw new ArgumentNullException("Backblaze:ApplicationKey");
            _bucketId = configuration["Backblaze:BucketId"] ?? throw new ArgumentNullException("Backblaze:BucketId");
            _bucketName = configuration["Backblaze:BucketName"] ?? throw new ArgumentNullException("Backblaze:BucketName");
        }

        public async Task<string> UploadFileAsync(string localFilePath, string remotePath)
        {
            if (!File.Exists(localFilePath))
                throw new FileNotFoundException("File not found", localFilePath);

            // 1️⃣ Authorize Account
            var authResponse = await AuthorizeAccountAsync();

            // 2️⃣ Get Upload URL
            var uploadData = await GetUploadUrlAsync(authResponse.apiUrl, authResponse.authorizationToken);

            // 3️⃣ Upload file
            var fileUrl = await UploadFileToB2Async(
                localFilePath,
                remotePath,
                uploadData.uploadUrl,
                uploadData.uploadAuthToken,
                authResponse.downloadUrl
            );

            return fileUrl;
        }

        private async Task<(string apiUrl, string authorizationToken, string downloadUrl)> AuthorizeAccountAsync()
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_accountId}:{_applicationKey}"));
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.backblazeb2.com/b2api/v2/b2_authorize_account");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var apiUrl = doc.RootElement.GetProperty("apiUrl").GetString();
            var authToken = doc.RootElement.GetProperty("authorizationToken").GetString();
            var downloadUrl = doc.RootElement.GetProperty("downloadUrl").GetString();

            return (apiUrl!, authToken!, downloadUrl!);
        }

        private async Task<(string uploadUrl, string uploadAuthToken)> GetUploadUrlAsync(string apiUrl, string authToken)
        {
            var requestUrl = $"{apiUrl}/b2api/v2/b2_get_upload_url";
            var body = new { bucketId = _bucketId };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            // ⚠️ FIX: Không dùng AuthenticationHeaderValue vì token không có scheme
            request.Headers.TryAddWithoutValidation("Authorization", authToken);
            request.Content = content;

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            var uploadUrl = doc.RootElement.GetProperty("uploadUrl").GetString();
            var uploadAuthToken = doc.RootElement.GetProperty("authorizationToken").GetString();

            return (uploadUrl!, uploadAuthToken!);
        }

        private async Task<string> UploadFileToB2Async(
            string localFilePath,
            string remotePath,
            string uploadUrl,
            string uploadAuthToken,
            string downloadUrl)
        {
            var fileBytes = await File.ReadAllBytesAsync(localFilePath);
            var fileNameEncoded = Uri.EscapeDataString(remotePath);

            // Tính SHA1 của file (bắt buộc)
            string fileSha1;
            using (var sha1 = SHA1.Create())
            {
                var hash = sha1.ComputeHash(fileBytes);
                fileSha1 = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }

            var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
            request.Headers.TryAddWithoutValidation("Authorization", uploadAuthToken);
            request.Headers.Add("X-Bz-File-Name", fileNameEncoded);
            request.Headers.Add("X-Bz-Content-Sha1", fileSha1);

            request.Content = new ByteArrayContent(fileBytes);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("b2/x-auto");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Parse response
            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);

            // Trả về URL public tải file
            var finalUrl = $"{downloadUrl}/file/{_bucketName}/{remotePath}";
            return finalUrl;
        }
    }
}
