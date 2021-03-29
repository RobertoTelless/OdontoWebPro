using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacientePerguntaRepository : IRepositoryBase<PACIENTE_ANAMNESE_PERGUNTA>
    {
        List<PACIENTE_ANAMNESE_PERGUNTA> GetAllItens(Int32? idAss);
        PACIENTE_ANAMNESE_PERGUNTA GetItemById(Int32 id);
    }
}
