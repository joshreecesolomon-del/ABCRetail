using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using ABCRetail.Entities;
using ABCRetail.Models;
using ABCRetail.Services;

namespace ABCRetail.Controllers
{
    public class CustomerController : Controller
    {
        private readonly TableClient _table;

        public CustomerController(TableStorageService storageService)
        {
            _table = storageService.GetTable("Customers");
            _table.CreateIfNotExists();
        }

        public IActionResult Index()
        {
            var customers = _table.Query<CustomerEntity>().ToList();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CustomerModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var customer = new CustomerEntity
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                RegistrationDate = DateTime.UtcNow
            };

            _table.AddEntity(customer);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(string partitionKey, string rowKey)
        {
            _table.DeleteEntity(partitionKey, rowKey);

            return RedirectToAction(nameof(Index));
        }
    }
}