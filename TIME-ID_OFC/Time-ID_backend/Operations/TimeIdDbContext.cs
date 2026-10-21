using Microsoft.EntityFrameworkCore;

namespace TimeId.Operations;

/// <summary>
/// Contexto SQLite preparado para persistência futura. A aplicação ainda usa DataStore;
/// nenhuma conexão, tabela ou arquivo SQLite é criado durante a inicialização.
/// </summary>
public sealed class TimeIdDbContext(DbContextOptions<TimeIdDbContext> options) : DbContext(options)
{
}
