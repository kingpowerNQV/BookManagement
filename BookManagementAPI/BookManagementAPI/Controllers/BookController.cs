using BookManagementAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace BookManagementAPI.Controllers
{
    [RoutePrefix("api/Book")]
    public class BookController : ApiController
    {
        private BookManagementEntities db = new BookManagementEntities();
        //GET: api/books
        //[Authentication.BasicAuthentication]
        [HttpGet]
        [Route("Books")]
        public List<Book> GetBooks()
        {
            return db.Books.ToList();
        }

        // GET: api/Book/1  
        [HttpGet]
        [Route("GetDetail")]
        public IHttpActionResult GetBookDetail(int id)
        {
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
        [HttpGet]
        [Route("Search")]
        public List<Book> SearchBooks(string title, string description)
        {
            var data = db.Books;
            if(!string.IsNullOrEmpty(title))
            {
                data.Where(s => s.Title.Contains(title));
            }
            if (!string.IsNullOrEmpty(description))
            {
                data.Where(s => s.Description.Contains(description));
            }
            return data.ToList();
        }
        // PUT: api/Book/5  
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBookDetail(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != book.Id)
            {
                return BadRequest();
            }

            db.Entry(book).State = EntityState.Modified;
            if(book.CoverId == null)
            {
                book.CoverId = UploadCover();
            }

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }
        //POST: api/book
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> PostBookDetail(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            UploadCover();
            book.CoverId = UploadCover();
            db.Books.Add(book);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = book.Id }, book);
        }

        // DELETE: api/Book/5  
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> DeleteBook(int id)
        {
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();

            return Ok(book);
        }
        private bool BookExists(int id)
        {
            return db.Books.Count(e => e.Id == id) > 0;
        }
        [HttpPost]
        [Route("UploadImage")]
        public int UploadCover()
        {
            int count = db.Covers.Count();
            string coverName = null;
            var httpRequest = HttpContext.Current.Request;
            //Upload Image
            var postedFile = httpRequest.Files["Image"];

            coverName = new String(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            coverName = coverName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            var filePath = HttpContext.Current.Server.MapPath("~/Image/" + coverName);
            postedFile.SaveAs(filePath);

            //Save to DB
            using (BookManagementEntities db = new BookManagementEntities())
            {
                Cover cover = new Cover()
                {
                    Id = count+1,
                    CoverName = coverName
                };
                db.Covers.Add(cover);
                db.SaveChanges();
            }
            return count + 1;
        }

    }

}
