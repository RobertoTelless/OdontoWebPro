using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacienteRecomendacaoRepository : IRepositoryBase<PACIENTE_RECOMENDACAO>
    {
        List<PACIENTE_RECOMENDACAO> GetAllItens(Int32? idAss);
        PACIENTE_RECOMENDACAO GetItemById(Int32 id);
    }
}
