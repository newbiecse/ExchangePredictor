namespace ExchangePredictor.Models
{
    public class MonthlyRate
    {
        public MonthlyRate(int month, decimal rate)
        {
            Month = month;
            Rate = rate;
        }

        public int Month { get; set; }
        public decimal Rate { get; set; }
    }
}
