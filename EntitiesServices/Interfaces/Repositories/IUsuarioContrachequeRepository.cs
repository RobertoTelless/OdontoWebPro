using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IUsuarioContrachequeRepository : IRepositoryBase<USUARIO_CONTRACHEQUE>
    {
        USUARIO_CONTRACHEQUE GetItemById(Int32 id);
        USUARIO_CONTRACHEQUE GetItemByUser(Int32 idUser, DateTime data);
    }
}
