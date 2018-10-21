using ExchangePredictor.Models;
using ExchangePredictor.Services;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ExchangePredictor.Test.Services
{
    [TestFixture]
    public class AppTest
    {
        private readonly Predictor _predictor;

        public AppTest()
        {
            _predictor = new Predictor();
        }

        [Test]
        public void Should_Throw_Argument_Null_Exception_When_Rates_Is_Null()
        {
            int month = 13;
            IEnumerable<MonthlyRate> rates = null;

            Assert.Throws<ArgumentNullException>(() => _predictor.Predict(month, rates));
        }

        [Test]
        public void Should_Throw_Argument_Null_Exception_When_Rates_Is_Empty()
        {
            int month = 13;
            var rates = new List<MonthlyRate>();

            Assert.Throws<Exception>(() => _predictor.Predict(month, rates));
        }

        [Test]
        public void Should_Throw_Divide_By_Zero_Exception()
        {
            int month = 13;
            var rates = new List<MonthlyRate>
            {
                new MonthlyRate(1, 1m)
            };

            Assert.Throws<DivideByZeroException>(() => _predictor.Predict(month, rates));
        }

        /// <summary>
        /// Test formulas https://www.easycalculation.com/statistics/learn-regression.php
        /// </summary>
        [Test]
        public void Predict()
        {
            int month = 64;
            var rates = new List<MonthlyRate>
            {
                new MonthlyRate(60, 3.1m),
                new MonthlyRate(61, 3.6m),
                new MonthlyRate(62, 3.8m),
                new MonthlyRate(63, 4m),
                new MonthlyRate(65, 4.1m)
            };

            var result = _predictor.Predict(month, rates);
            Assert.AreEqual(4.058, result);
        }
    }
}
