using System;
using System.Collections.Generic;
using EntitiesServices.Model;  
using ModelServices.Interfaces.Repositories;
using System.Linq;
using EntitiesServices.Work_Classes;
using System.Data.Entity;
using CrossCutting;
using System.ComponentModel.Design;

namespace DataServices.Repositories
{
    public class PacienteRepository : RepositoryBase<PACIENTE>, IPacienteRepository
    {
        public PACIENTE CheckExist(PACIENTE conta, Int32? idAss)
        {
            IQueryable<PACIENTE> query = Db.PACIENTE;
            query = query.Where(p => p.PACI_NR_CPF == conta.PACI_NR_CPF);
            query = query.Where(p => p.FILI_CD_ID == conta.FILI_CD_ID);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.FirstOrDefault();
        }

        public PACIENTE GetByNome(String nome, Int32? idAss)
        {
            IQueryable<PACIENTE> query = Db.PACIENTE.Where(p => p.PACI_IN_ATIVO == 1);
            query = query.Where(p => p.PACI_NM_NOME == nome);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            query = query.Include(p => p.ANAMNESE);
            query = query.Include(p => p.ORCAMENTO);
            query = query.Include(p => p.PACIENTE_PRESCRICAO);
            query = query.Include(p => p.PACIENTE_RECOMENDACAO);
            query = query.Include(p => p.PACIENTE_ANEXO);
            query = query.Include(p => p.PACIENTE_ACOMPANHAMENTO);
            return query.FirstOrDefault();
        }

        public PACIENTE GetItemById(Int32 id)
        {
            IQueryable<PACIENTE> query = Db.PACIENTE;
            query = query.Where(p => p.PACI_CD_ID == id);
            query = query.Include(p => p.ANAMNESE);
            query = query.Include(p => p.ORCAMENTO);
            query = query.Include(p => p.PACIENTE_PRESCRICAO);
            query = query.Include(p => p.PACIENTE_RECOMENDACAO);
            query = query.Include(p => p.PACIENTE_ANEXO);
            query = query.Include(p => p.PACIENTE_ACOMPANHAMENTO);
            return query.FirstOrDefault();
        }

        public List<PACIENTE> GetAllItens(Int32? idAss)
        {
            IQueryable<PACIENTE> query = Db.PACIENTE.Where(p => p.PACI_IN_ATIVO == 1);
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<PACIENTE> GetAllItensAdm(Int32? idAss)
        {
            IQueryable<PACIENTE> query = Db.PACIENTE;
            query = query.Where(p => p.ASSI_CD_ID == idAss);
            return query.ToList();
        }

        public List<PACIENTE> ExecuteFilter(Int32? catId, Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss)
        {
            List<PACIENTE> lista = new List<PACIENTE>();
            IQueryable<PACIENTE> query = Db.PACIENTE;
            if (catId != null)
            {
                query = query.Where(p => p.CAPA_CD_ID == catId);
            }
            if (filialId != null)
            {
                query = query.Where(p => p.FILI_CD_ID == filialId);
            }
            if (!String.IsNullOrEmpty(nome))
            {
                query = query.Where(p => p.PACI_NM_NOME.Contains(nome));
            }
            if (!String.IsNullOrEmpty(cpf))
            {
                query = query.Where(p => p.PACI_NR_CPF.Contains(cpf));
            }
            if (!String.IsNullOrEmpty(telefone))
            {
                query = query.Where(p => p.PACI_NR_TELEFONE.Contains(telefone));
            }
            if (!String.IsNullOrEmpty(celular))
            {
                query = query.Where(p => p.PACI_NR_CELULAR.Contains(celular));
            }
            if (!String.IsNullOrEmpty(cidade))
            {
                query = query.Where(p => p.PACI_NM_CIDADE.Contains(cidade));
            }
            if (!String.IsNullOrEmpty(email))
            {
                query = query.Where(p => p.PACI_NM_EMAIL.Contains(email));
            }
            if (dataNasc != DateTime.MinValue)
            {
                query = query.Where(p => p.PACI_DT_NASCIMENTO == dataNasc);
            }
            if (query != null)
            {
                query = query.Where(p => p.PACI_IN_ATIVO == 1);
                query = query.Where(p => p.ASSI_CD_ID == idAss);
                query = query.OrderBy(a => a.PACI_NM_NOME);
                lista = query.ToList<PACIENTE>();
            }
            return lista;
        }
    }
}
