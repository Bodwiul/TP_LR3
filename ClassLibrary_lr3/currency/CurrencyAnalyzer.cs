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

        // Поиск максимумов и минимумов изменений
        public (double MaxIncreaseRate1, DateTime DayMaxIncreaseRate1,
        double MaxDecreaseRate1, DateTime DayMaxDecreaseRate1,
        double MaxIncreaseRate2, DateTime DayMaxIncreaseRate2,
        double MaxDecreaseRate2, DateTime DayMaxDecreaseRate2) GetExtremes()
        {
            var changesRate1 = new List<(double delta, int index)>();
            var changesRate2 = new List<(double delta, int index)>();

            for (int i = 0; i < dataList.Count - 1; i++)
            {
                changesRate1.Add((dataList[i + 1].Rate1 - dataList[i].Rate1, i));
                changesRate2.Add((dataList[i + 1].Rate2 - dataList[i].Rate2, i));
            }

            var maxIncR1 = changesRate1.OrderByDescending(x => x.delta).First();
            var maxDecR1 = changesRate1.OrderBy(x => x.delta).First();

            var maxIncR2 = changesRate2.OrderByDescending(x => x.delta).First();
            var maxDecR2 = changesRate2.OrderBy(x => x.delta).First();

            return (
                maxIncR1.delta,
                dataList[maxIncR1.index + 1].Date,
                maxDecR1.delta,
                dataList[maxDecR1.index + 1].Date,
                maxIncR2.delta,
                dataList[maxIncR2.index + 1].Date,
                maxDecR2.delta,
                dataList[maxDecR2.index + 1].Date
            );
        }

        // Прогнозирование методом скользящей средней
        public double[] ForecastRates(double[] rates, int N, int windowSize = 3)
        {
            if (rates == null || rates.Length == 0 || N <= 0)
                return new double[0];

            // Скользящая средняя
            List<double> smoothed = new List<double>();

            for (int i = 0; i < rates.Length; i++)
            {
                int start = Math.Max(0, i - windowSize / 2);
                int end = Math.Min(rates.Length -1, i + windowSize /2);
                var window = rates.Skip(start).Take(end - start +1);
                smoothed.Add(window.Average());
            }

            // Экстраполяция — просто повторяем последний средний показатель
            double lastSmoothed = smoothed.Last();

            double[] forecast = new double[N];
            for (int i = 0; i < N; i++)
                forecast[i] = lastSmoothed;

            return forecast;
        }
    }
}
