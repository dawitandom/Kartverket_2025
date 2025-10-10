using FirstWebApplication.Models;

namespace FirstWebApplication.Repository
{
    public interface IAdviceRepository
    {
        Task<AdviceDto> AddAdvice(AdviceDto advicedto);

        Task<AdviceDto> GetElementById(int id);

        Task<IEnumerable<AdviceDto>> GetAllAdvice(AdviceDto adviceDto);

        Task<AdviceDto> DeleteById(int id);

        Task<AdviceDto> UpdateAdvice(AdviceDto adviceDto);

        //legge til obstacle??
    }
}
