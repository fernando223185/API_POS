using Domain.Entities;

namespace Application.Abstractions.Config
{
    /// <summary>
    /// Repositorio para gestión de empresas
    /// </summary>
    public interface ICompanyRepository
    {
        /// <summary>
        /// Obtener empresa por ID
        /// </summary>
        Task<Company?> GetByIdAsync(int id);

        /// <summary>
        /// Obtener empresa por código
        /// </summary>
        Task<Company?> GetByCodeAsync(string code);

        /// <summary>
        /// Obtener empresa por RFC
        /// </summary>
        Task<Company?> GetByTaxIdAsync(string taxId);

        /// <summary>
        /// Obtener todas las empresas activas
        /// </summary>
        Task<List<Company>> GetAllActiveAsync();

        /// <summary>
        /// Obtener empresas paginadas
        /// </summary>
        Task<(List<Company> companies, int totalRecords)> GetPagedAsync(
            int page, 
            int pageSize, 
            bool? isActive = null, 
            string? searchTerm = null);

        /// <summary>
        /// Crear nueva empresa
        /// </summary>
        Task<Company> CreateAsync(Company company);

        /// <summary>
        /// Actualizar empresa
        /// </summary>
        Task UpdateAsync(Company company);

        /// <summary>
        /// Eliminar empresa (físico)
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Verificar si existe una empresa con el RFC dado
        /// </summary>
        Task<bool> ExistsByTaxIdAsync(string taxId, int? excludeId = null);

        /// <summary>
        /// Obtener la empresa principal/matriz
        /// </summary>
        Task<Company?> GetMainCompanyAsync();

        /// <summary>
        /// Contar empresas activas
        /// </summary>
        Task<int> CountActiveAsync();

        /// <summary>
        /// Obtener siguiente código disponible
        /// </summary>
        Task<string> GetNextCodeAsync();
    }
}
