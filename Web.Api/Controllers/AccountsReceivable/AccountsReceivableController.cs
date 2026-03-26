using Application.Core.AccountsReceivable.Commands;
using Application.Core.AccountsReceivable.Queries;
using Application.DTOs.AccountsReceivable;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Authorization;

namespace Web.Api.Controllers.AccountsReceivable;

/// <summary>
/// Controller para gestión de Cuentas por Cobrar
/// </summary>
[Route("api/accounts-receivable")]
[ApiController]
public class AccountsReceivableController : ControllerBase
{
    private readonly IMediator _mediator;

    public AccountsReceivableController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Facturas PPD

    /// <summary>
    /// Obtiene facturas PPD pendientes de pago con paginación y filtros
    /// </summary>
    [HttpGet("invoices-ppd")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetInvoicesPPD([FromQuery] GetInvoicesPPDQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene una factura PPD por ID
    /// </summary>
    [HttpGet("invoices-ppd/{id}")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetInvoicePPDById(int id)
    {
        var query = new GetInvoicePPDByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound(new { message = "Factura PPD no encontrada" });
        
        return Ok(result);
    }

    /// <summary>
    /// Crea una factura PPD desde una factura existente
    /// Este endpoint se llama después de timbrar una factura con método de pago PPD
    /// </summary>
    [HttpPost("invoices-ppd")]
    [RequirePermission("CFDI", "Create")]
    public async Task<IActionResult> CreateInvoicePPD([FromBody] CreateInvoicePPDCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetInvoicePPDById), new { id = result.Id }, result);
    }

    #endregion

    #region Pagos

    /// <summary>
    /// Registra un pago a una o varias facturas PPD
    /// </summary>
    [HttpPost("payments")]
    [RequirePermission("CFDI", "Create")]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPaymentById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene un pago por ID con todas sus aplicaciones
    /// </summary>
    [HttpGet("payments/{id}")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetPaymentById(int id)
    {
        var query = new GetPaymentByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound(new { message = "Pago no encontrado" });
        
        return Ok(result);
    }

    /// <summary>
    /// Genera complementos de pago SAT para un pago específico
    /// </summary>
    [HttpPost("payments/{id}/generate-complements")]
    [RequirePermission("CFDI", "Create")]
    public async Task<IActionResult> GeneratePaymentComplements(
        int id, 
        [FromBody] GenerateComplementsRequest request)
    {
        var command = new GeneratePaymentComplementsCommand
        {
            PaymentId = id,
            SendEmailsAutomatically = request.SendEmailsAutomatically,
            ExecutedBy = 0 // TODO: Obtener del usuario actual
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #endregion

    #region Lotes de Pago

    /// <summary>
    /// Crea un lote de pagos masivo
    /// </summary>
    [HttpPost("batches")]
    [RequirePermission("CFDI", "Create")]
    public async Task<IActionResult> CreatePaymentBatch([FromBody] CreatePaymentBatchCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetPaymentBatchById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtiene un lote de pagos por ID
    /// </summary>
    [HttpGet("batches/{id}")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetPaymentBatchById(int id)
    {
        var query = new GetPaymentBatchByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound(new { message = "Lote no encontrado" });
        
        return Ok(result);
    }

    /// <summary>
    /// Genera todos los complementos de pago de un lote
    /// </summary>
    [HttpPost("batches/{id}/generate-complements")]
    [RequirePermission("CFDI", "Create")]
    public async Task<IActionResult> GenerateBatchComplements(
        int id,
        [FromBody] GenerateComplementsRequest request)
    {
        var command = new GenerateBatchComplementsCommand
        {
            BatchId = id,
            SendEmailsAutomatically = request.SendEmailsAutomatically,
            ExecutedBy = 0 // TODO: Obtener del usuario actual
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    #endregion

    #region Política de Crédito

    /// <summary>
    /// Obtiene la política de crédito de un cliente
    /// </summary>
    [HttpGet("customers/{customerId}/credit-policy")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetCustomerCreditPolicy(int customerId)
    {
        var query = new GetCustomerCreditPolicyQuery { CustomerId = customerId };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound(new { message = "Política de crédito no encontrada" });
        
        return Ok(result);
    }

    /// <summary>
    /// Crea o actualiza la política de crédito de un cliente
    /// </summary>
    [HttpPut("customers/{customerId}/credit-policy")]
    [RequirePermission("CFDI", "Edit")]
    public async Task<IActionResult> UpsertCustomerCreditPolicy(
        int customerId,
        [FromBody] UpsertCustomerCreditPolicyRequest request)
    {
        var command = new UpsertCustomerCreditPolicyCommand
        {
            CustomerId = customerId,
            CompanyId = request.CompanyId,
            CreditLimit = request.CreditLimit,
            CreditDays = request.CreditDays,
            OverdueGraceDays = request.OverdueGraceDays,
            AutoBlockOnOverdue = request.AutoBlockOnOverdue,
            Notes = request.Notes,
            ExecutedBy = 0 // TODO: Obtener del usuario actual
        };
        
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Bloquea o desbloquea el crédito de un cliente
    /// </summary>
    [HttpPut("customers/{customerId}/credit-status")]
    [RequirePermission("CFDI", "Edit")]
    public async Task<IActionResult> UpdateCreditStatus(
        int customerId,
        [FromBody] UpdateCreditStatusRequest request)
    {
        var command = new UpdateCreditStatusCommand
        {
            CustomerId = customerId,
            Status = request.Status,
            Reason = request.Reason,
            ExecutedBy = 0 // TODO: Obtener del usuario actual
        };
        
        var result = await _mediator.Send(command);
        return Ok(new { success = result });
    }

    #endregion

    #region Reportes y Dashboard

    /// <summary>
    /// Obtiene el dashboard principal de Cuentas por Cobrar
    /// </summary>
    [HttpGet("dashboard")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetDashboard([FromQuery] int companyId, [FromQuery] int? branchId = null)
    {
        var query = new GetAccountsReceivableDashboardQuery
        {
            CompanyId = companyId,
            BranchId = branchId
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene el estado de cuenta de un cliente
    /// </summary>
    [HttpGet("customers/{customerId}/statement")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetCustomerStatement(
        int customerId,
        [FromQuery] int companyId,
        [FromQuery] bool includeHistory = true)
    {
        var query = new GetCustomerStatementQuery
        {
            CustomerId = customerId,
            CompanyId = companyId,
            IncludeHistory = includeHistory
        };
        
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound(new { message = "Cliente no encontrado" });
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene reporte de facturas vencidas
    /// </summary>
    [HttpGet("reports/overdue")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetOverdueReport(
        [FromQuery] int companyId,
        [FromQuery] int? branchId = null,
        [FromQuery] int? minDaysOverdue = null)
    {
        var query = new GetOverdueInvoicesReportQuery
        {
            CompanyId = companyId,
            BranchId = branchId,
            MinDaysOverdue = minDaysOverdue
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene proyección de cobranza
    /// </summary>
    [HttpGet("reports/forecast")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetCollectionForecast(
        [FromQuery] int companyId,
        [FromQuery] int? branchId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] int days = 90)
    {
        var query = new GetCollectionForecastQuery
        {
            CompanyId = companyId,
            BranchId = branchId,
            FromDate = fromDate,
            Days = days
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene métricas de CxC (DSO, morosidad, etc.)
    /// </summary>
    [HttpGet("metrics")]
    [RequirePermission("CFDI", "View")]
    public async Task<IActionResult> GetMetrics(
        [FromQuery] int companyId,
        [FromQuery] int? branchId = null)
    {
        var query = new GetAccountsReceivableMetricsQuery
        {
            CompanyId = companyId,
            BranchId = branchId
        };
        
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    #endregion
}
