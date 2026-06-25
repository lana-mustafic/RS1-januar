using System.Diagnostics;
using System.Text;

namespace Market.API.Middleware;

/// <summary>
/// Middleware that logs incoming HTTP requests and outgoing responses,
/// including duration, method, path, and status code.
/// </summary>
public sealed class RequestResponseLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestResponseLoggingMiddleware> logger)
{
    private const int SlowRequestThresholdMs = 400; // 400 ms

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        // Read request body (only for POST/PUT)
        string? requestBody = null;
        if (request.Method is "POST" or "PUT")
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            request.Body.Position = 0;
        }

        // Capture original response stream
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            // read response text for logging
            responseBody.Seek(0, SeekOrigin.Begin);
            var responseText = await new StreamReader(responseBody).ReadToEndAsync();
            responseBody.Seek(0, SeekOrigin.Begin);

            var logMessage = new StringBuilder()
                .AppendLine("HTTP Request/Response Log:")
                .AppendLine($"  Path: {request.Path}")
                .AppendLine($"  Method: {request.Method}")
                .AppendLine($"  Status: {context.Response.StatusCode}")
                .AppendLine($"  Duration: {stopwatch.ElapsedMilliseconds} ms");

            if (!string.IsNullOrWhiteSpace(requestBody))
                logMessage.AppendLine($"  Request Body: {requestBody}");

            if (!string.IsNullOrWhiteSpace(responseText))
                logMessage.AppendLine($"  Response Body: {responseText}");

            var elapsed = stopwatch.ElapsedMilliseconds;
            if (elapsed > SlowRequestThresholdMs)
            {
                logger.LogWarning("[SLOW REQUEST] {Path} took {Elapsed} ms", request.Path, elapsed);

                // >>> Bugfix 27.10.2025: sigurno pisanje na disk (ne ruši response ako folder ne postoji)
                try
                {
                    var logDir = Path.Combine(AppContext.BaseDirectory, "Logs");
                    Directory.CreateDirectory(logDir);
                    var logPath = Path.Combine(logDir, "slow-requests.log");
                    await File.AppendAllTextAsync(logPath, $"{DateTime.UtcNow:u} | {request.Path} | {elapsed} ms{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed writing slow-request log.");
                }
            }

            logger.LogInformation("{Log}", logMessage.ToString());

            // >>> Bugfix 27.10.2025: KLJUČNO: vrati originalni stream i kopiraj tijelo nazad da se vidi json error poruka
            context.Response.Body = originalBodyStream;
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }
}