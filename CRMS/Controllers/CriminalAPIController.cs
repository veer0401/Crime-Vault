using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CRMS.Controllers
{
    [Route("api/[controller]")]
    [ApiController] // This attribute indicates that this is an API controller
    public class CriminalApiController : ControllerBase
    {
        private readonly AppDbContext context;

        public CriminalApiController(AppDbContext context)
        {
            this.context = context;
        }

        // GET: api/criminal
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Criminal>>> GetCriminals()
        {
            var criminals = await context.Criminal.ToListAsync();
            return Ok(new { data = criminals });
        }

        // Additional API methods can be added here (e.g., Get by ID, Update, Delete)
    }
}