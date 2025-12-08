# 📚 API de Sistema Contable - Documentación para Frontend

Backend para gestión de **Cuentas por Cobrar (CxC)** y **Contabilidad General** con validaciones inteligentes.

**Base URL:** `http://localhost:5000/api/v1`

**Autenticación:** Todos los endpoints requieren token JWT en header:
```
Authorization: Bearer <token>
```

---

## 🔐 1. Autenticación

### Login
```http
POST /api/v1/auth/login
```

**Request:**
```json
{
  "username": "admin",
  "password": "123456"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "idAuxiliar": 1
}
```

---

## 👥 2. Clientes

### Obtener todos los clientes
```http
GET /api/v1/clientes
```

**Response:**
```json
[
  {
    "idCliente": 1,
    "nombre": "Pedro Martínez",
    "cedula": "001-1234567-8",
    "rnc": null,
    "tipoCliente": "Persona",
    "telefono": "809-555-1234",
    "email": "pedro@email.com",
    "direccion": "Calle Principal #123",
    "limiteCredito": 50000.00,
    "estado": "Activo"
  }
]
```

### Obtener cliente por ID
```http
GET /api/v1/clientes/{id}
```

### Crear cliente
```http
POST /api/v1/clientes
```

**Request:**
```json
{
  "nombre": "Empresa ABC SRL",
  "cedula": null,
  "rnc": "123456789",
  "tipoCliente": "Empresa",
  "telefono": "809-555-0000",
  "email": "contacto@empresaabc.com",
  "direccion": "Av. Comercial #456",
  "limiteCredito": 100000.00
}
```

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `nombre` | string | ✅ | Nombre completo o razón social |
| `cedula` | string | ❌ | Cédula (para personas) |
| `rnc` | string | ❌ | RNC (para empresas) |
| `tipoCliente` | string | ❌ | `"Persona"` o `"Empresa"` (default: Persona) |
| `telefono` | string | ❌ | Teléfono de contacto |
| `email` | string | ❌ | Correo electrónico |
| `direccion` | string | ❌ | Dirección física |
| `limiteCredito` | decimal | ✅ | Límite de crédito en DOP |

### Actualizar cliente
```http
PUT /api/v1/clientes/{id}
```

### Eliminar cliente
```http
DELETE /api/v1/clientes/{id}
```

**Nota:** No se puede eliminar si tiene transacciones asociadas.

---

## 💰 3. Transacciones (Motor Contable)

### Registrar Transacción (Venta o Cobro)
```http
POST /api/v1/transacciones
```

**Request para VENTA (Factura):**
```json
{
  "idCliente": 1,
  "idTipoDocumento": 1,
  "numeroDocumento": "B01-00001",
  "tipoMovimiento": "DB",
  "monto": 11800.00,
  "concepto": "Venta de productos"
}
```

**Request para COBRO (Recibo):**
```json
{
  "idCliente": 1,
  "idTipoDocumento": 5,
  "numeroDocumento": "R-001",
  "tipoMovimiento": "CR",
  "monto": 5000.00,
  "concepto": "Abono a factura"
}
```

| Campo | Tipo | Valores | Descripción |
|-------|------|---------|-------------|
| `idCliente` | int | - | ID del cliente |
| `idTipoDocumento` | int | - | ID del tipo de documento |
| `numeroDocumento` | string | - | Número de factura/recibo |
| `tipoMovimiento` | string | `"DB"` / `"CR"` | DB=Venta, CR=Cobro |
| `monto` | decimal | > 0 | Monto de la transacción |
| `concepto` | string | - | Descripción (opcional) |

**Response (200):**
```json
{
  "idTransaccion": 1,
  "idAsientoGenerado": 3,
  "mensaje": "Transacción guardada y contabilizada correctamente."
}
```

**Errores comunes:**
```json
// Límite de crédito excedido
{ "error": "Riesgo Financiero: La operación excede el límite de crédito del cliente..." }

// Cobro mayor a la deuda
{ "error": "Error de Lógica: El cobro de $1,000.00 excede la deuda actual del cliente ($500.00)..." }

// Documento incorrecto para la operación
{ "error": "Incoherencia: El documento 'Factura' está configurado para Venta (Débito), pero estás intentando usarlo como Cobro (Crédito)." }
```

---

### Consultar Saldo del Cliente
```http
GET /api/v1/transacciones/saldo/{idCliente}
```

**Response:**
```json
{
  "idCliente": 1,
  "nombreCliente": "Pedro Martínez",
  "saldoActual": 6800.00,
  "limiteCredito": 50000.00,
  "creditoDisponible": 43200.00,
  "cantidadFacturas": 2,
  "cantidadPagos": 1
}
```

---

### Historial de Transacciones del Cliente
```http
GET /api/v1/transacciones/historial/{idCliente}
```

**Response:**
```json
[
  {
    "idTransaccion": 1,
    "tipoMovimiento": "DB",
    "tipoDocumento": "Factura de Crédito Fiscal (B01)",
    "numeroDocumento": "B01-00001",
    "fecha": "2025-12-06",
    "monto": 9000.00,
    "saldoAcumulado": 9000.00
  },
  {
    "idTransaccion": 2,
    "tipoMovimiento": "CR",
    "tipoDocumento": "Recibo de Ingreso",
    "numeroDocumento": "R-001",
    "fecha": "2025-12-07",
    "monto": 2200.00,
    "saldoAcumulado": 6800.00
  }
]
```

---

## 📄 4. Tipos de Documento

### Obtener todos
```http
GET /api/v1/TiposDocumento
```

**Response:**
```json
[
  {
    "idTipoDocumento": 1,
    "descripcion": "Factura de Crédito Fiscal (B01)",
    "idCuentaContable": 8,
    "nombreCuentaContable": "Cuentas x Cobrar Cliente",
    "estado": "Activo",
    "tipoMovimientoEsperado": "DB",
    "aplicaItbis": true,
    "tasaItbis": 18.00
  },
  {
    "idTipoDocumento": 5,
    "descripcion": "Recibo de Ingreso",
    "idCuentaContable": 8,
    "nombreCuentaContable": "Cuentas x Cobrar Cliente",
    "estado": "Activo",
    "tipoMovimientoEsperado": "CR",
    "aplicaItbis": false,
    "tasaItbis": 0.00
  }
]
```

### Filtrar por tipo de movimiento (⭐ IMPORTANTE para el frontend)
```http
GET /api/v1/TiposDocumento/por-movimiento/{tipoMovimiento}
```

| Endpoint | Uso en Frontend |
|----------|-----------------|
| `/por-movimiento/DB` | Dropdown de **Venta (Factura)** |
| `/por-movimiento/CR` | Dropdown de **Cobro (Recibo)** |

**Ejemplo de uso en React:**
```javascript
// Cuando el usuario selecciona "Venta"
const documentosVenta = await fetch('/api/v1/TiposDocumento/por-movimiento/DB');
// → Muestra: Factura B01, Factura B02, Nota de Débito

// Cuando el usuario selecciona "Cobro"
const documentosCobro = await fetch('/api/v1/TiposDocumento/por-movimiento/CR');
// → Muestra: Recibo de Ingreso, Nota de Crédito
```

### Crear tipo de documento
```http
POST /api/v1/TiposDocumento
```

**Request:**
```json
{
  "descripcion": "Factura de Consumo (B02)",
  "idCuentaContable": 8,
  "tipoMovimientoEsperado": "DB",
  "aplicaItbis": true,
  "tasaItbis": 18.00
}
```

---

## 📊 5. Cuentas Contables

### Obtener todas
```http
GET /api/v1/cuentas-contables
```

**Response:**
```json
[
  {
    "idCuentaContable": 1,
    "codigo": "1",
    "descripcion": "Activos",
    "permiteMovimiento": false,
    "nivel": 1,
    "idTipoCuenta": 1,
    "idCuentaPadre": null,
    "balance": 0.00,
    "tipoCuentaDescripcion": "Activo",
    "origenCuenta": "DB"
  },
  {
    "idCuentaContable": 3,
    "codigo": "1.1.01",
    "descripcion": "Caja General",
    "permiteMovimiento": true,
    "nivel": 3,
    "idTipoCuenta": 1,
    "idCuentaPadre": 2,
    "balance": 15000.00,
    "tipoCuentaDescripcion": "Activo",
    "origenCuenta": "DB"
  }
]
```

### Obtener por ID
```http
GET /api/v1/cuentas-contables/{id}
```

### Crear cuenta contable
```http
POST /api/v1/cuentas-contables
```

**Request:**
```json
{
  "codigo": "1.1.02",
  "descripcion": "Banco Popular",
  "permiteMovimiento": true,
  "idTipoCuenta": 1,
  "idCuentaPadre": 2
}
```

| Campo | Descripción |
|-------|-------------|
| `codigo` | Código jerárquico (ej: "1.1.02") |
| `descripcion` | Nombre de la cuenta |
| `permiteMovimiento` | `true` si es cuenta de detalle |
| `idTipoCuenta` | 1=Activo, 2=Pasivo, etc. |
| `idCuentaPadre` | ID de la cuenta padre (opcional) |

### Actualizar
```http
PUT /api/v1/cuentas-contables/{id}
```

### Eliminar
```http
DELETE /api/v1/cuentas-contables/{id}
```

**Nota:** No se puede eliminar si tiene asientos o subcuentas.

---

## 🏷️ 6. Tipos de Cuenta

### Obtener todos
```http
GET /api/v1/tipos-cuenta
```

**Response:**
```json
[
  { "idTipoCuenta": 1, "descripcion": "Activo", "origen": "DB" },
  { "idTipoCuenta": 2, "descripcion": "Pasivo", "origen": "CR" },
  { "idTipoCuenta": 3, "descripcion": "Capital", "origen": "CR" },
  { "idTipoCuenta": 4, "descripcion": "Ingreso", "origen": "CR" },
  { "idTipoCuenta": 5, "descripcion": "Gasto", "origen": "DB" }
]
```

| Origen | Significado | Aumenta con | Disminuye con |
|--------|-------------|-------------|---------------|
| `DB` | Deudor | Débito | Crédito |
| `CR` | Acreedor | Crédito | Débito |

### CRUD completo
```http
GET    /api/v1/tipos-cuenta/{id}
POST   /api/v1/tipos-cuenta
PUT    /api/v1/tipos-cuenta/{id}
DELETE /api/v1/tipos-cuenta/{id}
```

---

## 💱 7. Monedas

### Obtener todas
```http
GET /api/v1/monedas
```

**Response:**
```json
[
  { "idMoneda": 1, "codigoIso": "DOP", "descripcion": "Peso Dominicano", "tasaCambio": 1.0000 },
  { "idMoneda": 2, "codigoIso": "USD", "descripcion": "Dólar Estadounidense", "tasaCambio": 58.5000 }
]
```

### CRUD completo
```http
GET    /api/v1/monedas/{id}
POST   /api/v1/monedas
PUT    /api/v1/monedas/{id}
DELETE /api/v1/monedas/{id}
```

---

## 👤 8. Auxiliares (Módulos del Sistema)

### Obtener todos
```http
GET /api/v1/auxiliares
```

**Response:**
```json
[
  { "idAuxiliar": 1, "descripcion": "Contabilidad General", "activo": true },
  { "idAuxiliar": 2, "descripcion": "Cuentas por Cobrar", "activo": true },
  { "idAuxiliar": 3, "descripcion": "Cuentas por Pagar", "activo": true }
]
```

### CRUD completo
```http
GET    /api/v1/auxiliares/{id}
POST   /api/v1/auxiliares
PUT    /api/v1/auxiliares/{id}
DELETE /api/v1/auxiliares/{id}
```

---

## 📈 9. Reportes

### Dashboard (Métricas del Sistema)
```http
GET /api/v1/reportes/dashboard
```

**Response:**
```json
{
  "totalClientes": 4,
  "clientesActivos": 4,
  "debitosDelDia": 5000.00,
  "creditosDelDia": 1500.00,
  "saldoCxCTotal": 6299.00,
  "asientosDelDia": 2,
  "totalFacturas": 3,
  "totalPagos": 3,
  "topClientesDeudores": [
    {
      "idCliente": 3,
      "nombre": "María García",
      "saldoPendiente": 3500.00,
      "cantidadFacturas": 1
    }
  ]
}
```

| Campo | Descripción |
|-------|-------------|
| `totalClientes` | Total de clientes en el sistema |
| `clientesActivos` | Clientes con estado "Activo" |
| `debitosDelDia` | Suma de facturas emitidas HOY |
| `creditosDelDia` | Suma de cobros recibidos HOY |
| `saldoCxCTotal` | Total pendiente por cobrar (todas las fechas) |
| `asientosDelDia` | Cantidad de asientos contables generados HOY |
| `totalFacturas` | Total histórico de facturas |
| `totalPagos` | Total histórico de pagos |
| `topClientesDeudores` | Top 5 clientes con mayor deuda |

### Estado de Cuenta del Cliente
```http
GET /api/v1/reportes/estado-cuenta/{idCliente}
```

### Diario General
```http
GET /api/v1/reportes/diario?desde=2025-01-01&hasta=2025-12-31
```

---

## 🧮 Lógica Contable Automática

### Asiento de VENTA (Factura)
Cuando registras una venta con ITBIS 18%, el sistema genera automáticamente:

| Cuenta | Movimiento | Monto | Razón |
|--------|------------|-------|-------|
| **CxC Clientes** | DEBE | $11,800 | Cliente nos debe todo |
| **Ingresos x Venta** | HABER | $10,000 | Lo que realmente ganamos |
| **ITBIS por Pagar** | HABER | $1,800 | Impuesto (18%) |

### Asiento de COBRO (Recibo)
Cuando el cliente paga:

| Cuenta | Movimiento | Monto | Razón |
|--------|------------|-------|-------|
| **Caja General** | DEBE | $5,000 | Entra dinero |
| **CxC Clientes** | HABER | $5,000 | Baja la deuda |

---

## ⚠️ Validaciones Automáticas

| Validación | Mensaje de Error |
|------------|------------------|
| Cliente excede límite de crédito | `"Riesgo Financiero: La operación excede el límite de crédito..."` |
| Cobro mayor a la deuda | `"Error de Lógica: El cobro excede la deuda actual del cliente..."` |
| Documento incorrecto | `"Incoherencia: El documento 'Factura' está configurado para Venta..."` |
| Cuenta con origen incorrecto | `"Error Contable: La cuenta es de origen ACREEDOR..."` |
| Cliente inactivo | `"El cliente está inactivo y no puede operar."` |

---

## 🔧 Configuración del Sistema

Las cuentas contables por defecto se configuran en la tabla `configuracion_sistema`:

| Clave | Valor | Descripción |
|-------|-------|-------------|
| `CUENTA_CAJA_GENERAL` | 3 | Donde entra el dinero |
| `CUENTA_INGRESOS_VENTA` | 13 | Ingresos por ventas |
| `CUENTA_ITBIS_POR_PAGAR` | 4 | Impuesto por pagar |
| `TASA_ITBIS_DEFAULT` | 18.00 | Tasa de ITBIS % |
| `MONEDA_DEFAULT` | 1 | Moneda por defecto (DOP) |

---

## 📱 Ejemplo de Flujo en Frontend

### 1. Pantalla de Nueva Transacción

```javascript
// 1. Cargar clientes
const clientes = await fetch('/api/v1/clientes');

// 2. Usuario selecciona tipo de operación
const tipoOperacion = 'Venta'; // o 'Cobro'

// 3. Cargar documentos según operación
const tipoMov = tipoOperacion === 'Venta' ? 'DB' : 'CR';
const documentos = await fetch(`/api/v1/TiposDocumento/por-movimiento/${tipoMov}`);

// 4. Usuario selecciona cliente
const clienteId = 1;

// 5. Mostrar saldo y límite
const saldo = await fetch(`/api/v1/transacciones/saldo/${clienteId}`);
// → Mostrar: "Saldo: $6,800 | Límite: $50,000 | Disponible: $43,200"

// 6. Usuario completa formulario y envía
const transaccion = {
  idCliente: 1,
  idTipoDocumento: documentos[0].idTipoDocumento,
  numeroDocumento: 'B01-00002',
  tipoMovimiento: tipoMov,
  monto: 5000.00,
  concepto: 'Venta de productos'
};

const resultado = await fetch('/api/v1/transacciones', {
  method: 'POST',
  headers: { 
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
  },
  body: JSON.stringify(transaccion)
});

// 7. Mostrar resultado
if (resultado.ok) {
  const data = await resultado.json();
  alert(`✅ ${data.mensaje}\nAsiento #${data.idAsientoGenerado}`);
} else {
  const error = await resultado.json();
  alert(`❌ ${error.error}`);
}
```

---

## 📋 Códigos de Estado HTTP

| Código | Significado |
|--------|-------------|
| `200` | Éxito |
| `201` | Creado exitosamente |
| `204` | Eliminado exitosamente |
| `400` | Error de validación o negocio |
| `401` | No autenticado (token inválido) |
| `404` | Recurso no encontrado |
| `500` | Error interno del servidor |

---

## 🚀 Inicio Rápido

```bash
# Backend
cd ContabBack
dotnet run --project ContabBackApp

# El API estará en: http://localhost:5000
```

**Swagger UI:** `http://localhost:5000/swagger`
