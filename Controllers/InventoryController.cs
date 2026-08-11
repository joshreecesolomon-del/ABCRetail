using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class InventoryController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly FileStorageService _fileStorageService;

        public InventoryController(
            QueueStorageService queueService,
            FileStorageService fileStorageService)
        {
            _queueService = queueService;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Update()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Update(InventoryModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string message =
                $"Inventory update: {model.ProductName} " +
                $"quantity changed to {model.Quantity}";

            await _queueService.SendMessageAsync(
                "inventory-updates",
                message);

            await _fileStorageService.LogAsync(
                $"Inventory updated: {model.ProductName} | New quantity: {model.Quantity}");

            

            ViewBag.Message =
                "Inventory update successfully added to the queue.";

            return View();
        }
    }
}