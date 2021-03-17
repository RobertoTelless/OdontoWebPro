using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IPacienteAppService : IAppServiceBase<PACIENTE>
    {
        Int32 ValidateCreate(PACIENTE perfil, USUARIO usuario);
        Int32 ValidateEdit(PACIENTE perfil, PACIENTE perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(PACIENTE item, PACIENTE itemAntes);
        Int32 ValidateDelete(PACIENTE perfil, USUARIO usuario);
        Int32 ValidateReativar(PACIENTE perfil, USUARIO usuario);

        List<PACIENTE> GetAllItens(Int32? idAss);
        List<PACIENTE> GetAllItensAdm(Int32? idAss);
        PACIENTE GetItemById(Int32 id);
        PACIENTE GetByNome(String nome, Int32? idAss);
        PACIENTE CheckExist(PACIENTE conta, Int32? idAss);
        List<FILIAL> GetAllFiliais(Int32 idAss);
        List<UF> GetAllUF();
        UF GetUFBySigla(String sigla);
        PACIENTE_ANEXO GetAnexoById(Int32 id);
        Int32 ExecuteFilter(Int32? filialId, String nome, String cpf, String telefone, String celular, String cidade, DateTime dataNasc, String email, Int32? idAss, out List<PACIENTE> objeto);

        PACIENTE_ACOMPANHAMENTO GetAcompanhamentoById(Int32 id);

        PACIENTE_PRESCRICAO GetPrescricaoById(Int32 id);
        Int32 EditPrescricao(PACIENTE_PRESCRICAO item);
        Int32 CreatePrescricao(PACIENTE_PRESCRICAO item);

        PACIENTE_RECOMENDACAO GetRecomendacaoById(Int32 id);
        Int32 EditRecomendacao(PACIENTE_RECOMENDACAO item);
        Int32 CreateRecomendacao(PACIENTE_RECOMENDACAO item);
    }
}
