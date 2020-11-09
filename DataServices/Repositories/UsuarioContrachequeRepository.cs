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
    public class UsuarioContrachequeRepository : RepositoryBase<USUARIO_CONTRACHEQUE>, IUsuarioContrachequeRepository
    {
        public USUARIO_CONTRACHEQUE GetItemById(Int32 id)
        {
            IQueryable<USUARIO_CONTRACHEQUE> query = Db.USUARIO_CONTRACHEQUE;
            query = query.Where(p => p.USCC_CD_ID == id);
            return query.FirstOrDefault();
        }

        public USUARIO_CONTRACHEQUE GetItemByUser(Int32 id, DateTime data)
        {
            IQueryable<USUARIO_CONTRACHEQUE> query = Db.USUARIO_CONTRACHEQUE;
            query = query.Where(p => p.USUA_CD_ID == id);
            query = query.Where(p => p.USCC_DT_CADASTRO == data);
            return query.FirstOrDefault();
        }
    }
}
 