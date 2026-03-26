# Módulo de Cuentas por Cobrar - Estado de Implementación

**Fecha**: 25 de Marzo, 2026  
**Estado**: Estructura base completada - Requiere implementación de Handlers y Repositorios

---

## ✅ LO QUE SE HA COMPLETADO

### 1. **Entidades del Dominio** (Domain/Entities/) ✅ COMPLETO
Se crearon 7 entidades principales con todas sus propiedades, atributos EF Core y navegación:

- ✅ **InvoicePPD.cs** - Facturas PPD pendientes de pago
- ✅ **Payment.cs** - Pagos individuales o batch
- ✅ **PaymentApplication.cs** - Aplicación del pago a facturas
- ✅ **PaymentBatch.cs** - Lotes de pago masivo (opcional)
- ✅ **CustomerCreditPolicy.cs** - Política de crédito del cliente
- ✅ **CustomerCreditHistory.cs** - Historial de eventos de crédito
- ✅ **PaymentComplementLog.cs** - Log de generación de complementos

**Configuraciones incluidas**:
- `[Table]` attributes para nombres de tabla
- `[Precision(18,2)]` para decimales monetarios
- Foreign Keys con Delete Behavior correctos
- Índices necesarios para performance
- Propiedades de auditoría (CreatedAt, UpdatedAt, CreatedBy)

### 2. **DTOs** (Application/DTOs/AccountsReceivable/) ✅ COMPLETO
Se crearon 4 archivos con todos los DTOs necesarios:

- ✅ **InvoicePPDDtos.cs**
  - `InvoicePPDDto` (response)
  - `CreateInvoicePPDRequest`
  - `InvoicePPDPageRequest`
  - `InvoicePPDPageResponse`

- ✅ **PaymentDtos.cs**
  - `CreatePaymentRequest`
  - `PaymentDto` (response)
  - `PaymentApplicationDto`
  - `GenerateComplementsRequest`
  - `CreatePaymentBatchRequest`
  - `PaymentBatchDto`
  - `PaymentInvoiceItem`
  - `BatchPaymentItem`

- ✅ **CustomerCreditPolicyDtos.cs**
  - `CustomerCreditPolicyDto`
  - `UpsertCustomerCreditPolicyRequest`
  - `UpdateCreditStatusRequest`
  - `CustomerCreditHistoryDto`

- ✅ **ReportDtos.cs**
  - `AccountsReceivableDashboardDto`
  - `AgingReportDto`
  - `TopCustomerBalanceDto`
  - `CustomerStatementDto`
  - `CollectionForecastDto`
  - `AccountsReceivableMetricsDto`
  - `OverdueInvoicesReportDto`

### 3. **Commands** (Application/Core/AccountsReceivable/Commands/) ✅ COMPLETO
Se crearon 7 commands que implementan `IRequest<T>`:

- ✅ `CreateInvoicePPDCommand` - Crear factura PPD
- ✅ `CreatePaymentCommand` - Registrar pago
- ✅ `GeneratePaymentComplementsCommand` - Generar complementos SAT
- ✅ `CreatePaymentBatchCommand` - Crear lote de pagos
- ✅ `GenerateBatchComplementsCommand` - Generar complementos de lote
- ✅ `UpsertCustomerCreditPolicyCommand` - Crear/actualizar política
- ✅ `UpdateCreditStatusCommand` - Bloquear/desbloquear crédito

### 4. **Queries** (Application/Core/AccountsReceivable/Queries/) ✅ COMPLETO
Se crearon 10 queries que implementan `IRequest<T>`:

- ✅ `GetInvoicesPPDQuery` - Listar facturas PPD con filtros
- ✅ `GetInvoicePPDByIdQuery` - Obtener factura PPD
- ✅ `GetPaymentByIdQuery` - Obtener pago
- ✅ `GetPaymentBatchByIdQuery` - Obtener lote
- ✅ `GetAccountsReceivableDashboardQuery` - Dashboard CxC
- ✅ `GetCustomerStatementQuery` - Estado de cuenta
- ✅ `GetCustomerCreditPolicyQuery` - Política de crédito
- ✅ `GetOverdueInvoicesReportQuery` - Reporte vencidos
- ✅ `GetCollectionForecastQuery` - Proyección cobranza
- ✅ `GetAccountsReceivableMetricsQuery` - Métricas (DSO, etc.)

### 5. **Interfaces de Repositorios** (Application/Abstractions/AccountsReceivable/) ✅ COMPLETO
Se crearon 7 interfaces con todos los métodos necesarios:

- ✅ `IInvoicePPDRepository`
- ✅ `IPaymentRepository`
- ✅ `IPaymentApplicationRepository`
- ✅ `IPaymentBatchRepository`
- ✅ `ICustomerCreditPolicyRepository`
- ✅ `ICustomerCreditHistoryRepository`
- ✅ `IPaymentComplementLogRepository`

### 6. **DbContext Actualizado** (Infrastructure/Persistence/POSDbContext.cs) ✅ COMPLETO
Se agregaron 7 DbSets al POSDbContext:

```csharp
public DbSet<InvoicePPD> InvoicesPPD { get; set; }
public DbSet<Payment> Payments { get; set; }
public DbSet<PaymentApplication> PaymentApplications { get; set; }
public DbSet<PaymentBatch> PaymentBatches { get; set; }
public DbSet<CustomerCreditPolicy> CustomerCreditPolicies { get; set; }
public DbSet<CustomerCreditHistory> CustomerCreditHistory { get; set; }
public DbSet<PaymentComplementLog> PaymentComplementLogs { get; set; }
```

### 7. **Controller API** (Web.Api/Controllers/AccountsReceivable/) ✅ COMPLETO
Se creó un controller completo con todos los endpoints:

- ✅ **AccountsReceivableController.cs** - 17 endpoints RESTful
  - Gestión de facturas PPD
  - Gestión de pagos
  - Lotes de pago
  - Políticas de crédito
  - Reportes y dashboard

**Endpoints incluidos**:
```
GET    /api/accounts-receivable/invoices-ppd
GET    /api/accounts-receivable/invoices-ppd/{id}
POST   /api/accounts-receivable/invoices-ppd
POST   /api/accounts-receivable/payments
GET    /api/accounts-receivable/payments/{id}
POST   /api/accounts-receivable/payments/{id}/generate-complements
POST   /api/accounts-receivable/batches
GET    /api/accounts-receivable/batches/{id}
POST   /api/accounts-receivable/batches/{id}/generate-complements
GET    /api/accounts-receivable/customers/{customerId}/credit-policy
PUT    /api/accounts-receivable/customers/{customerId}/credit-policy
PUT    /api/accounts-receivable/customers/{customerId}/credit-status
GET    /api/accounts-receivable/dashboard
GET    /api/accounts-receivable/customers/{customerId}/statement
GET    /api/accounts-receivable/reports/overdue
GET    /api/accounts-receivable/reports/forecast
GET    /api/accounts-receivable/metrics
```

---

## ⚠️ LO QUE FALTA POR IMPLEMENTAR

### 1. **CommandHandlers** (Application/Core/AccountsReceivable/CommandHandlers/) ❌ PENDIENTE

Debes crear 7 handlers que implementen `IRequestHandler<TCommand, TResponse>`:

#### a) **CreateInvoicePPDCommandHandler.cs**
```csharp
public class CreateInvoicePPDCommandHandler : IRequestHandler<CreateInvoicePPDCommand, InvoicePPDDto>
{
    private readonly IInvoicePPDRepository _invoiceRepository;
    private readonly ICustomerCreditPolicyRepository _creditRepository;
    private readonly ICustomerCreditHistoryRepository _historyRepository;
    
    // Lógica:
    // 1. Crear InvoicePPD
    // 2. Actualizar CustomerCreditPolicy (TotalPendingAmount += Amount)
    // 3. Registrar evento en CustomerCreditHistory
    // 4. Retornar DTO
}
```

#### b) **CreatePaymentCommandHandler.cs**
```csharp
public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, PaymentDto>
{
    // Lógica:
    // 1. Generar PaymentNumber
    // 2. Crear Payment
    // 3. Para cada factura en el listado:
    //    - Crear PaymentApplication
    //    - Actualizar InvoicePPD (BalanceAmount, PaidAmount, Status)
    //    - Si balance = 0, cambiar status a "Paid"
    // 4. Actualizar CustomerCreditPolicy (TotalPendingAmount -= Amount)
    // 5. Registrar en CustomerCreditHistory
    // 6. Retornar PaymentDto con aplicaciones
}
```

#### c) **GeneratePaymentComplementsCommandHandler.cs**
```csharp
public class GeneratePaymentComplementsCommandHandler : IRequestHandler<GeneratePaymentComplementsCommand, GenerateComplementsResultDto>
{
    // Lógica:
    // 1. Obtener Payment y sus PaymentApplications
    // 2. Para cada PaymentApplication:
    //    - Generar XML del complemento de pago
    //    - Timbrar ante PAC
    //    - Guardar UUID, XML, PDF
    //    - Actualizar ComplementStatus = "Generated"
    //    - Si falla, registrar error
    //    - Crear PaymentComplementLog
    // 3. Actualizar Payment.ComplementsGenerated
    // 4. Opcionalmente enviar emails
    // 5. Retornar resultado con éxitos/errores
}
```

#### d) **CreatePaymentBatchCommandHandler.cs**
```csharp
public class CreatePaymentBatchCommandHandler : IRequestHandler<CreatePaymentBatchCommand, PaymentBatchDto>
{
    // Lógica:
    // 1. Generar BatchNumber
    // 2. Crear PaymentBatch
    // 3. Para cada cliente en el batch:
    //    - Crear Payment con BatchId
    //    - Proceso igual a CreatePaymentCommandHandler
    // 4. Actualizar totales del batch
    // 5. Retornar BatchDto con todos los pagos
}
```

#### e) **GenerateBatchComplementsCommandHandler.cs**
```csharp
public class GenerateBatchComplementsCommandHandler : IRequestHandler<GenerateBatchComplementsCommand, GenerateComplementsResultDto>
{
    // Lógica:
    // 1. Obtener PaymentBatch y todos sus Payments
    // 2. Obtener todas las PaymentApplications del batch
    // 3. Procesar asíncronamente (considerar usar BackgroundService o Hangfire)
    // 4. Actualizar progreso del batch
    // 5. Retornar resultado agregado
}
```

#### f) **UpsertCustomerCreditPolicyCommandHandler.cs**
```csharp
public class UpsertCustomerCreditPolicyCommandHandler : IRequestHandler<UpsertCustomerCreditPolicyCommand, CustomerCreditPolicyDto>
{
    // Lógica:
    // 1. Buscar si existe política para el cliente
    // 2. Si existe: Actualizar
    // 3. Si no existe: Crear nueva
    // 4. Registrar evento en CustomerCreditHistory
    // 5. Retornar DTO
}
```

#### g) **UpdateCreditStatusCommandHandler.cs**
```csharp
public class UpdateCreditStatusCommandHandler : IRequestHandler<UpdateCreditStatusCommand, bool>
{
    // Lógica:
    // 1. Obtener CustomerCreditPolicy
    // 2. Actualizar Status y BlockReason
    // 3. Registrar en CustomerCreditHistory
    // 4. Retornar true/false
}
```

### 2. **QueryHandlers** (Application/Core/AccountsReceivable/QueryHandlers/) ❌ PENDIENTE

Debes crear 10 handlers que implementen `IRequestHandler<TQuery, TResponse>`:

#### a) **GetInvoicesPPDQueryHandler.cs**
```csharp
public class GetInvoicesPPDQueryHandler : IRequestHandler<GetInvoicesPPDQuery, InvoicePPDPageResponse>
{
    // Lógica:
    // 1. Llamar repository.GetPagedAsync con filtros
    // 2. Map entidades a DTOs
    // 3. Calcular totales
    // 4. Retornar InvoicePPDPageResponse
}
```

#### b) **GetInvoicePPDByIdQueryHandler.cs**
```csharp
public class GetInvoicePPDByIdQueryHandler : IRequestHandler<GetInvoicePPDByIdQuery, InvoicePPDDto?>
{
    // Lógica simple: Get by ID y mapear a DTO
}
```

#### c) **GetPaymentByIdQueryHandler.cs**
```csharp
public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    // Lógica:
    // 1. Obtener Payment
    // 2. Obtener PaymentApplications relacionadas
    // 3. Mapear todo a PaymentDto con Applications
}
```

#### d) **GetAccountsReceivableDashboardQueryHandler.cs**
```csharp
public class GetAccountsReceivableDashboardQueryHandler : IRequestHandler<GetAccountsReceivableDashboardQuery, AccountsReceivableDashboardDto>
{
    // Lógica:
    // 1. Obtener totales de invoices pendientes
    // 2. Calcular total vencido
    // 3. Obtener aging report
    // 4. Top clientes con mayor saldo
    // 5. Calcular KPIs básicos
    // 6. Retornar dashboard completo
}
```

#### e) **GetCustomerStatementQueryHandler.cs**
```csharp
public class GetCustomerStatementQueryHandler : IRequestHandler<GetCustomerStatementQuery, CustomerStatementDto?>
{
    // Lógica:
    // 1. Obtener CustomerCreditPolicy
    // 2. Obtener InvoicesPPD pendientes del cliente
    // 3. Obtener Payments recientes
    // 4. Obtener CreditHistory reciente
    // 5. Armar CustomerStatementDto completo
}
```

#### f) **GetCustomerCreditPolicyQueryHandler.cs**
```csharp
// Simple: Obtener política y mapear a DTO
```

#### g) **GetOverdueInvoicesReportQueryHandler.cs**
```csharp
public class GetOverdueInvoicesReportQueryHandler : IRequestHandler<GetOverdueInvoicesReportQuery, OverdueInvoicesReportDto>
{
    // Lógica:
    // 1. Obtener facturas vencidas con filtros
    // 2. Agrupar por cliente
    // 3. Calcular totales y máximos días vencido
    // 4. Retornar reporte
}
```

#### h) **GetCollectionForecastQueryHandler.cs**
```csharp
public class GetCollectionForecastQueryHandler : IRequestHandler<GetCollectionForecastQuery, CollectionForecastDto>
{
    // Lógica:
    // 1. Obtener facturas que vencen en los próximos N días
    // 2. Agrupar por periodos (semana/mes)
    // 3. Calcular proyecciones
    // 4. Retornar CollectionForecastDto
}
```

#### i) **GetAccountsReceivableMetricsQueryHandler.cs**
```csharp
public class GetAccountsReceivableMetricsQueryHandler : IRequestHandler<GetAccountsReceivableMetricsQuery, AccountsReceivableMetricsDto>
{
    // Lógica:
    // 1. Calcular DSO: Total CxC / (Ventas 90 días / 90)
    // 2. Calcular tasa morosidad: Vencido / Total
    // 3. Calcular efectividad cobranza
    // 4. Edad promedio cartera
    // 5. Tasa recuperación
    // 6. Retornar métricas
}
```

#### j) **GetPaymentBatchByIdQueryHandler.cs**
```csharp
// Obtener batch con todos sus payments y applications
```

### 3. **Implementaciones de Repositorios** (Infrastructure/Repositories/) ❌ PENDIENTE

Debes crear 7 archivos que implementen las interfaces:

#### a) **InvoicePPDRepository.cs**
```csharp
public class InvoicePPDRepository : IInvoicePPDRepository
{
    private readonly POSDbContext _context;
    
    // Implementar todos los métodos usando EF Core:
    // - GetPagedAsync con IQueryable dinámico según filtros
    // - GetOverdueAsync
    // - GetAgingReportAsync con GROUP BY
    // - Etc.
}
```

#### b) **PaymentRepository.cs**
```csharp
public class PaymentRepository : IPaymentRepository
{
    // Implementar métodos
    // GeneratePaymentNumberAsync debe:
    // 1. Obtener último número del año actual
    // 2. Incrementar
    // 3. Formar "PAG-2026-001"
}
```

#### c) **PaymentApplicationRepository.cs**
#### d) **PaymentBatchRepository.cs**
#### e) **CustomerCreditPolicyRepository.cs**
- **Importante**: `ValidateCreditAvailabilityAsync` debe verificar si el cliente tiene crédito disponible

#### f) **CustomerCreditHistoryRepository.cs**
#### g) **PaymentComplementLogRepository.cs**

### 4. **Registro de Dependencias** ❌ PENDIENTE

En `Infrastructure/DependencyInjection.cs` debes registrar los repositorios:

```csharp
// Cuentas por Cobrar
services.AddScoped<IInvoicePPDRepository, InvoicePPDRepository>();
services.AddScoped<IPaymentRepository, PaymentRepository>();
services.AddScoped<IPaymentApplicationRepository, PaymentApplicationRepository>();
services.AddScoped<IPaymentBatchRepository, PaymentBatchRepository>();
services.AddScoped<ICustomerCreditPolicyRepository, CustomerCreditPolicyRepository>();
services.AddScoped<ICustomerCreditHistoryRepository, CustomerCreditHistoryRepository>();
services.AddScoped<IPaymentComplementLogRepository, PaymentComplementLogRepository>();
```

### 5. **Validadores con FluentValidation** ❌ PENDIENTE (OPCIONAL)

Puedes crear validadores para los commands:

#### `CreatePaymentCommandValidator.cs`
```csharp
public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.PaymentDate).LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.Invoices).NotEmpty().WithMessage("Debe incluir al menos una factura");
        RuleFor(x => x.PaymentMethodSAT).NotEmpty().Length(2);
    }
}
```

- `CreateInvoicePPDCommandValidator`
- `UpsertCustomerCreditPolicyCommandValidator`
- etc.

### 6. **Migración de Base de Datos** ❌ PENDIENTE

Después de completar todo:

```powershell
# Crear migración
Add-Migration AccountsReceivableModule -Project Infrastructure -StartupProject Web.Api

# Revisar la migración generada

# Aplicar migración
Update-Database -Project Infrastructure -StartupProject Web.Api
```

### 7. **Servicio de Generación de Complementos SAT** ❌ PENDIENTE

Crear un servicio para la generación de XMLs y timbrado:

#### `Application/Common/Services/IPaymentComplementService.cs`
```csharp
public interface IPaymentComplementService
{
    Task<(bool success, string? uuid, string? error)> GenerateComplementAsync(
        PaymentApplication application,
        Payment payment);
    
    Task<string> GenerateXmlAsync(PaymentApplication application, Payment payment);
    Task<string> StampWithPACAsync(string xml);
    Task<byte[]> GeneratePdfAsync(string xml, string uuid);
    Task SendEmailAsync(Guid customerId, string xmlPath, string pdfPath);
}
```

#### Implementación en `Infrastructure/Services/PaymentComplementService.cs`
- Usar librería de generación de XML CFDI
- Integrar con PAC (FinkelSAT, etc.)
- Generar PDF con iTextSharp o similar
- Enviar emails con SMTP

### 8. **Jobs Automáticos** ❌ PENDIENTE (OPCIONAL)

Crear jobs con Hangfire o Windows Service:

#### `UpdateInvoiceStatusJob.cs`
```csharp
// Ejecutar diario a las 00:00
// 1. Obtener facturas con balance > 0
// 2. Calcular días vencidos
// 3. Actualizar Status si cambió
// 4. Bloquear clientes si exceden días
```

#### `RecalculateCustomerBalancesJob.cs`
```csharp
// Ejecutar diario
// 1. Para cada cliente con facturas
// 2. Sumar TotalPendingAmount
// 3. Sumar TotalOverdueAmount
// 4. Actualizar AvailableCredit
```

### 9. **Integración con Módulo de Ventas** ❌ PENDIENTE

Modificar el proceso de facturación existente:

#### En `BillingController` o donde timbres facturas:
```csharp
// Después de timbrar CFDI con éxito
if (invoice.MetodoPago == "PPD")
{
    // Crear InvoicePPD automáticamente
    var command = new CreateInvoicePPDCommand
    {
        InvoiceId = invoice.Id,
        CustomerId = invoice.CustomerId,
        FolioUUID = invoice.UUID,
        // ... más datos
    };
    
    await _mediator.Send(command);
}
```

#### En proceso de venta a crédito:
```csharp
// ANTES de crear la venta
var creditPolicy = await _creditPolicyRepository.GetByCustomerIdAsync(customerId);

if (creditPolicy == null || creditPolicy.Status == "Blocked")
{
    return BadRequest("Cliente no tiene crédito activo");
}

// Validar crédito disponible
var isValid = await _creditPolicyRepository.ValidateCreditAvailabilityAsync(
    customerId, 
    saleTotal);

if (!isValid)
{
    return BadRequest("Cliente ha excedido su límite de crédito");
}

// DESPUÉS de crear venta, afectar crédito INMEDIATAMENTE
await _creditPolicyRepository.UpdateBalancesAsync(
    customerId,
    creditPolicy.TotalPendingAmount + saleTotal,
    creditPolicy.TotalOverdueAmount);
```

### 10. **Tests Unitarios** ❌ PENDIENTE (OPCIONAL)

Crear tests para handlers y repositorios usando xUnit/NUnit.

---

## 📋 CHECKLIST DE IMPLEMENTACIÓN

### Paso 1: Handlers (CRÍTICO)
- [ ] CreateInvoicePPDCommandHandler
- [ ] CreatePaymentCommandHandler  
- [ ] GeneratePaymentComplementsCommandHandler
- [ ] CreatePaymentBatchCommandHandler
- [ ] GenerateBatchComplementsCommandHandler
- [ ] UpsertCustomerCreditPolicyCommandHandler
- [ ] UpdateCreditStatusCommandHandler
- [ ] GetInvoicesPPDQueryHandler
- [ ] GetInvoicePPDByIdQueryHandler
- [ ] GetPaymentByIdQueryHandler
- [ ] GetPaymentBatchByIdQueryHandler
- [ ] GetAccountsReceivableDashboardQueryHandler
- [ ] GetCustomerStatementQueryHandler
- [ ] GetCustomerCreditPolicyQueryHandler
- [ ] GetOverdueInvoicesReportQueryHandler
- [ ] GetCollectionForecastQueryHandler
- [ ] GetAccountsReceivableMetricsQueryHandler

### Paso 2: Repositorios (CRÍTICO)
- [ ] InvoicePPDRepository
- [ ] PaymentRepository
- [ ] PaymentApplicationRepository
- [ ] PaymentBatchRepository
- [ ] CustomerCreditPolicyRepository
- [ ] CustomerCreditHistoryRepository
- [ ] PaymentComplementLogRepository

### Paso 3: Configuración (CRÍTICO)
- [ ] Registrar repositorios en DependencyInjection.cs
- [ ] Crear y aplicar migración
- [ ] Verificar que MediatR registre automáticamente los handlers

### Paso 4: Servicios Adicionales (IMPORTANTE)
- [ ] IPaymentComplementService (generación XML/timbrado)
- [ ] Implementación con PAC real
- [ ] Servicio de generación de PDF
- [ ] Servicio de envío de emails

### Paso 5: Integración (IMPORTANTE)
- [ ] Integrar con proceso de facturación (crear InvoicePPD al timbrar)
- [ ] Integrar con proceso de ventas (validar crédito)
- [ ] Afectar crédito al crear venta (ANTES de timbrar)

### Paso 6: Mejoras Opcionales
- [ ] Validadores FluentValidation
- [ ] Jobs automáticos (Hangfire)
- [ ] Tests unitarios
- [ ] Documentación Swagger mejorada

---

## 🚀 ORDEN RECOMENDADO DE IMPLEMENTACIÓN

1. **Primero**: Implementar repositorios básicos (sin lógica compleja)
2. **Segundo**: Implementar CommandHandlers más simples (Create, Update)
3. **Tercero**: Implementar QueryHandlers básicos (GetById, GetPaged)
4. **Cuarto**: Registrar dependencias y crear migración
5. **Quinto**: Probar endpoints básicos con Postman
6. **Sexto**: Implementar servicio de complementos SAT
7. **Séptimo**: Implementar handlers complejos (Generate Complements)
8. **Octavo**: Implementar QueryHandlers de reportes
9. **Noveno**: Integrar con módulos existentes
10. **Décimo**: Jobs y mejoras finales

---

## 📝 NOTAS IMPORTANTES

### Transacciones
Usa transacciones en handlers que modifican múltiples tablas:
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Operaciones
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### Mapeo de Entidades a DTOs
Considera usar AutoMapper o mapeo manual en handlers:
```csharp
var dto = new InvoicePPDDto
{
    Id = entity.Id,
    SerieAndFolio = entity.SerieAndFolio,
    // ... más propiedades
};
```

### Manejo de Errores
Los handlers deben lanzar excepciones personalizadas que el middleware maneje:
```csharp
if (creditPolicy == null)
    throw new NotFoundException("Política de crédito no encontrada");

if (!hasCredit)
    throw new BusinessException("Cliente sin crédito disponible");
```

### Permisos
El controller ya tiene `[RequirePermission("CuentasporCobrar", "View/Create/Edit")]`.
Asegúrate de crear el módulo "CuentasporCobrar" en la base de datos.

---

## 🎯 RESULTADO FINAL

Una vez completada la implementación, tendrás un módulo completo de Cuentas por Cobrar que:

✅ Registra automáticamente facturas PPD al timbrar  
✅ Valida crédito disponible al vender  
✅ Afecta el crédito inmediatamente (antes de facturar)  
✅ Permite pagos individuales o masivos  
✅ Genera complementos de pago SAT automáticamente  
✅ Proporciona dashboard y reportes en tiempo real  
✅ Controla políticas de crédito por cliente  
✅ Mantiene historial completo de eventos  
✅ Bloquea automáticamente clientes morosos  
✅ Proyecta cobranza futura  
✅ Calcula métricas (DSO, morosidad, etc.)  

**Cumple 100% con SAT para complementos de pago CFDI 4.0**

---

**¿Dudas? Revisa los archivos ya creados como referencia para los patrones a seguir.**
