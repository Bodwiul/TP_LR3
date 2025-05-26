using System.Windows.Forms;

namespace WindowsFormsApp_lr3
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.button_currency = new System.Windows.Forms.Button();
            this.button_migration = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.zedGraphControl1 = new ZedGraph.ZedGraphControl();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.button_forecast = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            
            // button_currency
            this.button_currency.Location = new System.Drawing.Point(6, 26);
            this.button_currency.Name = "button_currency";
            this.button_currency.Size = new System.Drawing.Size(512, 32);
            this.button_currency.TabIndex = 0;
            this.button_currency.Text = "Данные о курсе рубля";
            this.button_currency.UseVisualStyleBackColor = true;
            this.button_currency.Click += new System.EventHandler(this.button_currency_Click);
            
            // button_migration
            this.button_migration.Location = new System.Drawing.Point(6, 65);
            this.button_migration.Name = "button_migration";
            this.button_migration.Size = new System.Drawing.Size(512, 32);
            this.button_migration.TabIndex = 1;
            this.button_migration.Text = "Данные о миграции\r\n\r\n";
            this.button_migration.UseVisualStyleBackColor = true;
            this.button_migration.Click += new System.EventHandler(this.button_migration_Click_1);

            // groupBox1
            this.groupBox1.Controls.Add(this.dataGridView1);
            this.groupBox1.Controls.Add(this.button_currency);
            this.groupBox1.Controls.Add(this.button_migration);
            this.groupBox1.Location = new System.Drawing.Point(21, 18);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(524, 411);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Получение данных";

            // dataGridView1
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(6, 103);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.Size = new System.Drawing.Size(512, 302);
            this.dataGridView1.TabIndex = 0;

            // groupBox2
            this.groupBox2.Controls.Add(this.zedGraphControl1);
            this.groupBox2.Location = new System.Drawing.Point(550, 18);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(578, 411);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Результаты обработки";

            // zedGraphControl1
            this.zedGraphControl1.Location = new System.Drawing.Point(7, 27);
            this.zedGraphControl1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.zedGraphControl1.Name = "zedGraphControl1";
            this.zedGraphControl1.ScrollGrace = 0D;
            this.zedGraphControl1.ScrollMaxX = 0D;
            this.zedGraphControl1.ScrollMaxY = 0D;
            this.zedGraphControl1.ScrollMaxY2 = 0D;
            this.zedGraphControl1.ScrollMinX = 0D;
            this.zedGraphControl1.ScrollMinY = 0D;
            this.zedGraphControl1.ScrollMinY2 = 0D;
            this.zedGraphControl1.Size = new System.Drawing.Size(564, 382);
            this.zedGraphControl1.TabIndex = 1;
            this.zedGraphControl1.UseExtendedPrintDialog = true;

            // richTextBox1
            this.richTextBox1.Location = new System.Drawing.Point(550, 435);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(576, 187);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";

            // groupBox3
            this.groupBox3.Controls.Add(this.button_forecast);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Location = new System.Drawing.Point(21, 437);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.groupBox3.Size = new System.Drawing.Size(524, 188);
            this.groupBox3.TabIndex = 5;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Статистическое прогнозирование ";

            // button_forecast
            this.button_forecast.Location = new System.Drawing.Point(14, 143);
            this.button_forecast.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_forecast.Name = "button_forecast";
            this.button_forecast.Size = new System.Drawing.Size(501, 35);
            this.button_forecast.TabIndex = 4;
            this.button_forecast.Text = "Спрогнозировать";
            this.button_forecast.UseVisualStyleBackColor = true;
            this.button_forecast.Click += new System.EventHandler(this.button_forecast_Click);

            // textBox2
            this.textBox2.Location = new System.Drawing.Point(14, 109);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(499, 26);
            this.textBox2.TabIndex = 3;

            // label3
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 85);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(330, 20);
            this.label3.TabIndex = 2;
            this.label3.Text = "Ввведите количество лет (для миграции)";

            // textBox1
            this.textBox1.Location = new System.Drawing.Point(14, 49);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(499, 26);
            this.textBox1.TabIndex = 1;

            // label2
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 25);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(357, 20);
            this.label2.TabIndex = 0;
            this.label2.Text = "Ввведите количество дней (для курса рубля)";

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1137, 642);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Button button_currency;
        private Button button_migration;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private DataGridView dataGridView1;
        private RichTextBox richTextBox1;
        private GroupBox groupBox3;
        private Label label3;
        private TextBox textBox1;
        private Label label2;
        private Button button_forecast;
        private TextBox textBox2;
        private ZedGraph.ZedGraphControl zedGraphControl1;
    }
}