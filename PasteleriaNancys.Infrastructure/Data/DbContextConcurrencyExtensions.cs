using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PasteleriaNancys.Application.Common.Exceptions;

namespace PasteleriaNancys.Infrastructure.Data
{
    /// <summary>
    /// Traduce condiciones de carrera de EF Core/SQL Server (choque de concurrencia optimista vía
    /// RowVersion, o violación de índice único) a ConflictoException, para que la capa Application
    /// nunca necesite referenciar tipos de EF Core directamente.
    /// </summary>
    public static class DbContextConcurrencyExtensions
    {
        public static async Task GuardarConControlDeConcurrenciaAsync(this DbContext context, string mensajeConflicto)
        {
            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConflictoException(mensajeConflicto);
            }
            catch (DbUpdateException ex) when (EsViolacionDeIndiceUnico(ex))
            {
                throw new ConflictoException(mensajeConflicto);
            }
        }

        private static bool EsViolacionDeIndiceUnico(DbUpdateException ex) =>
            ex.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}
