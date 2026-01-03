using Microsoft.EntityFrameworkCore;
using Throb.Data.DbContext;
using Throb.Repository.Interfaces;

namespace Throb.Repository.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ThrobDbContext _context;

        public GenericRepository(ThrobDbContext context)
        {
            _context = context;
        }

        public void Add(T entity)
        {
            _context.Set<T>().Add(entity);
            _context.SaveChanges();
        }
        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            _context.SaveChanges();
        }
        public IEnumerable<T> GetAll()
        =>_context.Set<T>().ToList();
            
        public T GetById(int id)
        => _context.Set<T>().Find(id);

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
            _context.SaveChanges();
        }
    }
}
