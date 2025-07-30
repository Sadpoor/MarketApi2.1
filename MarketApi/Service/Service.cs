
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
        private readonly ILogger<Service> _logger;
        public Service(MarketDb context, IConfiguration configuration, UserMapper userMapper, ProductMapper productMapper, DiscoutCodeMapper discoutCodeMapper, ILogger<Service> logger)
        {
            _context = context;
            _configuration = configuration;
            _userMapper = userMapper;
            _productMapper = productMapper;
            _dicountCodeMapper = discoutCodeMapper;
            _logger = logger;
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
            _logger.LogInformation($"Request: add product:{product.Name} to user :{user.PhoneNumber} cart.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: product:{product.Name} added to user :{user.PhoneNumber} cart successfully.");
            }catch(Exception ex) {
                _logger.LogError($"Error adding product: {product.Name} to user: {user.PhoneNumber} cart. message: {ex.Message}");
                return -4; // error in saving changes
            }
    
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
            _logger.LogInformation($"Request: empty cart for user {user.PhoneNumber}.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: cart for user {user.PhoneNumber} emptied successfully.");
            }
            catch(Exception ex) {
                _logger.LogError($"Error emptying cart for user {user.PhoneNumber}: {ex.Message}");
                return -4; // error in saving changes
            }
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

            DiscountCode? discountCode = null;
            _logger.LogInformation($"Request: find discount code {Code} for user {user.PhoneNumber}.");
            try
            {
                discountCode= await _context.Discounts.FirstOrDefaultAsync(d => d.Code == Code);
                //_logger.LogInformation($"Result: discount code {Code} found for user {user.PhoneNumber}.");
            }catch(Exception ex)
            {
                _logger.LogError($"Error finding discount code {Code} for user {user.PhoneNumber}: {ex.Message}");
                //return false; // error in finding discount code
            }
            if (discountCode == null)
            {
                return false; // invalid discount code
            }
            user.UserCart.ApplyedDiscountCode = discountCode;
            _logger.LogInformation($"Request: apply discount code {Code} to user {user.PhoneNumber}.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: discount code {Code} applied to user {user.PhoneNumber} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error applying discount code {Code} for user {user.PhoneNumber}: {ex.Message}");
                //return false; // error in saving changes
            }
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
            _logger.LogInformation($"Request: checkout for user {user.PhoneNumber} with {user.UserCart.Products.Count} products.");

            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: checkout for user {user.PhoneNumber} completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during checkout for user {user.PhoneNumber}: {ex.Message}");
                //return false; // error in saving changes
            }
            user.UserCart.Products.Clear();
            return true;
        }

        public async Task<bool?> RateProductAsync(RateProductDto dto)
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
            var user = await findUserAsync(dto.UserID);
            if (user.Rates.Any(p => p.ProductRateID == dto.ProductID))
            {
                return null; // user already rated this product
            }

            product.Rate.Rates.Add(newRate);
            product.Rate.Number += 1;
            product.Rate.Average = ((product.Rate.Average * (product.Rate.Number - 1)) + dto.Rate) / product.Rate.Number;

            
            user.Rates.Add(newRate);
            _logger.LogInformation($"Request: user {user.PhoneNumber} rates product {product.Name} with {dto.Rate} stars.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: Rate {user.PhoneNumber} for product {product.Name} successfully added.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding rate for product {product.Name} by user {user.PhoneNumber}: {ex.Message}");
                //return false; // error in saving changes
            }
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
            _logger.LogInformation($"Request: sign up for new user: {newUser.PhoneNumber} .");
            try
            {
                await _context.Users.AddAsync(newUser);
                //_logger.LogInformation($"Result: user {newUser.PhoneNumber} added to database.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error adding user {newUser.PhoneNumber}: {ex.Message}");
                //throw; // rethrow the exception to handle it in the calling code
            }

            _logger.LogInformation($"Request: sign up for new user: {newUser.PhoneNumber} .");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: user {newUser.PhoneNumber} signed up successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error signing up user {newUser.PhoneNumber}: {ex.Message}");
                //throw; // rethrow the exception to handle it in the calling code
            }
           
            return newUser;
        }
        public async Task<string?> LoginAsync(LoginUserDto userDto)
        {
            User? user = null;
            _logger.LogInformation($"Request: login user phone: {userDto.PhoneNumber}.");
            try
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == userDto.PhoneNumber && u.Password == userDto.Password);
                //_logger.LogInformation($"Result: user {userDto.PhoneNumber} with this pass found. sent for generat Token.");
            }
            catch
            {
                _logger.LogError($"Error: Founding user {userDto.PhoneNumber} failed");
                //return ...
            }
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
            _logger.LogInformation($"Request: update user {user.PhoneNumber} information.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: user {user.PhoneNumber} information updated successfully.");
            }catch (Exception ex) {
                _logger.LogError($"Error updating user {user.PhoneNumber} information: {ex.Message}");
                //return null; // error in saving changes
            }
            return user;
        }

        // admin methods


        public async Task<Product> AddProductAsync(AddProductDto productDto)
        {
            var product = _productMapper.ToEntity(productDto);
            _logger.LogInformation($"Request: add product {product.Name} with price {product.Price} to inventory.");
            try
            {
                await _context.Products.AddAsync(product);
                //_logger.LogInformation($"Result: product {product.Name} added to inventory successfully.");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Error adding product {product.Name}: {ex.Message}");
                //throw; // rethrow the exception to handle it in the calling code
            }
            
            _logger.LogInformation($"Request: add product {product.Name} with price {product.Price} to inventory.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: product {product.Name} added to inventory successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding product {product.Name}: {ex.Message}");
                //throw; // rethrow the exception to handle it in the calling code
            }
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
            _logger.LogInformation($"Request: update product id: {product.ID}. old/new valu. " +
                $"name:{product.Name}/{productUp.Name} ,price {product.Price}/{productUp.Price}, " +
                $"discription:{product.Description}/{productUp.Description} , Catrgory: {product.Category}/{productUp.Category}" +
                $"DiscountPrecent: {product.DiscountPrecent}/{productUp.DiscountPrecent}.");
            try
            {
                await _context.SaveChangesAsync();
               // _logger.LogInformation($"Result: product {product.ID} updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating product {product.ID}: {ex.Message}");
                ///return
            }
                return product;
        }
        public async Task <bool> DeleteProductAsync(int id)
        {
            Product? product = await findProductAsync(id);
            if (product == null)
            {
                return false;
            }
            _logger.LogInformation($"Request: delete product id:{id} name:{product.Name}.");
            try
            {
                _context.Products.Remove(product);
                //_logger.LogInformation($"Result: product {product.Name} with id:{id} removed from inventory.");
            }
            catch
            {
                _logger.LogError($"Error deleting product {id}: product not found.");
                //return false; // product not found
            }
            
            _logger.LogInformation($"Reauest to delet product name:{product.Name} id:{product.ID} sent ");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"delet product id:{id} successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting product {id}: {ex.Message}");
                return false;
            }
            return true;
        }

        public async Task<bool> SetDiscountCodeAsync(AddDiscountCodeDto Code)
        {
            DiscountCode discountCode = _dicountCodeMapper.ToEntity(Code);
            _logger.LogInformation($"Request: set discountCode. Code:{Code.Code} DiscountPrecent: {Code.DiscountPrecent}.");
            try
            {
                _context.Discounts.Add(discountCode);
                //_logger.LogInformation($"Result: discountCode {Code.Code} added to database.");
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError($"Error adding discount code {Code.Code}: code already exists or invalid data.");
                //return false; // error in adding discount code
            }
            
            _logger.LogInformation($"Request: set dicountCode. Code:{Code.Code} DiscountPrecent: {Code.DiscountPrecent}");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: discountCode: {Code.Code} successfully added");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding discount code {Code.Code}: {ex.Message}");
            }
            return true;
        }
        public async Task<bool> AddToInventoryAsync(AddInventoryDto dto)
        {
            Product? product = await findProductAsync(dto.id);
            if (product == null) return false;
            product.Price = dto.price;
            product.Quantity += dto.quantity;
            _logger.LogInformation($"Request: add to inventory product id:{dto.id} name:{product.Name} with price {dto.price} and quantity {dto.quantity}.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: product {product.Name} with id:{dto.id} added to inventory successfully.");

            }
            catch( DbUpdateException ex) 
            {
                _logger.LogError($"Error adding product {dto.id} to inventory failed.");
            }
            return true;
        }
        public async Task<bool> DeleteUserAsync(int id)
        {
            User? user = await findUserAsync(id);
            if (user == null)
            {
                return false;
            }
            _logger.LogInformation($"Request: delete user id:{id} phoneNumber:{user.PhoneNumber}.");
            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: user {user.PhoneNumber} with id:{id} removed from database successfully.");
            }catch (Exception ex) {
                _logger.LogError($"Error deleting user {user.PhoneNumber}: {ex.Message}");
                //return false; // error in saving changes
            }


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
            _logger.LogInformation($"Request: Upgrad user {user.PhoneNumber} to Admin.");
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: User {user.PhoneNumber} upgraded to Admin successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error upgrading user {user.PhoneNumber}: {ex.Message}");
                return false;
            }
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
            _logger.LogInformation($"Request: Downgrade user {user.PhoneNumber} to User.");
            
            try
            {
                await _context.SaveChangesAsync();
                //_logger.LogInformation($"Result: User {user.PhoneNumber} downgraded to User successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downgrading user {user.PhoneNumber}: {ex.Message}");
                return false;
            }
            return true;
        }
        public async Task<User?> findUserAsync(int userID)
        {
            _logger.LogInformation($"Request: find user id:{userID}");
            User? user = null;
            try
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.ID == userID);
                //_logger.LogInformation($"use {userID} found seccessfully");
            }
            catch
            {
                _logger.LogError($"Error: find user {userID} failed.");
            }
            return user;
        }

        public async Task<Product?> findProductAsync(int productId)
        {
            _logger.LogInformation($"Request: find product id:{productId}");
            Product? product = null;
             { 
                product = await _context.Products.FirstOrDefaultAsync(u => u.ID == productId);
                _logger.LogInformation($"Product {productId} found successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error finding product {productId}: {ex.Message}");
            }
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
