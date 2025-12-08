using System;
using System.Collections.Generic;
using ContabBackApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ContabBackApp.Context;

public partial class MyDbContext : DbContext
{
    public MyDbContext()
    {
    }

    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AsientosCabecera> AsientosCabeceras { get; set; }

    public virtual DbSet<AsientosDetalle> AsientosDetalles { get; set; }

    public virtual DbSet<Auxiliare> Auxiliares { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<CuentasContable> CuentasContables { get; set; }

    public virtual DbSet<Moneda> Monedas { get; set; }

    public virtual DbSet<TiposCuentum> TiposCuenta { get; set; }

    public virtual DbSet<TiposDocumento> TiposDocumentos { get; set; }

    public virtual DbSet<TransaccionesCxc> TransaccionesCxcs { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<ConfiguracionSistema> ConfiguracionSistema { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AsientosCabecera>(entity =>
        {
            entity.HasKey(e => e.IdAsiento).HasName("asientos_cabecera_pkey");

            entity.ToTable("asientos_cabecera");

            entity.HasIndex(e => e.FechaAsiento, "idx_asientos_fecha");

            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Registrado'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.FechaAsiento).HasColumnName("fecha_asiento");
            entity.Property(e => e.IdAuxiliar).HasColumnName("id_auxiliar");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdMoneda).HasColumnName("id_moneda");
            entity.Property(e => e.TasaCambio)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("1.0000")
                .HasColumnName("tasa_cambio");

            entity.HasOne(d => d.IdAuxiliarNavigation).WithMany(p => p.AsientosCabeceras)
                .HasForeignKey(d => d.IdAuxiliar)
                .HasConstraintName("asientos_cabecera_id_auxiliar_fkey");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.AsientosCabeceras)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("asientos_cabecera_id_cliente_fkey");

            entity.HasOne(d => d.IdMonedaNavigation).WithMany(p => p.AsientosCabeceras)
                .HasForeignKey(d => d.IdMoneda)
                .HasConstraintName("asientos_cabecera_id_moneda_fkey");
        });

        modelBuilder.Entity<AsientosDetalle>(entity =>
        {
            entity.HasKey(e => e.IdAsientoDetalle).HasName("asientos_detalle_pkey");

            entity.ToTable("asientos_detalle");

            entity.Property(e => e.IdAsientoDetalle).HasColumnName("id_asiento_detalle");
            entity.Property(e => e.IdAsiento).HasColumnName("id_asiento");
            entity.Property(e => e.IdCuentaContable).HasColumnName("id_cuenta_contable");
            entity.Property(e => e.Monto)
                .HasPrecision(18, 2)
                .HasColumnName("monto");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdAsientoNavigation).WithMany(p => p.AsientosDetalles)
                .HasForeignKey(d => d.IdAsiento)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("asientos_detalle_id_asiento_fkey");

            entity.HasOne(d => d.IdCuentaContableNavigation).WithMany(p => p.AsientosDetalles)
                .HasForeignKey(d => d.IdCuentaContable)
                .HasConstraintName("asientos_detalle_id_cuenta_contable_fkey");
        });

        modelBuilder.Entity<Auxiliare>(entity =>
        {
            entity.HasKey(e => e.IdAuxiliar).HasName("auxiliares_pkey");

            entity.ToTable("auxiliares");

            entity.Property(e => e.IdAuxiliar).HasColumnName("id_auxiliar");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("clientes_pkey");

            entity.ToTable("clientes");

            entity.HasIndex(e => e.Cedula, "clientes_cedula_key").IsUnique();
            entity.HasIndex(e => e.Rnc, "idx_clientes_rnc").IsUnique();

            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.Cedula)
                .HasMaxLength(20)
                .HasColumnName("cedula");
            entity.Property(e => e.Rnc)
                .HasMaxLength(15)
                .HasColumnName("rnc");
            entity.Property(e => e.TipoCliente)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Persona'::character varying")
                .HasColumnName("tipo_cliente");
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .HasColumnName("telefono");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Direccion)
                .HasColumnName("direccion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Activo'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.LimiteCredito)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("limite_credito");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<CuentasContable>(entity =>
        {
            entity.HasKey(e => e.IdCuentaContable).HasName("cuentas_contables_pkey");

            entity.ToTable("cuentas_contables");

            entity.HasIndex(e => e.IdCuentaPadre, "idx_cuentas_padre");
            entity.HasIndex(e => e.Codigo, "idx_cuentas_codigo").IsUnique();

            entity.Property(e => e.IdCuentaContable).HasColumnName("id_cuenta_contable");
            entity.Property(e => e.Codigo)
                .HasMaxLength(20)
                .HasColumnName("codigo");
            entity.Property(e => e.Balance)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0.00")
                .HasColumnName("balance");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdCuentaPadre).HasColumnName("id_cuenta_padre");
            entity.Property(e => e.IdTipoCuenta).HasColumnName("id_tipo_cuenta");
            entity.Property(e => e.Nivel).HasColumnName("nivel");
            entity.Property(e => e.PermiteMovimiento)
                .HasDefaultValue(false)
                .HasColumnName("permite_movimiento");

            entity.HasOne(d => d.IdCuentaPadreNavigation).WithMany(p => p.InverseIdCuentaPadreNavigation)
                .HasForeignKey(d => d.IdCuentaPadre)
                .HasConstraintName("cuentas_contables_id_cuenta_padre_fkey");

            entity.HasOne(d => d.IdTipoCuentaNavigation).WithMany(p => p.CuentasContables)
                .HasForeignKey(d => d.IdTipoCuenta)
                .HasConstraintName("cuentas_contables_id_tipo_cuenta_fkey");
        });

        modelBuilder.Entity<Moneda>(entity =>
        {
            entity.HasKey(e => e.IdMoneda).HasName("monedas_pkey");

            entity.ToTable("monedas");

            entity.HasIndex(e => e.CodigoIso, "monedas_codigo_iso_key").IsUnique();

            entity.Property(e => e.IdMoneda).HasColumnName("id_moneda");
            entity.Property(e => e.CodigoIso)
                .HasMaxLength(3)
                .HasColumnName("codigo_iso");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
            entity.Property(e => e.TasaCambio)
                .HasPrecision(18, 4)
                .HasDefaultValueSql("1.0000")
                .HasColumnName("tasa_cambio");
        });

        modelBuilder.Entity<TiposCuentum>(entity =>
        {
            entity.HasKey(e => e.IdTipoCuenta).HasName("tipos_cuenta_pkey");

            entity.ToTable("tipos_cuenta");

            entity.Property(e => e.IdTipoCuenta).HasColumnName("id_tipo_cuenta");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
            entity.Property(e => e.Origen)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("origen");
        });

        modelBuilder.Entity<TiposDocumento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDocumento).HasName("tipos_documento_pkey");

            entity.ToTable("tipos_documento");

            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(50)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Activo'::character varying")
                .HasColumnName("estado");
            entity.Property(e => e.IdCuentaContable).HasColumnName("id_cuenta_contable");
            entity.Property(e => e.TipoMovimientoEsperado)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasDefaultValueSql("'DB'::bpchar")
                .HasColumnName("tipo_movimiento_esperado");
            entity.Property(e => e.AplicaItbis)
                .HasDefaultValue(true)
                .HasColumnName("aplica_itbis");
            entity.Property(e => e.TasaItbis)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("18.00")
                .HasColumnName("tasa_itbis");

            entity.HasOne(d => d.IdCuentaContableNavigation).WithMany(p => p.TiposDocumentos)
                .HasForeignKey(d => d.IdCuentaContable)
                .HasConstraintName("tipos_documento_id_cuenta_contable_fkey");
        });

        modelBuilder.Entity<TransaccionesCxc>(entity =>
        {
            entity.HasKey(e => e.IdTransaccion).HasName("transacciones_cxc_pkey");

            entity.ToTable("transacciones_cxc");

            entity.HasIndex(e => e.IdCliente, "idx_transacciones_cliente");

            entity.HasIndex(e => e.FechaTransaccion, "idx_transacciones_fecha");

            entity.Property(e => e.IdTransaccion).HasColumnName("id_transaccion");
            entity.Property(e => e.FechaTransaccion)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("fecha_transaccion");
            entity.Property(e => e.IdAsientoGenerado).HasColumnName("id_asiento_generado");
            entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
            entity.Property(e => e.IdTipoDocumento).HasColumnName("id_tipo_documento");
            entity.Property(e => e.Monto)
                .HasPrecision(18, 2)
                .HasColumnName("monto");
            entity.Property(e => e.NumeroDocumento)
                .HasMaxLength(50)
                .HasColumnName("numero_documento");
            entity.Property(e => e.TipoMovimiento)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("tipo_movimiento");

            entity.HasOne(d => d.IdAsientoGeneradoNavigation).WithMany(p => p.TransaccionesCxcs)
                .HasForeignKey(d => d.IdAsientoGenerado)
                .HasConstraintName("transacciones_cxc_id_asiento_generado_fkey");

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.TransaccionesCxcs)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("transacciones_cxc_id_cliente_fkey");

            entity.HasOne(d => d.IdTipoDocumentoNavigation).WithMany(p => p.TransaccionesCxcs)
                .HasForeignKey(d => d.IdTipoDocumento)
                .HasConstraintName("transacciones_cxc_id_tipo_documento_fkey");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("usuarios_pkey");

            entity.ToTable("usuarios");

            entity.HasIndex(e => e.Username, "usuarios_username_key").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.IdAuxiliar).HasColumnName("id_auxiliar");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_creacion");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");

            entity.HasOne(d => d.Auxiliar).WithMany()
                .HasForeignKey(d => d.IdAuxiliar)
                .HasConstraintName("usuarios_id_auxiliar_fkey");
        });

        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.IdConfiguracion).HasName("configuracion_sistema_pkey");

            entity.ToTable("configuracion_sistema");

            entity.HasIndex(e => e.Clave, "configuracion_sistema_clave_key").IsUnique();

            entity.Property(e => e.IdConfiguracion).HasColumnName("id_configuracion");
            entity.Property(e => e.Clave)
                .HasMaxLength(50)
                .HasColumnName("clave");
            entity.Property(e => e.Valor)
                .HasMaxLength(255)
                .HasColumnName("valor");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaModificacion)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("fecha_modificacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
