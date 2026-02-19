using Dapper;
using Microsoft.Data.SqlClient;
using Workera_Sync.Interfaces;

namespace Workera_Sync
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IWorkeraService _workeraService;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IWorkeraService workeraService, IConfiguration iconfiguration)
        {
            _logger = logger;
            _workeraService = workeraService;
            _configuration = iconfiguration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _logger.LogInformation("--- INICIANDO CARGA MAESTRA ---");
                // Sincronización de catálogos iniciales
                await _workeraService.SyncAllDepartmentsAsync();
                await _workeraService.SyncAllEmployeesAsync();
                _logger.LogInformation("--- CARGA MAESTRA COMPLETADA ---");

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        // 1. Abrimos una única conexión para este ciclo de sincronización
                        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

                        // 2. Leemos la fecha de la tabla que acabas de crear
                        var inicio = await connection.QueryFirstOrDefaultAsync<DateTime>(
                            "SELECT LastSyncDate FROM dbo.SyncLog WHERE SyncType = 'Attendance'");

                        var fin = DateTime.Now;
                        _logger.LogInformation("Sincronizando marcas desde: {inicio} hasta: {fin}", inicio, fin);

                        int paginaActual = 1;
                        int totalDePaginas = 1;

                        do
                        {
                            var resultado = await _workeraService.GetAttendanceResponseAsync(inicio, fin, paginaActual);
                            if (resultado?.data != null)
                            {
                                totalDePaginas = resultado.totalPages;
                                _logger.LogInformation("--> Procesando página {actual} de {total}", paginaActual, totalDePaginas);

                                foreach (var registro in resultado.data)
                                {
                                    try
                                    {
                                        if (registro.Employee == null) continue;

                                        string sql = @"
                    IF NOT EXISTS (SELECT 1 FROM dbo.Attendance WHERE EmployeeCode = @EmployeeCode AND AttendanceDate = @AttendanceDate AND Checksum = @Checksum)
                    BEGIN
                        INSERT INTO dbo.Attendance (EmployeeCode, AttendanceDate, AttendanceType, AttendanceStatus, Origin, OriginCode, Checksum) 
                        VALUES (@EmployeeCode, @AttendanceDate, @AttendanceType, 'PROCESADO', @Origin, @OriginCode, @Checksum)
                    END";

                                        await connection.ExecuteAsync(sql, new
                                        {
                                            EmployeeCode = registro.Employee.code.ToString(),
                                            AttendanceDate = registro.AttendanceDate,
                                            AttendanceType = registro.AttendanceType,
                                            Origin = registro.origin ?? "WORKERA",
                                            OriginCode = registro.originCode ?? "API",
                                            Checksum = registro.checksum ?? "S/C"
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        // Si falla un registro, lo logueamos pero seguimos con el siguiente
                                        _logger.LogWarning("Registro omitido (ID: {id}): {msg}", registro.identification, ex.Message);
                                    }
                                }

                                // --- MEJORA CLAVE: Actualizamos el log AL FINAL DE CADA PÁGINA ---
                                // Así, si el programa se corta en la página 50, la próxima vez empieza en la 50 y no en la 1.
                                await connection.ExecuteAsync(
                                    "UPDATE dbo.SyncLog SET LastSyncDate = @fechaProgreso WHERE SyncType = 'Attendance'",
                                    new { fechaProgreso = resultado.data.Max(x => x.AttendanceDate) });
                            }

                            paginaActual++;
                            // Eliminamos el Delay de 10 segundos aquí para procesar rápido

                        } while (paginaActual <= totalDePaginas);

                        // 4. ACTUALIZAMOS LA TABLA DE CONTROL: Marcamos el éxito de este ciclo
                        await connection.ExecuteAsync(
                            "UPDATE dbo.SyncLog SET LastSyncDate = @fin WHERE SyncType = 'Attendance'", new { fin });

                        _logger.LogInformation("Ciclo completado con éxito. Esperando 5 minutos...");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error en el ciclo de asistencia: {msg}", ex.Message);
                    }

                    // Espera de cortesía para no saturar la API
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical("Error fatal en el servicio: {msg}", ex.Message);
            }
        }

    }
}