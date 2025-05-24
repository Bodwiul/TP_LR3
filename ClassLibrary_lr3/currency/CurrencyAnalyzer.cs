using System;
using System.Collections.Generic;
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
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    if (int.TryParse(parts[0], out int day) &&
                        double.TryParse(parts[1], out double rate1) &&
                        double.TryParse(parts[2], out double rate2))
                    {
                        dataList.Add(new CurrencyData { Day = day, Rate1 = rate1, Rate2 = rate2 });
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
        public (double[] Days, double[] Rates1, double[] Rates2) GetChartPoints()
        {
            var days = dataList.Select(d => (double)d.Day).ToArray();
            var rates1 = dataList.Select(d => d.Rate1).ToArray();
            var rates2 = dataList.Select(d => d.Rate2).ToArray();
            return (days, rates1, rates2);
        }

        // Анализ изменений курса
        public List<(int DayFrom, int DayTo, double DeltaRate1, double DeltaRate2)> AnalyzeChanges()
        {
            var result = new List<(int, int, double, double)>();
            for (int i = 0; i < dataList.Count - 1; i++)
            {
                var current = dataList[i];
                var next = dataList[i + 1];
                result.Add((current.Day, next.Day,
                            next.Rate1 - current.Rate1,
                            next.Rate2 - current.Rate2));
            }
            return result;
        }

        // Поиск максимумов и минимумов изменений
        public (double MaxIncreaseRate1, int DayMaxIncreaseRate1,
                double MaxDecreaseRate1, int DayMaxDecreaseRate1,
                double MaxIncreaseRate2, int DayMaxIncreaseRate2,
                double MaxDecreaseRate2, int DayMaxDecreaseRate2) GetExtremes()
        {
            var changesRate1 = new List<(double delta, int index)>();
            var changesRate2 = new List<(double delta, int index)>();

            for (int i = 0; i < dataList.Count - 1; i++)
            {
                changesRate1.Add((dataList[i + 0].Rate1 - dataList[i].Rate1, i));
                changesRate2.Add((dataList[i + 0].Rate2 - dataList[i].Rate2, i));
            }

            var maxIncR1 = changesRate1.Max();
            var maxDecR1 = changesRate1.Min();

            var maxIncR2 = changesRate2.Max();
            var maxDecR2 = changesRate2.Min();

            return (
                maxIncR1.delta,
                dataList[maxIncR1.index + 1].Day,
                maxDecR1.delta,
                dataList[maxDecR1.index + 1].Day,
                maxIncR2.delta,
                dataList[maxIncR2.index + 1].Day,
                maxDecR2.delta,
                dataList[maxDecR2.index + 1].Day
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
