using Microsoft.AspNetCore.Mvc;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class OrderController : Controller
    {
        private readonly QueueStorageService _queueService;
        private readonly FileStorageService _fileStorageService;

        public OrderController(
            QueueStorageService queueService,
            FileStorageService fileStorageService)
                {
                    _queueService = queueService;
                    _fileStorageService = fileStorageService;
                }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string message =
                $"Processing order: {model.OrderId} | " +
                $"Customer: {model.CustomerName} | " +
                $"Product: {model.ProductName} | " +
                $"Quantity: {model.Quantity}";

            await _queueService.SendMessageAsync(
                "order-processing",
                message);

            await _fileStorageService.LogAsync(
                $"Order submitted: {model.OrderId} | Customer: {model.CustomerName} | Product: {model.ProductName} | Quantity: {model.Quantity}");

            ViewBag.Message = "Order successfully added to the processing queue.";

            return View();
        }
    }
}