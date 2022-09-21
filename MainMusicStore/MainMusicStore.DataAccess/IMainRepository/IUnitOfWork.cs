using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMusicStore.DataAccess.IMainRepository
{
   public interface IUnitOfWork:IDisposable
    {
        ICategoryRepository category { get; }
        ICoverTypeRepository coverType{ get; }
        ISPCallRepository sp_call { get; }
        IProductRepository product{ get; }
        ICompanyRepository company{ get; }
        IApplicationUserRepository applicationUser{ get; }
        IShoppingCartRepository shoppingCart{ get; }
        IOrderDetailsRepository orderDetails{ get; }
        IOrderHeaderRepository orderHeader{ get; }

        void Save();
    }
}
