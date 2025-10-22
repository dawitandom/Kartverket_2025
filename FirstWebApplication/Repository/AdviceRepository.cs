using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FirstWebApplication.Models;
using FirstWebApplication.DataContext;

namespace FirstWebApplication.Repository
{
    public class AdviceRepository : IAdviceRepository
    {
        private readonly ApplicationContext _context;

        public AdviceRepository(ApplicationContext context)
        {
            _context = context;
        }
        
        public IEnumerable<ObstacleTypeEntity> GetAllObstacleTypes()
        {
            return _context.ObstacleTypes.OrderBy(o => o.SortedOrder).ToList();
        }

        public async Task<AdviceDto> AddAdvice(AdviceDto advice)
        {
            // Hvis AdviceDto ikke er en database-entitet, må du mappe den til en entitet først
            // For nå, anta at vi bare returnerer den som ble sendt inn
            // (Dette må tilpasses basert på din faktiske database-struktur)
            
            return await Task.FromResult(advice);
        }

        public async Task<IEnumerable<AdviceDto>> GetAllAdvice()
        {
            // Implementer hvis du har en Advice-tabell i databasen
            return await Task.FromResult(new List<AdviceDto>());
        }
    }
}