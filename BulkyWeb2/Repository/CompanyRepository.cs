using BulkyWeb2.Data;
using BulkyWeb2.Models;
using BulkyWeb2.Repository.IRepository;
using System.Linq.Expressions;
namespace BulkyWeb2.Repository
{
    public class CompanyRepository : Repository<Company> , ICompanyRepository
    {
        private AppDbContext _db;
        public CompanyRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Company obj)
        {
            _db.Companies.Update(obj);
        }
    }
}
