using System;
using System.Collections.Generic;
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
        virtual public int Emigrants { get; }

        public int NetMigration => Immigrants - Emigrants; // Чистая миграция (разница между иммигрантами и эмигрантами)

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
        IEnumerable<MigrationRecord> ParseData(string filePath); // Метод для разбора данных из файла
    }

    // Интерфейс для анализатора миграционных данных
    public interface IMigrationAnalyzer
    {
        IReadOnlyList<MigrationRecord> MigrationData { get; } // Доступ к загруженным данным
        void LoadData(IEnumerable<MigrationRecord> data);     // Загрузка данных для анализа

        // Методы анализа:
        decimal CalculateMaxPercentageChange();               // Расчет максимального процентного изменения миграции

        MigrationRecord FindPeakImmigrationYear();            // Нахождение года с пиковой иммиграцией
        MigrationRecord FindPeakEmigrationYear();             // Нахождение года с пиковой эмиграцией
        bool IsDataLoaded { get; }                            // Флаг загружены ли данные

        // Прогнозирование с использованием скользящего среднего
        List<MigrationRecord> ForecastUsingMovingAverage(int periodsToForecast, int windowSize);
    }

    // Интерфейс сервиса для работы с миграционными данными
    public interface IMigrationDataService
    {
        IReadOnlyList<MigrationRecord> GetMigrationData(string filePath); // Получение данных миграции
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

            foreach (var line in lines.Skip(1)) // Пропускаем заголовок
            {
                var parts = line.Split(',');
                if (parts.Length >= 3)
                {
                    try
                    {
                        var year = int.Parse(parts[0].Trim());
                        var immigrants = int.Parse(parts[1].Trim());
                        var emigrants = int.Parse(parts[2].Trim());

                        records.Add(new MigrationRecord(year, immigrants, emigrants));
                    }
                    catch (FormatException ex)
                    {
                        throw new FormatException($"Invalid data format in CSV file: {line}", ex);
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

        // Внедрение зависимости через конструктор (можно использовать любой парсер)
        public MigrationDataService(IMigrationDataParser parser)
        {
            _parser = parser;
        }

        // Получение данных миграции из файла
        public IReadOnlyList<MigrationRecord> GetMigrationData(string filePath)
        {
            var data = _parser.ParseData(filePath);
            return new List<MigrationRecord>(data).AsReadOnly();
        }
    }

    // Основной класс для анализа миграционных данных
    public class MigrationAnalyzer : IMigrationAnalyzer
    {
        private List<MigrationRecord> _migrationData = new List<MigrationRecord>();

        public bool IsDataLoaded => _migrationData != null && _migrationData.Any();
        public IReadOnlyList<MigrationRecord> MigrationData => _migrationData.AsReadOnly();

        // Загрузка данных для анализа
        public void LoadData(IEnumerable<MigrationRecord> data)
        {
            _migrationData = data?.ToList() ?? throw new ArgumentNullException(nameof(data));
            if (!_migrationData.Any())
                throw new ArgumentException("Data collection cannot be empty", nameof(data));
        }

        // Расчет максимального процентного изменения миграции между годами
        public decimal CalculateMaxPercentageChange()
        {
            ValidateDataLoaded();

            decimal maxChange = 0;
            for (int i = 1; i < _migrationData.Count; i++)
            {
                var prevTotal = _migrationData[i - 1].Immigrants + _migrationData[i - 1].Emigrants;
                var currentTotal = _migrationData[i].Immigrants + _migrationData[i].Emigrants;

                if (prevTotal == 0) continue;

                var change = Math.Abs((currentTotal - prevTotal) / (decimal)prevTotal * 100);
                maxChange = Math.Max(maxChange, change);
            }
            return maxChange;
        }
        // Нахождение года с максимальной иммиграцией
        public MigrationRecord FindPeakImmigrationYear()
        {
            ValidateDataLoaded();
            return _migrationData.OrderByDescending(x => x.Immigrants).First();
        }

        // Нахождение года с максимальной эмиграцией
        public MigrationRecord FindPeakEmigrationYear()
        {
            ValidateDataLoaded();
            return _migrationData.OrderByDescending(x => x.Emigrants).First();
        }

        // Прогнозирование миграции на будущие периоды с использованием скользящего среднего
        public List<MigrationRecord> ForecastUsingMovingAverage(int periodsToForecast, int windowSize)
        {
            if (!IsDataLoaded)
                throw new InvalidOperationException("Данные не загружены");

            if (periodsToForecast <= 0)
                throw new ArgumentException("Количество периодов прогноза должно быть положительным", nameof(periodsToForecast));

            var data = _migrationData.OrderBy(x => x.Year).ToList();
            int lastYear = data.Last().Year;

            var forecast = new List<MigrationRecord>();
            var lastWindow = data.Skip(data.Count - windowSize).ToList();

            for (int i = 1; i <= periodsToForecast; i++)
            {
                int avgImmigrants = (int)lastWindow.Average(x => x.Immigrants);
                int avgEmigrants = (int)lastWindow.Average(x => x.Emigrants);

                var forecastRecord = new MigrationRecord(
                    year: lastYear + i,
                    immigrants: avgImmigrants,
                    emigrants: avgEmigrants
                );

                forecast.Add(forecastRecord);
                lastWindow.RemoveAt(0);
                lastWindow.Add(forecastRecord);
            }

            return forecast;
        }

        // Валидация загружены ли данные
        private void ValidateDataLoaded()
        {
            if (_migrationData == null || !_migrationData.Any())
                throw new InvalidOperationException("Migration data not loaded");
        }
    }
    public class CurrencyData
    {
        public DateTime Date { get; set; }
        public double Rate1 { get; set; }
        public double Rate2 { get; set; }

    }
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
