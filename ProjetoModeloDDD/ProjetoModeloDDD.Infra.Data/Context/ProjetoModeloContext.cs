using ProjetoModeloDDD.Domain.Entities;
using ProjetoModeloDDD.Infra.Data.EntityConfig;
using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ProjetoModeloDDD.Infra.Data.Context
{
    public class ProjetoModeloContext : DbContext
    {

        public ProjetoModeloContext()
            : base("ProjetoModeloDDD")
        {
            // the terrible hack
            var ensureDLLIsCopied =
                    System.Data.Entity.SqlServer.SqlProviderServices.Instance;

        }

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Produto> Produtos { get; set; }

        /// <summary>
        /// Configurar o Entity para criar o banco de dados conforme os padrões da empresa.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Impede que o entity tente colocar o nome da tabela no plural
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //Impede o Delete Cascade !!!Importante!!!
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();

            //Configurarmos para que as propriedades que tenham ID no nome seram consideradas como PK
            modelBuilder.Properties()
                .Where(p => p.Name == p.ReflectedType.Name + "Id")
                .Configure(p => p.IsKey());

            //Criar string como varchar com tamanho maximo de 200, caso não seja especificado o tamanho
            modelBuilder.Properties<string>()
                .Configure(p => p.HasColumnType("varchar"));

            modelBuilder.Properties<string>()
                .Configure(p => p.HasMaxLength(200));

            modelBuilder.Configurations.Add(new ClienteConfiguration());
            modelBuilder.Configurations.Add(new ProdutoConfiguration());
            //base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {

            //Campos DataCadastro são preenchidos com DateTime.now automaticamente na criação e são ignorados nas modificações
            foreach (var entry in ChangeTracker.Entries())
            {

                if (entry.Entity.GetType().GetProperty("DataCadastro") != null)
                {
                    if (entry.State == EntityState.Added)
                        entry.Property("DataCadastro").CurrentValue = DateTime.Now;

                    if (entry.State == EntityState.Modified)
                        entry.Property("DataCadastro").IsModified = false;
                }

                if (entry.Entity.GetType().GetProperty("DataAtualizacao") != null)
                {
                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                        entry.Property("DataAtualizacao").CurrentValue = DateTime.Now;

                }

            }

            return base.SaveChanges();
        }

    }


}
