using Shop.Models.Data;
using Shop.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Shop.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PagesController : Controller
    {
        // GET: Admin/Pages
        
        public ActionResult Index()
        {
            //Объявляем список для представления (PageVM)
            List<PageVM> pageList;
     
            //Инициализируем список (DB)
            using (Db db = new Db())
            {
                //var a = db.Database.SqlQuery<PagesDTO>("SELECT * FROM  tblPages ORDER BY Sorting").ToString();
                // pageList = db.Pages.FromSqlRaw("SELECT ");
                // var a = db.Pages.FromSqlRaw("SELECT * FROM Companies").ToList();

                //pageList = db.Pages.FromSql<PagesDTO>("SELECT * FROM tblPages").ToList<PageVM>();
                //var a = db.Pages.FromSql<PagesDTO>("SELECT * FROM tblPages ORDER BY Sorting").ToArray();
                pageList = db.Pages.ToArray().OrderBy(x => x.Sorting).Select(x => new PageVM(x)).ToList();

            }

            //Возвращаем список в представление
            return View(pageList);
        }
        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //Проврка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                

                //Объявляем переменную для краткого описания Slug
                string slug;

                //Инициализируем класс PageDTO
                PagesDTO dto = new PagesDTO();

                //Присвоем заголовок модели
                dto.Title = model.Title.ToUpper();
                
                //Проверяем, есть ли краткое описание, если нет, то присваеваем
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }

                //Убеждаемся что заголовк и краткое описание уникальное
                /*if(db.Pages.Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Этот заголовок уже существует.");
                    return View(model);
                }
                else if(db.Pages.Any(x => x.Slug == model.Slug))
                {
                    ModelState.AddModelError("", "Это описание уже существует.");
                    return View(model);
                }
                */
                //Присваеваем оставшиеся данные модели
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;

                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand($"INSERT INTO tblPages (Title, Slug, Body, Sorting, HasSidebar) VALUES (N'{dto.Title}' , N'{dto.Slug}' , N'{dto.Body}', '{dto.Sorting}' , '{dto.HasSidebar}') ");
                        db.SaveChanges();
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Это описание уже существует.");
                        return View(model);        
                    }
                }



                //db.Database.ExecuteSqlCommand("CREATE UNIQUE INDEX uniqueUserTitle ON tblPages(Title, Slug)");

               
                //Сохроняем модель в базу данных
                //db.Pages.Add(dto);

                //db.SaveChanges();

            }

            //Сообщение через TempData
            TempData["SM"] = "Данные успешно добавлены";

            //Переадресовыывем пользователя на метод INDEX
            return RedirectToAction("Index");

        }

        // GET: Admin/Pages/EditPage
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Объявление модели PageVM
            PageVM model;

            using (Db db = new Db())
            {
                //Получаем id страницы
                PagesDTO dto = db.Pages.Find(id);

                //Доступна ли страница
                if (dto == null)
                {
                    return Content("Страница не доступна");

                }

                //Инициализируем модель данными
                model = new PageVM(dto);
            }
                //Возвращаем модель и представление
                return View(model);
        }

        //POST: Admin/Pages/EditPage
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //Проверка модели на валидность
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //Получаем id страницы
                int id = model.Id;

                //Объявляем переменную краткого заголовка
                string slug = "home";

                //Получаем страницу по id
                PagesDTO dto = db.Pages.Find(id);

                //Присваиваем название из полученной модели в DTO
                dto.Title = model.Title; 

                //Проверяем краткий заголовок и присваиваем, если это необходимо
                if(model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }

                //Проверям заголовок и описание на уникальность
                if(db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title))
                {
                    ModelState.AddModelError("", "Этот заголовок не доступен");
                    return View(model);
                }
                else if (db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "Это описание не доступно");
                    return View(model);
                }

                //Записываем остальные значения в класс DTO
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;

                //Сохраняем изменения в базу
                db.SaveChanges();
            }

            //сообщение о успешном сохранении данных
            TempData["SM"] = "Данные успешно изменены";

            //Переадресация пользователя
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails
        [HttpGet]
        public ActionResult PageDetails(int id)
        {
            //Оъявляем модель PageVM
            PageVM model;

            using (Db db= new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);
                
                //убеждаемся что страница доступна
                if(dto == null)
                {
                    return Content("Это страница не доступна");
                }

                //Присваиваем модели информацию из базы данных
                model = new PageVM(dto);
            }


            //Возвращаем модель представления
            return View(model);
        }

        //GET: Admin/Pages/DeletePage
        [HttpGet]
        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //Получаем страницу
                PagesDTO dto = db.Pages.Find(id);

                //Удаляем страницу
                db.Pages.Remove(dto);

                //Сохраняем изменения в базу
                db.SaveChanges();
            }

            //Добовляем сообщение об удалении
            TempData["SM"] = "Страница успешно удалена";

            //Переадрисация пользователя
            return RedirectToAction("Index");
        }

        //Создаем метод сортировки
        //Post: Admin/Pages/ReorderPages
        [HttpPost]

        public void ReorderPages(int [] id)
        {
            using (Db db = new Db())
            {
                //Начальная переманная счетчик
                int count = 1;
                

                //Инициализируем модель данных
                PagesDTO dto;

                //Устанавливаем сортировку для каждой страницы
                foreach(var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;

                    db.SaveChanges();

                    count++;
                }
            }


        }

        //GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {
            //Инициализация модели
            SidebarVM model;

            using (Db db = new Db())
            {
                //Получаем данные из базы данных
                SidebarDTO dto = db.Sidebars.Find(1);

                //Заполняем модель которую мы объявили
                model = new SidebarVM(dto);

            }

            //Возвращаем прелставление с моделью
            return View(model);
        }

        //POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //Получить данные из базы данных
                SidebarDTO dto = db.Sidebars.Find(1);

                //Присвоить данные в тело (В свойство Body)
                dto.Body = model.Body;

                //Сохранить данные
                db.SaveChanges();

            }

            //Присвоить сообщение об спехе
            TempData["SM"] = "Вы отредактировали боковую панель";

            //Переадрисация пользователя
            return RedirectToAction("EditSidebar");
        }
    }
}