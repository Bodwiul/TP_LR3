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
            richTextBox1.AppendText($"Максимальное падение: {extremes.MaxDecreaseRate1:F4} за {extremes.DayMaxDecreaseRate1:dd.MM.yyyy}\n\n");

            richTextBox1.AppendText($"EUR/RUB\n");
            richTextBox1.AppendText($"Максимальный прирост: {extremes.MaxIncreaseRate2:F4} за {extremes.DayMaxIncreaseRate2:dd.MM.yyyy}\n");
            richTextBox1.AppendText($"Максимальное падение: {extremes.MaxDecreaseRate2:F4} за {extremes.DayMaxDecreaseRate2:dd.MM.yyyy}\n");
        }

    }
}
