using MarketApi.DTOs.User;
using MarketApi.models;
using MarketApi.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace MarketApi.Controllers
{
    [ApiController]
    [Route("MarketApi/public")]
    public class PublicController : Controller
    {
        private readonly IServices _service;
        public PublicController(IServices service)
        {
            _service = service;
        }

        [HttpGet("aii")]
        [AllowAnonymous]
        public IActionResult seedData()
        {
            var result = _service.seedData();
            return Ok(result);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetProduct([FromQuery] CategoryEnum? category, SortBy? sortBy, bool Accending, string? search, int? minPrice, int? maxPrice, [Range(0, 5)] int? minRate, int? minDiscountPrecent)
        {
            return Ok(await _service.GetProductsAsync(category, sortBy, search, minPrice, maxPrice, minRate, minDiscountPrecent, Accending));
        }


        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _service.findProductAsync(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        [AllowAnonymous]
        [HttpPost("Signup")]
        public async Task<IActionResult> SignupAsync([FromBody] AddUserDto user)
        {
            var newUser = await _service.SignupAsync(user);
            return Ok(newUser);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginUserDto user)
        {
            var token = await _service.LoginAsync(user);
            if (string.IsNullOrEmpty(token)) return Unauthorized("Invalid username or password.");
            return Ok(new { token });
        }

    }
}
