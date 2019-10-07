using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using blazor_experiment.Data;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace blazor_experiment
{
    [Route("api/[controller]")]
    public class AssetController : Controller
    {
        private Repository _repository;

        public AssetController(Repository repository)
        {
            _repository = repository;
        }

        // GET api/<controller>/5
        [HttpGet("thumbnail/{assetGuid}")]
        public ActionResult GetThumbnail(string assetGuid)
        {
            return File(_repository.GetThumbnail(assetGuid), "image/jpeg");
        }
    }
}
