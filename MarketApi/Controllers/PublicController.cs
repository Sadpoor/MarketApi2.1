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
        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetProduct([FromQuery] CategoryEnum? category, SortBy? sortBy, bool Accending, string? search, int? minPrice, int? maxPrice, [Range(0, 5)] int? minRate, int? minDiscountPrecent)
        {
            return Ok(_service.Products(category, sortBy, search, minPrice, maxPrice, minRate, minDiscountPrecent, Accending));
        }


        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            var product = _service.GetById(id);
            if (product == null) return NotFound();
            return Ok(product);
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

    }
}
