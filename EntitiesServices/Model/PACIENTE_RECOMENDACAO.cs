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
    
    public partial class PACIENTE_RECOMENDACAO
    {
        public int RECO_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        public Nullable<System.DateTime> RECO_DT_DATA { get; set; }
        public string RECO_NM_NOME { get; set; }
        public string RECO_DS_DESCRICAO { get; set; }
        public Nullable<int> RECO_IN_ATIVO { get; set; }
    
        public virtual PACIENTE PACIENTE { get; set; }
    }
}
