using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BaseRestAPI.Repos;
using BaseRestAPI.DTOs;
using BaseRestAPI.Model;
using Newtonsoft.Json;
using System;
//File contains base implementation for CRUD(CREATE READ UPDATE DELETE) operations for a table.
namespace BaseRestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseRestAPIController<TEntity, TRepository, IDType> : ControllerBase
        where TEntity : IEntity<IDType>
        where TRepository : IRepository<TEntity, IDType>
    {
        protected readonly TRepository repository;
        public BaseRestAPIController(TRepository repository)
        {
            this.repository = repository;
        }

        // GET: api/[controller]
        //Gets multple records from database, sa many as the page parameter asks for.
        [HttpGet]
        public virtual async Task<ActionResult<IEnumerable<TEntity>>> Get([FromQuery] PagingQuery page, [FromQuery] string Orderby, [FromQuery] bool asc)
        {
            var items = await repository.GetAll(page, Orderby, asc);
            var metadata = new
            {
                items.TotalCount,
                items.PageSize,
                items.CurrentPage,
                items.TotalPages,
                items.HasNext,
                items.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
            return items;
        }

        // GET: api/[controller]/5
        //Gets specific record with id.
        [HttpGet("{id}")]
        public virtual async Task<ActionResult<TEntity>> Get(IDType id, [FromQuery] bool detail)
        {
            var item = detail ? await repository.GetWithDetails(id) : await repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return Ok(item);
        }

        // PUT: api/[controller]/5
        //Updates a specific record with the TEntity item received.
        [HttpPut("{id}")]
        public virtual async Task<ActionResult<HttpResponse<TEntity>>> Put(IDType id, TEntity item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await repository.Put(item);
            }
            catch (Exception ex)
            {
                return BadRequest("Ekki hægt að uppfæra eigindi með breyttum gildum.");
            }
            return Ok(item);
        }


        // POST: api/[controller]
        // Inserts the received item as a new record
        [HttpPost]
        public virtual async Task<ActionResult<HttpResponse<TEntity>>> Post(TEntity item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var response = new HttpResponse<TEntity>();

            try
            {
                var newItem = await repository.Add(item);
                response.Item = newItem;
                return CreatedAtAction("Get", new { id = newItem.Id }, response);
            }
            catch (DbUpdateException ex)
            {
                Exception e = ex;
                var innermessage = "git it";
                while (e.InnerException != null) e = e.InnerException;
                innermessage = e.Message;
                return Conflict(innermessage);
            }
        }

        // DELETE: api/[controller]/5
        //Removes a record with the identifier id.
        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<TEntity>> Delete(IDType id)
        {
            var item = await repository.Delete(id);
            if (item == null)
            {
                return NotFound();
            }
            return item;
        }

        //GET: api/[controller]/
        //TODO 4 DOC: Don't remember how the uri parameter looks like as url
        //Receives a Dictionary from uri params.
        //This dictionary provides a key that is the same name as a table property intended to be search with the value this key indexes.
        [HttpGet("search")]
        public virtual async Task<ActionResult<List<TEntity>>> SearchGet([FromQuery] Dictionary<string, string[]> dict, [FromQuery] string Orderby, [FromQuery] PagingQuery page, [FromQuery] bool asc, [FromQuery] Dictionary<string, string[]> dateDict)
        {
            var results = await repository.Search(page, dict, Orderby, asc, dateDict);
            var metadata = new
            {
                results.TotalCount,
                results.PageSize,
                results.CurrentPage,
                results.TotalPages,
                results.HasNext,
                results.HasPrevious
            };

            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(metadata));
            return Ok(results);
        }
    }
}