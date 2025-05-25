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
            // Инициализация графика (если добавляете программно)
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
                string stats = $"Максимальное процентное изменение: {maxChange:F2}%\n" +
                              $"Всего записей: {_migrationAnalyzer.MigrationData.Count}";

                richTextBox1.Text = stats;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете статистики: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

    }
}
