using MainMusicStore.Models.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainMusicStore.DataAccess.IMainRepository
{
   public interface ICoverTypeRepository:IRepository<CoverType>
    {
        void Update(CoverType coverType);
    }
}
