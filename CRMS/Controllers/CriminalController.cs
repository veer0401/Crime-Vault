using CRMS.Data;
using CRMS.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CRMS.Services;

namespace CRMS.Controllers
{
    [Route("Criminal")]
    public class CriminalController : Controller
    {
        private readonly AppDbContext context;
        private readonly IWebHostEnvironment environment;

        public CriminalController(AppDbContext context, IWebHostEnvironment environment)
        {
            this.context = context;
            this.environment = environment;
        }

        // GET: Criminal/Viewlist
        [Route("Viewlist")]
        public IActionResult ViewList()
        {
            var criminals = context.Criminal.OrderBy(p => p.Id).ToList();
            return View(criminals);
        }

        // GET: Criminal/Add
        [Route("Add")]
        public IActionResult Add()
        {
            return View();
        }

        // POST: Criminal/Add
        [HttpPost]
        [Route("Add")]
        public async Task<IActionResult> Add(CriminalDTO criminalDTO)
        {
            // Validate the image file
            if (criminalDTO.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image is required");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                // Log the errors or inspect them
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return View(criminalDTO);
            }

            // Generate a unique filename
            string newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(criminalDTO.ImageFile.FileName);

            // Define the full path for saving the image
            string ImageFullpath = Path.Combine(environment.WebRootPath, "criminal", newfilename);

            // Save the image to the specified folder
            using (var stream = System.IO.File.Create(ImageFullpath))
            {
                await criminalDTO.ImageFile.CopyToAsync(stream); // Use async method
            }

            // Map DTO to Criminal model
            Criminal criminal = new Criminal()
            {  
                Name = criminalDTO.Name, 
                Alias = criminalDTO.Alias,
                Age = criminalDTO.Age.GetValueOrDefault(),
                Gender = criminalDTO.Gender,
                Description = criminalDTO.Description,
                ImageFilename = newfilename,
                CreatedAt = DateTime.Now,
                GangAffiliation = criminalDTO.GangAffiliation,
                Accomplices = criminalDTO.Accomplices,
                WeaponUsed = criminalDTO.WeaponUsed,
                KnownHabits = criminalDTO.KnownHabits,
                PsychologicalProfile = criminalDTO.PsychologicalProfile,
                Address = criminalDTO.Address,
                Caught = criminalDTO.Caught
            };

            // Add the new criminal to the database
            try
            {
                await context.Criminal.AddAsync(criminal); // Use async method
                await context.SaveChangesAsync(); // Use async method
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Unable to save changes. " + ex.Message);
                return View(criminalDTO);
            }

            // Redirect to the ViewList action
            return RedirectToAction("ViewList", "Criminal");
        }
        [HttpGet]
        [Route("Edit/{id}")]
        public IActionResult Edit(Guid id)
        {
            var criminal = context.Criminal.Find(id);
            if (criminal == null)
            {
                return RedirectToAction("ViewList", "Criminal");

            }
            
            var criminalDTO = new CriminalDTO()
            {
                Name = criminal.Name,
                Alias = criminal.Alias,
                Age = criminal.Age,
                Gender = criminal.Gender,
                Description = criminal.Description,
                //ImageFilename = newfilename,
                //CreatedAt = DateTime.Now,
                GangAffiliation = criminal.GangAffiliation,
                Accomplices = criminal.Accomplices,
                WeaponUsed = criminal.WeaponUsed,
                KnownHabits = criminal.KnownHabits,
                PsychologicalProfile = criminal.PsychologicalProfile,
                Address = criminal.Address,
                Caught = criminal.Caught
            };

            ViewData["CriminalId"] = criminal.Id;
            ViewData["ImageFileName"] = criminal.ImageFilename;
            ViewData["CreateAt"] = criminal.CreatedAt.ToString("dd/MM/yyyy");


            return View(criminalDTO);
        }
        [HttpPost]
        [Route("Edit/{id}")]
        public async Task<IActionResult> Edit(Guid id, CriminalDTO criminalDTO)
        {
            var criminal = await context.Criminal.FindAsync(id);
            if (criminal == null)
            {
                return RedirectToAction("ViewList", "Criminal");
            }

            // Validate the model state
            if (!ModelState.IsValid)
            {
                ViewData["CriminalId"] = criminal.Id;
                ViewData["ImageFileName"] = criminal.ImageFilename;
                ViewData["CreateAt"] = criminal.CreatedAt.ToString("dd/MM/yyyy");
                return View(criminalDTO);
            }

            string newfilename = criminal.ImageFilename; // Keep the old filename by default
            if (criminalDTO.ImageFile != null)
            {
                newfilename = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(criminalDTO.ImageFile.FileName);
                string ImageFullpath = Path.Combine(environment.WebRootPath, "criminal", newfilename);
                using (var stream = System.IO.File.Create(ImageFullpath))
                {
                    await criminalDTO.ImageFile.CopyToAsync(stream); // Use async method
                }
                // Delete the old image file
                string oldImageFullPath = Path.Combine(environment.WebRootPath, "criminal", criminal.ImageFilename);
                if (System.IO.File.Exists(oldImageFullPath))
                {
                    System.IO.File.Delete(oldImageFullPath);
                }
            }

            // Update the criminal's properties
            criminal.Name = criminalDTO.Name;
            criminal.Alias = criminalDTO.Alias;
            criminal.Age = criminalDTO.Age.GetValueOrDefault();
            criminal.Gender = criminalDTO.Gender;
            criminal.Description = criminalDTO.Description;
            criminal.ImageFilename = newfilename;
            criminal.GangAffiliation = criminalDTO.GangAffiliation;
            criminal.Accomplices = criminalDTO.Accomplices;
            criminal.WeaponUsed = criminalDTO.WeaponUsed;
            criminal.KnownHabits = criminalDTO.KnownHabits;
            criminal.PsychologicalProfile = criminalDTO.PsychologicalProfile;
            criminal.Address = criminalDTO.Address;
            // Check if the criminal's caught status has changed
            if (criminal.Caught != criminalDTO.Caught)
            {
                criminal.Caught = criminalDTO.Caught;
                // Update bounty when criminal is caught
                await BountyService.UpdateCriminalBounty(context, criminal.Id);
            }
            else
            {
                criminal.Caught = criminalDTO.Caught;
            }

            // Save changes to the database
            await context.SaveChangesAsync();

            return RedirectToAction("ViewList", "Criminal");
        }

        [HttpGet]
        [Route("Details/{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var criminal = await context.Criminal
                .FirstOrDefaultAsync(c => c.Id == id);

            if (criminal == null)
            {
                return RedirectToAction("ViewList");
            }

            // Load all bounties for this criminal, ordered by creation date
            var bounties = await context.Bounties
                .Include(b => b.Case)
                .Include(b => b.CreatedBy)
                .Where(b => b.CriminalId == id)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            ViewBag.Bounties = bounties;

            return View(criminal);
        }

        [HttpDelete] // Change this to HttpDelete
        [Route("Delete/{id}")]
        public IActionResult Delete(Guid id)
        {
            var criminal = context.Criminal.Find(id);
            if (criminal == null)
            {
                return NotFound(); // Return a 404 Not Found if the criminal does not exist
            }

            string imageFullPath = Path.Combine(environment.WebRootPath, "criminal", criminal.ImageFilename);

            // Check if the file exists before trying to delete it
            if (System.IO.File.Exists(imageFullPath))
            {
                System.IO.File.Delete(imageFullPath);
            }

            context.Criminal.Remove(criminal);
            context.SaveChanges();

            return NoContent(); // Return a 204 No Content response after successful deletion
        }

    }
}