﻿
namespace Sales.Backend.Controllers
{
    using Backend.Models;
    using Sales.Backend.Helpers;
    using Sales.Common.Models;
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Mvc;

    [Authorize]
    public class ProductsController : Controller
    {
        private LocalDataContext db = new LocalDataContext();

        // GET: Products
        public async Task<ActionResult> Index()
        {
            return View(await this.db.Products.OrderBy(P => P.Description).ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = await this.db.Products.FindAsync(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(ProductView view)
        {
            if (ModelState.IsValid)
            {

                var pic = string.Empty;
                var folder = "~/Content/Products";

                if (view.ImageFile != null)
                {
                    pic = FilesHelper.UploadPhoto(view.ImageFile, folder);
                    pic = $"{folder}/{pic}";
                }

                var product = this.ToProduct(view, pic);

                this.db.Products.Add(product);
                await this.db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(view);
        }

        private Product ToProduct(ProductView view, string pic)
        {
            return new Product
            {
                ProductId = view.ProductId,
                Description = view.Description,
                Remarks = view.Remarks,
                Price = view.Price,
                IsAvailable = view.IsAvailable,
                ImagePath = pic,
                PublishOn = view.PublishOn,                
            };
        }

        // GET: Products/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await this.db.Products.FindAsync(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            var view = ToView(product);
            return View(view);
        }

        private ProductView ToView(Product product)
        {
            return new ProductView
            {                     
                ProductId = product.ProductId,
                Description = product.Description,
                Remarks = product.Remarks,
                Price = product.Price,
                IsAvailable = product.IsAvailable,
                ImagePath = product.ImagePath,
                PublishOn = product.PublishOn,
            };
   
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(ProductView view)
        {
            if (ModelState.IsValid)
            {
                var pic = view.ImagePath;
                var folder = "~/Content/Products";

                if (view.ImageFile != null)
                {
                    pic = FilesHelper.UploadPhoto(view.ImageFile, folder);
                    pic = $"{folder}/{pic}";
                }

                var product = this.ToProduct(view, pic);
                this.db.Entry(product).State = EntityState.Modified;
                await this.db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(view);
        }


        // GET: Products/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = await this.db.Products.FindAsync(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var product = await this.db.Products.FindAsync(id);
            this.db.Products.Remove(product);
            await this.db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
