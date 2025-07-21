
using MarketApi.models;
using System.ComponentModel.DataAnnotations;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.DTOs.Cart;

namespace MarketApi.Service
{
    public interface IServices
    {
        int CheckRole();
        //user
        List<Product> GetAllProduct();
        List<Product> GetByCategory(string category);
        List<Product> Sort(string sortBy, bool isAscending);
        List<Product> Search(string word);
        List<Product> FilterByPrice(int minPrice, int maxPrice);
        Product? GetById(int id);
        bool AddToCart(int id);
        void EmptyCart();
        CheckOutCartDto Cart();
        bool EnterDiscountCode(string Code);
        void Checkout();
        bool RateProduct(int id, [Range(1, 5)] float rate);
        decimal TotalPrice();
        User Signup(AddUserDto user);
        bool Login(string Name, string password);
        User? UpdateUser(int id, UpdateUserDto user);
        bool Logout();



        //admin
        Product AddProduct(AddProductDto product);
        Product? UpdateProduct(int id, UpdateProductDto product);
        bool DeleteProduct(int Id);
        bool SetDiscountCode(string code, decimal discount);
        bool AddToInventory(int id, int quantity, int price);
        bool DeleteUser(int Id);
        bool UpgradeUser(int id);
        bool DowngradeUser(int id);
        List<User> GetAllUsers();
        void InventoryCheck(Product product);
    }
}
