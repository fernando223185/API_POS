using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountsReceivableModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerCreditHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RelatedEntity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    PreviousValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCreditHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerCreditHistory_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerCreditHistory_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "CustomerCreditPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreditDays = table.Column<int>(type: "int", nullable: false),
                    OverdueGraceDays = table.Column<int>(type: "int", nullable: false),
                    TotalPendingAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOverdueAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OldestInvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OldestOverdueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AveragePaymentDays = table.Column<int>(type: "int", nullable: false),
                    OnTimePaymentRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastPaymentAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BlockReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AutoBlockOnOverdue = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextReviewDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerCreditPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerCreditPolicies_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerCreditPolicies_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "InvoicesPPD",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerRFC = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    FolioUUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    Serie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SerieAndFolio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NextPartialityNumber = table.Column<int>(type: "int", nullable: false),
                    TotalPartialities = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DaysOverdue = table.Column<int>(type: "int", nullable: false),
                    LastPaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoicesPPD", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoicesPPD_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicesPPD_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InvoicesPPD_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PaymentBatches",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BatchNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalPayments = table.Column<int>(type: "int", nullable: false),
                    TotalInvoices = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DefaultPaymentMethodSAT = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    DefaultBankDestination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DefaultAccountDestination = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProcessingProgress = table.Column<int>(type: "int", nullable: false),
                    ComplementsGenerated = table.Column<int>(type: "int", nullable: false),
                    ComplementsWithError = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    ProcessedBy = table.Column<int>(type: "int", nullable: true),
                    CancelledBy = table.Column<int>(type: "int", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentBatches_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentBatches_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    IsBatchPayment = table.Column<bool>(type: "bit", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    PaymentMethodSAT = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    PaymentFormSAT = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    BankOrigin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountOrigin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    BankDestination = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BankAccountDestination = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Reference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AppliedToInvoices = table.Column<int>(type: "int", nullable: false),
                    ComplementsGenerated = table.Column<int>(type: "int", nullable: false),
                    ComplementsWithError = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CancelledBy = table.Column<int>(type: "int", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Payments_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Payments_PaymentBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "PaymentBatches",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentApplications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    InvoicePPDId = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FolioUUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    SerieAndFolio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    OriginalInvoiceAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PartialityNumber = table.Column<int>(type: "int", nullable: false),
                    PreviousBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountApplied = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NewBalance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ComplementUUID = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ComplementSerie = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ComplementFolio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ComplementSerieAndFolio = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ComplementStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ComplementError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    XmlPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PdfPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    XmlContent = table.Column<string>(type: "ntext", nullable: true),
                    EmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SATCertificationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SATSerialNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AppliedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    LastRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentApplications_InvoicesPPD_InvoicePPDId",
                        column: x => x.InvoicePPDId,
                        principalTable: "InvoicesPPD",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentApplications_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentComplementLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentApplicationId = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: false),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ExecutionTimeMs = table.Column<int>(type: "int", nullable: false),
                    ErrorCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "ntext", nullable: true),
                    PACResponse = table.Column<string>(type: "ntext", nullable: true),
                    PACTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentComplementLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentComplementLogs_PaymentApplications_PaymentApplicationId",
                        column: x => x.PaymentApplicationId,
                        principalTable: "PaymentApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentComplementLogs_PaymentBatches_BatchId",
                        column: x => x.BatchId,
                        principalTable: "PaymentBatches",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentComplementLogs_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditHistory_CompanyId",
                table: "CustomerCreditHistory",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditHistory_CustomerId",
                table: "CustomerCreditHistory",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditHistory_EventDate",
                table: "CustomerCreditHistory",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditPolicies_CompanyId",
                table: "CustomerCreditPolicies",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerCreditPolicies_CustomerId_CompanyId",
                table: "CustomerCreditPolicies",
                columns: new[] { "CustomerId", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_BranchId",
                table: "InvoicesPPD",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_CompanyId",
                table: "InvoicesPPD",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_CustomerId",
                table: "InvoicesPPD",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_DueDate",
                table: "InvoicesPPD",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_InvoiceId",
                table: "InvoicesPPD",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicesPPD_Status",
                table: "InvoicesPPD",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_InvoicePPDId",
                table: "PaymentApplications",
                column: "InvoicePPDId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentApplications_PaymentId",
                table: "PaymentApplications",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBatches_BranchId",
                table: "PaymentBatches",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBatches_CompanyId",
                table: "PaymentBatches",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBatches_PaymentDate",
                table: "PaymentBatches",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentBatches_Status",
                table: "PaymentBatches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentComplementLogs_AttemptDate",
                table: "PaymentComplementLogs",
                column: "AttemptDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentComplementLogs_BatchId",
                table: "PaymentComplementLogs",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentComplementLogs_PaymentApplicationId",
                table: "PaymentComplementLogs",
                column: "PaymentApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentComplementLogs_PaymentId",
                table: "PaymentComplementLogs",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BatchId",
                table: "Payments",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BranchId",
                table: "Payments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CompanyId",
                table: "Payments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CustomerId",
                table: "Payments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerCreditHistory");

            migrationBuilder.DropTable(
                name: "CustomerCreditPolicies");

            migrationBuilder.DropTable(
                name: "PaymentComplementLogs");

            migrationBuilder.DropTable(
                name: "PaymentApplications");

            migrationBuilder.DropTable(
                name: "InvoicesPPD");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentBatches");
        }
    }
}
