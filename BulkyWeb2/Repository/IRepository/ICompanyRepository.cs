using BulkyWeb2.Models;
using System.Linq.Expressions;

namespace BulkyWeb2.Repository.IRepository
{
    public interface ICompanyRepository : IRepository<Company>
    {
        void Update(Company obj);
    }
}
