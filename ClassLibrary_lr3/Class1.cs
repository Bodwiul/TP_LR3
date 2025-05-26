using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassLibrary_lr3
{
    // Абстрактный базовый класс для записи о миграции
    abstract public class Migration
    {
        virtual public int Year { get; }             // Год записи
        virtual public int Immigrants { get; }       // Количество иммигрантов
        virtual public int Emigrants { get; }        // Количество эмигрантов
    }

    // Класс, представляющий запись о миграции за определенный год
    public class MigrationRecord : Migration
    {
        override public int Year { get; }
        override public int Immigrants { get; }
        override public int Emigrants { get; }

        public int NetMigration => Immigrants - Emigrants; // Чистая миграция

        public MigrationRecord(int year, int immigrants, int emigrants)
        {
            Year = year;
            Immigrants = immigrants;
            Emigrants = emigrants;
        }
    }

    // Интерфейс для сбора миграционных данных
    public interface IMigrationDataParser
    {
        IEnumerable<MigrationRecord> ParseData(string filePath);
    }

    // Интерфейс для анализатора миграционных данных
    public interface IMigrationAnalyzer
    {
        IReadOnlyList<MigrationRecord> MigrationData { get; }
        void LoadData(IEnumerable<MigrationRecord> data);
        decimal CalculateMaxPercentageChange();
        MigrationRecord FindPeakImmigrationYear();
        MigrationRecord FindPeakEmigrationYear();
        bool IsDataLoaded { get; }
        List<MigrationRecord> ForecastUsingMovingAverage(int periodsToForecast, int windowSize);
    }

    // Интерфейс сервиса для работы с миграционными данными
    public interface IMigrationDataService
    {
        IReadOnlyList<MigrationRecord> GetMigrationData(string filePath);
    }

    // Реализация парсера CSV-файлов с миграционными данными
    public class CsvMigrationParser : IMigrationDataParser
    {
        public IEnumerable<MigrationRecord> ParseData(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("CSV file not found", filePath);

            var records = new List<MigrationRecord>();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    try
                    {
                        records.Add(new MigrationRecord(
                            int.Parse(parts[0].Trim()),
                            int.Parse(parts[1].Trim()),
                            int.Parse(parts[2].Trim())));
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException($"Invalid data format in CSV: {line}", ex);
                    }
                }
            }
            return records;
        }
    }

    // Сервис для работы с миграционными данными
    public class MigrationDataService : IMigrationDataService
    {
        private readonly IMigrationDataParser _parser;

        public MigrationDataService(IMigrationDataParser parser)
        {
            _parser = parser;
        }

        public IReadOnlyList<MigrationRecord> GetMigrationData(string filePath)
        {
            return new List<MigrationRecord>(_parser.ParseData(filePath)).AsReadOnly();
        }
    }

    // Основной класс для анализа миграционных данных
    public class MigrationAnalyzer : IMigrationAnalyzer
    {
        private List<MigrationRecord> _migrationData = new List<MigrationRecord>();

        public bool IsDataLoaded => _migrationData?.Any() ?? false;
        public IReadOnlyList<MigrationRecord> MigrationData => _migrationData.AsReadOnly();

        public void LoadData(IEnumerable<MigrationRecord> data)
        {
            _migrationData = data?.ToList() ?? throw new ArgumentNullException(nameof(data));
            if (!_migrationData.Any())
                throw new ArgumentException("Data collection cannot be empty", nameof(data));
        }

        public decimal CalculateMaxPercentageChange()
        {
            ValidateDataLoaded();
            decimal maxChange = 0;

            for (int i = 1; i < _migrationData.Count; i++)
            {
                var prevTotal = _migrationData[i - 1].Immigrants + _migrationData[i - 1].Emigrants;
                var currentTotal = _migrationData[i].Immigrants + _migrationData[i].Emigrants;

                if (prevTotal != 0)
                {
                    var change = Math.Abs((currentTotal - prevTotal) / (decimal)prevTotal * 100);
                    maxChange = Math.Max(maxChange, change);
                }
            }
            return maxChange;
        }

        public MigrationRecord FindPeakImmigrationYear()
        {
            ValidateDataLoaded();
            return _migrationData.OrderByDescending(x => x.Immigrants).First();
        }

        public MigrationRecord FindPeakEmigrationYear()
        {
            ValidateDataLoaded();
            return _migrationData.OrderByDescending(x => x.Emigrants).First();
        }

        public List<MigrationRecord> ForecastUsingMovingAverage(int periodsToForecast, int windowSize)
        {
            ValidateDataLoaded();
            if (periodsToForecast <= 0 || windowSize <= 0)
                throw new ArgumentException("Invalid parameters");

            var data = _migrationData.OrderBy(x => x.Year).ToList();
            var forecast = new List<MigrationRecord>();
            var lastWindow = data.TakeLast(windowSize).ToList();

            for (int i = 1; i <= periodsToForecast; i++)
            {
                var forecastRecord = new MigrationRecord(
                    data.Last().Year + i,
                    (int)lastWindow.Average(x => x.Immigrants),
                    (int)lastWindow.Average(x => x.Emigrants));

                forecast.Add(forecastRecord);
                lastWindow.RemoveAt(0);
                lastWindow.Add(forecastRecord);
            }
            return forecast;
        }

        private void ValidateDataLoaded()
        {
            if (!IsDataLoaded)
                throw new InvalidOperationException("Migration data not loaded");
        }
    }

    // Класс для хранения данных о валюте
    public class CurrencyData
    {
        public DateTime Date { get; set; }
        public double Rate1 { get; set; }
        public double Rate2 { get; set; }
    }

    // Анализатор валютных данных
    public class CurrencyAnalyzer
    {
        private List<CurrencyData> dataList = new List<CurrencyData>();

        public void LoadData(string filename)
        {
            dataList.Clear();
            foreach (var line in File.ReadAllLines(filename).Skip(1))
            {
                var parts = line.Split(new[] { ", " }, StringSplitOptions.None);
                if (parts.Length >= 3 && DateTime.TryParseExact(parts[0], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) &&
                    double.TryParse(parts[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var rate1) &&
                    double.TryParse(parts[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var rate2))
                {
                    dataList.Add(new CurrencyData { Date = date, Rate1 = rate1, Rate2 = rate2 });
                }
            }
        }

        public List<CurrencyData> GetData() => dataList;

        public (DateTime[] Date, double[] Rates1, double[] Rates2) GetChartPoints()
        {
            return (
                dataList.Select(d => d.Date).ToArray(),
                dataList.Select(d => d.Rate1).ToArray(),
                dataList.Select(d => d.Rate2).ToArray()
            );
        }

        public List<(DateTime DayFrom, DateTime DayTo, double DeltaRate1, double DeltaRate2)> AnalyzeChanges()
        {
            return dataList.Take(dataList.Count - 1)
                .Select((current, i) => (
                    current.Date,
                    dataList[i + 1].Date,
                    dataList[i + 1].Rate1 - current.Rate1,
                    dataList[i + 1].Rate2 - current.Rate2
                )).ToList();
        }

        public (double MaxIncreaseRate1, DateTime DayMaxIncreaseRate1,
                double MaxDecreaseRate1, DateTime DayMaxDecreaseRate1,
                double AvgChangeRate1,
                double MaxIncreaseRate2, DateTime DayMaxIncreaseRate2,
                double MaxDecreaseRate2, DateTime DayMaxDecreaseRate2,
                double AvgChangeRate2) GetExtremes()
        {
            if (dataList.Count < 2) throw new InvalidOperationException("Недостаточно данных");

            var changes = dataList.Take(dataList.Count - 1)
                .Select((current, i) => new {
                    Delta1 = dataList[i + 1].Rate1 - current.Rate1,
                    Delta2 = dataList[i + 1].Rate2 - current.Rate2,
                    NextDate = dataList[i + 1].Date
                }).ToList();

            return (
                changes.Max(x => x.Delta1), changes.OrderByDescending(x => x.Delta1).First().NextDate,
                changes.Min(x => x.Delta1), changes.OrderBy(x => x.Delta1).First().NextDate,
                changes.Average(x => x.Delta1),
                changes.Max(x => x.Delta2), changes.OrderByDescending(x => x.Delta2).First().NextDate,
                changes.Min(x => x.Delta2), changes.OrderBy(x => x.Delta2).First().NextDate,
                changes.Average(x => x.Delta2)
            );
        }

        public double[] ForecastRates(double[] rates, int N, int windowSize = 3)
        {
            if (rates == null || rates.Length == 0 || N <= 0 || windowSize <= 0)
                return Array.Empty<double>();

            var window = new Queue<double>(rates.TakeLast(windowSize));
            var forecast = new List<double>();

            for (int i = 0; i < N; i++)
            {
                double avg = window.Average();
                forecast.Add(avg);
                if (window.Count == windowSize) window.Dequeue();
                window.Enqueue(avg);
            }

            return forecast.ToArray();
        }
    }
}