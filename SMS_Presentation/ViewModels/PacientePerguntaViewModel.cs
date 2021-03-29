using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;

namespace OdontoWeb.ViewModels
{
    public class PacientePerguntaViewModel
    {
        [Key]
        public int PCAN_CD_ID { get; set; }
        public int PACI_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo PERGUNTA obrigatorio")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "A PERGUNTA deve ter no minimo 1 caractere e no máximo 500 caracteres.")]
        public string PCAN_NM_PERGUNTA { get; set; }
        [StringLength(500, ErrorMessage = "A RESPOSTA deve ter no máximo 500 caracteres.")]
        public string PCAN_NM_RESPOSTA { get; set; }
        public int PCAN_IN_ATIVO { get; set; }

        public virtual PACIENTE PACIENTE { get; set; }
    }
}