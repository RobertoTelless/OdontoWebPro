using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IPacienteService : IServiceBase<PACIENTE>
    {
        Int32 Create(PACIENTE perfil, LOG log);
        Int32 Create(PACIENTE perfil);
        Int32 Edit(PACIENTE perfil, LOG log);
        Int32 Edit(PACIENTE perfil);
        Int32 Delete(PACIENTE perfil, LOG log);

        PACIENTE CheckExist(PACIENTE conta, Int32? idAss);
        PACIENTE GetItemById(Int32 id);
        PACIENTE GetByNome(String nome, Int32? idAss);
        List<PACIENTE> GetAllItens(Int32? idAss);
        List<PACIENTE> GetAllItensAdm(Int32? idAss);
        List<PACIENTE> ExecuteFilter(Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss);

        List<FILIAL> GetAllFiliais(Int32 idAss);
        List<UF> GetAllUF();
        PACIENTE_ANEXO GetAnexoById(Int32 id);
        UF GetUFBySigla(String sigla);
        PACIENTE_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id);

        PACIENTE_PRESCRICAO GetPrescricaoById(Int32 id);
        Int32 EditPrescricao(PACIENTE_PRESCRICAO item);
        Int32 CreatePrescricao(PACIENTE_PRESCRICAO item);

        PACIENTE_RECOMENDACAO GetRecomendacaoById(Int32 id);
        Int32 EditRecomendacao(PACIENTE_RECOMENDACAO item);
        Int32 CreateRecomendacao(PACIENTE_RECOMENDACAO item);

    }
}
