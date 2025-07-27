using MarketApi.DTOs.DiscountCode;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        [HttpPost("AddProduct")]
        public async Task<IActionResult> AddProductAsync([FromBody] AddProductDto product)
        {

            var newProduct = await _service.AddProductAsync(product);
            if (newProduct == null)
            {
                return BadRequest("Failed to add product.");
            }
            return Ok(newProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateProduct")]
        public async Task<IActionResult> UpdateProductAsync([FromBody] UpdateProductDto product)
        {
            var updatedProduct = await _service.UpdateProductAsync(product);
            if (updatedProduct == null)
            {
                return BadRequest("Failed to update product.");
            }
            return Ok(updatedProduct);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deletedProduct = await _service.DeleteProductAsync(id);
            if (!deletedProduct)
            {
                return BadRequest("Failed to delete product.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("SetDiscountCode")]
        public async Task<IActionResult> SetDiscountCode([FromBody] AddDiscountCodeDto Code)
        {
            var result = await _service.SetDiscountCodeAsync(Code);
            if (!result)
            {
                return BadRequest("Failed to set discount code.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AddToInventory")]
        public async Task<IActionResult> AddToInventory(AddInventoryDto Dto)
        {
            var result = await _service.AddToInventoryAsync(Dto);
            if (!result)
            {
                return BadRequest("Failed to add to inventory.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var deletedUser = await _service.DeleteUserAsync(id);
            if (!deletedUser)
            {
                return BadRequest("Failed to delete user.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UpgradeUser")]
        public async Task<IActionResult> UpgradeUser(int id)
        {
            var upgradedUser = await _service.UpgradeUserAsync(id);
            if (!upgradedUser)
            {
                return BadRequest("Failed to upgrade user.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DowngradeUser")]
        public async Task<IActionResult> DowngradeUserAsync(int id)
        {
            var downgradedUser = await _service.DowngradeUserAsync(id);
            if (!downgradedUser)
            {
                return BadRequest("Failed to downgrade user.");
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var Users = await _service.GetAllUsersAsync();
            if (Users == null)
            {
                return NotFound("No users found.");
            }  
            return Ok(Users);
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
