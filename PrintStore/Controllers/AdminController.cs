﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PrintStore.Domain.Infrastructure.Concrete;
using PrintStore.Domain.Infrastructure.Abstract;
using PrintStore.Domain.Entities;
using PrintStore.Models;
using PrintStore.Infrastructure.Abstract;
using PrintStore.Infrastructure.Attributes;

namespace PrintStore.Controllers
{
    /// <summary>
    /// Controller accessible only by managers and administrators used to administrate the website
    /// </summary>
    [Authorize(Roles = "Admin, Manager")]
    [ExceptionLogging]
    public class AdminController : Controller
    {
        IBusinessLogicLayer businessLayer;
        IUserLayer userLayer;

        //Contstuctor that takes arguments from Ninject
        public AdminController(IBusinessLogicLayer businessLayerParam, IUserLayer userLayerParam)
        {
            businessLayer = businessLayerParam;
            userLayer = userLayerParam;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ViewResult GetUsers()
        {
            List<UserViewModel> userViewModels = new List<UserViewModel>();
            userViewModels = userLayer.Users.Select(u => new UserViewModel() { UserId = u.Id, Email = u.Email, Role = userLayer.GetRoleName(u.Id), IsBlocked = u.IsBlocked }).ToList();
            return View(userViewModels);
        }

        [HttpPost]
        public ActionResult ModifyUser(string userId, string role, int isBlocked)
        {
            if (isBlocked == 1)
            {
                userLayer.BlockUser(userId);
            }
            else
            {
                userLayer.UnblockUser(userId);
            }

            userLayer.ChangeUserRole(userId, role);
            TempData["message"] = string.Format("User profile was successfully updated");
            return RedirectToAction("GetUsers");
        }

        public ViewResult GetOrders()
        {
            IEnumerable<Order> orders = businessLayer.Orders;
            return View(orders.ToList());
        }

        [HttpPost]
        public ActionResult ModifyOrder(int orderId, string orderStatus, int isDeleted)
        {
            if (isDeleted == 1)
            {
                businessLayer.DeleteOrder(orderId);
            }
            else
            {
                businessLayer.RestoreOrder(orderId);
            }

            businessLayer.ChangeOrderStatus(orderId, orderStatus);
            TempData["message"] = string.Format("Order was successfully updated");
            return RedirectToAction("GetOrders");
        }

        public ViewResult GetCategories()
        {
            IEnumerable<Category> categories = businessLayer.Categories;
            return View(categories);
        }

        public ViewResult EditCategory(int categoryId)
        {
            Category category = businessLayer.Categories.Where(c => c.CategoryId == categoryId).First();
            return View(category);
        }

        [HttpPost]
        public ActionResult EditCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            businessLayer.SaveCategory(category);
            TempData["message"] = string.Format("{0} was successfully updated", category.Name);
            return RedirectToAction("GetCategories", "Admin");
        }

        public ViewResult AddCategory()
        {
            return View("EditCategory", new Category());
        }

        [HttpPost]
        public ActionResult DeleteCategory(int categoryId)
        {
            Category category = businessLayer.DeleteCategory(categoryId);
            if (category != null)
            {
                TempData["message"] = string.Format("{0} was successfully deleted", category.Name);
            }

            return RedirectToAction("GetCategories", "Admin");
        }

        public ViewResult EditProduct(int productId)
        {
            Product product = businessLayer.Products.Where(p => p.ProductId == productId).First();
            return View(product);
        }

        /// <summary>
        /// Edits product
        /// </summary>
        /// <param name="product">Product to save</param>
        /// <param name="image">Image posted from a form</param>
        /// <returns>Redirection to all categories and their products</returns>
        [HttpPost]
        public ActionResult EditProduct(Product product, HttpPostedFileBase image)
        {
            if (!ModelState.IsValid)
            {
                return View(product);
            }

            if (image != null)
            {
                //Stores image to a temporary location
                image.SaveAs(Server.MapPath(EFBusinessLogicLayer.TempImagePath));
                //Generates a new guid for naming bigger and smaller pictures of the product
                product.ImageGuid = Guid.NewGuid();
            }

            businessLayer.SaveProduct(product);
            TempData["message"] = string.Format("{0} was successfully updated", product.Name);
            return RedirectToAction("GetCategories", "Admin");
        }

        public ViewResult AddProduct(int categoryId)
        {
            Product product = new Product();
            product.CategoryId = categoryId;
            return View("EditProduct", product);
        }

        [HttpPost]
        public ActionResult DeleteProduct(int productId)
        {
            Product product = businessLayer.DeleteProduct(productId);
            if (product != null)
            {
                TempData["message"] = string.Format("{0} was successfully deleted", product.Name);
            }

            return RedirectToAction("GetCategories", "Admin");
        }
    }
}