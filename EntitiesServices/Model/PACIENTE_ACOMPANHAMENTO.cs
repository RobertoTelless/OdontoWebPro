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
    
    public partial class PACIENTE_ACOMPANHAMENTO
    {
        public int PAAC_CD_ID { get; set; }
        public Nullable<int> PACI_CD_ID { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }
        public Nullable<System.DateTime> PAAC_DT_DATA { get; set; }
        public string PAAC_TX_ACOMPANHENTO { get; set; }
        public Nullable<int> PAAC_IN_ATIVO { get; set; }
    
        public virtual PACIENTE PACIENTE { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}
