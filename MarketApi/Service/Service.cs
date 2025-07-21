using MarketApi.Data;
using MarketApi.DTOs.Product;
using MarketApi.DTOs.User;
using MarketApi.DTOs.Cart;
using MarketApi.models;
using System.ComponentModel.DataAnnotations;
namespace MarketApi.Service
{
    public class Service : IServices
    {   
        private AppDbContext _context;
        public Service(AppDbContext context)
        {
            _context = context;
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
        public List<Product> GetAllProduct() => _context.Products.ToList();
        public List<Product> GetByCategory(string category) => _context.Products.Where(p => p.Category.ToString() == category).ToList();
        public List<Product> Sort(string sortBy, bool isAscending)
        {
            switch (sortBy)
            {
                case "Price":
                    return isAscending ? _context.Products.OrderBy(p => p.Price).ToList() : _context.Products.OrderByDescending(p => p.Price).ToList();
                case "Rate":
                    return isAscending ? _context.Products.OrderBy(p => p.Rates.Average).ToList() : _context.Products.OrderByDescending(p => p.Rates.Average).ToList();
                case "Sales":
                    return isAscending ? _context.Products.OrderBy(p => p.Sales).ToList() : _context.Products.OrderByDescending(p => p.Sales).ToList();
                default:
                    return _context.Products.ToList();
            }
        }
        public List<Product> Search(string word) => _context.Products.Where(p => p.Name.Contains(word)).ToList();
        public List<Product> FilterByPrice(int minPrice, int maxPrice) => _context.Products.Where(p => p.Price >= minPrice && p.Price <= maxPrice).ToList();
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
        public User Signup(AddUserDto user)
        {
            User newUser = new()
            {
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Password = user.Password,
                phoneNumber = user.PhoneNumber ?? string.Empty
            };
            _context.Users.Add(newUser);
            _context.SaveChanges();
            return newUser;
        }
        public bool Login(string Name, string password)
        {
            User? user = _context.Users.FirstOrDefault(u => u.Name == Name && u.Password == password);
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
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return null;
            }
            user.Name = userUp.Name ?? user.Name;
            user.Email = userUp.Email ?? user.Email;
            user.Password = userUp.Password ?? user.Password;
            user.phoneNumber = userUp.PhoneNumber ?? user.phoneNumber;
            _context.SaveChanges();
            return user;
        }

        // admin methods


        public Product AddProduct(AddProductDto product)
        {
            Product newProduct = new()
            {
                Name = product.Name,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                Category = product.Category
            };
            _context.Products.Add(newProduct);
            return newProduct;
        }
        public Product? UpdateProduct(int id, UpdateProductDto productUp)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null)
            {
                return null;
            }
            product.Name = productUp.Name ?? product.Name;
            product.Description = productUp.Description ?? product.Description;
            product.Price = productUp.Price ?? product.Price;
            product.Category = productUp.Category ?? product.Category;
            _context.SaveChanges();
            return product;
        }
        public bool DeleteProduct(int id)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
            if (product == null)
            {
                return false;
            }
            _context.Products.Remove(product);
            _context.SaveChanges();
            return true;
        }
        
        public bool SetDiscountCode(string code, decimal discount)
        {
            DiscountCode discountCode = new()
            {
                Code = code,
                DiscountPrecent = discount
            };
            _context.Discounts.Add(discountCode);
            _context.SaveChanges();
            return true;
        }
        public bool AddToInventory(int id, int quantity, int price)
        {
            Product? product = _context.Products.FirstOrDefault(p => p.ID == id);
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
            _context.SaveChanges();
            return true;
        }
        public bool DeleteUser(int id)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return false;
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return true;
        }
        public bool UpgradeUser(int id)
        {
            User? user = _context.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return false;
            }
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


    }
}