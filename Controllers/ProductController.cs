using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using ABCRetail.Entities;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableClient _table;
        private readonly BlobStorageService _blobService;
        private readonly QueueStorageService _queueService;
        private readonly FileStorageService _fileStorageService;

        public ProductController(
            TableStorageService storageService,
            BlobStorageService blobService,
            QueueStorageService queueService,
            FileStorageService fileStorageService)

        {
            _table = storageService.GetTable("Products");
            _table.CreateIfNotExists();

            _blobService = blobService;
            _queueService = queueService;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            var products = _table.Query<ProductEntity>().ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"{error.Key}");

                    foreach (var e in error.Value.Errors)
                    {
                        Console.WriteLine(e.ErrorMessage);
                    }
                }

                return View(model);
            }

            string imageUrl = "";

            if (model.ImageFile != null)
            {
                imageUrl = await _blobService.UploadFileAsync(model.ImageFile);

                await _queueService.SendMessageAsync(
                    "image-processing",
                    $"Uploading image: {model.ImageFile.FileName}");

                await _fileStorageService.LogAsync(
                    $"Product image uploaded: {model.ImageFile.FileName}");
            }

            var product = new ProductEntity
            {
                ProductName = model.ProductName,
                Description = model.Description,
                Price = model.Price,
                Category = model.Category,
                Quantity = model.Quantity,
                ImageUrl = imageUrl
            };

            await _table.AddEntityAsync(product);

            await _fileStorageService.LogAsync(
                $"Product created: {product.ProductName} | Quantity: {product.Quantity} | Price: {product.Price}");

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await _table.DeleteEntityAsync(partitionKey, rowKey);

            return RedirectToAction(nameof(Index));
        }
    }
}