using BulkyWeb2.Models;
using System.Linq.Expressions;

namespace BulkyWeb2.Repository.IRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        void Update(Category obj);
    }
}
