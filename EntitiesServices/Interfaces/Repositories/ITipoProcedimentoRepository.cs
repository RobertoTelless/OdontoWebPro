using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITipoProcedimentoRepository : IRepositoryBase<TIPO_PROCEDIMENTO>
    {
        TIPO_PROCEDIMENTO CheckExist(TIPO_PROCEDIMENTO item, Int32? idAss);
        List<TIPO_PROCEDIMENTO> GetAllItens(Int32 idAss);
        List<TIPO_PROCEDIMENTO> GetAllItensAdm(Int32 idAss);
        TIPO_PROCEDIMENTO GetItemById(Int32 id);
        List<TIPO_PROCEDIMENTO> ExecuteFilter(String nome, String descricao, Int32? idFilial, Int32 idAss);
    }
}
