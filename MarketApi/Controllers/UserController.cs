using MarketApi.DTOs.User;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

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
        [HttpPost("AddToCart/{id}")]
        public IActionResult AddToCart(int id)
        {
            var product = _service.GetById(id);
            if (product == null) return NotFound();
            var result = _service.AddToCart(id);
            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpPost("EmptyCart")]
        public IActionResult EmptyCart()
        {
            _service.EmptyCart();
            return Ok("Cart emptied successfully.");
        }

        [Authorize(Roles = "User")]
        [HttpGet("Cart")]
        public IActionResult Cart()
        {
            var cartItems = _service.Cart();
            return Ok(cartItems);
        }

        [Authorize(Roles = "User")]
        [HttpPost("EnterDiscountCode")]
        public IActionResult EnterDiscountCode([FromBody] string code)
        {
            bool isValid = _service.EnterDiscountCode(code);
            if (!isValid) return BadRequest("Invalid discount code.");
            return Ok("Discount code applied successfully.");
        }

        [Authorize(Roles = "User")]
        [HttpGet("Checkout")]
        public IActionResult Checkout()
        {
            _service.Checkout();
            return Ok();
        }

        [Authorize(Roles = "User")]
        [HttpPost("RateProduct")]
        public IActionResult RateProduct(int id, float rate)
        {
            var message = _service.RateProduct(id, rate);
            return Ok(message);
        }

        //[Authorize(Roles = "User")]
        //[HttpPost("TotalPrice")]
        //public IActionResult ()
        //{
        //    var totalPrice = _service.TotalPrice();
        //    return Ok(totalPrice);
        //}

  
        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            _service.Logout();
            return Ok();
        }
    }
}
