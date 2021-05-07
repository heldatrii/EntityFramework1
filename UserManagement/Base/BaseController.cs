using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UserManagement.Repository.Interface;

namespace UserManagement.Base
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaseController<Entity, Repository, Key> : ControllerBase

     where Entity : class
     where Repository : IRepository<Entity, Key>
    {
        private readonly Repository repo;
        public BaseController(Repository repository)
        {
            this.repo = repository;
        }
        //panggil semua entity
        [HttpGet]
        public ActionResult Get()
        {
            IEnumerable<Entity> entities = repo.Get();
            return Ok(entities);
        }

        //delete dengan key yg ke
        [HttpDelete("{KEY}")]
        public ActionResult Delete(Key key)
        {
            Entity entity = repo.Get(key);
            if (entity == null)
            {
                return NotFound($"Eror{key} salah");
            }
            else
            {
                repo.Delete(key);
                return Ok();
            }

        }

        [HttpPut]
        public ActionResult Update(Entity entity) 
        {
            try
            {
                return Ok(repo.Update(entity));
            }
            catch (Exception)
            {
                return StatusCode(400, new { status = HttpStatusCode.BadRequest, message = "Data yang dimasukan belum lengkap" });
            }
        }

        [HttpPost]
        public ActionResult Post(Entity entity) 
        {
            repo.Insert(entity);
            return Ok();
            //try
            //{
            //    var result = repo.Insert(entity);
            //    return Ok();
            //}
            //catch (Exception)
            //{
            //    return StatusCode(405, new { status = HttpStatusCode.BadRequest, message = "Objek data yang dimasukan salah!!" });
            //}
        }

        [HttpGet("{KEY}")]
        public ActionResult Get(Key key) 
        {
            Entity entity = repo.Get(key);
            if (entity ==null)
            {
                return NotFound($"Erorr {key} salah");
            }
            return Ok(entity);
        }


        //penggunaan async
        //[HttpGet]
        //public async Task<ActionResult> Get()
        //{
        //    IEnumerable<Entity> entities = await Task.Run(() => repo.Get());
        //    return Ok(entities);
        //}

    }

}
