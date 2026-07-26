using ContactManager.Domain.Entities;
using ContactManager.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace ContactManager.Infrastructure.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contato> Contacts => Set<Contato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contato>(entity =>
        {
            entity.ToTable("Contatos");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Id)
                .ValueGeneratedNever();

            entity.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(c => c.DataNascimento)
                .IsRequired()
                .HasColumnType("date");

            entity.Property(c => c.Genero)
                .IsRequired()
                .HasConversion(new EnumToStringConverter<Genero>())
                .HasMaxLength(10);

            entity.Property(c => c.EstaAtivo)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(c => c.CriadoEm)
                .IsRequired();

            entity.Property(c => c.AtualizadoEm)
                .IsRequired(false);

            // Idade é calculada em tempo de execução — não persistida
            entity.Ignore(c => c.Idade);

            entity.HasIndex(c => new { c.Nome, c.DataNascimento })
                .HasDatabaseName("IX_Contatos_Nome_DataNascimento");

            entity.HasIndex(c => c.EstaAtivo)
                .HasDatabaseName("IX_Contatos_EstaAtivo");
        });
    }
}
