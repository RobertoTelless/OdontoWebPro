using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacientePrescricaoRepository : IRepositoryBase<PACIENTE_PRESCRICAO>
    {
        List<PACIENTE_PRESCRICAO> GetAllItens(Int32? idAss);
        PACIENTE_PRESCRICAO GetItemById(Int32 id);
    }
}
