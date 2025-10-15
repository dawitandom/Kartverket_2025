using System.Collections.Generic;
using System.Threading.Tasks;
using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    public interface IAdviceRepository
    {
        Task<AdviceDto> AddAdvice(AdviceDto advice);
        Task<IEnumerable<AdviceDto>> GetAllAdvice();
    }
}