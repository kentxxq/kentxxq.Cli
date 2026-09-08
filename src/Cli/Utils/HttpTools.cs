using System.Net.Http;
using System.Text.Json;
using Curl.CommandLine.Parser;

namespace Cli.Utils;

public static class HttpTools
{
    public static async Task<string?> ReadCurlFile(FileInfo? file, CancellationToken ct)
    {
        if (file is null) return null;
        if (!file.Exists) throw new FileNotFoundException("curl 文件不存在。");
        var text = await File.ReadAllTextAsync(file.FullName, ct);
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("curl 文件为空。");
        return text;
    }

    public static async Task<HttpRequestMessage> CreateRequest(string? url, string? curl)
    {
        if (curl is not null) return await CurlToHttpRequestMessage(curl);
        ValidateUrl(url);
        return new HttpRequestMessage(HttpMethod.Get, url);
    }

    private static void ValidateUrl(string? url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new ArgumentException("需要有效的 HTTP 或 HTTPS 地址。");
    }

    public static Task<HttpRequestMessage> CurlToHttpRequestMessage(string curlCommand)
    {
        var output = new CurlParser().Parse(curlCommand);
        if (!output.Success) throw new ArgumentException("无法解析 curl 文件。");
        ValidateUrl(output.Data.Url?.ToString());
        var request = new HttpRequestMessage(new HttpMethod(output.Data.HttpMethod), output.Data.Url);
        try
        {
            var uploads = output.Data.UploadData?.ToList();
            if (uploads is { Count: > 1 }) throw new ArgumentException("暂不支持多个 curl 请求体。");
            if (uploads is { Count: 1 }) request.Content = new StringContent(uploads[0].Content ?? "");
            foreach (var header in output.Data.Headers)
            {
                if (request.Headers.TryAddWithoutValidation(header.Key, header.Value)) continue;
                request.Content ??= new ByteArrayContent([]);
                request.Content.Headers.Remove(header.Key);
                if (!request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value))
                    throw new ArgumentException("不支持的 curl 请求头。");
            }
            return Task.FromResult(request);
        }
        catch
        {
            request.Dispose();
            throw;
        }
    }

    // 调试输出只保留协议元数据，不读取或记录请求头和正文。
    public static Task<string> HttpRequestMessageToString(HttpRequestMessage request) =>
        Task.FromResult(JsonSerializer.Serialize(new { method = request.Method.Method, url = "fake_url", body = "fake_info" }));

    public static Task<string> HttpResponseMessageToString(HttpResponseMessage response) =>
        Task.FromResult(JsonSerializer.Serialize(new { statusCode = (int)response.StatusCode, body = "fake_info" }));
}
