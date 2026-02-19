using Microsoft.Data.SqlClient;
using System.Data;
using Workera_Sync;
using Workera_Sync.Interfaces;
using Workera_Sync.Models;
using Workera_Sync.Services;

var builder = Host.CreateApplicationBuilder(args);
// 1. Obtenemos la cadena de conexión del appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient<IWorkeraService, WorkeraService>();
// 2. Registramos una función que cree la conexión a SQL Server cuando la necesitemos
builder.Services.AddTransient<IDbConnection>((sp) =>
    new SqlConnection(connectionString));

var host = builder.Build();
host.Run();
