using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassLibrary_lr3;
using System.Windows.Forms;

namespace WindowsFormsApp_lr3
{
    public partial class Form1 : Form
    {
        // Приватные поля для хранения сервисов и состояния формы
        private IMigrationDataService _migrationDataService; // Сервис для работы с данными миграции
        private IMigrationAnalyzer _migrationAnalyzer;       // Анализатор миграционных данных
        private string _currentFilePath;                    // Путь к текущему файлу с данными
        private Chart migrationChart;                       // График для визуализации данных

        public Form1()
        {
            InitializeComponent();    // Инициализация компонентов формы
            InitializeServices();     // Настройка сервисов
            InitializeChart();        // Инициализация графика
        }

        // Инициализация сервисов для работы с данными
        private void InitializeServices()
        {
            var csvParser = new CsvMigrationParser();  // Создаем парсер CSV-файлов
            _migrationDataService = new MigrationDataService(csvParser); // Сервис данных с внедренным парсером
            _migrationAnalyzer = new MigrationAnalyzer(); // Создаем анализатор данных
        }

        // Настройка графика для отображения данных
        private void InitializeChart()
        {
            migrationChart = new Chart();  // Создаем новый график
            migrationChart.Dock = DockStyle.Fill;  // Заполняем все доступное пространство
            var chartArea = new ChartArea();  // Создаем область для графика
            migrationChart.ChartAreas.Add(chartArea);  // Добавляем область на график
            groupBox2.Controls.Add(migrationChart);  // Добавляем график в группу на форме
        }

        // Расчет и отображение статистики по миграционным данным
        private void CalculateAndDisplayStatistics()
        {
            try
            {
                // Получаем статистические данные:
                var maxChange = _migrationAnalyzer.CalculateMaxPercentageChange(); // Макс. процентное изменение
                // Формируем строку со статистикой
                string stats = $"Максимальное процентное изменение: {maxChange:F2}%\n" +
                              $"Всего записей: {_migrationAnalyzer.MigrationData.Count}";

                richTextBox1.Text = stats;  // Выводим статистику в текстовое поле
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете статистики: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Отображение графика миграционных данных
        private void DisplayMigrationChart(IEnumerable<MigrationRecord> data)
        {
            if (migrationChart == null) return;

            // Очищаем предыдущие данные
            migrationChart.Series.Clear();
            migrationChart.ChartAreas.Clear();
            migrationChart.Legends.Clear();

            // Настройка области графика
            var chartArea = new ChartArea();
            chartArea.AxisX.Title = "Год";
            chartArea.AxisY.Title = "Количество людей";
            migrationChart.ChartAreas.Add(chartArea);

            // Добавление легенды
            var legend = new Legend();
            legend.Title = "Легенда";
            migrationChart.Legends.Add(legend);

            // Создание серий данных:

            // Серия для иммигрантов
            var seriesImmigrants = new Series("Иммигранты")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 2
            };

            // Серия для эмигрантов
            var seriesEmigrants = new Series("Эмигранты")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 2
            };

            // Серия для чистой миграции (разница между иммигрантами и эмигрантами)
            var seriesNet = new Series("Чистая миграция")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green,
                BorderWidth = 2
            };

            // Заполнение серий данными
            foreach (var record in data)
            {
                seriesImmigrants.Points.AddXY(record.Year, record.Immigrants);
                seriesEmigrants.Points.AddXY(record.Year, record.Emigrants);
                seriesNet.Points.AddXY(record.Year, record.NetMigration);
            }

            // Добавление серий на график
            migrationChart.Series.Add(seriesImmigrants);
            migrationChart.Series.Add(seriesEmigrants);
            migrationChart.Series.Add(seriesNet);

            // Обновление графика
            migrationChart.Invalidate();
        }

        // Обработчик кнопки "Данные о миграции"
        private void button_migration_Click_1(object sender, EventArgs e)
        {
            try
            {
                // Настройка диалога открытия файла
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",  // Фильтр для CSV-файлов
                    Title = "Выберите файл с данными о миграции"  // Заголовок окна
                };

                // Если пользователь выбрал файл
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFilePath = openFileDialog.FileName;  // Сохраняем путь к файлу

                    // Загружаем данные через сервис
                    var migrationData = _migrationDataService.GetMigrationData(_currentFilePath);

                    // Загружаем данные в анализатор
                    _migrationAnalyzer.LoadData(migrationData);

                    // Отображаем данные в таблице
                    dataGridView1.DataSource = _migrationAnalyzer.MigrationData;

                    // Вычисляем и показываем статистику
                    CalculateAndDisplayStatistics();

                    // Отображаем данные на графике
                    DisplayMigrationChart(_migrationAnalyzer.MigrationData);
                }
            }
            catch (Exception ex)
            {
                // Обработка ошибок при загрузке данных
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}