using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Attributes;
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
        [Required(ErrorMessage = "Campo CATEGORIA obrigatorio")]
        public Nullable<int> CAPA_CD_ID { get; set; }
        public string PACI_AQ_FOTO { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string PACI_NM_NOME { get; set; }
        [Required(ErrorMessage = "Campo CPF obrigatorio")]
        [StringLength(20, ErrorMessage = "O CPF deve ter no máximo 20 caracteres.")]
        [CustomValidationCPF(ErrorMessage = "CPF inválido")]
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
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Deve ser um e-mail válido")]
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
        public Nullable<int> PACI_IN_ALGUMA_DOENCA { get; set; }
        [StringLength(500, ErrorMessage = "DOENÇAS PRÉ-EXISTENTES deve ter no máximo 500 caracteres.")]
        public string PACI_DS_ALGUMA_DOENCA { get; set; }
        public Nullable<int> PACI_IN_ALERGICO_MEDICAMENTO { get; set; }
        [StringLength(500, ErrorMessage = "ALERGIAS MEDICAMENTOSAS deve ter no máximo 500 caracteres.")]
        public string PACI_DS_ALERGICO_MEDICAMENTO { get; set; }
        public Nullable<int> PACI_IN_USO_MEDICAMENTO { get; set; }
        [StringLength(500, ErrorMessage = "MEDICAMENTOS EM USO deve ter no máximo 500 caracteres.")]
        public string PACI_DS_USO_MEDICAMENTO { get; set; }
        public Nullable<int> PACI_IN_HOSPITALIZADO { get; set; }
        [StringLength(500, ErrorMessage = "HOSPITALIZAÇÕES deve ter no máximo 500 caracteres.")]
        public string PACI_DS_HOSPITALIZADO { get; set; }
        public Nullable<int> PACI_IN_FUMA { get; set; }
        [StringLength(500, ErrorMessage = "FUMANTE deve ter no máximo 500 caracteres.")]
        public string PACI_DS_FUMA { get; set; }
        public Nullable<int> PACI_IN_BEBE { get; set; }
        [StringLength(500, ErrorMessage = "CONSUMO ALCOÓLICO deve ter no máximo 500 caracteres.")]
        public string PACI_DS_BEBE { get; set; }

        public bool Doenca
        {
            get
            {
                if (PACI_IN_ALGUMA_DOENCA == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_ALGUMA_DOENCA = (value == true) ? 1 : 0;
            }
        }

        public bool Alergia
        {
            get
            {
                if (PACI_IN_ALERGICO_MEDICAMENTO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_ALERGICO_MEDICAMENTO = (value == true) ? 1 : 0;
            }
        }

        public bool Medicamento
        {
            get
            {
                if (PACI_IN_USO_MEDICAMENTO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_USO_MEDICAMENTO = (value == true) ? 1 : 0;
            }
        }

        public bool Hospital
        {
            get
            {
                if (PACI_IN_HOSPITALIZADO == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_HOSPITALIZADO = (value == true) ? 1 : 0;
            }
        }

        public bool Fuma
        {
            get
            {
                if (PACI_IN_FUMA == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_FUMA = (value == true) ? 1 : 0;
            }
        }

        public bool Bebe
        {
            get
            {
                if (PACI_IN_BEBE == 1)
                {
                    return true;
                }
                return false;
            }
            set
            {
                PACI_IN_BEBE = (value == true) ? 1 : 0;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ANAMNESE> ANAMNESE { get; set; }
        public virtual ASSINANTE ASSINANTE { get; set; }
        public virtual CATEGORIA_PACIENTE CATEGORIA_PACIENTE { get; set; }
        public virtual FILIAL FILIAL { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ORCAMENTO> ORCAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_ACOMPANHAMENTO> PACIENTE_ACOMPANHAMENTO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_ANEXO> PACIENTE_ANEXO { get; set; }
        public virtual UF UF { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_PRESCRICAO> PACIENTE_PRESCRICAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_RECOMENDACAO> PACIENTE_RECOMENDACAO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_ANAMESE_IMAGEM> PACIENTE_ANAMESE_IMAGEM { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_ANAMNESE_PERGUNTA> PACIENTE_ANAMNESE_PERGUNTA { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PACIENTE_BALANCO_FINANCEIRO> PACIENTE_BALANCO_FINANCEIRO { get; set; }

    }
}