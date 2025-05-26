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
        private IMigrationDataService _migrationDataService;
        private IMigrationAnalyzer _migrationAnalyzer;
        private string _currentFilePath;
        private Chart migrationChart;

        public Form1()
        {
            InitializeComponent();
            InitializeServices();
            InitializeChart();
        }

        private void InitializeServices()
        {
            var csvParser = new CsvMigrationParser();
            _migrationDataService = new MigrationDataService(csvParser);
            _migrationAnalyzer = new MigrationAnalyzer();
        }

        private void InitializeChart()
        {
            // Инициализация графика 
            migrationChart = new Chart();
            migrationChart.Dock = DockStyle.Fill;
            var chartArea = new ChartArea();
            migrationChart.ChartAreas.Add(chartArea);
            groupBox2.Controls.Add(migrationChart);
        }


        private void CalculateAndDisplayStatistics()
        {
            try
            {
                var maxChange = _migrationAnalyzer.CalculateMaxPercentageChange();
                var peakImmigration = _migrationAnalyzer.FindPeakImmigrationYear();
                var peakEmigration = _migrationAnalyzer.FindPeakEmigrationYear();

                string stats = $"Максимальное процентное изменение: {maxChange:F2}%\n" +
                     $"Пик иммиграции: {peakImmigration.Immigrants} чел. в {peakImmigration.Year} году\n" +
                              $"Пик эмиграции: {peakEmigration.Emigrants} чел. в {peakEmigration.Year} году\n" +
                              $"Всего записей: {_migrationAnalyzer.MigrationData.Count}";

                richTextBox1.Text = stats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете статистики: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DisplayForecast(List<MigrationRecord> historicalData, List<MigrationRecord> forecastData)
        {
            if (migrationChart == null) return;

            migrationChart.Series.Clear();

            // Настройка области графика
            var chartArea = migrationChart.ChartAreas[0];
            chartArea.AxisX.Title = "Год";
            chartArea.AxisY.Title = "Количество людей";
            chartArea.AxisX.Interval = 1;
            chartArea.AxisX.IsStartedFromZero = false;

            //Серия для исторических данных 
            Series histImmSeries = new Series("Иммигранты (история)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 3
            };

            Series histEmiSeries = new Series("Эмигранты (история)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 3
            };

            //Серия для прогноза
            Series forecastImmSeries = new Series("Иммигранты (прогноз)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.LightBlue,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dash
            };

            Series forecastEmiSeries = new Series("Эмигранты (прогноз)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Salmon,
                BorderWidth = 2,
                BorderDashStyle = ChartDashStyle.Dash
            };

            // Добавляем исторические данные
            foreach (var record in historicalData)
            {
                histImmSeries.Points.AddXY(record.Year, record.Immigrants);
                histEmiSeries.Points.AddXY(record.Year, record.Emigrants);
            }

            // Добавляем прогнозные данные 
            foreach (var record in forecastData)
            {
                forecastImmSeries.Points.AddXY(record.Year, record.Immigrants);
                forecastEmiSeries.Points.AddXY(record.Year, record.Emigrants);
            }

            // Соединяем последнюю точку истории с первой точкой прогноза
            if (historicalData.Count > 0 && forecastData.Count > 0)
            {
                var lastHistorical = historicalData.Last();
                var firstForecast = forecastData.First();

                // Создаем соединяющие серии (невидимые, только для линий)
                Series connectorImm = new Series()
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.Blue,
                    BorderWidth = 2,
                    IsVisibleInLegend = false
                };

                Series connectorEmi = new Series()
                {
                    ChartType = SeriesChartType.Line,
                    Color = Color.Red,
                    BorderWidth = 2,
                    IsVisibleInLegend = false
                };

                // Добавляем точки соединения
                connectorImm.Points.AddXY(lastHistorical.Year, lastHistorical.Immigrants);
                connectorImm.Points.AddXY(firstForecast.Year, firstForecast.Immigrants);

                connectorEmi.Points.AddXY(lastHistorical.Year, lastHistorical.Emigrants);
                connectorEmi.Points.AddXY(firstForecast.Year, firstForecast.Emigrants);

                // Добавляем серии на график (в правильном порядке)
                migrationChart.Series.Add(histImmSeries);
                migrationChart.Series.Add(histEmiSeries);
                migrationChart.Series.Add(connectorImm);
                migrationChart.Series.Add(connectorEmi);
                migrationChart.Series.Add(forecastImmSeries);
                migrationChart.Series.Add(forecastEmiSeries);
            }
            else
            {
                // Без соединения, если нет данных
                migrationChart.Series.Add(histImmSeries);
                migrationChart.Series.Add(histEmiSeries);
                migrationChart.Series.Add(forecastImmSeries);
                migrationChart.Series.Add(forecastEmiSeries);
            }

            // Настраиваем отображение
            chartArea.RecalculateAxesScale();
            chartArea.AxisX.IsMarginVisible = false;
        }

        private void DisplayMigrationChart(IEnumerable<MigrationRecord> data)
        {
            if (migrationChart == null) return;

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

            // Создание серий данных
            var seriesImmigrants = new Series("Иммигранты")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Blue,
                BorderWidth = 2
            };

            var seriesEmigrants = new Series("Эмигранты")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red,
                BorderWidth = 2
            };

            var seriesNet = new Series("Чистая миграция")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Green,
                BorderWidth = 2
            };

            // Заполнение данными
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
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Выберите файл с данными о миграции"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    _currentFilePath = openFileDialog.FileName;
                    var migrationData = _migrationDataService.GetMigrationData(_currentFilePath);
                    _migrationAnalyzer.LoadData(migrationData);

                    // Отображение данных в таблице
                    dataGridView1.DataSource = _migrationAnalyzer.MigrationData;

                    // Вычисление и отображение статистики
                    CalculateAndDisplayStatistics();

                    DisplayMigrationChart(_migrationAnalyzer.MigrationData);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button_forecast_Click_1(object sender, EventArgs e)
        {
            if (_migrationAnalyzer == null || !_migrationAnalyzer.IsDataLoaded)
            {
                MessageBox.Show("Сначала загрузите данные через кнопку 'Данные о миграции'");
                return;
            }

            if (!int.TryParse(textBox2.Text, out int yearsCount) || yearsCount <= 0)
            {
                MessageBox.Show("Введите корректное количество лет для прогноза (целое число больше 0)");
                return;
            }

            try
            {
                // Получаем прогноз с автоматическим подбором окна
                var forecastData = _migrationAnalyzer.ForecastUsingMovingAverage(yearsCount, 3);
                var historicalData = _migrationAnalyzer.MigrationData.ToList();

                // Отображаем результаты
                DisplayForecast(historicalData, forecastData);
                dataGridView1.DataSource = forecastData;

                // Выводим информацию о методе
                richTextBox1.Text += $"\n\nПрогноз методом скользящей средней";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка прогнозирования: {ex.Message}");
            }
        }
    }
}
