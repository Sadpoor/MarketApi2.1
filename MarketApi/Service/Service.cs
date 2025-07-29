
using MarketApi.Data.MarketDb;
using MarketApi.DTOs.Actions;
using MarketApi.DTOs.Cart;
using MarketApi.DTOs.DiscountCode;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.Mappers;
using MarketApi.Models.DiscountCode;
using MarketApi.Models.Products;
using MarketApi.Models.Rating;
using MarketApi.Models.Users;
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
        private MarketDb _context;
        private readonly ProductMapper _productMapper;
        private readonly DiscoutCodeMapper _dicountCodeMapper;
        private readonly UserMapper _userMapper;
        private readonly IConfiguration _configuration;
        public Service(MarketDb context, IConfiguration configuration, UserMapper userMapper, ProductMapper productMapper, DiscoutCodeMapper discoutCodeMapper)
        {
            _context = context;
            _configuration = configuration;
            _userMapper = userMapper;
            _productMapper = productMapper;
            _dicountCodeMapper = discoutCodeMapper;
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
                        if (Accending) filteredProduct = filteredProduct.OrderBy(p => p.Rate.Average);
                        else filteredProduct = filteredProduct.OrderByDescending(p => p.Rate.Average);
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
            if (minRate != null) filteredProduct = filteredProduct.Where(p => p.Rate.Average >= minRate);
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
            var inventory= product.Quantity;
            if (inventory == 0) 
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
                product.Quantity -= 1;
                product.Sales += 1;
            }
            await _context.SaveChangesAsync();
            user.UserCart.Products.Clear();
            return true;
        }

        public async Task<bool> RateProductAsync(RateProductDto dto)
        {
            Product? product = await findProductAsync(dto.ProductID);
            if (product == null)
            {
                return false;
            }
            CustomerRate newRate= new(){
                Rate = dto.Rate,
                Discription = dto.Discription 
            };

            product.Rate.Rates.Add(newRate);
            product.Rate.Number += 1;
            product.Rate.Average = ((product.Rate.Average * (product.Rate.Number - 1)) + dto.Rate) / product.Rate.Number;

            var user = await findUserAsync(dto.UserID);
            user.Rates.Add(newRate);

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
            totalPrice *= (1 - ((userCart.ApplyedDiscountCode.DiscountPrecent) / 100)); // with discount
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
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userDto.PhoneNumber && u.Password == userDto.Password);
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
            product.Price = dto.price;
            product.Quantity = dto.quantity;
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
