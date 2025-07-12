using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.models;
using System.ComponentModel.DataAnnotations;
namespace MarketApi.Service
{
    public class Service : IServices
    {   //first initialize the lists

        public int ali = 2;
        private static List<Product> _products = new(){
        new Product{Id = 1,Name = "Product1",Price = 100,Category = CategoryEnum.Mobile,Sales = 0,Rates = new RateClass{Number = 0,Average = 0},Discount = 0,},
    }; //make product list with seed data
        private static List<User> _users = new(){
        new User{Id = 1,Name = "Morteza",Email = "Morteza.Sadpoo@gmail.com",Password = "123456",phoneNumber = "09203695741",Role = RoleEnum.Admin},
        new User{Id = 2,Name = "Hasan",Email = "Ali.Aliyev@gmail.com",Password = "123456",phoneNumber = "09203695742",Role = RoleEnum.User}
    }; //make user list with seed data
        User? currentUser = new();
        private static List<DiscountCode> _discountCodes = new(){
        new DiscountCode{Code = "123",DiscountPrecent = 10}
    };
        DiscountCode? currentDiscountCode = null;

        public void InventoryCheck(Product product)
        {
            if (product.Inventory != null && product.Inventory[0].Quantity == 0)
            {
                product.Inventory.RemoveAt(0);
            }
            return;
        }


        //now we can start the methods
        public int CheckRole()
        {
            if (currentUser == null)
            {
                return -1; // not logged in
            }
            switch (currentUser.Role)
            {
                case RoleEnum.Admin:
                    return 1; // admin
                case RoleEnum.User:
                    return 0; // user   
            }
            return -1; // unknown role
        }
        public List<Product> GetAllProduct() => _products;
        public List<Product> GetByCategory(string category) => _products.Where(p => p.Category.ToString() == category).ToList();
        public List<Product> Sort(string sortBy, bool isAscending)
        {
            switch (sortBy)
            {
                case "Price":
                    return isAscending ? _products.OrderBy(p => p.Price).ToList() : _products.OrderByDescending(p => p.Price).ToList();
                case "Rate":
                    return isAscending ? _products.OrderBy(p => p.Rates.Average).ToList() : _products.OrderByDescending(p => p.Rates.Average).ToList();
                case "Sales":
                    return isAscending ? _products.OrderBy(p => p.Sales).ToList() : _products.OrderByDescending(p => p.Sales).ToList();
                default:
                    return _products;
            }
        }
        public List<Product> Search(string word) => _products.Where(p => p.Name.Contains(word)).ToList();
        public List<Product> FilterByPrice(int minPrice, int maxPrice) => _products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
        public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);
        public bool AddToCart(int id)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return false;
            }
            InventoryCheck(product);
            if (product.Inventory == null || product.Inventory[0].Quantity == 0)
            {
                return false;
            }
            currentUser!.cart.Add(product);
            return true;
        }
        public void EmptyCart()
        {
            currentUser!.cart= new List<Product>();
        }
        public List<Product> Cart() => currentUser!.cart;
        public bool EnterDiscountCode(string Code)
        {
            DiscountCode? discountCode = _discountCodes.FirstOrDefault(d => d.Code == Code);
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
            currentUser!.cart.Clear();
            return;
        }

        public bool RateProduct(int id, [Range(1, 5)] float rate)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return false;
            }
            product.Rates.Number += 1;
            product.Rates.Average = ((product.Rates.Average * (product.Rates.Number - 1)) + rate) / product.Rates.Number;
            return true;
        }
        public decimal TotalPrice()
        {
            if (currentUser!.cart.Count == 0) return 0; //if cart is empty return 0
            decimal totalPrice = 0;
            foreach (var product in currentUser!.cart)
            {
                totalPrice += ((decimal)product.Price) * (1 - product.Discount);
            }
            if (currentDiscountCode == null) return totalPrice;
            totalPrice *= ((1 - currentDiscountCode.DiscountPrecent) / 100);
            return totalPrice;
        }
        public User Signup(AddUserDto user)
        {
            User newUser = new()
            {
                Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Password = user.Password,
                phoneNumber = user.PhoneNumber ?? string.Empty
            };
            _users.Add(newUser);
            return newUser;
        }
        public bool Login(string Name, string password)
        {
            User? user = _users.FirstOrDefault(u => u.Name == Name && u.Password == password);
            if (user == null)
            {
                return false;
            }
            currentUser = user;
            return true;
        }
                public bool Logout()
        {
            currentUser = null;
            return true;
        }


        public User? UpdateUser(int id, UpdateUserDto userUp)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return null;
            }
            user.Name = userUp.Name ?? user.Name;
            user.Email = userUp.Email ?? user.Email;
            user.Password = userUp.Password ?? user.Password;
            user.phoneNumber = userUp.PhoneNumber ?? user.phoneNumber;
            return user;
        }

        // admin methods


        public Product AddProduct(AddProductDto product)
        {
            Product newProduct = new()
            {
                Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1,
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                Category = product.Category
            };
            _products.Add(newProduct);
            return newProduct;
        }
        public Product? UpdateProduct(int id, UpdateProductDto productUp)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return null;
            }
            product.Name = productUp.Name ?? product.Name;
            product.Description = productUp.Description ?? product.Description;
            product.Price = productUp.Price ?? product.Price;
            product.Category = productUp.Category ?? product.Category;
            return product;
        }
        public bool DeleteProduct(int id)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return false;
            }
            _products.Remove(product);
            return true;
        }
        public bool SetDiscountCode(string code, decimal discount)
        {
            DiscountCode discountCode = new()
            {
                Code = code,
                DiscountPrecent = discount
            };
            _discountCodes.Add(discountCode);
            return true;
        }
        public bool AddToInventory(int id, int quantity, int price)
        {
            Product? product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return false;
            }
            InventoryClass inventory = new()
            {
                Quantity = quantity,
                Price = price
            };
            product.Inventory.Add(inventory);
            return true;
        }
        public bool DeleteUser(int id)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return false;
            }
            _users.Remove(user);
            return true;
        }
        public bool UpgradeUser(int id)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return false;
            }
            user.Role = RoleEnum.Admin;
            return true;
        }
        public bool DowngradeUser(int id)
        {
            User? user = _users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return false;
            }
            user.Role = RoleEnum.User;
            return true;
        }
        public List<User> GetAllUsers() => _users;


    }
}