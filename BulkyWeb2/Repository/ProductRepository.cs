using BulkyWeb2.Data;
using BulkyWeb2.Models;
using BulkyWeb2.Repository.IRepository;
using System.Linq.Expressions;

namespace BulkyWeb2.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private AppDbContext _db;
        public ProductRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product obj)
        {
            _db.Products.Update(obj);
        }
    }
}
