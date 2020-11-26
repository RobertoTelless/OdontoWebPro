using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;
using EntitiesServices.Work_Classes;
using System.Data.Entity;

namespace DataServices.Repositories
{
    public class TipoProcedimentoAnexoRepository : RepositoryBase<TIPO_PROCEDIMENTO_ANEXO>, ITipoProcedimentoAnexoRepository
    {
        public List<TIPO_PROCEDIMENTO_ANEXO> GetAllItens()
        {
            return Db.TIPO_PROCEDIMENTO_ANEXO.ToList();
        }

        public TIPO_PROCEDIMENTO_ANEXO GetItemById(Int32 id)
        {
            IQueryable<TIPO_PROCEDIMENTO_ANEXO> query = Db.TIPO_PROCEDIMENTO_ANEXO.Where(p => p.TPAN_CD_ID == id);
            return query.FirstOrDefault();
        }
    }
}
 