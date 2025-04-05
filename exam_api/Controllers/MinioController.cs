using exam_api.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Minio;

namespace exam_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinioController(MinioService service) : ControllerBase
    {
        private readonly MinioService service = service;
    
        //public async Task<IActionResult> UploadFile()

        [HttpGet("{name}")]
        public async Task<IActionResult> GetFileUrl(string name)
        {
            string url = await service.GetFileUrlAsync(name);
            return Ok(url);
        }
    }   
}