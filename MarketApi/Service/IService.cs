
using MarketApi.DTOs.Cart;
using MarketApi.DTOs.DiscountCode;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.Models.Users;
using MarketApi.Models.Products;
using System.ComponentModel.DataAnnotations;
using MarketApi.DTOs.Actions;


namespace MarketApi.Service
{
    public interface IServices
    {

        Task<List<Product>> GetProductsAsync(CategoryEnum? category, SortBy? sortBy, string? search, int? minPrice, int? maxPrice, int? minRate, int? minDiscountPrecent, bool Accending);
        Task<Product?> findProductAsync(int id);
        Task<int> AddProductToCartAsync(int productId, int userId);
        Task<int> EmptyCartAsync(int userID);
        Task<CheckOutCartDto?> GetCartAsync(int userID);
        Task<bool?> EnterDiscountCodeAsync(string Code, int userId);
        Task<bool?> CheckoutAsync(int userId);
        Task<bool?> RateProductAsync(RateProductDto dto);
        decimal TotalPrice(Cart userCart);
        Task<User> SignupAsync(AddUserDto userDto);
        Task<string?> LoginAsync(LoginUserDto userDto);
        Task<User?> UpdateUserAsync(UpdateUserDto user, int userId);
        //bool Logout();  فعلا نداریم



        //admin
        Task<User?> findUserAsync(int userID);
        Task<Product> AddProductAsync(AddProductDto product);
        Task<Product?> UpdateProductAsync(UpdateProductDto product);
        Task<bool> DeleteProductAsync(int Id);
        Task<bool> SetDiscountCodeAsync(AddDiscountCodeDto Code);
        Task<bool> AddToInventoryAsync(AddInventoryDto dto);
        Task<bool> DeleteUserAsync(int Id);
        Task<bool> UpgradeUserAsync(int id);
        Task<bool> DowngradeUserAsync(int id);
        Task<List<User>> GetAllUsersAsync();

    }
}
