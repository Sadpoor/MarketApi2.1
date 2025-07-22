using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

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
        public IActionResult Get()
        {
            var products = _service.GetAllProduct();
            return Ok(products);
        }

        [HttpPost("GetByCategory")]
        public IActionResult Post([FromBody] string category)
        {
            var product = _service.GetByCategory(category);
            return Ok(product);
        }
        [HttpPost("Sort")]
        public IActionResult Sort([FromBody] string sortBy, bool isAscending)
        {
            var sortedProducts = _service.Sort(sortBy, isAscending);
            return Ok(sortedProducts);
        }
        [HttpPost("Search")]
        public IActionResult Search([FromBody] string word)
        {
            var searchedProducts = _service.Search(word);
            return Ok(searchedProducts);
        }
        [HttpPost("FilterByPrice")]
        public IActionResult FilterByPrice([FromBody] int minPrice, int maxPrice)
        {
            var filteredProducts = _service.FilterByPrice(minPrice, maxPrice);
            return Ok(filteredProducts);
        }
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var product = _service.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
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
            int role = _service.CheckRole();
            if (role == -1)
            {
                return Unauthorized("You must be logged in to enter a discount code.");
            }
            else if (role == 1)
            {
                return Forbid("Admins cannot enter discount codes.");
            }
            bool isValid = _service.EnterDiscountCode(code);
            if (!isValid)
            {
                return BadRequest("Invalid discount code.");
            }
            return Ok("Discount code applied successfully.");
        }
        [Authorize(Roles = "User")]
        [HttpGet("Checkout")]
        public IActionResult Checkout()
        {
            int role = _service.CheckRole();
            if (role == -1) return Unauthorized("You must be logged in first");
            else if (role == 1) return Forbid("Admins cannot...");
            _service.Checkout();
            return Ok();
        }
        [Authorize(Roles = "User")]
        [HttpPost("RateProduct")]
        public IActionResult RateProduct(int id, float rate)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("You must be logged in to enter a discount code.");
            else if (role == 1) return Forbid("...");
            var message = _service.RateProduct(id, rate);
            return Ok(message);
        }
        [Authorize(Roles = "User")]
        [HttpPost("TotalPrice")]
        public IActionResult TotalPrice()
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 1) return Forbid("ADMIN");
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
        public IActionResult Login([FromBody] string name, string password)
        {
            var token = _service.Login(name, password);
            if (token)
                return Unauthorized("Invalid username or password.");
            return Ok(new { token });
        }

        [Authorize]
        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 1) return Forbid("Admin");
            _service.Logout();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateUser")]
        public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto user)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("User cannot update user");
            var updatedUser = _service.UpdateUser(id, user);
            return Ok(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("AddProduct")]
        public IActionResult AddProduct([FromBody] AddProductDto product)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var newProduct = _service.AddProduct(product);
            return Ok(newProduct);
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("UpdateProduct")]
        public IActionResult UpdateProduct(int id, [FromBody] UpdateProductDto product)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var updatedProduct = _service.UpdateProduct(id, product);
            return Ok(updatedProduct);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteProduct")]
        public IActionResult DeleteProduct(int id)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var deletedProduct = _service.DeleteProduct(id);
            return Ok(deletedProduct);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("SetDiscountCode")]
        public IActionResult SetDiscountCode([FromBody] string code, decimal discount)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var setDiscountCode = _service.SetDiscountCode(code, discount);
            return Ok(setDiscountCode);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("AddToInventory")]
        public IActionResult AddToInventory(int id, int quantity, int price)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var addedToInventory = _service.AddToInventory(id, quantity, price);
            return Ok(addedToInventory);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser(int id)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var deletedUser = _service.DeleteUser(id);
            return Ok(deletedUser);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("UpgradeUser")]
        public IActionResult UpgradeUser(int id)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var upgradedUser = _service.UpgradeUser(id);
            return Ok(upgradedUser);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("DowngradeUser")]
        public IActionResult DowngradeUser(int id)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var downgradedUser = _service.DowngradeUser(id);
            return Ok(downgradedUser);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            var allUsers = _service.GetAllUsers();
            return Ok(allUsers);
        }
        [Authorize(Roles = "Admin")]
        [HttpPatch("InventoryCheck")]
        public IActionResult InventoryCheck([FromBody] Product product)
        {
            int role = _service.CheckRole();
            if (role == -1) return BadRequest("NO LOGGED IN");
            else if (role == 0) return Forbid("you do not have access");
            _service.InventoryCheck(product);
            return Ok();
        }
    }
}
