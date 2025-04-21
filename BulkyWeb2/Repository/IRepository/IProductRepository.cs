using BulkyWeb2.Models;

namespace BulkyWeb2.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
    }
}
