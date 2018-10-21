using ExchangePredictor.Models;
using ExchangePredictor.Services;
using ExchangePredictor.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExchangePredictor.Test
{
    [TestFixture]
    public class AppTest
    {
        private Mock<ILogger<App>> _logger;
        private IOptions<OpenExchangeSettings> _config;
        private Mock<IPredictor> _predictor;

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<ILogger<App>>();

            _config = Options.Create(new OpenExchangeSettings
            {
                Endpoint = "https://openexchangerates.org",
                AppId = "11551268e7f94447a0a382e5a771fa4d"
            });

            _predictor = new Mock<IPredictor>();
        }

        private App GetInstance()
        {
            return new App(_logger.Object, _config, _predictor.Object);
        }

        [Test]
        public async Task Get_Currencies_Async()
        {
            var app = GetInstance();

            var currencies = await app.GetCurrenciesAsync();

            Assert.AreEqual(true, currencies.Any());
        }

        [Test]
        public async Task Validate_Currency_Async()
        {
            var app = GetInstance();

            var hasUSD = await app.ValidateCurrencyAsync("USD");
            Assert.IsTrue(hasUSD);

            var hasVND = await app.ValidateCurrencyAsync("VND");
            Assert.IsTrue(hasVND);

            var hasXXX = await app.ValidateCurrencyAsync("XXX");
            Assert.IsFalse(hasXXX);

            var hasNull = await app.ValidateCurrencyAsync(null);
            Assert.IsFalse(hasNull);
        }

        [Test]
        public void Should_Throw_Exception_When_Get_Rate_Async_With_Invalid_From_Currency()
        {
            var app = GetInstance();

            var day = new DateTime(2017, 1, 15);
            Assert.ThrowsAsync<Exception>(() => app.GetRateAsync(day, "XXX", "VND"));
        }

        [Test]
        public void Should_Throw_Exception_When_Get_Rate_Async_With_Invalid_To_Currency()
        {
            var app = GetInstance();

            var day = new DateTime(2017, 1, 15);
            Assert.ThrowsAsync<Exception>(() => app.GetRateAsync(day, "USD", "YYY"));
        }

        [Test]
        public void Should_Throw_Exception_When_Get_Rate_Async_With_From_To_Currency_Are_The_Same()
        {
            var app = GetInstance();

            var day = new DateTime(2017, 1, 15);
            Assert.ThrowsAsync<Exception>(() => app.GetRateAsync(day, "USD", "usd"));
        }

        [Test]
        public async Task Get_Rate_Async()
        {
            var app = GetInstance();

            var day = new DateTime(2016, 12, 15);
            var rate = await app.GetRateAsync(day, "USD", "TRY");

            Assert.AreEqual(rate.Month, day.Month);
            Assert.AreNotEqual(rate.Rate, 0);
        }

        [Test]
        public async Task Get_Rates_Async()
        {
            var app = GetInstance();

            var rates = await app.GetRatesAsync("USD", "TRY");

            Assert.AreEqual(rates.Count(), 12);
            Assert.IsFalse(rates.Any(r => r.Rate == 0));
        }
    }
}
