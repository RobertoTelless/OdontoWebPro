using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;
using EntitiesServices.Attributes;

namespace OdontoWeb.ViewModels
{
    public class UsuarioViewModel
    {
        [Key]
        public int USUA_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo PERFIL obrigatorio")]
        public int PERF_CD_ID { get; set; }
        public int ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo CARGO obrigatorio")]
        public Nullable<int> CARG_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50.")]
        public string USUA_NM_NOME { get; set; }
        public string USUA_NM_MATRICULA { get; set; }
        public string USUA_NM_LOGIN { get; set; }
        [Required(ErrorMessage = "Campo E-MAIL obrigatorio")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "O E-MAIL deve ter no minimo 1 caractere e no máximo 100.")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Deve ser um e-mail válido")]
        public string USUA_NM_EMAIL { get; set; }
        [StringLength(50, ErrorMessage = "O TELEFONE deve ter no máximo 50 caracteres.")]
        public string USUA_NR_TELEFONE { get; set; }
        [StringLength(50, ErrorMessage = "O CELULAR deve ter no máximo 50 caracteres.")]
        public string USUA_NR_CELULAR { get; set; }
        [Required(ErrorMessage = "Campo SENHA obrigatorio")]
        public string USUA_NM_SENHA { get; set; }
        [StringLength(10, MinimumLength = 1, ErrorMessage = "O SENHA deve ter no minimo 1 caractere e no máximo 10.")]
        public string USUA_NM_SENHA_CONFIRMA { get; set; }
        [StringLength(10, MinimumLength = 1, ErrorMessage = "O SENHA deve ter no minimo 1 caractere e no máximo 10.")]
        public string USUA_NM_NOVA_SENHA { get; set; }
        public int USUA_IN_BLOQUEADO { get; set; }
        public int USUA_IN_PROVISORIO { get; set; }
        public int USUA_IN_LOGIN_PROVISORIO { get; set; }
        public int USUA_IN_ATIVO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "DATA DE BLOQUEIO Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_BLOQUEADO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "DATA DE ALTERAÇÃO Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_ALTERACAO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_TROCA_SENHA { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_ACESSO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_ULTIMA_FALHA { get; set; }
        public System.DateTime USUA_DT_CADASTRO { get; set; }
        public int USUA_NR_ACESSOS { get; set; }
        public int USUA_NR_FALHAS { get; set; }
        public string USUA_TX_OBSERVACOES { get; set; }
        public string USUA_AQ_FOTO { get; set; }
        public Nullable<int> USUA_IN_COMPRADOR { get; set; }
        public Nullable<int> USUA_IN_APROVADOR { get; set; }
        public Nullable<int> COLA_CD_ID { get; set; }
        public Nullable<int> FILI_CD_ID { get; set; }
        public Nullable<int> FUNC_CD_ID { get; set; }
        public Nullable<int> DEPT_CD_ID { get; set; }
        public Nullable<int> USUA_IN_LOGADO { get; set; }
        public Nullable<System.DateTime> USUA_DT_LOGADO { get; set; }
        public string USUA_SG_UF { get; set; }

        [Required(ErrorMessage = "Campo CATEGORIA obrigatorio")]
        public Nullable<int> CAUS_CD_ID { get; set; }
        [StringLength(20, ErrorMessage = "O CPF deve ter no máximo 20 caracteres.")]
        [CustomValidationCPF(ErrorMessage = "CPF inválido")]
        public string USUA_NR_CPF { get; set; }
        [StringLength(50, ErrorMessage = "A CTPS deve ter no máximo 50 caracteres.")]
        public string USUA_NR_CTPS { get; set; }
        [StringLength(10, ErrorMessage = "A SÉRIE DA CTPS deve ter no máximo 10 caracteres.")]
        public string USUA_NR_CTPS_SERIE { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_CTPS_EMISSAO { get; set; }
        public Nullable<int> USUA_CD_CTPS_UF { get; set; }
        [StringLength(20, ErrorMessage = "O NIS deve ter no máximo 20 caracteres.")]
        public string USUA_NR_NIS { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_ADMISSAO { get; set; }
        [DataType(DataType.Date, ErrorMessage = "Deve ser uma data válida")]
        public Nullable<System.DateTime> USUA_DT_DEMISSAO { get; set; }
        [StringLength(500, ErrorMessage = "A JUSTIFICATIVA DE DEMISSÃO deve ter no máximo 500 caracteres.")]
        public string USUA_DS_JUSTIFICATIVA_DEMISSAO { get; set; }
        public Nullable<int> USUA_IN_TIPO_DEMISSAO { get; set; }
        [StringLength(100, ErrorMessage = "O ENDEREÇO deve ter no máximo 100 caracteres.")]
        public string USUA_NM_ENDERECO { get; set; }
        [StringLength(50, ErrorMessage = "O BAIRRO deve ter no máximo 50 caracteres.")]
        public string USUA_NM_BAIRRO { get; set; }
        [StringLength(50, ErrorMessage = "A CIDADE deve ter no máximo 50 caracteres.")]
        public string USUA_NM_CIDADE { get; set; }
        public Nullable<int> USUA_SG_CIDADE_UF { get; set; }
        [StringLength(10, ErrorMessage = "O CEP deve ter no máximo 10 caracteres.")]
        public string USUA_NR_CEP { get; set; }
        [StringLength(20, ErrorMessage = "O CRO deve ter no máximo 20 caracteres.")]
        public string USUA_NR_CRO { get; set; }
        [StringLength(50, ErrorMessage = "A EMPRESA deve ter no máximo 50 caracteres.")]
        public string USUA_NM_EMPRESA { get; set; }
        [StringLength(20, ErrorMessage = "O CNPJ deve ter no máximo 20 caracteres.")]
        [CustomValidationCNPJ(ErrorMessage = "CNPJ inválido")]
        public string USUA_NR_CNPJ { get; set; }
        public Nullable<int> USUA_IN_CATEGORIA { get; set; }
        [RegularExpression(@"^[0-9]+([,.][0-9]+)?$", ErrorMessage = "O SALÁRIO deve ser um valor numérico positivo")]
        public Nullable<decimal> USUA_VL_SALARIO { get; set; }
        public Nullable<int> SITU_CD_ID { get; set; }
        public Nullable<System.DateTime> USUA_DT_NASCIMENTO { get; set; }
        public string USUA_NR_RG { get; set; }

        public bool Bloqueio
        {
            get
            {
                if (USUA_IN_BLOQUEADO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                USUA_IN_BLOQUEADO = (value == true) ? 1 : 0;
            }
        }
        public bool LoginProvisorio
        {
            get
            {
                if (USUA_IN_LOGIN_PROVISORIO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                USUA_IN_LOGIN_PROVISORIO = (value == true) ? 1 : 0;
            }
        }
        public bool Provisoria
        {
            get
            {
                if (USUA_IN_PROVISORIO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                USUA_IN_PROVISORIO = (value == true) ? 1 : 0;
            }
        }

        public bool Comprador
        {
            get
            {
                if (USUA_IN_COMPRADOR == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                USUA_IN_COMPRADOR = (value == true) ? 1 : 0;
            }
        }
        public bool Aprovador
        {
            get
            {
                if (USUA_IN_APROVADOR == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                USUA_IN_APROVADOR = (value == true) ? 1 : 0;
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AGENDA> AGENDA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AGENDA> AGENDA1 { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CARGO CARGO { get; set; }
        public virtual FILIAL FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<LOG> LOG { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTICIA_COMENTARIO> NOTICIA_COMENTARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NOTIFICACAO> NOTIFICACAO { get; set; }
        public virtual PERFIL PERFIL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA> TAREFA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA> TAREFA1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA> TAREFA2 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA> TAREFA3 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA_ACOMPANHAMENTO> TAREFA_ACOMPANHAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<TAREFA_NOTIFICACAO> TAREFA_NOTIFICACAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO_ANEXO> USUARIO_ANEXO { get; set; }
        public virtual CATEGORIA_USUARIO CATEGORIA_USUARIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MOVIMENTO_ESTOQUE_PRODUTO> MOVIMENTO_ESTOQUE_PRODUTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PERCENTUAL_REMUNERACAO> PERCENTUAL_REMUNERACAO { get; set; }
        public virtual UF UF { get; set; }
        public virtual UF UF1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO_CONTRACHEQUE> USUARIO_CONTRACHEQUE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO_PREMIO> USUARIO_PREMIO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USUARIO_REMUNERACAO> USUARIO_REMUNERACAO { get; set; }
        public virtual SITUACAO SITUACAO { get; set; }
    }
}