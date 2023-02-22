using Shop.Models.Data;
using Shop.Models.ViewModels.Account;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Shop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        //GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }

        //GET: account/Login
        [HttpGet]
        public ActionResult Login()
        {
            //Проверка что пользователь не авторизован
            string userName = User.Identity.Name;
            if (!string.IsNullOrEmpty(userName))
            {
                return RedirectToAction("user-profile");
            }

            //Вовзращаем представление
            return View();
        }

        //POST: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }

            //Проверяем соотвествие пароля
            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Пароли не совпадают");
                return View("CreateAccount", model);
            }

            using(Db db = new Db())
            {
                //Проверяем имя на уникальность
                if (db.Users.Any(x => x.UserName.Equals(model.UserName)))
                {
                    ModelState.AddModelError("", $"Пользователь с таким логином {model.UserName} уже существует");
                    model.UserName = "";
                    return View("CreateAccount", model);

                }
                //Создаем экземпляр класса UserDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAdress = model.EmailAdress,
                    UserName = model.UserName,
                    Password = model.Password
                };

                //Добавляем данные в модель
                db.Users.Add(userDTO);

                //Сохранить данные
                db.SaveChanges();

                //Добовляем роль пользоваетелю
                int id = userDTO.Id;

                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId = 2
                };

                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();

                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{model.UserName}'").ToList();
                var userId = 0;
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{model.UserName}', 'Signup', '{DateTime.Now}')");


            }

            //Записываем сообщение в TempData
            TempData["SM"] = "Аккаунт успешно создан";

            //Переадресовываем пользователя
            return RedirectToAction("Login");
        }

        //POST: account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            //Проверяем пользователя на валидность
            bool isValid = false;
            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.UserName.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                }

                if (!isValid)
                {
                    ModelState.AddModelError("", "Неправильное имя пользователя или пароль");
                    return View(model);
                }
                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username, model.RememberMe);
                    //var idUser = db.Database.ExecuteSqlCommand($"SELECT Id FROM tblUsers WHERE UserName = '{model.Username}'");
                    //var a = db.Database.ExecuteSqlCommand("SELECT * FROM tblUsers");
                    var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{model.Username}'").ToList();
                    var userId = 0;
                    foreach(var item in sqlQuery)
                    {
                        userId = item.Id;
                    }
             
                    db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{model.Username}', 'Login', '{DateTime.Now}')");
                    
                    return RedirectToAction(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
              
            }
        }

        //GET: /account/logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        [Authorize]
        public ActionResult UserNavPartial()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявляем модель 
            UserNavPartialVM model;

            using(Db db = new Db()){

                //Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                //Заполняем модель данными из контекста (DTO)
                model = new UserNavPartialVM()
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName
                    
                };
            }


            //Возвращаем частичное представление с моделью
            return PartialView(model);
        }

        //GET: /account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile ()
        {
            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявляем модель
            UserProfileVM model;
            using(Db db = new Db())
            {
                //Получаем пользователя
                UserDTO dto = db.Users.FirstOrDefault(x => x.UserName == userName);

                //Инициализируем модель данными
                model = new UserProfileVM(dto);

            }

            //Возвращаем модель с данными в представление
            return View("UserProfile", model);
        }

        //POST: /account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {

            bool userNameIsChanged = false;

            //Проверяем модель на валидность 
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }


            //Проверяем пароль (если пользователь хочет его сменить
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                    ModelState.AddModelError("", "Пароли не совпадают");
                    return View("UserProfile", model);
                }
            }
            
            using (Db db = new Db())
            {
                //Получаем имя пользователя
                string userName = User.Identity.Name;

                //Проверяем сменилось ли имя пользователя
                if(userName != model.UserName)
                {
                    userName = model.UserName;
                    userNameIsChanged = true;
                }

                //Проверяем имя на уникальность
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.UserName == userName))
                {
                    ModelState.AddModelError("", $"Пользователь с таким логином {model.UserName} уже существует");
                    model.UserName = "";
                    return View("UserProfile", model);
                }

                //Изменяем модель контекста данных
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAdress = model.EmailAdress;
                dto.UserName = model.UserName;

                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;
                }

                //Сохраняем изменения
                db.SaveChanges();

            }



            //Устанавливаем сообщение в TempData
            TempData["SM"]= "Данные успешно изменены";

            if (!userNameIsChanged)
            {
                //Возвращаем представление с моделью
                return View("UserProfile", model);
            }
            else
            {
                return RedirectToAction("Logout");
            }
            
        }

        //GET: /account/Orders
        [Authorize(Roles= "User")]
        public ActionResult Orders()
        {
            //Инициализируем модель OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            //$("SELECT * FROM tblOrder WHERE UserId='{model.UserId}' JOIN tblOrderDetails ON tblOrderDetails.Id = tblOrders.OrderId")
            
            using( Db db = new Db())
            {
                //Получаем Id пользователя
                UserDTO user = db.Users.FirstOrDefault(x => x.UserName == User.Identity.Name);
                int userId = user.Id;
                //Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираем список товаров в OrderVM
                foreach(var order in orders)
                {
                    //Инициализируем словарь товаров
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();

                    //Объявляем переменную конечной суммы
                    decimal total = 0m;

                    //Инициализируем модель OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //Перебираем список OrderDetailsDTO
                    foreach(var orderDetails in orderDetailsDTO)
                    {

                        //Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем цену товара
                        decimal price = product.Price;

                        //Получаем имя товара
                        string productName = product.Name;

                        //Добовляем товар в словарь
                        productsAndQty.Add(productName, orderDetails.Quantity);

                        //Получаем конечную стоимость товаров
                        total += orderDetails.Quantity * price;

                        
                    }
                    //Добовляем полученные данные в модель OrdersForUserVM
                    ordersForUser.Add(new OrdersForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

            //Возвращаем представление с моделью
            return View(ordersForUser);
        }
    }
}