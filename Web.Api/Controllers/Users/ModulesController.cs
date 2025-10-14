using Application.Abstractions.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using Web.Api.Authorization;

namespace Web.Api.Controllers.Users
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModulesController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public ModulesController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet("user/{userId}/menu")]
        [RequireAuthentication] // Solo requiere estar autenticado
        public async Task<IActionResult> GetUserMenu(int userId)
        {
            try
            {
                // Obtener el userId real del token JWT
                var currentUserId = HttpContext.Items["UserId"] as int? ?? 0;
                
                // Solo permitir ver el propio menú o si es administrador
                if (currentUserId != userId && !await IsAdministrator())
                {
                    return Forbid();
                }

                var permissions = await _permissionService.GetUserPermissionsAsync(userId);
                
                var menuItems = new List<object>
                {
                    new {
                        name = "Inicio",
                        icon = "faHome",
                        path = "/dashboard",
                        hasAccess = permissions.Any(p => p.Module.Name == "Dashboard"),
                        moduleId = 1
                    },
                    new {
                        name = "Ventas",
                        icon = "faShoppingCart", 
                        path = "/sales",
                        hasAccess = permissions.Any(p => p.Module.Name == "Sales"),
                        moduleId = 2,
                        submodules = new[]
                        {
                            new { name = "Nueva Venta", desc = "Crear ticket y capturar productos", path = "/sales/new", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Sale" && p.Name == "CreateSale") },
                            new { name = "Historial", desc = "Ventas recientes y filtros", path = "/sales/history", color = "from-sky-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Sale" && p.Name == "ViewHistory") },
                            new { name = "Productos", desc = "Catálogo, precios y stock", path = "/sales/products", color = "from-violet-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Sale" && p.Name == "ViewProducts") },
                            new { name = "Cobrar", desc = "Pagos y métodos de cobro", path = "/sales/checkout", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Sale" && p.Name == "ProcessPayment") }
                        }
                    },
                    new {
                        name = "Productos",
                        icon = "faBoxOpen",
                        path = "/products",
                        hasAccess = permissions.Any(p => p.Module.Name == "Products"),
                        moduleId = 3,
                        submodules = new[]
                        {
                            new { name = "Catálogo", desc = "Ver, buscar y gestionar productos", path = "/products/catalog", color = "from-blue-500 to-indigo-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ViewCatalog") },
                            new { name = "Nuevo Producto", desc = "Alta rápida con códigos y precios", path = "/products/new", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "Create") },
                            new { name = "Búsqueda Avanzada", desc = "Filtros por categoría, marca, SKU", path = "/products/search", color = "from-purple-500 to-violet-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "AdvancedSearch") },
                            new { name = "Control de Stock", desc = "Inventario actual y movimientos", path = "/products/inventory", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ViewStock") },
                            new { name = "Categorías", desc = "Organización del catálogo", path = "/products/categories", color = "from-cyan-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManageCategories") },
                            new { name = "Códigos de Barras", desc = "Generación e impresión", path = "/products/barcodes", color = "from-slate-500 to-gray-700", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManageBarcodes") },
                            new { name = "Listas de Precios", desc = "Precios por cliente/mayoreo", path = "/products/pricing", color = "from-rose-500 to-pink-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManagePricing") },
                            new { name = "Reportes", desc = "Ventas, rotación y análisis", path = "/products/reports", color = "from-green-500 to-emerald-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ViewReports") },
                            new { name = "Stock Mínimo", desc = "Alertas de reposición", path = "/products/alerts", color = "from-red-500 to-rose-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManageAlerts") },
                            new { name = "Proveedores", desc = "Gestión de proveedores", path = "/products/suppliers", color = "from-indigo-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManageSuppliers") },
                            new { name = "Órdenes de Compra", desc = "Reposición de inventario", path = "/products/orders", color = "from-teal-500 to-cyan-600", hasAccess = permissions.Any(p => p.Resource == "Product" && p.Name == "ManageOrders") }
                        }
                    },
                    new {
                        name = "Inventario",
                        icon = "faWarehouse",
                        path = "/inventory", 
                        hasAccess = permissions.Any(p => p.Module.Name == "Inventory"),
                        moduleId = 4,
                        submodules = new[]
                        {
                            new { name = "Existencias", desc = "Stock por almacén y costos", path = "/inventory/stock", color = "from-sky-500 to-indigo-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ViewStock") },
                            new { name = "Kardex / Movs.", desc = "Entradas, salidas y ajustes", path = "/inventory/kardex", color = "from-cyan-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ViewKardex") },
                            new { name = "Entradas", desc = "Recepciones y compras", path = "/inventory/inbound", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageInbound") },
                            new { name = "Salidas", desc = "Consumos y ventas", path = "/inventory/outbound", color = "from-rose-500 to-pink-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageOutbound") },
                            new { name = "Ajustes", desc = "Correcciones de inventario", path = "/inventory/adjustments", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "MakeAdjustments") },
                            new { name = "Traspasos", desc = "Entre almacenes", path = "/inventory/transfers", color = "from-fuchsia-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageTransfers") },
                            new { name = "Almacenes", desc = "Configuración y zonas", path = "/inventory/warehouses", color = "from-slate-500 to-gray-700", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageWarehouses") },
                            new { name = "Conteos / Ciclos", desc = "Cíclicos y generales", path = "/inventory/counts", color = "from-lime-500 to-green-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageCounts") },
                            new { name = "Etiquetas / Códigos", desc = "Barcodes y diseños", path = "/inventory/labels", color = "from-indigo-500 to-violet-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageLabels") },
                            new { name = "Alertas de Stock", desc = "Mínimos y rotación", path = "/inventory/alerts", color = "from-red-500 to-orange-500", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ViewAlerts") },
                            new { name = "Recepciones", desc = "OC y embarques", path = "/inventory/receipts", color = "from-teal-500 to-cyan-600", hasAccess = permissions.Any(p => p.Resource == "Inventory" && p.Name == "ManageReceipts") }
                        }
                    },
                    new {
                        name = "Clientes",
                        icon = "faUsers",
                        path = "/customers",
                        hasAccess = permissions.Any(p => p.Module.Name == "Customers"),
                        moduleId = 5,
                        submodules = new[]
                        {
                            new { name = "Listado", desc = "Buscar, filtrar y editar clientes", path = "/customers/list", color = "from-sky-500 to-indigo-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ViewList") },
                            new { name = "Nuevo Cliente", desc = "Alta rápida con datos fiscales", path = "/customers/new", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "Create") },
                            new { name = "Solicitudes", desc = "Aprobación/Rechazo de altas", path = "/customers/requests", color = "from-cyan-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageRequests") },
                            new { name = "Crédito y Límites", desc = "Líneas, días y condiciones", path = "/customers/credit", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageCredit") },
                            new { name = "Contactos", desc = "Personas de contacto", path = "/customers/contacts", color = "from-violet-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageContacts") },
                            new { name = "Direcciones", desc = "Facturación y envío", path = "/customers/addresses", color = "from-lime-500 to-green-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageAddresses") },
                            new { name = "Identificación", desc = "RFC, uso CFDI, documentos", path = "/customers/identity", color = "from-slate-500 to-gray-700", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageIdentification") },
                            new { name = "Precios y Descuentos", desc = "Listas y promociones", path = "/customers/pricing", color = "from-rose-500 to-pink-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManagePricing") },
                            new { name = "CFDI / Facturas", desc = "Timbrado y reenvío", path = "/customers/documents", color = "from-fuchsia-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageDocuments") },
                            new { name = "Estado de Cuenta", desc = "Cargos, abonos y saldo", path = "/customers/ledger", color = "from-indigo-500 to-violet-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ViewLedger") },
                            new { name = "Saldos a Favor", desc = "Aplicaciones y devoluciones", path = "/customers/overpayments", color = "from-teal-500 to-cyan-600", hasAccess = permissions.Any(p => p.Resource == "Customer" && p.Name == "ManageOverpayments") }
                        }
                    },
                    new {
                        name = "CFDI",
                        icon = "faFileInvoice",
                        path = "/billing",
                        hasAccess = permissions.Any(p => p.Module.Name == "Billing"),
                        moduleId = 6,
                        submodules = new[]
                        {
                            new { name = "Listado", desc = "Buscar, filtrar y gestionar facturas", path = "/billing/invoices", color = "from-sky-500 to-indigo-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ViewInvoices") },
                            new { name = "Nueva Factura", desc = "Crear CFDI con múltiples empresas", path = "/billing/new", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "CreateInvoice") },
                            new { name = "Pendientes", desc = "Facturas por timbrar", path = "/billing/pending", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ViewPending") },
                            new { name = "Timbrado", desc = "Procesar timbrado SAT", path = "/billing/stamp", color = "from-cyan-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ProcessStamping") },
                            new { name = "Timbradas", desc = "Facturas completadas", path = "/billing/stamped", color = "from-lime-500 to-green-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ViewStamped") },
                            new { name = "Cancelaciones", desc = "Gestionar cancelaciones SAT", path = "/billing/cancelled", color = "from-rose-500 to-pink-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ManageCancellations") },
                            new { name = "Empresas", desc = "Gestionar empresas emisoras", path = "/billing/companies", color = "from-violet-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ManageCompanies") },
                            new { name = "Descargas", desc = "XML, PDF y archivos masivos", path = "/billing/downloads", color = "from-slate-500 to-gray-700", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ManageDownloads") },
                            new { name = "Reportes", desc = "Análisis y estadísticas", path = "/billing/reports", color = "from-fuchsia-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ViewReports") },
                            new { name = "Exportación", desc = "Envío a contabilidad", path = "/billing/export", color = "from-indigo-500 to-violet-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ManageExport") },
                            new { name = "Configuración", desc = "Certificados y datos SAT", path = "/billing/settings", color = "from-teal-500 to-cyan-600", hasAccess = permissions.Any(p => p.Resource == "Billing" && p.Name == "ManageSettings") }
                        }
                    },
                    new {
                        name = "Configuración",
                        icon = "faCog",
                        path = "/config",
                        hasAccess = permissions.Any(p => p.Module.Name == "Configuration"),
                        moduleId = 7,
                        submodules = new[]
                        {
                            new { name = "General", desc = "Empresa, sistema, zona horaria y apariencia", path = "/config/general", color = "from-slate-500 to-gray-700", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageGeneral") },
                            new { name = "Usuarios & Roles", desc = "Altas, permisos y perfiles", path = "/config/users", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageUsers") },
                            new { name = "Permisos", desc = "Accesos por módulo/acción", path = "/config/permissions", color = "from-cyan-500 to-blue-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManagePermissions") },
                            new { name = "Sucursales", desc = "Datos y horarios", path = "/config/branches", color = "from-amber-500 to-orange-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageBranches") },
                            new { name = "Almacenes", desc = "Zonas, ubicaciones y picking", path = "/config/warehouses", color = "from-sky-500 to-indigo-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageWarehouses") },
                            new { name = "Paqueterías", desc = "Proveedores y coberturas", path = "/config/shipping", color = "from-teal-500 to-cyan-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageShipping") },
                            new { name = "Listas de Precios", desc = "Niveles, reglas y promos", path = "/config/pricing", color = "from-violet-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManagePricing") },
                            new { name = "Métodos de Pago", desc = "Efectivo, tarjeta, transferencias", path = "/config/payments", color = "from-lime-500 to-green-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManagePayments") },
                            new { name = "CFDI / Facturación", desc = "Certificados, series y timbrado", path = "/config/cfdi", color = "from-rose-500 to-pink-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageCFDI") },
                            new { name = "POS", desc = "Impresoras, caja y atajos", path = "/config/pos", color = "from-fuchsia-500 to-purple-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManagePOS") },
                            new { name = "Integraciones", desc = "Kingdee, Lark, Webhooks", path = "/config/integrations", color = "from-indigo-500 to-violet-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageIntegrations") },
                            new { name = "Notificaciones", desc = "Email, WhatsApp y alertas", path = "/config/notifications", color = "from-red-500 to-orange-500", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageNotifications") },
                            new { name = "Plantillas", desc = "PDF, correos y layouts", path = "/config/templates", color = "from-emerald-500 to-teal-600", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageTemplates") },
                            new { name = "Auditoría", desc = "Bitácora y seguimiento", path = "/config/audit", color = "from-slate-600 to-gray-800", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ViewAudit") },
                            new { name = "Respaldos", desc = "Backups y restauración", path = "/config/backups", color = "from-amber-600 to-orange-700", hasAccess = permissions.Any(p => p.Resource == "Configuration" && p.Name == "ManageBackups") }
                        }
                    }
                };

                // Filtrar solo los módulos con acceso
                var filteredMenuItems = menuItems.Where(m => ((dynamic)m).hasAccess).ToArray();

                // Contar submódulos de forma segura usando reflection
                int totalSubmodules = 0;
                foreach (var item in filteredMenuItems)
                {
                    var itemType = item.GetType();
                    var submodulesProperty = itemType.GetProperty("submodules");
                    if (submodulesProperty != null)
                    {
                        var submodulesArray = submodulesProperty.GetValue(item) as Array;
                        if (submodulesArray != null)
                        {
                            totalSubmodules += submodulesArray.Length;
                        }
                    }
                }

                return Ok(new 
                { 
                    message = "Menu structure retrieved successfully",
                    error = 0,
                    userId = userId,
                    menuItems = filteredMenuItems,
                    totalModules = filteredMenuItems.Length,
                    totalSubmodules = totalSubmodules
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = 2, details = ex.Message });
            }
        }

        private async Task<bool> IsAdministrator()
        {
            var roleId = HttpContext.Items["RoleId"] as int? ?? 0;
            return roleId == 1; // ID del rol Administrador
        }
    }
}