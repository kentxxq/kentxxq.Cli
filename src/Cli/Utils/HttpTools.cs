using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Curl.CommandLine.Parser;

namespace Cli.Utils;

public class HttpTools
{
    /// <summary>
    /// curl命令转换成HttpRequestMessage对象
    /// </summary>
    /// <param name="curlCommand"></param>
    /// <returns></returns>
    public static async Task<HttpRequestMessage?> CurlToHttpRequestMessage(string curlCommand)
    {
        HttpRequestMessage? request = null;
        var parser = new CurlParser();
        var output = parser.Parse(curlCommand);
        if (output is { Success: true })
        {
            MyLog.Logger?.Debug("curl文件解析成功");
            // MyLog.Logger?.Debug($"{output.Data.HttpMethod} {output.Data.Url}");
            // MyLog.Logger?.Debug($"Header: {string.Join(",",output.Data.Headers)}");
            // MyLog.Logger?.Debug($"Body: {output.Data.UploadData.First().Content??"无Content"}");

            //header
            request = new HttpRequestMessage(new HttpMethod(output.Data.HttpMethod), output.Data.Url);
            foreach (var header in output.Data.Headers)
            {
                if (header.Key != "content-type")
                {
                    request.Headers.Add(header.Key,header.Value);
                }
            }

            //content
            if (output.Data.Headers.ContainsKey("content-type"))
            {
                request.Content = new StringContent(output.Data.UploadData.First().Content);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(output.Data.Headers["content-type"]);
            }
            
            MyLog.Logger?.Debug(await HttpRequestMessageToString(request));

        }

        return request;
    }

    /// <summary>
    /// HttpResponseMessage转成格式化好的字符串
    /// </summary>
    /// <param name="httpResponseMessage"></param>
    /// <returns></returns>
    public static async Task<string> HttpResponseMessageToString(HttpResponseMessage httpResponseMessage)
    {
        var data = httpResponseMessage.Headers.ToDictionary(h => h.Key, h => h.Value.First().ToString());
        var body = await httpResponseMessage.Content.ReadAsStringAsync();
        data.Add("body",body);
        data.Add("statusCode",$"{(int)httpResponseMessage.StatusCode}{httpResponseMessage.StatusCode}");
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
    
    /// <summary>
    /// HttpRequestMessage转成格式化好的字符串
    /// </summary>
    /// <param name="httpRequestMessage"></param>
    /// <returns></returns>
    public static async Task<string> HttpRequestMessageToString(HttpRequestMessage httpRequestMessage)
    {
        var data = httpRequestMessage.Headers.ToDictionary(h => h.Key, h => h.Value.First().ToString());
        data.Add("url",httpRequestMessage.RequestUri?.ToString()??string.Empty);

        if (httpRequestMessage.Content is StringContent stringContent)
        {
            data.Add("body",await stringContent.ReadAsStringAsync());
        }
        
        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }
}