using PagedList;
using Shop.Areas.Admin.Models.ViewModels.Shop;
using Shop.Models.Data;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        // GET: Admin/Shop
        [HttpGet]
        public ActionResult Categories()
        {
            //Объевляем модель типа list
            List<CategoryVM> categoryVMList;

            
            using (Db db = new Db())
            {
                //Инициализируем модель данными
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();

            }
            //Возвращаем List в представления
            return View(categoryVMList);
        }

        //POST: /Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Объявляем строковую переменную id
            string id;
            
            using (Db db = new Db())
            {
                //Проверка на уникальность
                if(db.Categories.Any(x => x.Name == catName))
                {
                    return "titletaken";
                }

                //Инициализация модели DTO
                CategoryDTO dto = new CategoryDTO();

                //Заполняем данными модель
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                //Сохраняем
                db.Categories.Add(dto);
                db.SaveChanges();

                //Получаем id для возврата его в представление
                id = dto.Id.ToString();

            }

            //Возвращаем id
            return id;
        }

        //POST: /Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //Начальная переманная счетчик
                int count = 1;


                //Инициализируем модель данных
                CategoryDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        //GET: Admin/Shop/DeleteCategory
        [HttpGet]
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //Получаем Категорию
                CategoryDTO dto = db.Categories.Find(id);

                //Удаляем категорию
                db.Categories.Remove(dto);

                //Сохраняем изменения в базу
                db.SaveChanges();
            }

            //Добовляем сообщение об удалении
            TempData["SM"] = "Категория успешно удалена";

            //Переадрисация пользователя
            return RedirectToAction("Categories");
        }

        //POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using (Db db = new Db())
            {
                //Проверить имя на уникальность
                if(db.Categories.Any(x => x.Name == newCatName))
                {
                    return "titletaken";
                }

                //Получаем данные из базы данных
                CategoryDTO dto = db.Categories.Find(id);

                //Редактируем модель DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                //Сохранить изменения
                db.SaveChanges();
            }

            //Возвращаем слово
            return "ok";
        }

        //GET: Admin/Shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Объявляем модель данных
            ProductVM model = new ProductVM();

            //$("SELECT * FROM tblProduct JOIN tblCategory ON tblCategory.Id = tblProduct.CategoryId WHERE tblCategory.Name = '{model.Name}')

            //Добовлчем список категорий из базы в мдель
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "id", "Name");
                
            }
            //вернуть модель в представление
            return View(model);
        }

        //POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Проверяем на валидность
            if (!ModelState.IsValid)
            {
                using(Db db = new Db())
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            //Проверяем имя на уникальность
            using(Db db = new Db())
            {
                if(db.Products.Any(x => x.Name == model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "Товар с таким названием уже сущестует");
                    return View(model);
                }
            }

            //Объевляем переменную ProductID
            int id;

            //Инициализируем и сохраняем модель на основе ProductDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = new CategoryDTO();
                catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                id = product.Id;

            }

            //Добавляем сообщение в TempData
            TempData["SM"] = "Товар успешно добавлен";

            #region Upload Image

            //Создаем пути для картинок
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            //thubs - уменьшенные копии картинок
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            //Проверям если эти пути если нету, то создаем
            if (!Directory.Exists(pathString1)) { Directory.CreateDirectory(pathString1); }

            if (!Directory.Exists(pathString2)) { Directory.CreateDirectory(pathString2); }

            if (!Directory.Exists(pathString3)) { Directory.CreateDirectory(pathString3); }

            if (!Directory.Exists(pathString4)) { Directory.CreateDirectory(pathString4); }

            if (!Directory.Exists(pathString5)) { Directory.CreateDirectory(pathString5); }

            //Проверяем бы ли этот файл загружен
            if (file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();

                //Проверяем расширение файла
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "Неверное расширение изображения");
                        return View(model);

                    }
                }


                //Объевляем переменную с еменем изображения
                string imageName = file.FileName;

                //Сохраняем имя изображения в модель DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = new ProductDTO();
                    dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //Назначаеем пути к оригинальному изображению и уменьшиному
                var path = string.Format($"{pathString2}\\{imageName}");
                var path2 = string.Format($"{pathString3}\\{imageName}");

                //Сохраняем оригинальное изображение
                file.SaveAs(path);

                //Создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }
            #endregion

            //Переадресовываем пользователя
            return RedirectToAction("AddProduct");
        }
        
        //GET: Admin/Shop/Products
        [HttpGet]
        public ActionResult Products(int? page, int? catId)
        {
            //Объявляем модель productVM типа лист
            List<ProductVM> listOfProductVM;

            //Устанавливаем номер страницы
            var pageNumber = page ?? 1;

            using(Db db = new Db())
            {
                //Инициализируем list и заполняем данными
                listOfProductVM = db.Products.ToArray().Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x)).ToList();


                //Заполняем категории данными
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Устанавливаем выбранную категорию
                ViewBag.SelectedCat = catId.ToString();
            }

            //Устанавливаем постраничную новигацию
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.onePageOfProducts = onePageOfProducts;

            //Возвращаем представление с данными 
            return View(listOfProductVM);
        }

        //GET: Admin/Shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Объявляем модель ProductVM
            ProductVM model;
            
            using(Db db = new Db())
            {
                //Получаем товар
                ProductDTO dto = db.Products.Find(id);

                //Проверяем доступен ли продукт
                if(dto == null)
                {
                    return Content("Такого товара нету");

                }

                //Инициализируем модель данными
                model = new ProductVM(dto);

                //Создаем список категорий
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                //Получаем все изображения из галереии
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(x => Path.GetFileName(x));


            }


            //Возвращаем модель в представление
            return View(model);
        }
    
        //POST: Admin/Shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model, HttpPostedFileBase file)
        {
            //Получаем id продукта
            int id = model.Id;

            //Заполняем выпадающий список категориями и изображений
            using(Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }

            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs")).Select(x => Path.GetFileName(x));

            //Проверяем модель на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //проверяем имя товара на уникальность
            using(Db db = new Db())
            {
                if(db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "Товар с таким иемнем уже есть");
                    return View(model);
                }
            }
            //Обновляем товар в базе данных
            using(Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;

                db.SaveChanges();
            }

            //Устанавливаем сообщение в TempData
            TempData["SM"] = "Товар успешно изменен";


            //Логика обработки изображений

            #region Image Upload
            
            //Проверям загрузку файла
            if(file != null && file.ContentLength > 0)
            {
                //Получаем расширение файла
                string ext = file.ContentType.ToLower();

                //Проверяем расширение файла
                if (ext != "image/jpg" && ext != "image/jpeg" && ext != "image/pjpeg" && ext != "image/gif" && ext != "image/png")
                {
                    using (Db db = new Db())
                    {
                      
                        ModelState.AddModelError("", "Неверное расширение изображения");
                        return View(model);

                    }
                }


                //Устанавливаем пути загрузки
                var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Удаляем существующие файлы и директориях
                DirectoryInfo di1 = new DirectoryInfo(pathString1);
                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (var file2 in di1.GetFiles())
                {
                    file2.Delete();
                }

                foreach (var file3 in di2.GetFiles())
                {
                    file3.Delete();
                }

                //Сохраняем изображения 
                string imageName = file.FileName;
                using(Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }

                //Сохраняем оригинал и превью версии
                var path = string.Format($"{pathString1}\\{imageName}");
                var path2 = string.Format($"{pathString2}\\{imageName}");

                //Сохраняем оригинальное изображение
                file.SaveAs(path);

                //Создаем и сохраняем уменьшенную копию
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200).Crop(1, 1);
                img.Save(path2);
            }



            #endregion

            //Переадресация пользователя
            return RedirectToAction("EditProduct");
        }
    
        //GET: Admin/Shop/DelteProduct
        [HttpGet]
        public ActionResult DeleteProduct(int id)
        {
            //Удаляем товар из базы данных
            using(Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();
            }

            //Удаляем директорию товара (изображения)
            var originalDirectory = new DirectoryInfo(string.Format($"{Server.MapPath(@"\")}Images\\Uploads"));
            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString1))
            {
                Directory.Delete(pathString1, true);
            }

            //Переадрисация пользователя
            return RedirectToAction("Products");
        }

        //GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            //Инициализируем модель OrderForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>();

            using(Db db = new Db())
            {
                //Инициализируем модель OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Перебираем данные модели OrderVM
                foreach(var order in orders)
                {
                    //Инициализируем словарь товаров
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    //Объяаляем переменную общей суммы
                    decimal total = 0m;

                    //Инициализируем лист OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    //Получаем имя пользователя
                    UserDTO user = db.Users.FirstOrDefault(x => x.Id == order.UserId);
                    string userName = user.UserName;

                    //Перебираем список товаров из OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Получаем товар
                        ProductDTO product = db.Products.FirstOrDefault(x => x.Id == orderDetails.ProductId);

                        //Получаем цену товара
                        decimal price = product.Price;

                        //Получаем название товара
                        string productName = product.Name;

                        //Добовляем товар в словарь
                        productAndQty.Add(productName, orderDetails.Quantity);

                        //Получаем общую стоимость
                        total += orderDetails.Quantity * price;
                    }

                    //Добовляем данные в модель OrdersForAdminVM
                    ordersForAdmin.Add(new OrdersForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userName,
                        Total = total,
                        ProductsAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }

            }

            //Возвращаем представление с моделью OrdersForAdminVM
            return View(ordersForAdmin);
        }
    }


}