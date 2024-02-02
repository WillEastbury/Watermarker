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
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        log.LogInformation("C# HTTP trigger function processed a request.");

        // Get URL and text from the request
        string imageUrl = req.Form["imageUrl"];
        string watermarkText = req.Form["watermarkText"];

        // Download image from URL
        byte[] imageBytes;
        using (HttpClient client = new HttpClient())
        {
            imageBytes = await client.GetByteArrayAsync(imageUrl);
        }

        // Apply watermark using ImageMagick.NET
        using (MemoryStream inputStream = new MemoryStream(imageBytes))
        {
            using (MagickImage image = new MagickImage(inputStream))
            {
                IMagickGeometry magickGeometry = new MagickGeometry(0, 0, image.Width, image.Height);
                // Add watermark text
                image.Annotate(watermarkText, magickGeometry, Gravity.South);

                // Save the watermarked image
                using (MemoryStream outputStream = new MemoryStream())
                {
                    image.Write(outputStream);
                    return new FileContentResult(outputStream.ToArray(), "image/png");
                }
            }
        }
    }
}