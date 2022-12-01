using SportsMockService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportsMockService.Contracts
{
    public interface IBookRepository
    {
        public Task<IEnumerable<Book>> GetBooks();
    }
}
