using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.DTOs.Cart;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using System.ComponentModel.DataAnnotations;
using MarketApi.DTOs.DiscountCode;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("MarketApi")]
    public class HomeController : ControllerBase
    {
        private readonly IServices _service;
        public HomeController(IServices service)
        {
            _service = service;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProduct([FromQuery] CategoryEnum? category, SortBy? sortBy, bool Accending, string? search, int? minPrice, int? maxPrice, [Range(0, 5)] int? minRate, int? minDiscount)
        {
            return Ok(_service.Products(category, sortBy, search, minPrice, maxPrice, minRate, minDiscount,Accending));  
        }

        
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var product = _service.GetById(id);
            if (product == null) return NotFound();
            return Ok(product);
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
        
        [Authorize(Roles = "User")]
        [HttpPost("TotalPrice")]
        public IActionResult TotalPrice()
        { 
            var totalPrice = _service.TotalPrice();
            return Ok(totalPrice);
        }
        
        [AllowAnonymous]
        [HttpPatch("Signup")]
        public IActionResult Signup([FromBody] AddUserDto user)
        {
            var newUser = _service.Signup(user);
            return Ok(newUser);
        }
        
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginUserDto user)
        {
            var token = _service.Login(user);
            if (string.IsNullOrEmpty(token)) return Unauthorized("Invalid username or password.");
            return Ok(new { token });
        }

        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            _service.Logout();
            return Ok();
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
