using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClassLibrary_lr3.currency;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ZedGraph;

namespace WindowsFormsApp_lr3
{
    public partial class Form1 : Form
    {
        private CurrencyAnalyzer analyzer;
        public Form1()
        {
            InitializeComponent();
            analyzer = new CurrencyAnalyzer();
        }

        private void button_currency_Click(object sender, EventArgs e)
        {
            string filename = @"C:\Users\valen\Downloads\currency_lab3_tp.txt";
            analyzer.LoadData(filename);
            DisplayData();
            PlotChart();
            ShowExtremes();

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


        private void button_forecast_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBox1.Text, out int N) || N <= 0)
            {
                MessageBox.Show("Введите корректное положительное число дней для прогноза.");
                return;
            }

            var chartPoints = analyzer.GetChartPoints();

            // Прогноз для каждой валюты
            var forecast1 = analyzer.ForecastRates(chartPoints.Rates1, N, 3);
            var forecast2 = analyzer.ForecastRates(chartPoints.Rates2, N, 3);

            // Дата последнего измерения
            var datal = analyzer.GetData();
            DateTime lastDate = datal.Last().Date;

            // Подготовка точек для прогноза
            var forecastDates = new List<double>();
            for (int i = 1; i <= N; i++)
                forecastDates.Add(new DateTime(lastDate.Year, lastDate.Month, lastDate.Day).AddDays(i).ToOADate());

            // Очистка и подготовка графика
            var pane = zedGraphControl1.GraphPane;
            pane.CurveList.Clear();
            pane.Title.Text = "Курс рубля с прогнозом";
            pane.XAxis.Title.Text = "Дата";
            pane.YAxis.Title.Text = "Курс";
            pane.XAxis.Type = AxisType.Date;

            // Исходные данные (отрисовать линии)
            var list1 = new PointPairList();
            var list2 = new PointPairList();
            for (int i = 0; i < chartPoints.Date.Length; i++)
            {
                list1.Add(chartPoints.Date[i].ToOADate(), chartPoints.Rates1[i]);
                list2.Add(chartPoints.Date[i].ToOADate(), chartPoints.Rates2[i]);
            }
            pane.AddCurve("USD/RUB", list1, Color.Blue, SymbolType.Circle);
            pane.AddCurve("EUR/RUB", list2, Color.Red, SymbolType.Diamond);

            // Прогноз (добавляем с другим цветом, без символов)
            var forecastList1 = new PointPairList();
            var forecastList2 = new PointPairList();
            for (int i = 0; i < N; i++)
            {
                forecastList1.Add(forecastDates[i], forecast1[i]);
                forecastList2.Add(forecastDates[i], forecast2[i]);
            }
            pane.AddCurve("USD/RUB", forecastList1, Color.Green, SymbolType.None);
            pane.AddCurve("EUR/RUB", forecastList2, Color.Orange, SymbolType.None);

            // Обновление графика
            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();
        }
    }
}
