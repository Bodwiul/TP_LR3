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
    }
}
