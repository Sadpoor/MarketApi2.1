using MarketApi.Data;
using MarketApi.DTOs.Cart;
using MarketApi.DTOs.DiscountCode;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.Mappers;
using MarketApi.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MarketApi.Service
{
    public class Service : IServices
    {
        private AppDbContext _context;
        private readonly ProductMapper _productMapper;
        private readonly DiscoutCodeMapper _dicountCodeMapper;
        private readonly UserMapper _userMapper;
        private readonly IConfiguration _configuration;
        public Service(AppDbContext context, IConfiguration configuration, UserMapper userMapper, ProductMapper productMapper, DiscoutCodeMapper discoutCodeMapper)
        {
            _context = context;
            _configuration = configuration;
            _userMapper = userMapper;
            _productMapper = productMapper;
            _dicountCodeMapper = discoutCodeMapper;
        }
        //User? currentUser = null;
        //DiscountCode? currentDiscountCode = null;

        public void InventoryCheck(Product product)
        {
            if (product.Inventory != null && product.Inventory[0].Quantity == 0)
            {
                product.Inventory.RemoveAt(0);
            }
            return;
        }
        public async Task<List<Product>> GetProductsAsync(CategoryEnum? category, SortBy? sortBy, string? search, int? minPrice, int? maxPrice, int? minRate, int? minDiscountPrecent, bool Accending = true)
        {
            var filteredProduct = _context.Products.AsQueryable();
            if (category != null) filteredProduct = filteredProduct.Where(p => p.Category == category);
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
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.DiscountPrecent);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.DiscountPrecent);
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
            if (minDiscountPrecent != null) filteredProduct = filteredProduct.Where(p => p.DiscountPrecent >= minDiscountPrecent);
            return await filteredProduct.ToListAsync();

        }
        //currentUser !.Discount
        public async Task<Product?> GetProductByIdAsync(int id) => await _context.Products.FirstOrDefaultAsync(p => p.ID == id);
        public async Task<int> AddProductToCartAsync(int productId, int userId)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(p => p.ID == productId);
            if (product == null) 
            { 
                return -1; // product not found
            }
            InventoryCheck(product);
            if (product.Inventory == null) 
            { 
                return -2; // do not mojood
            }
            var user = await findUserAsync(userId);
            if (user == null)
            {
                return -3; //invalid usern in token . may be token expired
            }
            user.UserCart.Products.Add(product);
            await _context.SaveChangesAsync();
            return 1; //succeed
        }
        public async Task<int> EmptyCartAsync(int userID)
        {
            
            var user = await findUserAsync(userID);
            if (user == null)
            {
                return -3; //invalid usern in token . may be token expired
            }
            user.UserCart.Products.Clear();
            await _context.SaveChangesAsync();
            return 1; //succeed
        }
        public async Task<CheckOutCartDto?> GetCartAsync(int userID)
        {
            var user = await findUserAsync(userID);
            if (user == null)
            {
                return null;
            }
            if (user.UserCart.Products.Count == 0)
            {
                return null;
            }
            CheckOutCartDto cartDto = new()
            {

                ProductsInCart = user.UserCart.Products,
                TotalPrice = TotalPrice(user.UserCart),
                AppliedDiscountCode = user.UserCart.ApplyedDiscountCode
            };
            return cartDto;
        }
        public async Task<bool?> EnterDiscountCodeAsync(string Code,int userId)
        {
            var user = await findUserAsync(userId);
            if (user == null)
            {
                return null; // in valid user in token
            }
            DiscountCode? discountCode = await _context.Discounts.FirstOrDefaultAsync(d => d.Code == Code);
            if (discountCode == null)
            {
                return false; // invalid discount code
            }
            user.UserCart.ApplyedDiscountCode = discountCode;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool?> CheckoutAsync(int userId)
        {
            var user = await findUserAsync(userId);
            if (user == null)
            {
                return null; // invalid user in token
            }
            foreach (var product in user.UserCart.Products)
            {
                product.Inventory[0].Quantity -= 1;
                product.Sales += 1;
            }
            await _context.SaveChangesAsync();
            user.UserCart.Products.Clear();
            return true;
        }

        public async Task<bool> RateProductAsync(int productId, [Range(1, 5)] float rate)
        {
            Product? product = await findProductAsync(productId);
            if (product == null)
            {
                return false;
            }
            product.Rates.Number += 1;
            product.Rates.Average = ((product.Rates.Average * (product.Rates.Number - 1)) + rate) / product.Rates.Number;
            await _context.SaveChangesAsync();
            return true;
        }
        public decimal TotalPrice(Cart userCart)
        {
            decimal totalPrice = 0;
            foreach (var product in userCart.Products)
            {
                totalPrice += (product.Price * (1 - product.DiscountPrecent / 100));
            }
            if (userCart.ApplyedDiscountCode == null)
            {
                return totalPrice; //no discountCode
            }
            totalPrice *= ((1 - userCart.ApplyedDiscountCode.DiscountPrecent) / 100); // with discount
            return totalPrice; 
        }
        public async Task<User> SignupAsync(AddUserDto userDto)
        {
            User newUser = _userMapper.ToEntity(userDto);
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
            return newUser;
        }
        public async Task<string?> LoginAsync(LoginUserDto userDto)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Name == userDto.UserName && u.Password == userDto.Password);
            if (user == null) return null;
            return GenerateJwtToken(user);
        }
        //public bool Logout()   you will logged out after one hour!
        //{
        //    currentUser = null;
        //    return true;
        //} فعلا نداریم 


        public async Task<User?> UpdateUserAsync(UpdateUserDto userUp, int userId)
        {
            var user = await findUserAsync(userId);
            if (user == null)
            {
                return null;
            }
            _userMapper.UpdateEntity(userUp, user);
            await _context.SaveChangesAsync();
            return user;
        }

        // admin methods


        public async Task<Product> AddProductAsync(AddProductDto productDto)
        {
            var product = _productMapper.ToEntity(productDto);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task<Product?> UpdateProductAsync(UpdateProductDto productUp)
        {
            Product? product = await findProductAsync(productUp.Id);
            if (product == null)
            {
                return null;
            }
            _productMapper.UpdateEntity(productUp, product);
            await _context.SaveChangesAsync();
            return product;
        }
        public async Task <bool> DeleteProductAsync(int id)
        {
            Product? product = await findProductAsync(id);
            if (product == null)
            {
                return false;
            }
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetDiscountCodeAsync(AddDiscountCodeDto Code)
        {
            DiscountCode discountCode = _dicountCodeMapper.ToEntity(Code);
            _context.Discounts.Add(discountCode);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> AddToInventoryAsync(AddInventoryDto dto)
        {
            Product? product = await findProductAsync(dto.id);
            if (product == null) return false;
            InventoryClass inventory = new()
            {
                Quantity = dto.quantity,
                Price = dto.price
            };
            product.Inventory.Add(inventory);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            User? user = await findUserAsync(id);
            if (user == null)
            {
                return false;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpgradeUserAsync(int id)
        {
            User? user = await findUserAsync(id);
            if (user == null)
            {
                return false;
            }
            user.Role = RoleEnum.Admin;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> DowngradeUserAsync(int id)
        {
            User? user = await findUserAsync(id);
            if (user == null)
            {
                return false;
            }
            user.Role = RoleEnum.User;
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<User?> findUserAsync(int userID)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ID == userID);
            return user;
        }

        public async Task<Product?> findProductAsync(int productId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(u => u.ID == productId);
            return product;
        }

        public async Task<List<User>> GetAllUsersAsync() => await _context.Users.ToListAsync();

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
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


        
    }

}
