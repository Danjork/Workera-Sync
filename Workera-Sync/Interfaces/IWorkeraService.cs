using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Workera_Sync.Models;

namespace Workera_Sync.Interfaces
{
    public interface IWorkeraService
    {
        
        Task<AttendanceResponse> GetAttendanceResponseAsync(DateTime start, DateTime end, int page = 1);

        // 3. Añadimos los nuevos métodos para que sean visibles desde el Worker
        Task SyncDepartmentsAsync(List<DepartmentData> departments);
        Task SyncAllEmployeesAsync();

        Task SyncAllDepartmentsAsync();

    }
}
