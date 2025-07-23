
using MarketApi.models;
using System.ComponentModel.DataAnnotations;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.DTOs.Cart;
using MarketApi.DTOs.DiscountCode;
//using MarketApi.DTOs.Actions;
using Microsoft.AspNetCore.Mvc;

namespace MarketApi.Service
{
    public interface IServices
    {
        List<Product> Products(CategoryEnum? category, SortBy? sortBy, string? search, int? minPrice, int? maxPrice, int? minRate, int? minDiscount, bool Accending);
        Product? GetById(int id);
        bool AddToCart(int id);
        void EmptyCart();
        CheckOutCartDto Cart();
        bool EnterDiscountCode(string Code);
        void Checkout();
        bool RateProduct(int id, [Range(1, 5)] float rate);
        decimal TotalPrice();
        User Signup(AddUserDto userDto);
        string Login(LoginUserDto userDto);
        User? UpdateUser(UpdateUserDto user);
        bool Logout();



        //admin
        Product AddProduct(AddProductDto product);
        Product? UpdateProduct(UpdateProductDto product);
        bool DeleteProduct(int Id);
        bool SetDiscountCode(AddDiscountCodeDto Code);
        bool AddToInventory(AddInventoryDto dto);
        bool DeleteUser(int Id);
        bool UpgradeUser(int id);
        bool DowngradeUser(int id);
        List<User> GetAllUsers();
        void InventoryCheck(Product product);
    }
}
