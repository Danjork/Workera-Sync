
using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Workera_Sync.Interfaces;
using Workera_Sync.Models;

namespace Workera_Sync.Services
{
    public class WorkeraService: IWorkeraService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private string? _baseUrl;
        private readonly string _connectionString;//1.
        private readonly ILogger<WorkeraService> _logger;


        //constructor
        public WorkeraService(HttpClient httpClient, IConfiguration iconfiguration, ILogger<WorkeraService> logger)
        {
            _httpClient = httpClient;
            _configuration = iconfiguration;
            var apiUser = _configuration["ApiSettings:API_USER"];
            var apiKey = _configuration["ApiSettings:API_KEY"];
            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            _baseUrl = _configuration["ApiSettings:BaseUrl"];
            _logger = logger; // <-- Lo asignamos



            _httpClient.BaseAddress = new Uri(baseUrl!);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("API_USER", apiUser);
            _httpClient.DefaultRequestHeaders.Add("API_KEY", apiKey);

            // 2. Leemos la conexión directamente de tu 'appsettings.json'
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                                ?? throw new Exception("No se encontró la conexión DefaultConnection");
        }

        //invaremos el Iworkera el metodo GetAttendaceRecordsAsync
        public async Task<AttendanceResponse> GetAttendanceResponseAsync( DateTime start, DateTime end, int page = 1)
        {


            var nuevaBaseUrl = $"{_baseUrl}/attendanceData?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}&page={page}";

            var response = await _httpClient.GetAsync(nuevaBaseUrl);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<AttendanceResponse>();

            return resultado!;
        }

        public async Task SyncDepartmentsAsync(List<DepartmentData> departments)
        {
            using var db = new SqlConnection(_connectionString);

            // El SQL utiliza la estructura de tu tabla dbo.Department
            string sql = @"
        MERGE INTO dbo.Department AS Target
        USING (SELECT @Code AS Code) AS Source
        ON (Target.Code = Source.Code)
        WHEN MATCHED THEN
            UPDATE SET 
                WorkeraDeptId = @WorkeraDeptId,
                Name = @Name,
                Description = @Description,
                Status = @Status,
                DefaultDepartment = @DefaultDepartment,
                EmployeesCount = @EmployeesCount
        WHEN NOT MATCHED THEN
            INSERT (WorkeraDeptId, Code, Name, Description, Status, DefaultDepartment, EmployeesCount)
            VALUES (@WorkeraDeptId, @Code, @Name, @Description, @Status, @DefaultDepartment, @EmployeesCount);";

            foreach (var dept in departments)
            {
                await db.ExecuteAsync(sql, new
                {
                    WorkeraDeptId = dept.Id,
                    Code = dept.Code,
                    Name = dept.Name,
                    Description = dept.Description,
                    Status = dept.Status,
                    DefaultDepartment = dept.DefaultDepartment ? 1 : 0,
                    EmployeesCount = dept.EmployeesCount
                });
            }
        }

        public async Task SyncEmployeesAsync(List<EmployeeData> employees)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                string sql = @"
            MERGE INTO dbo.Employee AS Target
            USING (SELECT @Code AS Code) AS Source
            ON (Target.Code = Source.Code)
            WHEN MATCHED THEN
                UPDATE SET 
                    Identification = @Identification,
                    Name = @Name,
                    LastName = @LastName,
                    BranchOfficeCode = @BranchOfficeCode,
                    DepartmentCode = @DepartmentCode,
                    EmployeeStatus = @EmployeeStatus
            WHEN NOT MATCHED THEN
                INSERT (Code, Identification, Name, LastName, BranchOfficeCode, DepartmentCode, EmployeeStatus)
                VALUES (@Code, @Identification, @Name, @LastName, @BranchOfficeCode, @DepartmentCode, @EmployeeStatus);";

                foreach (var emp in employees)
                {
                    await db.ExecuteAsync(sql, new
                    {
                        Code = emp.code.ToString(),
                        Identification = emp.identification,
                        Name = emp.name,
                        LastName = emp.lastName,
                        BranchOfficeCode = "SUC_01", // Forzamos la sucursal única
                        DepartmentCode = emp.department ?? "GEN", // Valor por defecto si viene nulo
                        EmployeeStatus = emp.employeeStatus
                    });
                }
            }
        }

        public async Task SyncAllEmployeesAsync()
        {
            int currentPage = 1;
            int totalPages = 1;

            do
            {
                // 1. Descargamos la página actual de empleados
                var response = await _httpClient.GetFromJsonAsync<EmployeeResponse>($"v1/employee?page={currentPage}");

                if (response?.Data != null)
                {
                    // AÑADE ESTA LÍNEA: Confirma que llegaron datos
                    _logger.LogInformation("Página {p} recibida con {c} empleados. Iniciando guardado en DB...",
                        currentPage, response.Data.Count);

                    totalPages = response.TotalPages;
                    using var db = new SqlConnection(_connectionString);

                    // 2. SQL MERGE para insertar o actualizar
                    string sql = @"
                MERGE INTO dbo.Employee AS Target
                USING (SELECT @Code AS Code) AS Source
                ON (Target.Code = Source.Code)
                WHEN MATCHED THEN
                    UPDATE SET 
                        Identification = @Identification,
                        Name = @Name,
                        LastName = @LastName,
                        BranchOfficeCode = @BranchOfficeCode,
                        DepartmentCode = @DepartmentCode,
                        EmployeeStatus = @EmployeeStatus,
                        CompanyName = @CompanyName
                WHEN NOT MATCHED THEN
                    INSERT (Code, Identification, Name, LastName, BranchOfficeCode, DepartmentCode, EmployeeStatus, CompanyName)
                    VALUES (@Code, @Identification, @Name, @LastName, @BranchOfficeCode, @DepartmentCode, @EmployeeStatus, @CompanyName);";

                    foreach (var emp in response.Data)
                    {
                        await db.ExecuteAsync(sql, new
                        {
                            Code = emp.code.ToString(),
                            Identification = emp.identification ?? "S/I",
                            Name = emp.name,
                            LastName = emp.lastName,
                            BranchOfficeCode = "MATRIZ", // Tu sucursal única
                            DepartmentCode = emp.department ?? "INICIAL", // Evitamos nulos para la FK
                            EmployeeStatus = emp.employeeStatus,
                            CompanyName = emp.companyName
                        });
                    }
                    _logger.LogInformation("Sincronizada página {p} de {t} de empleados.", currentPage, totalPages);
                }
                currentPage++;
            } while (currentPage <= totalPages);
        }


        public async Task SyncAllDepartmentsAsync()
        {
            int currentPage = 1;
            int totalPages = 1;

            do
            {
                var response = await _httpClient.GetFromJsonAsync<DepartmentResponse>($"v1/department?page={currentPage}");
                if (response?.Data != null)
                {
                    totalPages = response.TotalPages;
                    using var db = new SqlConnection(_connectionString);

                    string sql = @"
                MERGE INTO dbo.Department AS Target
                USING (SELECT @Code AS Code) AS Source
                ON (Target.Code = Source.Code)
                WHEN MATCHED THEN
                    UPDATE SET Name = @Name, Status = @Status, EmployeesCount = @EmployeesCount
                WHEN NOT MATCHED THEN
                    INSERT (Code, Name, Status, EmployeesCount)
                    VALUES (@Code, @Name, @Status, @EmployeesCount);";

                    foreach (var dept in response.Data)
                    {
                        await db.ExecuteAsync(sql, new
                        {
                            Code = dept.Code,
                            Name = dept.Name,
                            Status = dept.Status,
                            EmployeesCount = dept.EmployeesCount
                        });
                    }
                    _logger.LogInformation("Departamentos: Página {p} de {t} sincronizada.", currentPage, totalPages);
                }
                currentPage++;
            } while (currentPage <= totalPages);
        }

    }
}
