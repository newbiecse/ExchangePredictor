using ExchangePredictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExchangePredictor.Services
{
    public class Predictor : IPredictor
    {
        private decimal RegressionEquationFormula(int x, IEnumerable<Tuple<int, decimal>> learningSet)
        {
            int N = learningSet.Count();
            var ΣX = learningSet.Sum(i => i.Item1);
            var ΣY = learningSet.Sum(i => i.Item2);
            var ΣXY = learningSet.Sum(i => i.Item1 * i.Item2);
            var ΣX2 = learningSet.Sum(i => i.Item1 * i.Item1);

            var b = (N * ΣXY - ΣX * ΣY) / (N * ΣX2 - ΣX * ΣX);
            var a = (ΣY - b * ΣX) / N;

            var y = a + b * x;

            return y;
        }

        public decimal Predict(int month, IEnumerable<MonthlyRate> rates)
        {
            if (rates == null)
            {
                throw new ArgumentNullException(nameof(rates));
            }

            if (!rates.Any())
            {
                throw new Exception($"The {nameof(rates)} is empty.");
            }

            var learningSet = rates.Select(i => new Tuple<int, decimal>(i.Month, i.Rate));

            var result = RegressionEquationFormula(month, learningSet);

            return Math.Round(result, 3);
        }
    }
}
