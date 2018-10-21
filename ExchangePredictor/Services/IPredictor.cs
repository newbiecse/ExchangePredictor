using ExchangePredictor.Models;
using System.Collections.Generic;

namespace ExchangePredictor.Services
{
    public interface IPredictor
    {
        decimal Predict(int month, IEnumerable<MonthlyRate> rates);
    }
}
