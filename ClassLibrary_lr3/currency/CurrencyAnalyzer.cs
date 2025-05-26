using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary_lr3.currency
{
    public class CurrencyAnalyzer
    {
        private List<CurrencyData> dataList = new List<CurrencyData>();

        // Загрузка данных из файла (CSV)
        public void LoadData(string filename)
        {
            dataList.Clear();
            var lines = System.IO.File.ReadAllLines(filename);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(new string[] { ", " }, StringSplitOptions.None);
                if (parts.Length >= 3)
                {
                    if (DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date) &&
                        double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out double rate1) &&
                        double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out double rate2))
                    {
                        dataList.Add(new CurrencyData { Date = date, Rate1 = rate1, Rate2 = rate2 });
                    }

                }
            }
        }

        // Получение данных для отображения
        public List<CurrencyData> GetData()
        {
            return dataList;
        }

        // Построение графика - возвращает список точек для графика
        public (DateTime[] Date, double[] Rates1, double[] Rates2) GetChartPoints()
        {
            var date = dataList.Select(d => d.Date).ToArray();   // DateTime[] без ToOADate()
            var rates1 = dataList.Select(d => d.Rate1).ToArray();
            var rates2 = dataList.Select(d => d.Rate2).ToArray();
            return (date, rates1, rates2);
        }


        // Анализ изменений курса
        public List<(DateTime DayFrom, DateTime DayTo, double DeltaRate1, double DeltaRate2)> AnalyzeChanges()
        {
            var result = new List<(DateTime, DateTime, double, double)>();
            for (int i = 0; i < dataList.Count - 1; i++)
            {
                var current = dataList[i];
                var next = dataList[i + 1];
                result.Add((current.Date, next.Date, next.Rate1 - current.Rate1,
                    next.Rate2 - current.Rate2));
            }
            return result;
        }

        // Поиск максимумов, минимумов и среднего значения изменений
        public (double MaxIncreaseRate1, DateTime DayMaxIncreaseRate1,
                double MaxDecreaseRate1, DateTime DayMaxDecreaseRate1,
                double AvgChangeRate1,
                double MaxIncreaseRate2, DateTime DayMaxIncreaseRate2,
                double MaxDecreaseRate2, DateTime DayMaxDecreaseRate2,
                double AvgChangeRate2) GetExtremes()
        {
            if (dataList == null || dataList.Count < 2)
                throw new InvalidOperationException("Недостаточно данных для анализа");

            var changesRate1 = new List<(double delta, int index)>();
            var changesRate2 = new List<(double delta, int index)>();

            for (int i = 0; i < dataList.Count - 1; i++)
            {
                changesRate1.Add((dataList[i + 1].Rate1 - dataList[i].Rate1, i));
                changesRate2.Add((dataList[i + 1].Rate2 - dataList[i].Rate2, i));
            }

            var maxIncR1 = changesRate1.OrderByDescending(x => x.delta).First();
            var maxDecR1 = changesRate1.OrderBy(x => x.delta).First();
            var avgChangeR1 = changesRate1.Average(x => x.delta);

            var maxIncR2 = changesRate2.OrderByDescending(x => x.delta).First();
            var maxDecR2 = changesRate2.OrderBy(x => x.delta).First();
            var avgChangeR2 = changesRate2.Average(x => x.delta);

            return (
                maxIncR1.delta,
                dataList[maxIncR1.index + 1].Date,
                maxDecR1.delta,
                dataList[maxDecR1.index + 1].Date,
                avgChangeR1,
                maxIncR2.delta,
                dataList[maxIncR2.index + 1].Date,
                maxDecR2.delta,
                dataList[maxDecR2.index + 1].Date,
                avgChangeR2
            );
        }


        // Прогнозирование методом скользящей средней
        public double[] ForecastRates(double[] rates, int N, int windowSize = 3)
        {
            if (rates == null || rates.Length == 0 || N <= 0 || windowSize <= 0)
                return new double[0];

            List<double> forecastList = new List<double>();

            // Копируем последние windowSize значений из реальных данных для начала прогноза
            Queue<double> window = new Queue<double>();

            // Если данных меньше windowSize, берем столько, сколько есть
            int startIdx = Math.Max(0, rates.Length - windowSize);
            for (int i = startIdx; i < rates.Length; i++)
                window.Enqueue(rates[i]);

            for (int i = 0; i < N; i++)
            {
                double avg = window.Average();  // среднее по текущему окну
                forecastList.Add(avg);

                // Убираем самый старый и добавляем текущий прогноз (скользящее окно)
                if (window.Count == windowSize)
                    window.Dequeue();
                window.Enqueue(avg);
            }

            return forecastList.ToArray();
        }

    }
}
