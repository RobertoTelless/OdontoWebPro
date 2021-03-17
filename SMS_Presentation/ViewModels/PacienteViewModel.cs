using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class PacienteViewModel
    {
        [Key]
        public int PACI_CD_ID { get; set; }
        public Nullable<int> ASSI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo FILIAL obrigatorio")]
        public Nullable<int> FILI_CD_ID { get; set; }
        public string PACI_AQ_FOTO { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string PACI_NM_NOME { get; set; }
        [Required(ErrorMessage = "Campo CPF obrigatorio")]
        [StringLength(20, ErrorMessage = "O CPF deve ter no máximo 20 caracteres.")]
        public string PACI_NR_CPF { get; set; }
        [StringLength(20, ErrorMessage = "O RG deve ter no máximo 20 caracteres.")]
        public string PACI_NR_RG { get; set; }
        [Required(ErrorMessage = "Campo NÚMERO DO PRONTUÁRIO obrigatorio")]
        [StringLength(20, MinimumLength = 1, ErrorMessage = "O NÚMERO DO PRONTUÁRIO deve ter no minimo 1 caractere e no máximo 20 caracteres.")]
        public string PACI_NR_PRONTUARIO { get; set; }
        [Required(ErrorMessage = "Campo SEXO obrigatorio")]
        public Nullable<int> PACI_IN_SEXO { get; set; }
        [StringLength(20, ErrorMessage = "O TELEFONE deve ter no máximo 20 caracteres.")]
        public string PACI_NR_TELEFONE { get; set; }
        [StringLength(20, ErrorMessage = "O CELULAR deve ter no máximo 20 caracteres.")]
        public string PACI_NR_CELULAR { get; set; }
        [StringLength(20, ErrorMessage = "O WHATSAPP deve ter no máximo 20 caracteres.")]
        public string PACI_NR_WHATSAPP { get; set; }
        [StringLength(100, ErrorMessage = "O E-MAIL deve ter no máximo 100 caracteres.")]
        public string PACI_NM_EMAIL { get; set; }
        [StringLength(50, ErrorMessage = "O ENDEREÇO deve ter no máximo 50 caracteres.")]
        public string PACI_NM_ENDERECO { get; set; }
        [StringLength(50, ErrorMessage = "O BAIRRO deve ter no máximo 50 caracteres.")]
        public string PACI_NM_BAIRRO { get; set; }
        [StringLength(50, ErrorMessage = "A CIDADE deve ter no máximo 50 caracteres.")]
        public string PACI_NM_CIDADE { get; set; }
        public Nullable<int> UF_CD_ID { get; set; }
        [StringLength(10, ErrorMessage = "O CEP deve ter no máximo 10 caracteres.")]
        public string PACI_NR_CEP { get; set; }
        public Nullable<decimal> PACI_VL_SALDO_FINANCEIRO { get; set; }
        public Nullable<int> PACI_IN_ATIVO { get; set; }
        public Nullable<System.DateTime> PACI_DT_CADASTRO { get; set; }
        [Required(ErrorMessage = "Campo DATA DE NASCIMENTO obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "DATA DE NASCIMENTO Deve ser uma data válida")]
        public Nullable<System.DateTime> PACI_DT_NASCIMENTO { get; set; }
        public string PACI_TX_OBSERVACOES { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ANAMNESE> ANAMNESE { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual FILIAL FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO> ORCAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIE_NTE_ACOMPANHAMENTO> PACIE_NTE_ACOMPANHAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_ANEXO> PACIENTE_ANEXO { get; set; }
        public virtual UF UF { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRESCRICAO> PRESCRICAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RECOMENDACAO> RECOMENDACAO { get; set; }

    }
}