using BulkyWeb2.Models;

namespace BulkyWeb2.Repository.IRepository
{
    public interface IUnitOfWork
    {
        ICategoryRepository Category { get; }
        IProductRepository Product { get; }
        ICompanyRepository Company { get; }

		void Save();
    }
}
