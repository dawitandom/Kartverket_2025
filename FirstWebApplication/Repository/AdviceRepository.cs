using FirstWebApplication.DataContext;
using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Repository
{
    public class AdviceRepository : IAdviceRepository
    {
        private readonly ApplicationContext _context;

        public AdviceRepository(ApplicationContext context)
        {
            _context = context; //leser variabel context og putter den i _context som er en lokal variabel
        }

        public async Task<AdviceDto> AddAdvice(AdviceDto adviceDto)
        {
            await _context.Advices.AddAsync(adviceDto); //Legger til adviceDto i Advices tabellen i databasen
            await _context.SaveChangesAsync(); //Lagrer endringene i databasen
            return adviceDto; //Returnerer det som ble lagt til
        }

        public async Task<AdviceDto> GetElementById(int id)
        {
            //return await _context.Advices.Where(x => x.AdviceId == id).FirstOrDefaultAsync(); //alternativ måte å skrive det på
            var findById = await _context.Advices.Where(getID => getID.AdviceId == id).FirstOrDefaultAsync(); //Henter elementet med gitt id fra Advices tabellen
            if (findById != null)
            {
               return findById;
            }

            else
            {
                return null;
            }
        }

        public async Task<AdviceDto> DeleteById(int id)
        { var elementById = await _context.Advices.FindAsync(id); //Finner elementet med gitt id
        if (elementById != null)
            {
                _context.Advices.Remove(elementById); //Fjerner
                await _context.SaveChangesAsync(); //Lagrer endringene i databasen
                return elementById; //Returnerer det som ble slettet
            }

            else
            {
                return null; //Hvis ikke funnet, returner null
            }
        }

        public async Task<AdviceDto> UpdateAdvice(AdviceDto adviceDto)
        {
            _context.Advices.Update(adviceDto); //Oppdaterer adviceDto i Advices tabellen
            await _context.SaveChangesAsync(); //Lagrer endringene i databasen
            return adviceDto; //Returnerer det som ble oppdatert
        }

        public async Task<IEnumerable<AdviceDto>> GetAllAdvice(AdviceDto adviceDto)
        {
            var GetAllData = await _context.Advices.Take(50).ToListAsync(); //Henter maks 50 elementer fra Advices tabellen og lagrer i en liste
            return GetAllData; //Henter alle elementene i Advices tabellen (maks 50) og returnerer som en liste
        }
    }
}
