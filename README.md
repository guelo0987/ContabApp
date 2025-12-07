# ContabApp - Sistema de Contabilidad con CxC

API REST completa en .NET 9 para gestión contable con módulo de Cuentas por Cobrar (CxC), autenticación JWT y generación automática de asientos contables con partida doble.

---

## 🏗️ Arquitectura del Backend

### Stack Tecnológico
- **.NET 9** (ASP.NET Core Web API)
- **PostgreSQL** (Base de datos)
- **Entity Framework Core 9** (ORM)
- **JWT Bearer** (Autenticación)
- **Swagger/OpenAPI** (Documentación)

### Estructura del Proyecto
```
ContabBackApp/
├── Controllers/     # Endpoints REST
├── Services/        # Lógica de negocio
├── DTOs/            # Objetos de transferencia
├── Models/          # Entidades de base de datos
├── Context/         # DbContext de EF Core
└── Migrations/      # Migraciones de BD
```

### Patrón de Arquitectura
- **3 Capas**: Controller → Service → Repository (EF Core)
- **DTOs** para request/response (no expone modelos internos)
- **Inyección de Dependencias** configurada en `Program.cs`
- **Validaciones** con Data Annotations
- **Transacciones** atómicas con rollback automático

---

## 🔐 Autenticación y Seguridad

### Flujo de Autenticación

1. **Registro** (opcional, solo para setup inicial):
   ```http
   POST /api/v1/auth/register
   {
     "username": "admin_cxc",
     "password": "tu_password",
     "idAuxiliar": 5
   }
   ```

2. **Login** (obligatorio para todo):
   ```http
   POST /api/v1/auth/login
   {
     "username": "admin_cxc",
     "password": "tu_password"
   }
   ```

   **Respuesta exitosa:**
   ```json
   {
     "isOk": true,
     "message": "Authentication successful",
     "data": {
       "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
       "auth": { "username": "admin_cxc" },
       "auxiliarySystem": { "id": 5, "name": "Cuentas x Cobrar" }
     }
   }
   ```

3. **Uso del Token**:
   - Header: `Authorization: Bearer {token}`
   - **TODOS los endpoints requieren este header** (excepto auth/login y auth/register)
   - El token contiene claims: `id_usuario` e `id_auxiliar`

### Claims JWT Importantes
- `id_usuario`: ID del usuario autenticado
- `id_auxiliar`: ID del sistema/módulo (ej: 5 = CxC)
- `sub`: Username

---

## 📋 Endpoints Disponibles

### Base URL
```
http://localhost:5160
```

### Swagger UI
```
http://localhost:5160/
```

---

## 1️⃣ Módulo de Autenticación

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/auth/register` | Registrar usuario | ❌ |
| POST | `/api/v1/auth/login` | Iniciar sesión | ❌ |

---

## 2️⃣ Módulo de Clientes

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/clientes` | Listar todos los clientes | ✅ |
| POST | `/api/v1/clientes` | Crear nuevo cliente | ✅ |

### Ejemplo: Crear Cliente
```json
POST /api/v1/clientes
{
  "nombre": "Juan Pérez",
  "cedula": "00112345678",
  "limiteCredito": 50000.00
}
```

**Validaciones automáticas:**
- Cédula única (no duplicada)
- Límite de crédito mínimo 0

---

## 3️⃣ Módulo de Tipos de Documento

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/tiposdocumento` | Listar tipos de documento | ✅ |
| POST | `/api/v1/tiposdocumento` | Crear tipo de documento | ✅ |

### Ejemplo: Crear Tipo de Documento
```json
POST /api/v1/tiposdocumento
{
  "descripcion": "Factura de Venta",
  "idCuentaContable": 5
}
```

**Validaciones automáticas:**
- La cuenta contable debe existir
- La cuenta debe permitir movimientos (no puede ser cuenta padre)

---

## 4️⃣ Módulo de Transacciones CxC (⭐ MOTOR PRINCIPAL)

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| POST | `/api/v1/transacciones` | Registrar venta o cobro | ✅ |

### Tipos de Movimiento
- **"DB"** (Débito): Factura/Venta → Aumenta la deuda del cliente
- **"CR"** (Crédito): Recibo/Cobro → Disminuye la deuda del cliente

### Ejemplo: Registrar Venta (Factura)
```json
POST /api/v1/transacciones
{
  "idCliente": 1,
  "idTipoDocumento": 1,
  "numeroDocumento": "B01-00001",
  "tipoMovimiento": "DB",
  "monto": 11800.00,
  "concepto": "Venta de productos"
}
```

**Respuesta:**
```json
{
  "idTransaccion": 1,
  "idAsientoGenerado": 1,
  "mensaje": "Transacción guardada y contabilizada correctamente."
}
```

### Ejemplo: Registrar Cobro (Recibo)
```json
POST /api/v1/transacciones
{
  "idCliente": 1,
  "idTipoDocumento": 2,
  "numeroDocumento": "R01-00001",
  "tipoMovimiento": "CR",
  "monto": 5000.00,
  "concepto": "Abono a factura B01-00001"
}
```

### ⚙️ Lógica Automática del Motor Transaccional

#### Para Ventas ("DB"):
El sistema **automáticamente desglosa el ITBIS (18%)**:

```
Monto Total: $11,800
├─ Ingreso Real: $10,000 (11800 / 1.18)
└─ ITBIS (Impuesto): $1,800 (a pagar al gobierno)
```

**Asiento Contable Generado:**
```
DEBE  | Cuentas por Cobrar    | $11,800
HABER | Ingresos por Ventas   | $10,000
HABER | ITBIS por Pagar       | $ 1,800
```

#### Para Cobros ("CR"):
```
DEBE  | Caja General          | $5,000
HABER | Cuentas por Cobrar    | $5,000
```

### Validaciones Automáticas
1. ✅ Cliente existe y está activo
2. ✅ Tipo de documento existe
3. ✅ **Límite de crédito** (solo para ventas):
   - Saldo actual + nueva venta ≤ límite de crédito
   - Si se excede, rechaza la venta
4. ✅ **Partida doble**: Débitos = Créditos (tolerancia: 0.01)
5. ✅ **Transaccionalidad**: Si algo falla, rollback automático

---

## 5️⃣ Módulo de Reportes

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/reportes/estado-cuenta/{idCliente}` | Estado de cuenta del cliente | ✅ |
| GET | `/api/v1/reportes/diario?desde={fecha}&hasta={fecha}` | Diario general contable | ✅ |

### Ejemplo: Estado de Cuenta
```http
GET /api/v1/reportes/estado-cuenta/1
```

**Respuesta:**
```json
{
  "idCliente": 1,
  "nombreCliente": "Juan Pérez",
  "saldoTotal": 6800.00,
  "movimientos": [
    {
      "fecha": "2024-01-15",
      "tipoDoc": "Factura de Venta",
      "numero": "B01-00001",
      "debito": 11800.00,
      "credito": 0,
      "idAsientoRef": 1
    },
    {
      "fecha": "2024-01-20",
      "tipoDoc": "Recibo de Cobro",
      "numero": "R01-00001",
      "debito": 0,
      "credito": 5000.00,
      "idAsientoRef": 2
    }
  ]
}
```

### Ejemplo: Diario General
```http
GET /api/v1/reportes/diario?desde=2024-01-01&hasta=2024-12-31
```

**Respuesta:**
```json
[
  {
    "idAsiento": 1,
    "fecha": "2024-01-15",
    "descripcion": "Factura de Venta No. B01-00001",
    "origen": "Cuentas x Cobrar",
    "estaCuadrado": true,
    "detalles": [
      { "cuenta": "5 - Cuentas por Cobrar", "debito": 11800.00, "credito": 0 },
      { "cuenta": "13 - Ingresos por Ventas", "debito": 0, "credito": 10000.00 },
      { "cuenta": "4 - ITBIS por Pagar", "debito": 0, "credito": 1800.00 }
    ]
  }
]
```

---

## 6️⃣ Módulo de Catálogos (Mantenimientos)

| Método | Endpoint | Descripción | Auth |
|--------|----------|-------------|------|
| GET | `/api/v1/auxiliares` | Listar auxiliares (módulos) | ✅ |
| POST | `/api/v1/auxiliares` | Crear auxiliar | ✅ |
| GET | `/api/v1/monedas` | Listar monedas | ✅ |
| POST | `/api/v1/monedas` | Crear moneda | ✅ |
| GET | `/api/v1/tipos-cuenta` | Listar tipos de cuenta | ✅ |
| POST | `/api/v1/tipos-cuenta` | Crear tipo de cuenta | ✅ |
| GET | `/api/v1/cuentas-contables` | Listar cuentas contables | ✅ |
| POST | `/api/v1/cuentas-contables` | Crear cuenta contable | ✅ |

### Ejemplo: Crear Cuenta Contable
```json
POST /api/v1/cuentas-contables
{
  "descripcion": "Banco Popular - Cuenta Corriente",
  "permiteMovimiento": true,
  "idTipoCuenta": 1,
  "idCuentaPadre": 3
}
```

**Lógica de Niveles:**
- Sin padre → Nivel 1 (Cuenta padre)
- Con padre → Nivel = Nivel padre + 1
- No se puede crear subcuenta bajo una cuenta que permite movimientos

---

## 📊 Modelo de Datos Simplificado

### Entidades Principales

```
Usuarios
├─ IdUsuario (PK)
├─ Username
├─ Password (encriptado)
├─ IdAuxiliar (FK → Auxiliares)
└─ Activo

Clientes
├─ IdCliente (PK)
├─ Nombre
├─ Cedula (único)
├─ LimiteCredito
└─ Estado

TiposDocumento
├─ IdTipoDocumento (PK)
├─ Descripcion
├─ IdCuentaContable (FK)
└─ Estado

TransaccionesCxc
├─ IdTransaccion (PK)
├─ IdCliente (FK)
├─ IdTipoDocumento (FK)
├─ NumeroDocumento
├─ TipoMovimiento (DB/CR)
├─ Monto
├─ FechaTransaccion
└─ IdAsientoGenerado (FK)

AsientosCabecera
├─ IdAsiento (PK)
├─ Descripcion
├─ IdAuxiliar (FK)
├─ FechaAsiento
├─ IdCliente (FK)
└─ Estado

AsientosDetalle
├─ IdAsientoDetalle (PK)
├─ IdAsiento (FK)
├─ IdCuentaContable (FK)
├─ TipoMovimiento (DB/CR)
└─ Monto

CuentasContables
├─ IdCuentaContable (PK)
├─ Descripcion
├─ PermiteMovimiento
├─ IdTipoCuenta (FK)
├─ IdCuentaPadre (FK)
├─ Nivel
└─ Balance
```

---

## 🎯 Flujo de Trabajo Completo (Para el Frontend)

### 1. Setup Inicial (Admin)
```mermaid
1. Crear Tipos de Cuenta (Activo, Pasivo, Ingreso, etc.)
2. Crear Catálogo de Cuentas Contables (Plan de Cuentas)
3. Crear Tipos de Documento (Factura, Recibo, etc.)
```

### 2. Operación Diaria
```mermaid
Login → Crear/Ver Clientes → Registrar Ventas/Cobros → Ver Reportes
```

### 3. Pantallas Recomendadas para el Frontend

#### Pantalla 1: Login
- Formulario: username, password
- Guardar token en localStorage/sessionStorage
- Redirigir a Dashboard

#### Pantalla 2: Dashboard Principal
- Resumen: Total de clientes, ventas del día, cobros del día
- Gráfico: Ventas vs Cobros (últimos 7 días)
- Acceso rápido: Nueva venta, Nuevo cobro

#### Pantalla 3: Gestión de Clientes
- Tabla con: Nombre, Cédula, Límite, Saldo Actual, Estado
- Botón: Nuevo Cliente
- Acción: Ver Estado de Cuenta (modal o nueva página)

#### Pantalla 4: Registrar Transacción
- **Modo: Venta (DB)**
  - Select: Cliente
  - Select: Tipo de Documento
  - Input: Número de Documento
  - Input: Monto (automáticamente calcula ITBIS)
  - Textarea: Concepto (opcional)
  
- **Modo: Cobro (CR)**
  - Select: Cliente
  - Mostrar: Saldo actual del cliente
  - Select: Tipo de Documento
  - Input: Número de Documento
  - Input: Monto
  - Textarea: Concepto

#### Pantalla 5: Estado de Cuenta
- Header: Datos del cliente + Saldo Total
- Tabla de movimientos:
  - Fecha | Tipo | Número | Débito | Crédito | Saldo
- Botón: Imprimir/Exportar PDF

#### Pantalla 6: Diario General
- Filtros: Fecha Desde/Hasta
- Lista de asientos con collapse:
  - Cabecera: Fecha, Descripción, Origen
  - Detalle: Tabla con Cuenta | Débito | Crédito
  - Indicador visual: ✅ Cuadrado / ❌ Descuadrado

#### Pantalla 7: Mantenimientos (Admin)
- Tabs:
  - Tipos de Documento
  - Cuentas Contables (vista árbol)
  - Monedas
  - Tipos de Cuenta

---

## 🔴 Errores Comunes y Manejo

### Respuestas de Error Estándar

```json
{
  "error": "Descripción del error"
}
```

### Códigos HTTP
- `200 OK`: Operación exitosa
- `201 Created`: Recurso creado
- `400 Bad Request`: Datos inválidos o regla de negocio violada
- `401 Unauthorized`: Token inválido o faltante
- `404 Not Found`: Recurso no encontrado
- `500 Internal Server Error`: Error del servidor

### Ejemplos de Errores de Negocio

```json
// Cédula duplicada
{ "error": "El cliente con cédula 00112345678 ya existe." }

// Límite de crédito excedido
{ "error": "Crédito excedido. Límite: $50,000.00, Saldo Actual: $45,000.00, Intento: $11,800.00" }

// Cliente inactivo
{ "error": "El cliente está inactivo y no puede operar." }

// Cuenta contable inválida
{ "error": "La cuenta contable no existe o es una cuenta padre (no permite movimientos)." }

// Token inválido
{ "error": "El token es inválido o no tiene permisos de auxiliar." }
```

---

## 🎨 Consideraciones de UX para el Frontend

### Validaciones en Tiempo Real
1. **Límite de crédito**: Al seleccionar cliente, mostrar saldo disponible
2. **Formato de cédula**: Validar 11 dígitos (República Dominicana)
3. **Formato de número de documento**: Sugerir formato (ej: B01-00001)
4. **Cálculo de ITBIS**: Mostrar desglose automático al escribir monto

### Retroalimentación Visual
- ✅ Transacción exitosa → Mostrar ID de asiento generado
- ⚠️ Advertencia → Cliente cerca del límite de crédito (>90%)
- ❌ Error → Mostrar mensaje claro del backend

### Performance
- **Caché**: Lista de clientes, tipos de documento (cambio poco)
- **Paginación**: Tabla de movimientos si hay >100 registros
- **Lazy Load**: Cargar reportes bajo demanda

---

## 🚀 Variables de Entorno Necesarias (Para el Backend)

El frontend no necesita estas variables, solo para referencia:

```env
DATA_BASE_CONNECTION_STRING=Host=localhost;Port=5432;Database=CONTAB;Username=postgres;Password=***
JWT_KEY=***
JWT_ISSUER=ContabBackAPI
JWT_AUDIENCE=ContabBackClient
PORT=5160
```

---

## 📝 Notas Finales para el Prompt de Lovable

### Características Técnicas del Frontend Deseado:
1. **Framework**: React + TypeScript (preferido por Lovable)
2. **Estilos**: Tailwind CSS o shadcn/ui
3. **Estado**: Context API o Zustand (para auth y datos globales)
4. **HTTP Client**: Axios o Fetch con interceptors para JWT
5. **Routing**: React Router
6. **Forms**: React Hook Form + Zod (validaciones)
7. **Tablas**: TanStack Table (react-table)
8. **Gráficos**: Recharts o Chart.js
9. **Iconos**: Lucide React o Heroicons

### Librerías Sugeridas:
```json
{
  "axios": "^1.6.0",
  "react-router-dom": "^6.20.0",
  "react-hook-form": "^7.48.0",
  "zod": "^3.22.0",
  "@tanstack/react-table": "^8.10.0",
  "recharts": "^2.10.0",
  "date-fns": "^3.0.0",
  "lucide-react": "^0.300.0"
}
```

### Estructura de Carpetas Sugerida:
```
src/
├── api/           # Servicios HTTP
├── components/    # Componentes reutilizables
├── contexts/      # Context API (AuthContext)
├── hooks/         # Custom hooks
├── pages/         # Páginas/Vistas
├── types/         # Tipos TypeScript
└── utils/         # Utilidades
```

---

## 📞 Soporte

Para dudas sobre el backend, revisar:
- Swagger UI: `http://localhost:5160/`
- Logs de consola del backend
- Respuestas de error detalladas

---

**Versión del API**: 1.0.0  
**Última actualización**: Diciembre 2024
