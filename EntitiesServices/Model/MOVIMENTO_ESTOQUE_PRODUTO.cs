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
    
    public partial class MOVIMENTO_ESTOQUE_PRODUTO
    {
        public int MOEP_CD_ID { get; set; }
        public int PROD_CD_ID { get; set; }
        public int FILI_CD_ID { get; set; }
        public int USUA_CD_ID { get; set; }
        public Nullable<System.DateTime> MOEP_DT_MOVIMENTO { get; set; }
        public Nullable<int> MOEP_IN_TIPO_MOVIMENTO { get; set; }
        public Nullable<int> MOEP_QN_QUANTIDADE { get; set; }
        public string MOEP_NM_ORIGEM { get; set; }
        public Nullable<int> MOEP_IN_ATIVO { get; set; }
        public string MOEP_DS_JUSTIFICATIVA { get; set; }
    
        public virtual FILIAL FILIAL { get; set; }
        public virtual PRODUTO PRODUTO { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}
