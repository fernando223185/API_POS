using Application.Abstractions.Authorization;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services
{
    /// <summary>
    /// Servicio de permisos - SISTEMA UNIFICADO
    /// Usa RoleModulePermissions y UserModulePermissions
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly POSDbContext _context;

        public PermissionService(POSDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Verifica si un usuario tiene permiso para un recurso y acción
        /// Mapea Resource + Action a Módulo/Submódulo
        /// </summary>
        public async Task<bool> HasPermissionAsync(int userId, string resource, string action)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return false;

            // Mapear resource/action a módulo/submódulo/acción
            var (moduleId, submoduleId, permissionType) = MapResourceActionToModule(resource, action);

            if (moduleId == 0)
                return false; // Mapeo no encontrado

            // 1. Verificar permisos personalizados del usuario (tienen prioridad)
            var userPermission = await _context.UserModulePermissions
                .Where(up => up.UserId == userId &&
                           up.ModuleId == moduleId &&
                           (submoduleId == null || up.SubmoduleId == submoduleId) &&
                           up.HasAccess)
                .FirstOrDefaultAsync();

            if (userPermission != null)
            {
                return CheckPermissionType(userPermission.CanView, userPermission.CanCreate,
                                          userPermission.CanEdit, userPermission.CanDelete, permissionType);
            }

            // 2. Si no tiene permisos personalizados, verificar permisos del rol
            var rolePermission = await _context.RoleModulePermissions
                .Where(rp => rp.RoleId == user.RoleId &&
                           rp.ModuleId == moduleId &&
                           (submoduleId == null || rp.SubmoduleId == submoduleId) &&
                           rp.HasAccess)
                .FirstOrDefaultAsync();

            if (rolePermission != null)
            {
                return CheckPermissionType(rolePermission.CanView, rolePermission.CanCreate,
                                          rolePermission.CanEdit, rolePermission.CanDelete, permissionType);
            }

            return false;
        }

        /// <summary>
        /// Obtiene todos los permisos de un usuario (combinando rol + personalizados)
        /// Retorna lista de strings en formato "Resource:Action"
        /// </summary>
        public async Task<List<string>> GetUserPermissionClaimsAsync(int userId)
        {
            var user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId && u.Active);

            if (user == null || !user.Role.IsActive)
                return new List<string>();

            var claims = new HashSet<string>();

            // 1. Obtener permisos del rol
            var rolePermissions = await _context.RoleModulePermissions
                .Where(rp => rp.RoleId == user.RoleId && rp.HasAccess)
                .ToListAsync();

            foreach (var rp in rolePermissions)
            {
                var resourceActions = MapModuleToResourceActions(rp.ModuleId, rp.SubmoduleId,
                                                                 rp.CanView, rp.CanCreate, rp.CanEdit, rp.CanDelete);
                claims.UnionWith(resourceActions);
            }

            // 2. Agregar permisos personalizados del usuario
            var userPermissions = await _context.UserModulePermissions
                .Where(up => up.UserId == userId && up.HasAccess)
                .ToListAsync();

            foreach (var up in userPermissions)
            {
                var resourceActions = MapModuleToResourceActions(up.ModuleId, up.SubmoduleId,
                                                                 up.CanView, up.CanCreate, up.CanEdit, up.CanDelete);
                claims.UnionWith(resourceActions);
            }

            return claims.ToList();
        }

        #region Helper Methods

        /// <summary>
        /// Mapea Resource + Action a ModuleId, SubmoduleId y tipo de permiso
        /// </summary>
        private (int moduleId, int? submoduleId, string permissionType) MapResourceActionToModule(string resource, string action)
        {
            // Mapeo Customer
            if (resource.Equals("Customer", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("Create", StringComparison.OrdinalIgnoreCase))
                    return (5, 52, "Create"); // Módulo Clientes ? Nuevo Cliente
                if (action.Equals("Read", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ViewList", StringComparison.OrdinalIgnoreCase))
                    return (5, 51, "View"); // Módulo Clientes ? Listado de Clientes
                if (action.Equals("Update", StringComparison.OrdinalIgnoreCase))
                    return (5, 51, "Edit"); // Módulo Clientes ? Listado de Clientes
                if (action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                    return (5, 51, "Delete"); // Módulo Clientes ? Listado de Clientes
            }

            // Mapeo Sale / Sales (singular y plural)
            if (resource.Equals("Sale", StringComparison.OrdinalIgnoreCase) ||
                resource.Equals("Sales", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("Create", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("CreateSale", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ProcessPayment", StringComparison.OrdinalIgnoreCase))
                    return (2, 10, "Create"); // Módulo Ventas ? Nueva Venta (ID: 10)
                
                if (action.Equals("View", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("Read", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ViewHistory", StringComparison.OrdinalIgnoreCase))
                    return (2, 11, "View"); // Módulo Ventas ? Lista de Ventas (ID: 11)
                
                if (action.Equals("Update", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("Edit", StringComparison.OrdinalIgnoreCase))
                    return (2, 11, "Edit"); // Módulo Ventas ? Lista de Ventas (ID: 11)
                
                if (action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                    return (2, 11, "Delete"); // Módulo Ventas ? Lista de Ventas (ID: 11)
                
                if (action.Equals("Cancel", StringComparison.OrdinalIgnoreCase))
                    return (2, 11, "Delete"); // Módulo Ventas ? Lista de Ventas (permite cancelar)
                
                if (action.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ProcessPayments", StringComparison.OrdinalIgnoreCase))
                    return (2, 13, "Create"); // Módulo Ventas ? Cobranza (ID: 13)
                
                if (action.Equals("ViewReports", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ViewStatistics", StringComparison.OrdinalIgnoreCase))
                    return (2, 12, "View"); // Módulo Ventas ? Reportes de Ventas (ID: 12)
                
                if (action.Equals("ProcessRefund", StringComparison.OrdinalIgnoreCase))
                    return (2, null, "Create"); // Módulo Ventas ? Devoluciones (si existe)
            }

            // Mapeo Product
            if (resource.Equals("Product", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("Create", StringComparison.OrdinalIgnoreCase))
                    return (3, 32, "Create"); // Módulo Productos ? Nuevo Producto
                if (action.Equals("Read", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ViewCatalog", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ExportProducts", StringComparison.OrdinalIgnoreCase))
                    return (3, 31, "View"); // Módulo Productos ? Catálogo de Productos
                if (action.Equals("Update", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("ManagePrices", StringComparison.OrdinalIgnoreCase))
                    return (3, 31, "Edit"); // Módulo Productos ? Catálogo de Productos
                if (action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                    return (3, 31, "Delete"); // Módulo Productos ? Catálogo de Productos
                if (action.Equals("ImportProducts", StringComparison.OrdinalIgnoreCase))
                    return (3, 33, "Create"); // Módulo Productos ? Importar Productos
                if (action.Equals("ManageCategories", StringComparison.OrdinalIgnoreCase))
                    return (3, 34, "Edit"); // Módulo Productos ? Categorías
            }

            // Mapeo User
            if (resource.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("Create", StringComparison.OrdinalIgnoreCase))
                    return (8, 81, "Create"); // Módulo Configuración ? Usuarios
                if (action.Equals("Read", StringComparison.OrdinalIgnoreCase))
                    return (8, 81, "View"); // Módulo Configuración ? Usuarios
                if (action.Equals("Update", StringComparison.OrdinalIgnoreCase))
                    return (8, 81, "Edit"); // Módulo Configuración ? Usuarios
                if (action.Equals("Delete", StringComparison.OrdinalIgnoreCase))
                    return (8, 81, "Delete"); // Módulo Configuración ? Usuarios
            }

            // Mapeo Inventory
            if (resource.Equals("Inventory", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("ViewStock", StringComparison.OrdinalIgnoreCase))
                    return (4, 41, "View"); // Módulo Inventario ? Stock Actual
                if (action.Equals("ViewKardex", StringComparison.OrdinalIgnoreCase))
                    return (4, 42, "View"); // Módulo Inventario ? Kardex
                if (action.Equals("AdjustStock", StringComparison.OrdinalIgnoreCase) ||
                    action.Equals("TransferStock", StringComparison.OrdinalIgnoreCase))
                    return (4, 44, "Create"); // Módulo Inventario ? Movimientos
            }

            // Mapeo Billing (CFDI)
            if (resource.Equals("Billing", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("CreateInvoice", StringComparison.OrdinalIgnoreCase))
                    return (6, 61, "Create"); // Módulo CFDI ? Nueva Factura
                if (action.Equals("ViewInvoices", StringComparison.OrdinalIgnoreCase))
                    return (6, 62, "View"); // Módulo CFDI ? Facturas Emitidas
                if (action.Equals("ViewPending", StringComparison.OrdinalIgnoreCase))
                    return (6, 63, "View"); // Módulo CFDI ? Facturas Pendientes
                if (action.Equals("ProcessStamping", StringComparison.OrdinalIgnoreCase))
                    return (6, 63, "Create"); // Módulo CFDI ? Facturas Pendientes
                if (action.Equals("ViewStamped", StringComparison.OrdinalIgnoreCase))
                    return (6, 64, "View"); // Módulo CFDI ? Facturas Timbradas
            }

            // Mapeo Configuration
            if (resource.Equals("Configuration", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("ManageGeneral", StringComparison.OrdinalIgnoreCase))
                    return (8, null, "View"); // Módulo Configuración (general)
                if (action.Equals("ManageUsers", StringComparison.OrdinalIgnoreCase))
                    return (8, 81, "Edit"); // Módulo Configuración ? Usuarios
                if (action.Equals("ManagePermissions", StringComparison.OrdinalIgnoreCase))
                    return (8, 83, "Edit"); // Módulo Configuración ? Permisos
                if (action.Equals("ManageCompany", StringComparison.OrdinalIgnoreCase))
                    return (8, 84, "Edit"); // Módulo Configuración ? Datos de Empresa
                if (action.Equals("ManageBranches", StringComparison.OrdinalIgnoreCase))
                    return (8, 85, "Edit"); // Módulo Configuración ? Sucursales
            }

            // Mapeo Reports
            if (resource.Equals("Reports", StringComparison.OrdinalIgnoreCase))
            {
                if (action.Equals("ViewSalesReport", StringComparison.OrdinalIgnoreCase))
                    return (7, 71, "View"); // Módulo Reportes ? Reporte de Ventas
                if (action.Equals("ViewInventoryReport", StringComparison.OrdinalIgnoreCase))
                    return (7, 72, "View"); // Módulo Reportes ? Reporte de Inventario
                if (action.Equals("ViewProductReport", StringComparison.OrdinalIgnoreCase))
                    return (7, 73, "View"); // Módulo Reportes ? Reporte de Productos
                if (action.Equals("ViewCustomerReport", StringComparison.OrdinalIgnoreCase))
                    return (7, 74, "View"); // Módulo Reportes ? Reporte de Clientes
                if (action.Equals("ExportReports", StringComparison.OrdinalIgnoreCase))
                    return (7, null, "View"); // Módulo Reportes (general)
            }

            return (0, null, ""); // No encontrado
        }

        /// <summary>
        /// Verifica si tiene el tipo de permiso requerido
        /// </summary>
        private bool CheckPermissionType(bool canView, bool canCreate, bool canEdit, bool canDelete, string permissionType)
        {
            return permissionType.ToLower() switch
            {
                "view" => canView,
                "create" => canCreate,
                "edit" => canEdit,
                "delete" => canDelete,
                _ => false
            };
        }

        /// <summary>
        /// Mapea módulo/submódulo a Resource:Action claims
        /// </summary>
        private List<string> MapModuleToResourceActions(int moduleId, int? submoduleId,
                                                         bool canView, bool canCreate, bool canEdit, bool canDelete)
        {
            var claims = new List<string>();

            // Retornar claims basados en los permisos que tiene
            // Esto es una simplificación, se puede extender según necesidad
            var modulePrefix = GetModuleResourcePrefix(moduleId, submoduleId);

            if (!string.IsNullOrEmpty(modulePrefix))
            {
                if (canView) claims.Add($"{modulePrefix}:Read");
                if (canCreate) claims.Add($"{modulePrefix}:Create");
                if (canEdit) claims.Add($"{modulePrefix}:Update");
                if (canDelete) claims.Add($"{modulePrefix}:Delete");
            }

            return claims;
        }

        /// <summary>
        /// Obtiene el prefijo del Resource según el módulo
        /// </summary>
        private string GetModuleResourcePrefix(int moduleId, int? submoduleId)
        {
            return moduleId switch
            {
                2 => "Sales", // Módulo Ventas
                3 => "Product",
                4 => "Inventory",
                5 => "Customer",
                6 => "Billing",
                7 => "Reports",
                8 => "Configuration",
                _ => ""
            };
        }

        #endregion
    }
}