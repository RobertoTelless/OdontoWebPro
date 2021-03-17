using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class PacienteRecomendacaoViewModel
    {
        [Key]
        public int RECO_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo DATA obrigatorio")]
        [DataType(DataType.Date, ErrorMessage = "DATA deve ser uma data válida")]
        public Nullable<System.DateTime> RECO_DT_DATA { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve ter no minimo 1 caractere e no máximo 50 caracteres.")]
        public string RECO_NM_NOME { get; set; }
        [Required(ErrorMessage = "Campo DESCRIÇÃO obrigatorio")]
        [StringLength(5000, MinimumLength = 1, ErrorMessage = "O TEXTO deve ter no minimo 1 caractere e no máximo 5000 caracteres.")]
        public string RECO_DS_DESCRICAO { get; set; }
        public Nullable<int> RECO_IN_ATIVO { get; set; }

        public virtual PACIENTE PACIENTE { get; set; }
    }
}