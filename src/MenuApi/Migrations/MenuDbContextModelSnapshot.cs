﻿// <auto-generated />
using System;
using MenuApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MenuApi.Migrations
{
    [DbContext(typeof(MenuDbContext))]
    partial class MenuDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("MenuApi.Entities.Produto", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("character varying(300)");

                    b.Property<bool>("Disponivel")
                        .HasColumnType("boolean");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<decimal>("Preco")
                        .HasColumnType("numeric");

                    b.Property<int>("Quantidade")
                        .HasColumnType("integer");

                    b.Property<int>("Tipo")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Produtos");
                });
#pragma warning restore 612, 618
        }
    }
}
