using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedProductCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =======================================
            // SEED DE CATEGORÍAS DE PRODUCTOS
            // =======================================
            
            migrationBuilder.Sql(@"
                -- Insertar categorías principales de productos
                INSERT INTO [ProductCategories] ([Name], [Description], [Code], [IsActive], [CreatedAt])
                VALUES 
                    ('Electrónica', 'Dispositivos electrónicos, smartphones, computadoras, televisores, audio y video', 'ELECT', 1, GETUTCDATE()),
                    ('Ropa y Accesorios', 'Vestimenta, calzado, bolsos, joyería y accesorios de moda', 'ROPA', 1, GETUTCDATE()),
                    ('Hogar y Jardín', 'Muebles, decoración, artículos para el hogar, jardinería y herramientas', 'HOGAR', 1, GETUTCDATE()),
                    ('Deportes', 'Artículos deportivos, ropa deportiva, equipos de ejercicio y accesorios', 'DEPORT', 1, GETUTCDATE()),
                    ('Salud y Belleza', 'Productos de cuidado personal, cosméticos, vitaminas y equipos médicos', 'SALUD', 1, GETUTCDATE()),
                    ('Libros', 'Libros físicos, e-books, revistas, material educativo y papelería', 'LIBROS', 1, GETUTCDATE()),
                    ('Juguetes', 'Juguetes para niños, juegos de mesa, figuras de acción y entretenimiento infantil', 'JUGUE', 1, GETUTCDATE()),
                    ('Automotriz', 'Autopartes, accesorios para vehículos, lubricantes y herramientas automotrices', 'AUTO', 1, GETUTCDATE()),
                    ('Alimentos', 'Alimentos procesados, bebidas, dulces, productos orgánicos y suplementos', 'ALIMEN', 1, GETUTCDATE()),
                    ('Otros', 'Productos diversos que no encajan en las categorías anteriores', 'OTROS', 1, GETUTCDATE());
            ");

            // Verificar inserción exitosa
            migrationBuilder.Sql(@"
                -- Mostrar mensaje de confirmación en logs
                PRINT '✅ Se insertaron ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' categorías de productos exitosamente';
                PRINT '📋 Categorías disponibles:';
                PRINT '   1. Electrónica (ELECT)';
                PRINT '   2. Ropa y Accesorios (ROPA)';
                PRINT '   3. Hogar y Jardín (HOGAR)';
                PRINT '   4. Deportes (DEPORT)';
                PRINT '   5. Salud y Belleza (SALUD)';
                PRINT '   6. Libros (LIBROS)';
                PRINT '   7. Juguetes (JUGUE)';
                PRINT '   8. Automotriz (AUTO)';
                PRINT '   9. Alimentos (ALIMEN)';
                PRINT '   10. Otros (OTROS)';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: Eliminar las categorías insertadas
            migrationBuilder.Sql(@"
                -- Eliminar productos que puedan estar usando estas categorías (opcional)
                -- UPDATE Products SET CategoryId = NULL WHERE CategoryId IN (SELECT Id FROM ProductCategories WHERE Code IN ('ELECT', 'ROPA', 'HOGAR', 'DEPORT', 'SALUD', 'LIBROS', 'JUGUE', 'AUTO', 'ALIMEN', 'OTROS'));
                
                -- Eliminar las categorías insertadas
                DELETE FROM [ProductCategories] 
                WHERE [Code] IN ('ELECT', 'ROPA', 'HOGAR', 'DEPORT', 'SALUD', 'LIBROS', 'JUGUE', 'AUTO', 'ALIMEN', 'OTROS');
                
                PRINT '🗑️ Categorías de productos eliminadas exitosamente (rollback)';
            ");
        }
    }
}
