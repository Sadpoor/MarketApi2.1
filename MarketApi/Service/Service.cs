using MarketApi.Data;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.DTOs.Cart;
using MarketApi.models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketApi.Mappers;
using MarketApi.DTOs.DiscountCode;
//using MarketApi.DTOs.Actions;
using System.Linq;
using Microsoft.EntityFrameworkCore.Storage.Json;
namespace MarketApi.Service
{
    public class Service : IServices
    {   
        private AppDbContext _context;
        private readonly ProductMapper _productMapper;
        private readonly DiscoutCodeMapper _dicountCodeMapper;
        private readonly UserMapper _userMapper;
        private readonly IConfiguration _configuration;
        public Service(AppDbContext context, IConfiguration configuration, UserMapper userMapper,ProductMapper productMapper,DiscoutCodeMapper discoutCodeMapper)
        {
            _context = context;
            _configuration = configuration;
            _userMapper = userMapper;
            _productMapper = productMapper;
            _dicountCodeMapper = discoutCodeMapper;
        }
        User? currentUser = null;
        DiscountCode? currentDiscountCode = null;

        public void InventoryCheck(Product product)
        {
            if (product.Inventory != null && product.Inventory[0].Quantity == 0)
            {
                product.Inventory.RemoveAt(0);
            }
            return;
        }
        public List<Product> Products(CategoryEnum? category, SortBy? sortBy, string? search, int? minPrice, int? maxPrice, int? minRate, int? minDiscount, bool Accending=true)
        {
            var filteredProduct = _context.Products.AsQueryable();
            if(category!=null) filteredProduct = filteredProduct.Where(p => p.Category == category);
            if (sortBy != null)
            {
                switch (sortBy)
                {
                    case SortBy.price:
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.Price);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.Price);
                        break;
                    case SortBy.rate:
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.Rates.Average);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.Rates.Average);
                        break;
                    case SortBy.sale:
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.Sales);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.Sales);
                        break;
                    case SortBy.discount:
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.Discount);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.Discount);
                        break;
                    default:
                        break;
                }
            }
            if (search != null) filteredProduct = filteredProduct.Where(p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search)
                );
            if (minPrice != null) filteredProduct = filteredProduct.Where(p => p.Price >= minPrice);
            if (maxPrice != null) filteredProduct = filteredProduct.Where(p => p.Price <= maxPrice);
            if (minRate != null) filteredProduct = filteredProduct.Where(p => p.Rates.Average >= minRate);
            if (minDiscount != null) filteredProduct = filteredProduct.Where(p => p.Discount >= minDiscount);
            return filteredProduct.ToList();

        }
            //currentUser !.Discount
        public Product? GetById(int id) => _context.Products.FirstOrDefault(p => p.ID == id);
        public bool AddToCart(int id)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null) { return false; }
            InventoryCheck(product);
            if (product.Inventory == null) {return false;}
            currentUser!.cart.Add(product);
            _context.SaveChanges();
            return true;
        }
        public void EmptyCart()
        {
            currentUser!.cart.Clear();
            _context.SaveChanges();
            return;
        }
        public CheckOutCartDto Cart()
        {
            CheckOutCartDto cartDto = new()
            {
                ProductsInCart = currentUser!.cart,
                TotalPrice = TotalPrice(),
                AppliedDiscountCode = currentDiscountCode
            };
            return cartDto ;
        }
        public bool EnterDiscountCode(string Code)
        {
            DiscountCode? discountCode = _context.Discounts.FirstOrDefault(d => d.Code == Code);
            if (discountCode == null)
            {
                return false;
            }
            currentDiscountCode = discountCode;
            return true;
        }

        public void Checkout()
        {
            foreach (var product in currentUser!.cart)
            {
                product.Inventory[0].Quantity -= 1;
                product.Sales += 1;
            }
            _context.SaveChanges();
            currentUser!.cart.Clear();
            return;
        }

        public bool RateProduct(int id, [Range(1, 5)] float rate)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null)
            {
                return false;
            }
            product.Rates.Number += 1;
            product.Rates.Average = ((product.Rates.Average * (product.Rates.Number - 1)) + rate) / product.Rates.Number;
            _context.SaveChanges();
            return true;
        }
        public decimal TotalPrice()
        {
            if (currentUser!.cart.Count == 0) return 0; //if cart is empty return 0
            decimal totalPrice = 0;
            foreach (var product in currentUser!.cart)
            {
                totalPrice += (product.Price * (1 - product.Discount));
            }
            if (currentDiscountCode == null) return totalPrice;
            totalPrice *= ((1 - currentDiscountCode.DiscountPrecent) / 100);
            return totalPrice;
        }
        public User Signup(AddUserDto userDto)
        {
            User newUser = _userMapper.ToEntity(userDto);
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return newUser;
        }
        public string? Login(LoginUserDto userDto)
        {
            User? user = _context.Users.FirstOrDefault(u => u.Name == userDto.UserName && u.Password == userDto.Password);
            if (user == null) return null;
            currentUser = user;
            return GenerateJwtToken(user);
        }
        public bool Logout()
        {
            currentUser = null;
            return true;
        }


        public User? UpdateUser(UpdateUserDto userUp)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == userUp.Id);
            if (user == null) return null;
            _userMapper.UpdateEntity(userUp, user);
            _context.SaveChanges();
            return user;
        }

        // admin methods


        public Product AddProduct(AddProductDto productDto)
        {
            var product = _productMapper.ToEntity(productDto);
            _context.Products.Add(product);
            _context.SaveChanges();
            return product;
        }
        public Product? UpdateProduct(UpdateProductDto productUp)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == productUp.Id);
            if (product == null) return null;
            _productMapper.UpdateEntity(productUp,product);
            _context.SaveChanges();
            return product;
        }
        public bool DeleteProduct(int id)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null) return false;
            _context.Products.Remove(product);
            _context.SaveChanges();
            return true;
        }
        
        public bool SetDiscountCode(AddDiscountCodeDto Code)
        {
            DiscountCode discountCode = _dicountCodeMapper.ToEntity(Code);
            _context.Discounts.Add(discountCode);
            _context.SaveChanges();
            return true;
        }
        public bool AddToInventory(AddInventoryDto dto)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == dto.id);
            if (product == null) return false;
            InventoryClass inventory = new()
            {
                Quantity = dto.quantity,
                Price = dto.price
            };
            product.Inventory.Add(inventory);
            _context.SaveChanges();
            return true;
        }
        public bool DeleteUser(int id)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null) return false;
            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
        public bool UpgradeUser(int id)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null) return false;
            user.Role = RoleEnum.Admin;
            _context.SaveChanges();
            return true;
        }
        public bool DowngradeUser(int id)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return false;
            }
            user.Role = RoleEnum.User;
            _context.SaveChanges();
            return true;
        }
        public List<User> GetAllUsers() => _context.Users.ToList();

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"])),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public User? UpdateUser(int id, UpdateUserDto user)
        {
            throw new NotImplementedException();
        }

        public Product? UpdateProduct(int id, UpdateProductDto product)
        {
            throw new NotImplementedException();
        }

        
    }
}