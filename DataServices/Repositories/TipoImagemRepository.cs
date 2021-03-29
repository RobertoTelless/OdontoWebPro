using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using ModelServices.Interfaces.Repositories;

namespace DataServices.Repositories
{
    public class TipoImagemRepository : RepositoryBase<TIPO_IMAGEM>, ITipoImagemRepository
    {
        public TIPO_IMAGEM GetItemById(Int32 id)
        {
            IQueryable<TIPO_IMAGEM> query = Db.TIPO_IMAGEM;
            query = query.Where(p => p.TPIM_CD_ID == id);
            return query.FirstOrDefault();
        }

        public List<TIPO_IMAGEM> GetAllItens(Int32 idAss)
        {
            IQueryable<TIPO_IMAGEM> query = Db.TIPO_IMAGEM;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }
    }
}
 