﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Odonto_DBEntities : DbContext
    {
        public Odonto_DBEntities()
            : base("name=Odonto_DBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AGENDA> AGENDA { get; set; }
        public virtual DbSet<AGENDA_ANEXO> AGENDA_ANEXO { get; set; }
        public virtual DbSet<ASSINANTE> ASSINANTE { get; set; }
        public virtual DbSet<CARGO> CARGO { get; set; }
        public virtual DbSet<CATEGORIA_AGENDA> CATEGORIA_AGENDA { get; set; }
        public virtual DbSet<CATEGORIA_NOTIFICACAO> CATEGORIA_NOTIFICACAO { get; set; }
        public virtual DbSet<CODIGO_REGIME_TRIBUTARIO> CODIGO_REGIME_TRIBUTARIO { get; set; }
        public virtual DbSet<CONFIGURACAO> CONFIGURACAO { get; set; }
        public virtual DbSet<FILIAL> FILIAL { get; set; }
        public virtual DbSet<LOG> LOG { get; set; }
        public virtual DbSet<MATRIZ> MATRIZ { get; set; }
        public virtual DbSet<NOTICIA> NOTICIA { get; set; }
        public virtual DbSet<NOTICIA_COMENTARIO> NOTICIA_COMENTARIO { get; set; }
        public virtual DbSet<NOTICIA_TAG> NOTICIA_TAG { get; set; }
        public virtual DbSet<NOTIFICACAO> NOTIFICACAO { get; set; }
        public virtual DbSet<NOTIFICACAO_ANEXO> NOTIFICACAO_ANEXO { get; set; }
        public virtual DbSet<PAIS> PAIS { get; set; }
        public virtual DbSet<PERFIL> PERFIL { get; set; }
        public virtual DbSet<REGIME_TRIBUTARIO> REGIME_TRIBUTARIO { get; set; }
        public virtual DbSet<SEXO> SEXO { get; set; }
        public virtual DbSet<SITUACAO> SITUACAO { get; set; }
        public virtual DbSet<TAREFA> TAREFA { get; set; }
        public virtual DbSet<TAREFA_ACOMPANHAMENTO> TAREFA_ACOMPANHAMENTO { get; set; }
        public virtual DbSet<TAREFA_ANEXO> TAREFA_ANEXO { get; set; }
        public virtual DbSet<TAREFA_NOTIFICACAO> TAREFA_NOTIFICACAO { get; set; }
        public virtual DbSet<TEMPLATE> TEMPLATE { get; set; }
        public virtual DbSet<TIPO_CONTRIBUINTE> TIPO_CONTRIBUINTE { get; set; }
        public virtual DbSet<TIPO_PESSOA> TIPO_PESSOA { get; set; }
        public virtual DbSet<TIPO_TAG> TIPO_TAG { get; set; }
        public virtual DbSet<TIPO_TAREFA> TIPO_TAREFA { get; set; }
        public virtual DbSet<UF> UF { get; set; }
        public virtual DbSet<UNIDADE> UNIDADE { get; set; }
        public virtual DbSet<USUARIO> USUARIO { get; set; }
        public virtual DbSet<USUARIO_ANEXO> USUARIO_ANEXO { get; set; }
    }
}
