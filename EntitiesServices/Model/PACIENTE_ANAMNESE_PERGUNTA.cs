//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EntitiesServices.Model
{
    using System;
    using System.Collections.Generic;
    
    public partial class PACIENTE_ANAMNESE_PERGUNTA
    {
        public int PCAN_CD_ID { get; set; }
        public int PACI_CD_ID { get; set; }
        public string PCAN_NM_PERGUNTA { get; set; }
        public string PCAN_NM_RESPOSTA { get; set; }
        public int PCAN_IN_ATIVO { get; set; }
    
        public virtual PACIENTE PACIENTE { get; set; }
    }
}
