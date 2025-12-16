using AdditionApi.Models;
using AdditionApi.Services;

namespace AdditionApi.Services
{
    public interface IDatabaseService
    {
        IEnumerable<Calculation> GetCalculations();
        void SaveCalculation(Calculation calculation);
    }
}
