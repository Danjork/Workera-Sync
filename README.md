Workera-Sync 🚀
Servicio de Windows (Worker Service) encargado de la sincronización automatizada de datos de asistencia desde la API de Workera hacia una base de datos local en SQL Server.

📋 Descripción del Proyecto
Este sistema realiza un consumo de servicios REST para extraer información de departamentos, empleados y registros de asistencia, garantizando la integridad de los datos y evitando duplicidades mediante una lógica de sincronización incremental. El motor de base de datos utilizado es SQL Server 2017.

🛠️ Tecnologías y Requisitos
Framework: .NET 8.0.

Acceso a Datos: Dapper (Micro-ORM para alto rendimiento).

Base de Datos: SQL Server 2017 o superior.

Seguridad: Implementación forzada de TLS 1.2/1.3 para comunicaciones seguras con la API.

🗄️ Estructura de Base de Datos
El proyecto requiere una base de datos llamada WORKERA con las siguientes tablas:

dbo.BranchOffice: Catálogo de sucursales.

dbo.Department: Registro de áreas y departamentos.

dbo.Employee: Información maestra de trabajadores (vinculada a departamentos).

dbo.Attendance: Histórico de marcas (Entradas/Salidas) con validación de duplicados mediante Checksum.

dbo.SyncLog: Tabla de control que almacena la fecha del último registro procesado (LastSyncDate).

⚙️ Configuración (appsettings.json)
Es fundamental configurar correctamente la cadena de conexión y la URL base de la API:

JSON

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=DASHBOARDSRV\\SQLEXPRESS;Database=WORKERA;Integrated Security=True;trustServerCertificate=True;"
  },
  "WorkeraApi": {
    "BaseUrl": "https://workera.com/apiClient/v1/"
  }
}
🔄 Lógica de Sincronización
El servicio opera en ciclos automáticos (configurados cada 5 minutos) con dos fases principales:

Carga Maestra de Catálogos: Al iniciar, el sistema sincroniza automáticamente Departamentos y Empleados para asegurar que no existan errores de llaves foráneas.

Sincronización Incremental de Marcas:

Lee la fecha de la última sincronización desde dbo.SyncLog.

Descarga solo los registros nuevos generados desde esa fecha.

Utiliza una sentencia IF NOT EXISTS en SQL para prevenir duplicidad de marcas.

Actualiza el LastSyncDate en la base de datos al finalizar cada página procesada con éxito.

🚀 Despliegue
Para instalar el ejecutable como un servicio de Windows, utilice el siguiente comando en una terminal con privilegios de administrador:

PowerShell

sc create WorkeraSyncService binPath= "C:\Ruta\Al\Ejecutable\Workera-Sync.exe" start= auto
📝 Mantenimiento
Logs: El sistema emite información detallada en consola y logs del sistema sobre el estado de cada página procesada.

Errores Comunes: Fallos en la conexión a la base de datos (verificar permisos del usuario de Windows) o errores 404 en la API (verificar la URL en appsettings.json).

Desarrollado por: Departamento de TI
Fecha de última actualización: Febrero 2026