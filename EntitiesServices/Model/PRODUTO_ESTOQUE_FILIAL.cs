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
    
    public partial class PRODUTO_ESTOQUE_FILIAL
    {
        public int PREF_CD_ID { get; set; }
        public int PROD_CD_ID { get; set; }
        public Nullable<int> FILI_CD_ID { get; set; }
        public Nullable<int> PREF_QN_ESTOQUE { get; set; }
        public Nullable<System.DateTime> PREF_DT_ULTIMO_MOVIMENTO { get; set; }
        public Nullable<int> PREF_IN_ATIVO { get; set; }
    
        public virtual FILIAL FILIAL { get; set; }
        public virtual PRODUTO PRODUTO { get; set; }
    }
}
