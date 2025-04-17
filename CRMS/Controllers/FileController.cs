using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CRMS.Models;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using CRMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CRMS.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<FileController> _logger;

        public FileController(
            AppDbContext context, 
            UserManager<Users> userManager, 
            IWebHostEnvironment webHostEnvironment,
            ILogger<FileController> logger = null)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Get all files with their uploader information
            var files = _context.Files
                .Include(f => f.UploadedByUser) // Include the user navigation property
                .Select(f => new FileViewModel
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileSize = f.FileSize,
                    UploadDate = f.UploadDate,
                    Description = f.Description,
                    UploadedBy = f.UploadedByUser != null ? f.UploadedByUser.FullName : "Unknown User",
                    CanDownload = f.UploadedByUserId == User.FindFirstValue(ClaimTypes.NameIdentifier) ||
                                _context.FilePermissions.Any(fp => fp.FileId == f.Id && 
                                                                 fp.RequestedByUserId == User.FindFirstValue(ClaimTypes.NameIdentifier) && 
                                                                 fp.IsApproved)
                })
                .ToList();
            return View(files);
        }

        public IActionResult MyFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var files = _context.Files
                .Where(f => f.UploadedByUserId == userId)
                .Select(f => new FileViewModel
                {
                    Id = f.Id,
                    FileName = f.FileName,
                    FileType = f.FileType,
                    FileSize = f.FileSize,
                    UploadDate = f.UploadDate,
                    Description = f.Description,
                    UploadedBy = _context.Users.FirstOrDefault(u => u.Id == f.UploadedByUserId).FullName,
                    CanDownload = true // User can always download their own files
                })
                .ToList();
            return View(files);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, string description)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                
                // Ensure the uploads directory exists
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger?.LogInformation($"Created uploads directory at {uploadsFolder}");
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger?.LogInformation($"File saved to {filePath}");

                var fileModel = new FileModel
                {
                    FileName = file.FileName,
                    FilePath = uniqueFileName, // Store only the filename, not the full path
                    FileType = file.ContentType,
                    FileSize = file.Length,
                    UploadedByUserId = userId,
                    UploadDate = DateTime.Now,
                    Description = description
                };

                _context.Files.Add(fileModel);
                await _context.SaveChangesAsync();

                _logger?.LogInformation($"File record created in database for {file.FileName}");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error uploading file");
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        [HttpGet]
        public IActionResult RequestAccess(int fileId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existingRequest = _context.FilePermissions
                .FirstOrDefault(fp => fp.FileId == fileId && fp.RequestedByUserId == userId);

            if (existingRequest != null)
            {
                TempData["Message"] = "You have already requested access to this file.";
                return RedirectToAction(nameof(Index));
            }

            var permission = new FilePermission
            {
                FileId = fileId,
                RequestedByUserId = userId,
                RequestDate = DateTime.Now,
                IsApproved = false
            };

            _context.FilePermissions.Add(permission);
            _context.SaveChanges();

            TempData["Message"] = "Access request has been sent to the file owner.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult ManageRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Eager load the related entities and handle null cases
            var requests = _context.FilePermissions
                .Include(fp => fp.File)
                .Where(fp => fp.File != null && 
                            fp.File.UploadedByUserId == userId && 
                            !fp.IsApproved)
                .Select(fp => new FileRequestViewModel
                {
                    Id = fp.Id,
                    FileId = fp.FileId,
                    FileName = fp.File != null ? fp.File.FileName : "Unknown File",
                    RequestedBy = _context.Users
                        .Where(u => u.Id == fp.RequestedByUserId)
                        .Select(u => u.FullName)
                        .FirstOrDefault() ?? "Unknown User",
                    RequestDate = fp.RequestDate,
                    Description = fp.File != null ? fp.File.Description : string.Empty
                })
                .ToList();

            return View(requests);
        }

        [HttpPost]
        public IActionResult ApproveRequest(int permissionId)
        {
            var permission = _context.FilePermissions
                .Include(fp => fp.File)
                .FirstOrDefault(fp => fp.Id == permissionId && 
                                    fp.File != null && 
                                    fp.File.UploadedByUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (permission != null)
            {
                permission.IsApproved = true;
                permission.ApprovalDate = DateTime.Now;
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Access request approved successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Request not found or you don't have permission to approve it.";
            }

            return RedirectToAction(nameof(ManageRequests));
        }

        [HttpPost]
        public IActionResult RejectRequest(int permissionId)
        {
            var permission = _context.FilePermissions
                .Include(fp => fp.File)
                .FirstOrDefault(fp => fp.Id == permissionId && 
                                    fp.File != null && 
                                    fp.File.UploadedByUserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (permission != null)
            {
                _context.FilePermissions.Remove(permission);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Access request rejected successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Request not found or you don't have permission to reject it.";
            }

            return RedirectToAction(nameof(ManageRequests));
        }

        [HttpGet]
        public IActionResult ViewFile(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var file = _context.Files.Find(id);

                if (file == null)
                {
                    _logger?.LogWarning($"File with ID {id} not found");
                    return NotFound();
                }

                // Get the physical path to the file
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads");
                var fileName = Path.GetFileName(file.FilePath);
                var physicalPath = Path.Combine(uploadsFolder, fileName);

                _logger?.LogInformation($"Looking for file at path: {physicalPath}");

                if (!System.IO.File.Exists(physicalPath))
                {
                    _logger?.LogWarning($"File not found at path: {physicalPath}");
                    return NotFound($"File not found at path: {physicalPath}");
                }

                // Check permissions
                if (file.UploadedByUserId == userId)
                {
                    _logger?.LogInformation($"User {userId} downloading their own file {file.FileName}");
                    return PhysicalFile(physicalPath, file.FileType, file.FileName);
                }

                var hasPermission = _context.FilePermissions
                    .Any(fp => fp.FileId == id && fp.RequestedByUserId == userId && fp.IsApproved);

                if (!hasPermission)
                {
                    _logger?.LogWarning($"User {userId} attempted to download file {file.FileName} without permission");
                    return Forbid();
                }

                _logger?.LogInformation($"User {userId} downloading file {file.FileName} with permission");
                return PhysicalFile(physicalPath, file.FileType, file.FileName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error downloading file with ID {id}");
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }

    public class FileViewModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public DateTime UploadDate { get; set; }
        public string Description { get; set; }
        public string UploadedBy { get; set; }
        public bool CanDownload { get; set; }
    }

    public class FileRequestViewModel
    {
        public int Id { get; set; }
        public int FileId { get; set; }
        public string FileName { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestDate { get; set; }
        public string Description { get; set; }
    }
} 