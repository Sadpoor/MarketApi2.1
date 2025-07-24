using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using System.ComponentModel.DataAnnotations;
using MarketApi.DTOs.DiscountCode;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("MarketApi/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IServices _service;
        public AdminController(IServices service)
        {
            _service = service;
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateUser")]
        public IActionResult UpdateUser([FromBody] UpdateUserDto user)
        { 
            var updatedUser = _service.UpdateUser(user);
            return Ok(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddProduct")]
        public IActionResult AddProduct([FromBody] AddProductDto product)
        {

            var newProduct = _service.AddProduct(product);
            return Ok(newProduct);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateProduct")]
        public IActionResult UpdateProduct([FromBody] UpdateProductDto product)
        {
            var updatedProduct = _service.UpdateProduct(product);
            return Ok(updatedProduct);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteProduct")]
        public IActionResult DeleteProduct(int id)
        {
            var deletedProduct = _service.DeleteProduct(id);
            return Ok(deletedProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("SetDiscountCode")]
        public IActionResult SetDiscountCode([FromBody] AddDiscountCodeDto Code)
        { 
            return Ok(_service.SetDiscountCode(Code));
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("AddToInventory")]
        public IActionResult AddToInventory(AddInventoryDto Dto)
        {
            return Ok(_service.AddToInventory(Dto));
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser(int id)
        {
            var deletedUser = _service.DeleteUser(id);
            return Ok(deletedUser);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("UpgradeUser")]
        public IActionResult UpgradeUser(int id)
        {
            var upgradedUser = _service.UpgradeUser(id);
            return Ok(upgradedUser);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPost("DowngradeUser")]
        public IActionResult DowngradeUser(int id)
        {
            var downgradedUser = _service.DowngradeUser(id);
            return Ok(downgradedUser);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            var allUsers = _service.GetAllUsers();
            return Ok(allUsers);
        }
        
        [Authorize(Roles = "Admin")]
        [HttpPatch("InventoryCheck")]
        public IActionResult InventoryCheck([FromBody] Product product)
        {
            _service.InventoryCheck(product);
            return Ok();
        }
    }
}
