namespace Application.Abstractions.Common
{
    /// <summary>
    /// Servicio para generación de códigos únicos y secuenciales
    /// </summary>
    public interface ICodeGeneratorService
    {
        /// <summary>
        /// Genera el siguiente código para una entidad
        /// </summary>
        /// <param name="prefix">Prefijo del código (ej: "OC", "REC", "MOV")</param>
        /// <param name="tableName">Nombre de la tabla</param>
        /// <param name="codeColumnName">Nombre de la columna del código</param>
        /// <param name="length">Longitud del número (ej: 3 para "001")</param>
        /// <returns>Código generado (ej: "OC-001")</returns>
        Task<string> GenerateNextCodeAsync(string prefix, string tableName, string codeColumnName = "Code", int length = 3);
    }
}
