using System.Collections.Generic;
using System.Threading.Tasks;
using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    public interface IAdviceRepository
    {
        IEnumerable<ObstacleTypeEntity> GetAllObstacleTypes();

        Task<AdviceDto> AddAdvice(AdviceDto advice);
        Task<IEnumerable<AdviceDto>> GetAllAdvice();
    }
}