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
    
    public partial class PERCENTUAL_REMUNERACAO
    {
        public int PERE_CD_ID { get; set; }
        public Nullable<int> FILI_CD_ID { get; set; }
        public Nullable<int> CAUS_CD_ID { get; set; }
        public Nullable<int> USUA_CD_ID { get; set; }
        public Nullable<decimal> PERE_PC_TAREFAS { get; set; }
        public Nullable<decimal> PERE_PC_PRODUCAO { get; set; }
        public Nullable<decimal> PERE_PC_AVAL_EQUIPE { get; set; }
        public Nullable<decimal> PERE_PC_AVAL_PACIENTES { get; set; }
        public Nullable<decimal> PERE_PC_CONVERSAO { get; set; }
        public Nullable<int> PERE_IN_ATIVO { get; set; }
    
        public virtual CATEGORIA_USUARIO CATEGORIA_USUARIO { get; set; }
        public virtual FILIAL FILIAL { get; set; }
        public virtual USUARIO USUARIO { get; set; }
    }
}
