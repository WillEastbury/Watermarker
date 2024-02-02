using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ImageMagick;
using System.Net.Http;
public static class ApplyWatermark
{
    [FunctionName("ApplyWatermark")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WaterMarker/{imageUrl}/{x}/{y}/{watermarkText}")] HttpRequest req, 
        string imageUrl, int x, int y, string watermarkText,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request for a watermark.");
        byte[] imageBytes;
        using (HttpClient client = new HttpClient())
        {
            imageBytes = await client.GetByteArrayAsync(imageUrl);
        }
        using (MemoryStream inputStream = new MemoryStream(imageBytes))
        {
            using (MagickImage image = new MagickImage(inputStream))
            {
                IMagickGeometry magickGeometry = new MagickGeometry(x, y, image.Width, image.Height);
                image.Annotate(watermarkText, magickGeometry, Gravity.South);
                using (MemoryStream outputStream = new MemoryStream())
                {
                    image.Write(outputStream);
                    return new FileContentResult(outputStream.ToArray(), "image/png");
                }
            }
        }
    }
}
