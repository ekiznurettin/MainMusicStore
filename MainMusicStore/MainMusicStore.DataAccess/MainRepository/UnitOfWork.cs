using MainMusicStore.Data;
using MainMusicStore.DataAccess.IMainRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMusicStore.DataAccess.MainRepository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;
            category = new CategoryRepository(_db);
            sp_call = new SPCallRepository(_db);
            coverType = new CoverTypeRepository(_db);
            product = new ProductRepository(_db);
            company = new CompanyRepository(_db);
            applicationUser = new ApplicationUserRepository(_db);
            shoppingCart = new ShoppingCartRepository(_db);
            orderHeader = new OrderHeaderRepository(_db);
            orderDetails = new OrderDetailsRepository(_db);
        }

        public ICategoryRepository category { get; private set; }

        public ICoverTypeRepository coverType { get; private set; }

        public ISPCallRepository sp_call { get; private set; }
        public IProductRepository product { get; private set; }
        public ICompanyRepository company { get; private set; }
        public IApplicationUserRepository applicationUser{ get; private set; }
        public IShoppingCartRepository shoppingCart{ get; private set; }
        public IOrderHeaderRepository orderHeader{ get; private set; }
        public IOrderDetailsRepository orderDetails{ get; private set; }


        public void Dispose()
        {
            _db.Dispose();
        }

        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
