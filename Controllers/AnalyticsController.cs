using AnalyticsApi.Models;
using AnalyticsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AnalyticsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _analyticsService;


        public AnalyticsController(AnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }


        [HttpGet("datetime", Name = "GetDataByRange")]
        public ActionResult<DTOModel> GetDataByRange(DateTime startdatetime, DateTime enddatetime)
        {
            var data = _analyticsService.GetDataByRange(startdatetime,enddatetime);
            if (data == null)
            {
                return NotFound();
            }
            return new JsonResult(data);
        }

        [HttpGet("filter", Name = "GetData")]
        public ActionResult<DTOModel> Get(DateTime datetime)
        {
            var data = _analyticsService.Get(datetime);
            if (data == null)
            {
                return NotFound();
            }
            return new JsonResult(data);
        }
        [HttpGet]                                                               
        public ActionResult<List<DTOModel>> Get()
        {
            var list = _analyticsService.Get();

  
            return new JsonResult(list);
        }

        [HttpPost("upload", Name = "upload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile(
         IFormFile file,
         CancellationToken cancellationToken)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
        { 
            if (CheckIfExcelFile(file))
            {

                await WriteFile(file);
            }
            else
            {
                return BadRequest(new { message = "Invalid file extension" });
            }

            return Ok();
        }
        private bool CheckIfExcelFile(IFormFile file)
        {
            var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
            return (extension == ".csv"); // Change the extension based on your need
        }

        private async Task<bool> WriteFile(IFormFile file)
        {
            bool isSaveSuccess = false;
            string fileName;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = file.FileName;
                var pathBuilt = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files");

                if (!Directory.Exists(pathBuilt))
                {
                    Directory.CreateDirectory(pathBuilt);
                }

                var path = Path.Combine(Directory.GetCurrentDirectory(), "Upload\\files",
                   fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                isSaveSuccess = true;
                _analyticsService.ConvertAndMergeModel(fileName);
            }
            catch (Exception e)
            {
                //log error
            }


            return isSaveSuccess;
        }

        //[HttpPut("datetime")]
        //public IActionResult Update2(string datetime, DTOModel data)
        //{
        //    var dtomodel = _analyticsService.Get(datetime);

        //    if (dtomodel == null)
        //    {
        //        return NotFound();
        //    }
        //    _analyticsService.Update2(datetime, data);
        //    return new JsonResult(dtomodel);
        //    //return CreatedAtRoute("GetAnalyticsModel", new { id = analyticsmodel.Id.ToString() }, analyticsmodel);
        //}

        [HttpDelete("datetime")]
        public IActionResult Delete(DateTime datetime)
        {
            var data = _analyticsService.Get(datetime);
            if (data == null)
            {
                return NotFound();
            }
            _analyticsService.Remove(data.DateTime);

            return NoContent();
        }


    }
}