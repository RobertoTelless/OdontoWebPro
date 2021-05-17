using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ISubProcedimentoAnexoRepository : IRepositoryBase<SUB_PROCEDIMENTO_ANEXO>
    {
        List<SUB_PROCEDIMENTO_ANEXO> GetAllItens();
        SUB_PROCEDIMENTO_ANEXO GetItemById(Int32 id);
    }
}
