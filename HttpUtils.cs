namespace HttpUtils;

public static class Utils  // 改为 public static
{
    private static void Log(string message)
    {
        Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
    }

    /// <summary>
    /// 发起 GET 请求并返回响应内容
    /// </summary>
    /// <param name="url">请求的 URL</param>
    /// <returns>响应内容的字符串（JSON/XML/Plain Text）</returns>
    public static async Task<string?> HttpGetAsync(string url)
    {
        using HttpClient client = new();
        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Log("请求成功，响应内容已返回");
                return responseBody;  // 返回响应内容
            }
            else
            {
                Log($"请求失败，状态码: {response.StatusCode}, 原因: {response.ReasonPhrase}");
                return null;  // 或 throw new HttpRequestException(...)
            }
        }
        catch (HttpRequestException e)
        {
            Log("请求错误: " + e.Message);
            return null;  // 或重新抛出异常 throw;
        }
    }
    /// <summary>
    /// 发起 POST 请求并返回响应内容
    /// </summary>
    /// <param name="url">请求的 URL</param>
    /// <param name="content">要发送的内容</param>
    /// <param name="contentType">内容类型（如 "application/json"）</param>
    /// <returns>响应内容的字符串</returns>
    /// var response = await Utils.HttpPostAsync("https://example.com/api", "{\"name\":\"value\"}");
    public static async Task<string?> HttpPostAsync(string url, string content, string contentType = "application/json")
    {
        using HttpClient client = new();
        try
        {
            var httpContent = new StringContent(content, System.Text.Encoding.UTF8, contentType);
            HttpResponseMessage response = await client.PostAsync(url, httpContent);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                Log("POST 请求成功，响应内容已返回");
                return responseBody;
            }
            else
            {
                Log($"POST 请求失败，状态码: {response.StatusCode}, 原因: {response.ReasonPhrase}");
                return null;
            }
        }
        catch (HttpRequestException e)
        {
            Log("POST 请求错误: " + e.Message);
            return null;
        }
    }
}