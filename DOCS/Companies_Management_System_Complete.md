# ?? **Sistema Completo de Gestión de Empresas - IMPLEMENTADO**

## ?? **COMPLETADO - MÓDULO LISTO**

---

## ?? **Resumen**

Se ha implementado un **sistema completo de gestión de empresas** con:

1. ? **CRUD completo** (Crear, Leer, Actualizar, Desactivar)
2. ? **Patrón CQRS** (Commands + Queries + Handlers)
3. ? **Configuración fiscal** (RFC, Régimen SAT, CFDI)
4. ? **Multi-empresa** con empresa principal/matriz
5. ? **Relación con sucursales** (Branch -> Company)
6. ? **Generación automática de códigos**
7. ? **Validaciones robustas**
8. ? **Auditoría completa** (Creado por, Actualizado por)

---

## ?? **Endpoints Disponibles**

### **1. Crear Nueva Empresa**

```http
POST /api/companies
Authorization: Bearer {token}
Content-Type: application/json

{
  "legalName": "EXPANDA TECHNOLOGIES S.A. DE C.V.",
  "tradeName": "Expanda Tech",
  "taxId": "ETE190101ABC",
  "satTaxRegime": "601",
  "fiscalZipCode": "64000",
  "fiscalAddress": "AV. CONSTITUCIÓN #100, COL. CENTRO",
  "phone": "8181234567",
  "email": "contacto@expandatech.com",
  "website": "https://www.expandatech.com",
  "invoiceSeries": "A",
  "invoiceStartingFolio": 1,
  "defaultCurrency": "MXN",
  "logoUrl": "https://s3.amazonaws.com/logos/expanda.png",
  "slogan": "Innovación y Tecnología",
  "isMainCompany": true,
  "notes": "Empresa principal del grupo"
}
```

**Respuesta:**
```json
{
  "message": "Empresa creada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "COMP-001",
    "legalName": "EXPANDA TECHNOLOGIES S.A. DE C.V.",
    "tradeName": "Expanda Tech",
    "taxId": "ETE190101ABC",
    "satTaxRegime": "601",
    "fiscalZipCode": "64000",
    "fiscalAddress": "AV. CONSTITUCIÓN #100, COL. CENTRO",
    "phone": "8181234567",
    "email": "contacto@expandatech.com",
    "website": "https://www.expandatech.com",
    "invoiceSeries": "A",
    "invoiceStartingFolio": 1,
    "invoiceCurrentFolio": 1,
    "defaultCurrency": "MXN",
    "logoUrl": "https://s3.amazonaws.com/logos/expanda.png",
    "slogan": "Innovación y Tecnología",
    "isActive": true,
    "isMainCompany": true,
    "notes": "Empresa principal del grupo",
    "createdAt": "2026-03-11T10:30:00Z",
    "createdByUserName": "Admin"
  }
}
```

---

### **2. Actualizar Empresa**

```http
PUT /api/companies/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "legalName": "EXPANDA TECHNOLOGIES S.A. DE C.V.",
  "tradeName": "Expanda Tech Solutions",
  "satTaxRegime": "601",
  "fiscalZipCode": "64000",
  "fiscalAddress": "AV. CONSTITUCIÓN #200, COL. CENTRO",
  "phone": "8181234568",
  "email": "info@expandatech.com",
  "website": "https://www.expandatech.com",
  "invoiceSeries": "A",
  "defaultCurrency": "MXN",
  "logoUrl": "https://s3.amazonaws.com/logos/expanda-new.png",
  "slogan": "Tecnología de Vanguardia",
  "notes": "Actualización de datos fiscales"
}
```

---

### **3. Obtener Empresa por ID**

```http
GET /api/companies/1
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Empresa obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "COMP-001",
    "legalName": "EXPANDA TECHNOLOGIES S.A. DE C.V.",
    "tradeName": "Expanda Tech",
    ...
  }
}
```

---

### **4. Obtener Todas las Empresas Activas**

```http
GET /api/companies/active
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Empresas activas obtenidas exitosamente",
  "error": 0,
  "data": [
    {
      "id": 1,
      "code": "COMP-001",
      "legalName": "EXPANDA TECHNOLOGIES S.A. DE C.V.",
      "tradeName": "Expanda Tech",
      "isActive": true,
      ...
    }
  ]
}
```

---

### **5. Obtener Empresas Paginadas con Filtros**

```http
GET /api/companies?page=1&pageSize=20&isActive=true&searchTerm=expanda
Authorization: Bearer {token}
```

**Parámetros:**
| Parámetro | Tipo | Descripción | Default |
|-----------|------|-------------|---------|
| `page` | int | Número de página | 1 |
| `pageSize` | int | Tamaño de página | 20 |
| `isActive` | bool? | Filtrar por estado | null (todas) |
| `searchTerm` | string? | Búsqueda en código, nombre, RFC, email | null |

**Respuesta:**
```json
{
  "message": "Empresas obtenidas exitosamente",
  "error": 0,
  "data": [...],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 5,
  "totalPages": 1
}
```

---

### **6. Obtener Empresa Principal/Matriz**

```http
GET /api/companies/main
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Empresa principal obtenida exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "code": "COMP-001",
    "isMainCompany": true,
    ...
  }
}
```

---

### **7. Desactivar Empresa (Baja Lógica)**

```http
PUT /api/companies/2/deactivate
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Empresa desactivada exitosamente",
  "error": 0
}
```

**?? Restricciones:**
- No se puede desactivar la empresa principal (`IsMainCompany = true`)

---

### **8. Reactivar Empresa**

```http
PUT /api/companies/2/reactivate
Authorization: Bearer {token}
```

**Respuesta:**
```json
{
  "message": "Empresa reactivada exitosamente",
  "error": 0
}
```

---

### **9. Actualizar Configuración Fiscal**

```http
PUT /api/companies/1/fiscal-config
Authorization: Bearer {token}
Content-Type: application/json

{
  "satCertificatePath": "/certs/empresa.cer",
  "satKeyPath": "/certs/empresa.key",
  "satKeyPassword": "encrypted_password_here",
  "invoiceCurrentFolio": 150
}
```

**Respuesta:**
```json
{
  "message": "Configuración fiscal actualizada exitosamente",
  "error": 0,
  "data": {
    "id": 1,
    "invoiceCurrentFolio": 150,
    ...
  }
}
```

---

## ?? **Modelo de Datos**

### **Campos de la Entidad `Company`**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| `Id` | int | ID autogenerado | PK |
| `Code` | string(20) | Código único (COMP-001) | Unique, autogenerado |
| `LegalName` | string(300) | Razón social | Required |
| `TradeName` | string(200) | Nombre comercial | Required |
| `TaxId` | string(13) | RFC | Required, Unique, Regex |
| `SatTaxRegime` | string(10) | Régimen fiscal (601, 603, etc.) | Required |
| `FiscalZipCode` | string(10) | CP fiscal | Required, 5 dígitos |
| `FiscalAddress` | string(500) | Dirección fiscal | Required |
| `Phone` | string(20) | Teléfono | Required, formato |
| `Email` | string(100) | Email | Required, EmailAddress |
| `Website` | string(200) | Sitio web | URL válida |
| `InvoiceSeries` | string(10) | Serie de facturas (A, B, etc.) | Optional |
| `InvoiceStartingFolio` | int | Folio inicial | Default: 1 |
| `InvoiceCurrentFolio` | int | Folio actual | Default: 1 |
| `SatCertificatePath` | string(500) | Ruta certificado .cer | Optional |
| `SatKeyPath` | string(500) | Ruta llave .key | Optional |
| `SatKeyPassword` | string(500) | Password encriptado | Optional |
| `DefaultCurrency` | string(3) | Moneda (MXN, USD, EUR) | Default: MXN |
| `LogoUrl` | string(500) | URL del logo | Optional |
| `Slogan` | string(200) | Eslogan | Optional |
| `IsActive` | bool | Estado activo/inactivo | Default: true |
| `IsMainCompany` | bool | Es empresa principal | Default: false |
| `Notes` | string(1000) | Notas adicionales | Optional |
| `CreatedAt` | DateTime | Fecha de creación | Auto |
| `UpdatedAt` | DateTime? | Fecha de actualización | Auto |
| `CreatedByUserId` | int? | Usuario creador | FK Users |
| `UpdatedByUserId` | int? | Usuario actualizador | FK Users |

---

## ?? **Relaciones**

### **Company -> Branches** (1:N)
```
Una empresa puede tener múltiples sucursales
```

**Actualización en `Branch`:**
```csharp
public int? CompanyId { get; set; }
public virtual Company? Company { get; set; }
```

---

## ?? **Validaciones Implementadas**

### **Crear Empresa:**
- ? RFC único (no puede duplicarse)
- ? Formato de RFC válido (regex)
- ? Email válido
- ? Código postal de 5 dígitos
- ? URL de sitio web válida
- ? Moneda válida (MXN, USD, EUR)

### **Actualizar Empresa:**
- ? Empresa debe existir
- ? Validaciones de formato

### **Desactivar:**
- ? Empresa debe existir
- ? No puede estar ya inactiva
- ? No puede ser la empresa principal

---

## ?? **Archivos Creados**

### **Domain:**
1. ? `Domain/Entities/Company.cs`

### **Application:**
2. ? `Application/DTOs/Company/CompanyDtos.cs`
3. ? `Application/Abstractions/Config/ICompanyRepository.cs`
4. ? `Application/Core/Company/Commands/CompanyCommands.cs`
5. ? `Application/Core/Company/Queries/CompanyQueries.cs`
6. ? `Application/Core/Company/CommandHandlers/CompanyCommandHandlers.cs`
7. ? `Application/Core/Company/QueryHandlers/CompanyQueryHandlers.cs`

### **Infrastructure:**
8. ? `Infrastructure/Repositories/CompanyRepository.cs`
9. ? `Infrastructure/Migrations/20260311000000_CreateCompaniesTable.cs`

### **Web.Api:**
10. ? `Web.Api/Controllers/Config/CompaniesController.cs`

### **Modificados:**
11. ? `Domain/Entities/Branch.cs` (agregada relación con Company)
12. ? `Infrastructure/Persistence/POSDbContext.cs` (agregado DbSet)
13. ? `Web.Api/Program.cs` (registrado repositorio)

---

## ?? **Pruebas en Postman**

### **1. Crear empresa principal:**
```http
POST http://localhost:7254/api/companies
Authorization: Bearer {token}
```

### **2. Obtener empresas:**
```http
GET http://localhost:7254/api/companies?page=1&pageSize=10
Authorization: Bearer {token}
```

### **3. Obtener empresa principal:**
```http
GET http://localhost:7254/api/companies/main
Authorization: Bearer {token}
```

### **4. Actualizar empresa:**
```http
PUT http://localhost:7254/api/companies/1
Authorization: Bearer {token}
```

### **5. Desactivar empresa:**
```http
PUT http://localhost:7254/api/companies/2/deactivate
Authorization: Bearer {token}
```

---

## ?? **Para AWS/Producción**

### **Paso 1: Aplicar migración de base de datos**

```bash
# Opción A: Desde tu PC (Windows) conectado a AWS RDS
cd C:\Users\PCX\Source\Repos\API_POS
dotnet ef database update --project Infrastructure --startup-project Web.Api --connection "Server=TU-RDS-ENDPOINT;Database=ERP;User Id=admin;Password=TU-PASSWORD;TrustServerCertificate=true;"

# Opción B: En el servidor AWS (después de publicar)
cd /home/ubuntu/api/publish
dotnet ef database update
```

**?? Importante:** Asegúrate de que tu `appsettings.Production.json` tenga la cadena de conexión correcta.

### **Paso 2: Publicar código**
```bash
dotnet publish Web.Api/Web.Api.csproj -c Release -o ./publish --runtime linux-x64 --self-contained false
```

### **Paso 3: Subir a AWS**
```bash
scp -i tu-key.pem -r ./publish/* ubuntu@TU-IP:/home/ubuntu/api/publish
```

### **Paso 4: Reiniciar servicio**
```bash
ssh -i tu-key.pem ubuntu@TU-IP
sudo systemctl restart erpapi
```

---

## ?? **Migración Aplicada**

**Archivo:** `Infrastructure/Migrations/20260311000000_CreateCompaniesTable.cs`

**Qué hace:**
- ? Crea tabla `Companies` con todos los campos
- ? Crea índices únicos en `Code` y `TaxId`
- ? Crea índices de búsqueda
- ? Crea FKs a tabla `Users`
- ? Agrega columna `CompanyId` a tabla `Branches`
- ? Crea FK `Branches -> Companies`

**Rollback:** Si necesitas revertir:
```bash
dotnet ef database update <MigraciónAnterior> --project Infrastructure --startup-project Web.Api
```

---

## ?? **Permisos Requeridos**

Para usar los endpoints de empresas, el usuario debe tener:

**Módulo:** `Configuration`

**Permisos:**
- `View` - Ver empresas
- `ManageCompanies` - Crear/Actualizar/Desactivar empresas

---

## ? **Características Destacadas**

### **1. Generación Automática de Códigos**
```
COMP-001, COMP-002, COMP-003, ...
```

### **2. Validación de RFC**
```csharp
[RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-V1-9][A-Z0-9][0-9A]$")]
```

### **3. Empresa Principal/Matriz**
```
Solo puede haber una empresa marcada como IsMainCompany = true
No se puede desactivar
```

### **4. Configuración Fiscal Separada**
```
Endpoint dedicado para actualizar certificados SAT sin modificar otros datos
```

### **5. Búsqueda Inteligente**
```
Busca en: Código, Razón Social, Nombre Comercial, RFC, Email
```

---

## ?? **Casos de Uso**

### **1. Sistema Multi-Empresa**
Una empresa matriz con varias filiales que comparten el mismo sistema ERP.

### **2. Facturación Electrónica**
Configuración de certificados SAT y folios de facturación por empresa.

### **3. Gestión Fiscal**
Control de RFC, régimen fiscal y configuración CFDI por empresa.

### **4. Auditoría Completa**
Registro de quién creó/modificó cada empresa y cuándo.

---

## ?? **Estado Final**

- ? **9 endpoints** implementados
- ? **Patrón CQRS completo**
- ? **Validaciones robustas**
- ? **Multi-empresa soportada**
- ? **Relación con Sucursales**
- ? **Configuración fiscal**
- ? **Auditoría completa**
- ? **Listo para producción**

---

**?? SISTEMA DE EMPRESAS COMPLETAMENTE IMPLEMENTADO** ?

**Fecha:** 2026-03-11  
**Endpoints:** 9 endpoints completos  
**Patrón:** CQRS + Repository  
**Estado:** ? **LISTO PARA USAR**
