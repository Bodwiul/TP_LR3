
﻿using System;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ZedGraph;
namespace WindowsFormsApp_lr3
{
    public partial class Form1 : Form
    {
        private IMigrationDataService _migrationDataService;
        private IMigrationAnalyzer _migrationAnalyzer;
        private string _currentFilePath;
        private string _currentFilePath_cur;
        private System.Windows.Forms.DataVisualization.Charting.Chart migrationChart;
        private CurrencyAnalyzer analyzer;

        public Form1()
        {
            InitializeComponent();
            InitializeServices();
            InitializeChart();
            analyzer = new CurrencyAnalyzer();
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
            migrationChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
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
            var legend = new System.Windows.Forms.DataVisualization.Charting.Legend();
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
                    // Сброс привязки данных перед загрузкой новых
                    dataGridView1.DataSource = null;
                    dataGridView1.Columns.Clear();

                    _currentFilePath = openFileDialog.FileName;
                    var migrationData = _migrationDataService.GetMigrationData(_currentFilePath);
                    _migrationAnalyzer.LoadData(migrationData);

                    // Создаем новый BindingSource для миграционных данных
                    var bindingSource = new BindingSource();
                    bindingSource.DataSource = new BindingList<ClassLibrary_lr3.MigrationRecord>(_migrationAnalyzer.MigrationData.ToList());
                    dataGridView1.DataSource = bindingSource;

                    // Настройка столбцов
                    dataGridView1.AutoGenerateColumns = true;
                    if (dataGridView1.Columns.Count >= 2)
                    {
                        dataGridView1.Columns[0].HeaderText = "Год";
                        dataGridView1.Columns[1].HeaderText = "Количество мигрантов";
                    }

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

        private void DisplayData()
        {
            var data = analyzer.GetData();

            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add("Date", "Дата");
            dataGridView1.Columns.Add("Rate1", "Курс USD/RUB");
            dataGridView1.Columns.Add("Rate2", "Курс EUR/RUB");
            foreach (var item in data)
            {
                dataGridView1.Rows.Add(item.Date.ToString("dd.MM.yyyy"), item.Rate1, item.Rate2);
            }
        }

        private void PlotChart()
        {
            // Получаем данные с датами в формате DateTime
            var points = analyzer.GetChartPoints(); // теперь возвращает (DateTime[] Date, double[] Rates1, double[] Rates2)

            var pane = zedGraphControl1.GraphPane;
            pane.CurveList.Clear();

            // Преобразуем DateTime в массив double через ToOADate для ZedGraph
            double[] x = points.Date.Select(d => d.ToOADate()).ToArray();

            // Добавляем кривые с преобразованными координатами X
            pane.AddCurve("USD/RUB", x, points.Rates1, System.Drawing.Color.Blue);
            pane.AddCurve("EUR/RUB", x, points.Rates2, System.Drawing.Color.Red);

            // Форматируем ось X для отображения дат
            pane.XAxis.Type = ZedGraph.AxisType.Date;
            pane.XAxis.Scale.Format = "dd.MM.yyyy"; // формат даты по желанию
            pane.XAxis.Scale.MajorUnit = ZedGraph.DateUnit.Day;
            pane.XAxis.Scale.MinorUnit = ZedGraph.DateUnit.Hour;

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }

        private void ShowExtremes()
        {
            var extremes = analyzer.GetExtremes();

            richTextBox1.Clear();
            richTextBox1.AppendText($"USD/RUB\n");
            richTextBox1.AppendText($"Максимальный прирост: {extremes.MaxIncreaseRate1:F4} за {extremes.DayMaxIncreaseRate1:dd.MM.yyyy}\n");
            richTextBox1.AppendText($"Максимальное падение: {extremes.MaxDecreaseRate1:F4} за {extremes.DayMaxDecreaseRate1:dd.MM.yyyy}\n");
            richTextBox1.AppendText($"Среднее изменение: {extremes.AvgChangeRate1:F4}\n\n");

            richTextBox1.AppendText($"EUR/RUB\n");
            richTextBox1.AppendText($"Максимальный прирост: {extremes.MaxIncreaseRate2:F4} за {extremes.DayMaxIncreaseRate2:dd.MM.yyyy}\n");
            richTextBox1.AppendText($"Максимальное падение: {extremes.MaxDecreaseRate2:F4} за {extremes.DayMaxDecreaseRate2:dd.MM.yyyy}\n");
            richTextBox1.AppendText($"Среднее изменение: {extremes.AvgChangeRate2:F4}\n");
        }


        private void button_currency_Click_1(object sender, EventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv",
                    Title = "Выберите файл с данными о курсе валют"
                };

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Сброс привязки данных перед загрузкой новых
                    dataGridView1.DataSource = null;
                    dataGridView1.Columns.Clear();

                    _currentFilePath_cur = openFileDialog.FileName;
                    analyzer.LoadData(_currentFilePath_cur);

                    // Отображение данных в таблице
                    DisplayData();
                    PlotChart();
                    ShowExtremes();
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
            bool hasCurrencyData = int.TryParse(textBox1.Text, out int daysCount) && daysCount > 0;
            bool hasMigrationData = int.TryParse(textBox2.Text, out int yearsCount) && yearsCount > 0;

            if (!hasCurrencyData && !hasMigrationData)
            {
                MessageBox.Show("Введите корректные данные для прогноза в нужном поле");
                return;
            }

            try
            {
                // Прогнозирование миграции (если заполнено textBox2)
                if (hasMigrationData)
                {
                    var forecastData = _migrationAnalyzer.ForecastUsingMovingAverage(yearsCount, 3);
                    var historicalData = _migrationAnalyzer.MigrationData.ToList();

                    DisplayForecast(historicalData, forecastData);

                    // Используем фактический тип данных, возвращаемый ForecastUsingMovingAverage
                    dataGridView1.DataSource = new BindingList<ClassLibrary_lr3.MigrationRecord>(forecastData);

                    richTextBox1.Text += $"\n\nПрогноз методом скользящей средней";
                }

                // Прогнозирование курсов валют (если заполнено textBox1)
                if (hasCurrencyData)
                {
                    var chartPoints = analyzer.GetChartPoints();

                    // Прогноз для каждой валюты
                    var forecast1 = analyzer.ForecastRates(chartPoints.Rates1, daysCount, 3);
                    var forecast2 = analyzer.ForecastRates(chartPoints.Rates2, daysCount, 3);

                    // Дата последнего измерения
                    var datal = analyzer.GetData();
                    DateTime lastDate = datal.Last().Date;

                    // Подготовка точек для прогноза
                    var forecastDates = new List<double>();
                    for (int i = 1; i <= daysCount; i++)
                        forecastDates.Add(new DateTime(lastDate.Year, lastDate.Month, lastDate.Day).AddDays(i).ToOADate());

                    // Очистка и подготовка графика
                    var pane = zedGraphControl1.GraphPane;
                    pane.CurveList.Clear();
                    pane.Title.Text = "Курс рубля с прогнозом";
                    pane.XAxis.Title.Text = "Дата";
                    pane.YAxis.Title.Text = "Курс";
                    pane.XAxis.Type = ZedGraph.AxisType.Date;

                    // Исходные данные
                    var list1 = new PointPairList();
                    var list2 = new PointPairList();
                    for (int i = 0; i < chartPoints.Date.Length; i++)
                    {
                        list1.Add(chartPoints.Date[i].ToOADate(), chartPoints.Rates1[i]);
                        list2.Add(chartPoints.Date[i].ToOADate(), chartPoints.Rates2[i]);
                    }
                    pane.AddCurve("USD/RUB", list1, Color.Blue, SymbolType.Circle);
                    pane.AddCurve("EUR/RUB", list2, Color.Red, SymbolType.Diamond);

                    // Прогноз
                    var forecastList1 = new PointPairList();
                    var forecastList2 = new PointPairList();
                    for (int i = 0; i < daysCount; i++)
                    {
                        forecastList1.Add(forecastDates[i], forecast1[i]);
                        forecastList2.Add(forecastDates[i], forecast2[i]);
                    }
                    pane.AddCurve("USD/RUB (прогноз)", forecastList1, Color.Green, SymbolType.None);
                    pane.AddCurve("EUR/RUB (прогноз)", forecastList2, Color.Orange, SymbolType.None);

                    // Обновление графика
                    zedGraphControl1.AxisChange();
                    zedGraphControl1.Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка прогнозирования: {ex.Message}");
            }
        }
    }
}

