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
        bool IsDataLoaded { get; }                            // Флаг загружены ли данные

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

            foreach (var line in lines.Skip(1)) 
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

        // Внедрение зависимости через конструктор
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

        // Валидация загружены ли данные
        private void ValidateDataLoaded()
        {
            if (_migrationData == null || !_migrationData.Any())
                throw new InvalidOperationException("Migration data not loaded");
        }
    }
}