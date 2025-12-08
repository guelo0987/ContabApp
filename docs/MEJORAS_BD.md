# 📋 Mejoras Propuestas para la Base de Datos

## 1. Modificar tabla `tipos_documento`

```sql
ALTER TABLE tipos_documento
ADD COLUMN tipo_movimiento_esperado CHAR(2) DEFAULT 'DB' CHECK (tipo_movimiento_esperado IN ('DB', 'CR')),
ADD COLUMN aplica_itbis BOOLEAN DEFAULT TRUE,
ADD COLUMN tasa_itbis DECIMAL(5,2) DEFAULT 18.00;

-- Actualizar registros existentes
UPDATE tipos_documento 
SET tipo_movimiento_esperado = 'DB', aplica_itbis = TRUE 
WHERE descripcion ILIKE '%factura%' OR descripcion ILIKE '%nota de débito%';

UPDATE tipos_documento 
SET tipo_movimiento_esperado = 'CR', aplica_itbis = FALSE 
WHERE descripcion ILIKE '%recibo%' OR descripcion ILIKE '%nota de crédito%';
```

---

## 2. Modificar tabla `cuentas_contables`

```sql
ALTER TABLE cuentas_contables
ADD COLUMN codigo VARCHAR(20) UNIQUE;

-- Ejemplo de actualización
UPDATE cuentas_contables SET codigo = '1' WHERE id_cuenta_contable = 1; -- Activos
UPDATE cuentas_contables SET codigo = '1.1' WHERE id_cuenta_contable = 2; -- Activos Corrientes
UPDATE cuentas_contables SET codigo = '1.1.01' WHERE id_cuenta_contable = 3; -- Caja
-- etc...
```

---

## 3. Nueva tabla `configuracion_sistema`

```sql
CREATE TABLE configuracion_sistema (
    id_configuracion SERIAL PRIMARY KEY,
    clave VARCHAR(50) UNIQUE NOT NULL,
    valor VARCHAR(255) NOT NULL,
    descripcion VARCHAR(255),
    tipo VARCHAR(20) DEFAULT 'STRING', -- STRING, INTEGER, DECIMAL, BOOLEAN
    fecha_modificacion TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Datos iniciales
INSERT INTO configuracion_sistema (clave, valor, descripcion, tipo) VALUES
('CUENTA_CAJA_GENERAL', '3', 'ID de la cuenta de Caja General', 'INTEGER'),
('CUENTA_INGRESOS_VENTA', '13', 'ID de la cuenta de Ingresos por Ventas', 'INTEGER'),
('CUENTA_ITBIS_POR_PAGAR', '4', 'ID de la cuenta de ITBIS por Pagar', 'INTEGER'),
('TASA_ITBIS_DEFAULT', '18.00', 'Tasa de ITBIS por defecto (%)', 'DECIMAL'),
('MONEDA_DEFAULT', '1', 'ID de la moneda por defecto (DOP)', 'INTEGER'),
('EMPRESA_NOMBRE', 'Mi Empresa SRL', 'Nombre de la empresa', 'STRING'),
('EMPRESA_RNC', '123456789', 'RNC de la empresa', 'STRING');
```

---

## 4. Nueva tabla `periodos_contables`

```sql
CREATE TABLE periodos_contables (
    id_periodo SERIAL PRIMARY KEY,
    anio INTEGER NOT NULL,
    mes INTEGER NOT NULL CHECK (mes BETWEEN 1 AND 12),
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    estado VARCHAR(20) DEFAULT 'Abierto' CHECK (estado IN ('Abierto', 'Cerrado', 'Bloqueado')),
    fecha_cierre TIMESTAMP,
    usuario_cierre INTEGER REFERENCES usuarios(id_usuario),
    UNIQUE(anio, mes)
);

-- Generar períodos para 2025
INSERT INTO periodos_contables (anio, mes, fecha_inicio, fecha_fin)
SELECT 2025, m, 
       MAKE_DATE(2025, m, 1),
       (MAKE_DATE(2025, m, 1) + INTERVAL '1 month - 1 day')::DATE
FROM generate_series(1, 12) AS m;
```

---

## 5. Modificar tabla `clientes`

```sql
ALTER TABLE clientes
ADD COLUMN rnc VARCHAR(15),
ADD COLUMN tipo_cliente VARCHAR(20) DEFAULT 'Persona' CHECK (tipo_cliente IN ('Persona', 'Empresa')),
ADD COLUMN telefono VARCHAR(20),
ADD COLUMN email VARCHAR(100),
ADD COLUMN direccion TEXT;

-- Índice para búsqueda por RNC
CREATE UNIQUE INDEX idx_clientes_rnc ON clientes(rnc) WHERE rnc IS NOT NULL;
```

---

## 6. Modificar tabla `asientos_cabecera`

```sql
ALTER TABLE asientos_cabecera
ADD COLUMN numero_asiento INTEGER,
ADD COLUMN id_periodo INTEGER REFERENCES periodos_contables(id_periodo);

-- Crear secuencia por período (opcional, manejable por código)
-- El número de asiento sería consecutivo por período
```

---

## 7. Agregar relación entre asientos y transacciones (opcional)

Actualmente `transacciones_cxc` tiene `id_asiento_generado`. Esto es correcto, pero podríamos agregar el campo inverso en asientos para trazabilidad:

```sql
ALTER TABLE asientos_cabecera
ADD COLUMN id_transaccion_origen INTEGER REFERENCES transacciones_cxc(id_transaccion);
```

---

## 📊 Diagrama ERD Actualizado

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           DIAGRAMA ENTIDAD-RELACIÓN                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌─────────────┐         ┌─────────────────┐         ┌─────────────────┐   │
│  │  USUARIOS   │────────▶│   AUXILIARES    │◀────────│ASIENTOS_CABECERA│   │
│  └─────────────┘         └─────────────────┘         └────────┬────────┘   │
│                                                               │            │
│                                                               ▼            │
│  ┌─────────────┐         ┌─────────────────┐         ┌─────────────────┐   │
│  │  CLIENTES   │◀───────▶│TRANSACCIONES_CXC│────────▶│ ASIENTOS_DETALLE│   │
│  └──────┬──────┘         └────────┬────────┘         └────────┬────────┘   │
│         │                         │                           │            │
│         │                         ▼                           ▼            │
│         │                ┌─────────────────┐         ┌─────────────────┐   │
│         └───────────────▶│ TIPOS_DOCUMENTO │         │CUENTAS_CONTABLES│   │
│                          └─────────────────┘         └────────┬────────┘   │
│                                                               │            │
│                                                               ▼            │
│  ┌─────────────┐                                     ┌─────────────────┐   │
│  │   MONEDAS   │                                     │  TIPOS_CUENTA   │   │
│  └─────────────┘                                     └─────────────────┘   │
│                                                                             │
│  ┌─────────────────────┐         ┌─────────────────────────────────────┐   │
│  │ PERIODOS_CONTABLES  │  (NEW)  │     CONFIGURACION_SISTEMA           │   │
│  └─────────────────────┘         └─────────────────────────────────────┘   │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## ✅ Resumen de Cambios

| Tabla | Acción | Prioridad |
|-------|--------|-----------|
| `tipos_documento` | Agregar campos ITBIS y movimiento | 🔴 Alta |
| `cuentas_contables` | Agregar código contable | 🟡 Media |
| `configuracion_sistema` | Crear tabla nueva | 🔴 Alta |
| `periodos_contables` | Crear tabla nueva | 🟡 Media |
| `clientes` | Agregar campos adicionales | 🟢 Baja |
| `asientos_cabecera` | Agregar número y período | 🟡 Media |
