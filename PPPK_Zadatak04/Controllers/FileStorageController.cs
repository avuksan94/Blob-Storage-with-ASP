using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PPPK_Zadatak04.Models;
using PPPK_Zadatak04.Repository;

namespace PPPK_Zadatak04.Controllers
{
    public class FileStorageController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var filesByExtension = new Dictionary<string, List<string>>();
            var allFiles = await FileStorageRepository.ListFilesAsync();

            foreach (var fileName in allFiles)
            {
                var segments = fileName.Split('/');
                if (segments.Length > 1)
                {
                    var extension = segments[0];
                    if (!filesByExtension.ContainsKey(extension))
                    {
                        filesByExtension[extension] = new List<string>();
                    }

                    filesByExtension[extension].Add(fileName);
                }
            }

            var model = filesByExtension.Select(kv => new FileVM { Extension = kv.Key, Files = kv.Value }).ToList();
            return View(model);
        }

        public async Task<IActionResult> Upload(FileUploadVM model)
        {
            if (model.File != null && model.File.Length > 0)
            {
                await FileStorageRepository.UploadFileAsync(model.File);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Download(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("A valid file name is required.");
            }

            try
            {
                var stream = await FileStorageRepository.DownloadFileAsync(fileName);
                var contentType = "APPLICATION/octet-stream";
                return File(stream, contentType, Path.GetFileName(fileName));
            }
            catch
            {
                return NotFound($"File: {fileName} not found.");
            }
        }

        public async Task<IActionResult> Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("A valid file name is required.");
            }

            try
            {
                await FileStorageRepository.DeleteFileAsync(fileName);
                return RedirectToAction("Index");
            }
            catch
            {
                return NotFound($"File: {fileName} not found.");
            }
        }

        public async Task<IActionResult> Display(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest("A valid file name is required.");
            }

            var fileUrl = await FileStorageRepository.DisplayFileAsync(fileName);
            if (fileUrl != null)
            {
                var fileExtension = Path.GetExtension(fileName).ToLower();
                var viewModel = new FileDisplayVM
                {
                    FileUrl = fileUrl,
                    FileExtension = fileExtension
                };
                return View("Display", viewModel);
            }

            return NotFound($"File: {fileName} not found.");
        }


    }
}
