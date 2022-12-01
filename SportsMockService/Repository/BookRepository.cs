using Dapper;
using SportsMockService.Context;
using SportsMockService.Contracts;
using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Repository
{
    public class BookRepository : IBookRepository
    {
        private readonly DapperContext _context;

        public BookRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetBooks()
        {
            var query = "SELECT * FROM Books";

            using (var connection = _context.CreateConnection())
            {
                var books = await connection.QueryAsync<Book>(query);
                return books.ToList();
            }
        }
    }
}
