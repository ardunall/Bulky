using BulkyWeb2.Data;
using BulkyWeb2.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BulkyWeb2.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;
        internal DbSet<T> dbSet;

        public Repository(AppDbContext db)
        {
            _db = db;
            this.dbSet =_db.Set<T>();
            // _db.Categories == dbSet
            _db.Products.Include(u => u.Category).Include(u => u.CategortId);


        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> filter, string? inculdeProperties = null)
        {
            IQueryable<T> query = dbSet; 
            query = query.Where(filter);
            if (!string.IsNullOrEmpty(inculdeProperties))
            {
                foreach (var inculudeProp in inculdeProperties
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(inculudeProp);
                }

            }
            return query.FirstOrDefault(); 
        }


        public IEnumerable<T> GetAll(string? inculdeProperties=null)
        {
            IQueryable<T> query = dbSet;
            if (!string.IsNullOrEmpty(inculdeProperties))
            {
                foreach(var inculudeProp in inculdeProperties
                    .Split(new char[] {','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(inculudeProp);
                }

            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);  
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            dbSet.RemoveRange(entity);
        }
    }
}
