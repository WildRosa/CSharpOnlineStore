using Shop.Models.Data;
using Shop.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Shop.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Pages");

        }

        public ActionResult CategoryMenuPartial()
        {
            //Объявляем модель типа list CategoryVM
            List<CategoryVM> categoryVMList;

            //Инициализация модели данныех
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();

            }
            //Возвращаем частичное представление модели данных
            return PartialView("_CategoryMenuPartial", categoryVMList);
        }

        //GET: Shop/Category/name
        public ActionResult Category(string name)
        {
            //Объявляем список типа лист
            List<ProductVM> productVMList;

            using(Db db =new Db())
            {
                //Получаем ID категории
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();

                int catId = categoryDTO.Id;

                //Инициализируем список данными
                productVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                //Инициализируем имя категории
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();

                //Делаем проверку на NULL
                if(productCat == null)
                {
                    var catName = db.Categories.Where(x => x.Slug == name).Select(x => x.Name).FirstOrDefault();
                    ViewBag.CategoryName = catName;
                }
                else
                {
                    ViewBag.CategoryName = productCat.CategoryName;
                }
            }

            //Возвращаем представление с моделью 
            return View(productVMList);
        }

        //GET: Shop/product-details/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {
            //Объявялем модели DTO и VM
            ProductDTO dto;
            ProductVM model;

            //Инициализируем Id продукта
            int id = 0;

            using(Db db = new Db())
            {
                //Проверяем доступен ли продукт
                if(!db.Products.Any(x => x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index", "Shop");
                }
                
                //Инициализируем модель DTO данными
                dto = db.Products.Where(x => x.Slug == name).FirstOrDefault();

                //Получаем ID 
                id = dto.Id;

                //Инициализируем модель VM данными
                model = new ProductVM(dto);
            }
          
            //Возвращаем модель в представление
            return View("ProductDetails", model);
        }
    }
}