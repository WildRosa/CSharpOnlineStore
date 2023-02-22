using Shop.Models.Data;
using Shop.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Объявляем лист типа CartVM
            // если в корзине ничего нету, то пропускаем первый шаг и создаем экземпляр класса лист
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Проверяем не пустая ли картина
            if (cart.Count == 0 || Session["cart"] == null)
            {
                ViewBag.Message = "Корзина пустая";
                return View();
            }
            //Складываем сумму и записываем во ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            //Возвращаем лист в представление
            return View(cart);
        }

        public ActionResult CartPartial()
        {
            //Объявляем модель CartVM
            CartVM model = new CartVM();

            //Объявляем переменную количество
            int qty = 0;

            //Объявляем переменную цены
            decimal price = 0m;

            //Проверяем сессию корзины
            if(Session["cart"] != null)
            {
                //Получаем общее количесвто и цену
                var list = (List<CartVM>)Session["cart"];

                foreach(var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity *  item.Price;
                }

                model.Quantity = qty;
                model.Price = price;

            }
            else
            {
                //Или устанавливаем количество и цену в ноль
                model.Quantity = 0;
                model.Price = 0m;

            }

            //Возвращаем частичное представление с моделью
            return PartialView("_CartPartial", model); ;
        }


        public ActionResult AddToCartPartial(int id)
        {
            //Объявляем лист, параметризированный типом CartVM
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Объевляем модель CartVM
            CartVM model = new CartVM();

            using(Db db = new Db())
            {
                //Получаем продукт
                ProductDTO product = db.Products.Find(id);

                //Проверяем, находится ли товар уже в корзине
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);

                //если нет , то добовляем новый товар в корзину
                if(productInCart == null)
                {
                    cart.Add(new CartVM() { 
                    
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });
                }
                //Если находится, то добовляем еденицу
                else
                {
                    productInCart.Quantity++;
                }

                string userName = User.Identity.Name;
                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{userName}'").ToList();
                var userId = 0;
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{userName}', 'AddToCart', '{DateTime.Now}')");

            }

            //Получаем общее количесвто, цену и добовляем данные в модель
            int qty = 0;
            decimal price = 0m;

            foreach(var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            //Сохраняем состояние корзины в  сессию
            Session["cart"] = cart;

           

            //Возвращаем частичное представление с моделью
            return PartialView("_AddToCartPartial", model);
        }

        //GET: /cart/IncrementProduct
        public JsonResult IncrementProduct(int productId)
        {
            //Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {

                //Получаем cartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Добовляем количество
                model.Quantity++;

                //Сохраняем данные
                var result = new { qty = model.Quantity, price = model.Price };

                string userName = User.Identity.Name;
                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{userName}'").ToList();
                var userId = 0;
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{userName}', 'IncrementProduct', '{DateTime.Now}')");


                //Вовзращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        //GET: /cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            //Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {

                //Получаем cartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                //Отнимаем количество
                if(model.Quantity > 1)
                {
                    model.Quantity--;
                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                

                //Сохраняем данные
                var result = new { qty = model.Quantity, price = model.Price };

                string userName = User.Identity.Name;
                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{userName}'").ToList();
                var userId = 0;
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{userName}', 'DecrementProduct', '{DateTime.Now}')");


                //Вовзращаем JSON ответ с данными
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        public void RemoveProduct (int productId)
        {
            //Объявляем лист cart
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using( Db db = new Db()){
                //Получаем cartVM из листа
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                cart.Remove(model);

                string userName = User.Identity.Name;
                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{userName}'").ToList();
                var userId = 0;
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{userName}', 'RemoveProduct', '{DateTime.Now}')");

            }
        }

        public ActionResult PaypalPartial()
        {
            //Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Возвращаем частичное представление с листом
            return PartialView(cart);
        }

        //POST: /cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //Получаем список товаров в корзине
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //Получаем имя пользователя
            string userName = User.Identity.Name;

            //Объявляем переменную для OrderId
            int orderId = 0;

            
            using(Db db = new Db())
            {
                //Объявляем модель OrderDTO
                OrderDTO orderDto = new OrderDTO();

                //Получаем ID пользователя
                var q = db.Users.FirstOrDefault(x => x.UserName == userName);
                int userId = q.Id;

                //Заполняем модель OrderDTO данными и сохраняем
                orderDto.UserId = userId;
                orderDto.CreatedAt = DateTime.Now;
               
                db.Orders.Add(orderDto);
                db.SaveChanges();

                //Получаем orderId
                orderId = orderDto.OrderId;

                //Объявляем модель OrderDetailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();

                //Добавляем в модель данные
                foreach(var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;
                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }

                var sqlQuery = db.Database.SqlQuery<UserDTO>($"SELECT * FROM tblUsers WHERE UserName = '{userName}'").ToList();
                foreach (var item in sqlQuery)
                {
                    userId = item.Id;
                }

                db.Database.ExecuteSqlCommand($"INSERT INTO tblLog (UserId, UserName, Event, EventTime) VALUES ('{userId}', '{userName}', 'Pay', '{DateTime.Now}')");

            }



            //Обновляем сессию
            Session["cart"] = null;
        }
    }
}