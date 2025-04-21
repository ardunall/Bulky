using System.Linq.Expressions;

namespace BulkyWeb2.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        IEnumerable<T> GetAll(string? inculdeProperties = null);
        T Get(Expression<Func<T, bool>> filter, string? inculdeProperties = null);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);


    }
}
