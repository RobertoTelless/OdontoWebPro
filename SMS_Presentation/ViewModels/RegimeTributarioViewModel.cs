using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using EntitiesServices.Model;
using EntitiesServices.Attributes;

namespace OdontoWeb.ViewModels
{
    public class RegimeTributarioViewModel
    {
        [Key]
        public int RETR_CD_ID { get; set; }
        [Required(ErrorMessage = "Campo NOME obrigatorio")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "O NOME deve conter no minimo 1 e no máximo 50 caracteres.")]
        public string RETR_NM_NOME { get; set; }
        public Nullable<int> RETR_IN_ATIVO { get; set; }


    }
}