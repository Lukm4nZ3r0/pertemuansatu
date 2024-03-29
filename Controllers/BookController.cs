using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using pertemuansatu.Models.Param;
using pertemuansatu.Models.View;
using System.Linq;
using System;
using pertemuansatu.Models;
using pertemuansatu.Models.Entity;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace pertemuansatu.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    
    public class BooksController : ControllerBase
    {
        private readonly BasicDbContext dbContext;

        public BooksController(BasicDbContext dbContext)
        {
            this.dbContext = dbContext;
            if (this.dbContext.Books.Count() == 0)
            {
                this.dbContext.Books.Add(new Book
                {
                    Id = 1,
                    Date = DateTime.UtcNow,
                    Title = "Hello There!"
                });
                this.dbContext.SaveChanges();
            }
        }

        public BookView[] BookList {get; set;}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookView>>> Get([FromQuery]GetBookParam param)
        {
            var dataSource = await dbContext.Books
                .Select(book => new BookView {
                    Title = book.Title,
                    Date = book.Date
                })
                .ToListAsync();
            if (string.IsNullOrWhiteSpace(param.Title))
            {
                return dataSource;
            }
            return dataSource.Where(item => item.Title.ToLower()
                .Contains(param.Title.ToLower()))
                .ToList();
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BookView>>> Post([FromBody] BookView param)
        {
            var newId = await dbContext.Books.CountAsync() + 1;
            dbContext.Add(new Book {
                Id = newId,
                Title = param.Title,
                Date = param.Date
            });
            await this.dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status201Created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<IEnumerable<BookView>>> Put([FromRoute]int id, [FromBody] BookView param)
        {
            var selectedBook = await dbContext.Books.SingleOrDefaultAsync(item => item.Id == id);
            if (selectedBook == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            selectedBook.Title = param.Title;
            selectedBook.Date = param.Date;
            await this.dbContext.SaveChangesAsync();
            return StatusCode(StatusCodes.Status200OK);
        }
    }
}