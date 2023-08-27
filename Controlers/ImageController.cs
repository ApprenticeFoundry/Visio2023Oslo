using Microsoft.AspNetCore.Mvc;
using IoBTMessage.Models;
using SkiaSharp;
using FoundryBlazor.Extensions;
using FoundryRulesAndUnits.Models;
using FoundryRulesAndUnits.Extensions;

namespace Foundry.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {
        [HttpPost("UploadSave")]
        public async Task<ActionResult<ContextWrapper<UDTO_Image>>> UploadSave(IFormFile file)
        {
            try
            {
                var wrap = new ContextWrapper<UDTO_Image>($"{file.FileName} not available.");

                var rootPath = "wwwroot";
                var imagesPath = "images";
                var fileName = file.FileName;
                var fullFilePath = $"{rootPath}/{imagesPath}/{fileName}";

                await using FileStream fs = new(fullFilePath, FileMode.Create);
                await file.OpenReadStream().CopyToAsync(fs);
                fs.Close();

                var imgURL = $"{imagesPath}/{fileName}";
                // $"ImageUploadSave imgURL={imgURL}".WriteLine(ConsoleColor.Yellow);

                var img = SKBitmap.Decode(fullFilePath);
                // $"ImageUploadSave {img.Width}, {img.Height}".WriteLine(ConsoleColor.Yellow);

                var response = new UDTO_Image()
                {
                    url = imgURL,
                    width = img.Width,
                    height = img.Height
                };


                //_pubSub.Notify("IMAGE_SAVED", source);

                wrap = new ContextWrapper<UDTO_Image>(response);
                return Ok(wrap);
            }
            catch (Exception ex)
            {
                // return StatusCode(500, ex.Message);
                $"ImageUploadSave Error: {ex.Message}".WriteLine(ConsoleColor.Gray);
                var wrap = new ContextWrapper<UDTO_Image>(ex.Message);
                return BadRequest(wrap);
            }
        }


        // [HttpPost("ImageUpload")]
        // public async Task<ActionResult<ContextWrapper<DT_StatusText>>> ImageUpload(IFormFile file)
        // {
        //     try
        //     {
        //         long size = file.Length;
        //         $"file size={size}".WriteLine(ConsoleColor.Yellow);

        //         var wrap = new ContextWrapper<DT_StatusText>($"{file.FileName} not available.");
        //         if (size > 0)
        //         {
        //             var data = await file.ReadAsBase64Async();

        //             var errors = new List<DT_StatusText>();

        //             _pubSub.Notify("IMAGE_UPLOADED", data);

        //             wrap = new ContextWrapper<DT_StatusText>(errors);
        //         }
        //         // await Task.FromResult(0);
        //         return Ok(wrap);
        //     }
        //     catch (Exception ex)
        //     {
        //         var wrap = new ContextWrapper<DT_Error>(ex.Message);
        //         return BadRequest(wrap);
        //     }
        // }

    }
}
