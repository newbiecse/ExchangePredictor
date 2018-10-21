using ExchangePredictor.Models;
using ExchangePredictor.Services;
using ExchangePredictor.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ExchangePredictor
{
    public class App
    {
        private readonly ILogger<App> _logger;
        private readonly OpenExchangeSettings _config;
        private readonly IPredictor _predictor;
        private List<string> _currencies = new List<string>();

        public App(ILogger<App> logger, IOptions<OpenExchangeSettings> config, IPredictor predictor)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config != null ? config.Value : throw new ArgumentNullException(nameof(config));
            _predictor = predictor ?? throw new ArgumentNullException(nameof(predictor));
        }

        private MonthlyRate Parse(DateTime day, string response, string desCurrency)
        {
            JObject obj = JObject.Parse(response);
            JToken token = obj["rates"][desCurrency.ToUpper()];

            var rate = (decimal)token;

            var result = new MonthlyRate(day.Month, rate);

            return result;
        }

        private async Task GetCurrenciesAsync()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.Endpoint);
                var url = $"{_config.Endpoint}/api/currencies.json";
                var response = await client.GetStringAsync(url);

                JObject obj = JObject.Parse(response);
                _currencies = obj.Properties().Select(p => p.Name).ToList();
            }
        }

        private async Task<bool> ValidateCurrencyAsync(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            {
                return false;
            }

            if (!_currencies.Any())
            {
                await GetCurrenciesAsync();
            }

            return _currencies.Any(c => c == currency.ToUpper());
        }

        private async Task<MonthlyRate> GetRateAsync(DateTime day, string fromCurrency, string toCurrency)
        {
            var dateFormatted = day.ToString("yyyy-MM-dd");

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_config.Endpoint);
                var url = $"{_config.Endpoint}/api/historical/{dateFormatted}.json?app_id={_config.AppId}&base={fromCurrency.ToUpper()}";
                var response = await client.GetStringAsync(url);

                return Parse(day, response, toCurrency.ToUpper());
            }
        }

        private async Task<IEnumerable<MonthlyRate>> GetRatesAsync(string fromCurrency, string toCurrency)
        {
            var from = new DateTime(2016, 1, 15);
            var days = new List<DateTime>();

            var tasks = new List<Task<MonthlyRate>>();

            for (var i = 0; i < 12; i++)
            {
                tasks.Add(GetRateAsync(from.AddMonths(i), fromCurrency, toCurrency));
            }

            var rates = await Task.WhenAll(tasks);

            return rates;
        }

        public async Task RunAsync()
        {
            Console.Write("From Currency: ");
            string fromCurrency = Console.ReadLine();

            Console.Write("To Currency: ");
            string toCurrency = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(fromCurrency))
            {
                Console.WriteLine("Please enter value for From Currency");
                return;
            }

            if (string.IsNullOrWhiteSpace(toCurrency))
            {
                Console.WriteLine("Please enter value for To Currency");
                return;
            }

            if (!await ValidateCurrencyAsync(fromCurrency))
            {
                Console.WriteLine($"From Currency: {fromCurrency} currency is invalid.");
                return;
            }

            if (!await ValidateCurrencyAsync(toCurrency))
            {
                Console.WriteLine($"To Currency: {fromCurrency} currency is invalid.");
                return;
            }

            int month = 13;

            var rates = await GetRatesAsync(fromCurrency.ToUpper(), toCurrency.ToUpper());

            var result = _predictor.Predict(month, rates);

            Console.WriteLine($"The predicted currency exchange from {fromCurrency.ToUpper()} to {toCurrency.ToUpper()} for 15/1/2017 is {result}");
        }
    }
}
