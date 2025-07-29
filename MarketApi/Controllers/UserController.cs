using MarketApi.DTOs.Actions;
using MarketApi.DTOs.User;
using MarketApi.Models.DiscountCode;
using MarketApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("MarketApi/User")]
    public class UserController : Controller
    {
        private readonly IServices _service;
        public UserController(IServices service)
        {
            _service = service;
        }

        [Authorize(Roles = "User")]
        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddProductToCartAsync(int productId)
        {
            var product = await _service.findProductAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }
            var userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized("Cannot decode token and find ID");
            }
            var result = await _service.AddProductToCartAsync(productId, userId);
            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpPost("EmptyCart")]
        public async Task<IActionResult> EmptyCartAsync()
        {
            var userId = GetUserIdFromToken();
            if (userId == -1)
            {
                return Unauthorized("Cannot decode token and find ID");
            }
            await _service.EmptyCartAsync(userId);
            return Ok("Cart emptied successfully.");
        }

        [Authorize(Roles = "User")]
        [HttpGet("Cart")]
        public async Task<IActionResult> GetCartAsync()
        {
            var userId = GetUserIdFromToken();
            var cartItems = await _service.GetCartAsync(userId);
            return Ok(cartItems);
        }

        [Authorize(Roles = "User")]
        [HttpPost("EnterDiscountCode")]
        public async Task<IActionResult> EnterDiscountCodeAsync([FromBody] string code)
        {
            var userId = GetUserIdFromToken();
            bool? isValid = await _service.EnterDiscountCodeAsync(code,userId);
            if(isValid == null) return BadRequest("invalid user in token");
            if (isValid==false) return BadRequest("Invalid discount code.");
            return Ok("Discount code applied successfully.");
        }

        [Authorize(Roles = "User")]
        [HttpGet("Checkout")]
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserIdFromToken();
            await _service.CheckoutAsync(userId);
            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpPost("RateProduct")]
        public async Task<IActionResult> RateProduct([FromBody] RateProductDto dto)
        {
            var message = await _service.RateProductAsync(dto);
            if (!message)
            {
                return BadRequest();
            }
            return Ok(message);//باید اضافه شود که هرکس فقط بتواند یکبار نظر دهد
        }

        public int GetUserIdFromToken()
        {
            var userIdToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool success = int.TryParse(userIdToken, out int userId);
            if (!success)
            {
                return -1;
            }
            return userId;
        }
        [Authorize(Roles = "User")]
        [HttpPatch("UpdateUser")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto userDto)
        {
            var userId = GetUserIdFromToken();
            var updatedUser = await _service.UpdateUserAsync(userDto,userId);
            if (updatedUser == null)
            {
                return BadRequest("Failed to update user.");
            }
            return Ok(updatedUser);
        }
    }
}
