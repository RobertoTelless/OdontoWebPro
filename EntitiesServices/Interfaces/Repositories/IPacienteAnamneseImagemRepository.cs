using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPacienteAnamneseImagemRepository : IRepositoryBase<PACIENTE_ANAMESE_IMAGEM>
    {
        List<PACIENTE_ANAMESE_IMAGEM> GetAllItens(Int32? idAss);
        PACIENTE_ANAMESE_IMAGEM GetItemById(Int32 id);
    }
}
