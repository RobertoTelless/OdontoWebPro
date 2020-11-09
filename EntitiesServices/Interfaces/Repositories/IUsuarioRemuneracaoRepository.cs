using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IUsuarioRemuneracaoRepository : IRepositoryBase<USUARIO_REMUNERACAO>
    {
        USUARIO_REMUNERACAO GetItemById(Int32 id);
        USUARIO_REMUNERACAO GetItemByUser(Int32 idUser, DateTime data);
    }
}
