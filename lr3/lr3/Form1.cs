using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace lr3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }
        private MySqlInterface MSI = new MySqlInterface();
        private void Form1_Load(object sender, EventArgs e)
        {
            MSI.machineToolType(comboBox_TypeStanka);
        }
        private void button_Monitoring_Click(object sender, EventArgs e)
        {
           //ДОПИСАТЬ КОД ОБРАБОТКИ, НО ХЗ

        }

        private void button_Glavnaya_Click(object sender, EventArgs e)
        {

        }

        private void button_Analiz_Click(object sender, EventArgs e)
        {

        }

        private void comboBox_TypeStanka_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_TypeStanka.SelectedValue != null && comboBox_TypeStanka.SelectedValue is int)
            {
                int selectedTypeId = (int)comboBox_TypeStanka.SelectedValue;
                MSI.machineToolName(selectedTypeId, comboBox_Name_Stanka);
            }
        }

        private void comboBox_Name_Stanka_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_TypeStanka.SelectedValue != null && comboBox_TypeStanka.SelectedValue is int)
            {
                if (comboBox_Name_Stanka.SelectedItem == null)
                {
                    return;
                }
                string selectedMachineName = comboBox_Name_Stanka.GetItemText(comboBox_Name_Stanka.SelectedItem);
                label_Main_Name_and_Type_Stanka.Text = selectedMachineName;
                label_NameStanka_Under_Foto.Text = selectedMachineName;
            }
            if (comboBox_Name_Stanka.SelectedValue == null) return;
            int selectedMachineId = -1;
            if (comboBox_Name_Stanka.SelectedValue is int)
            {
                selectedMachineId = (int)comboBox_Name_Stanka.SelectedValue;
            }
            else if (comboBox_Name_Stanka.SelectedValue is DataRowView)
            {
                var row = (DataRowView)comboBox_Name_Stanka.SelectedValue;
                selectedMachineId = Convert.ToInt32(row["id_mtn"]);
            }

            if (selectedMachineId != -1)
            {
                try
                {
                    DataTable loadTable = MSI.GetMachineLoad(selectedMachineId);
                    dataGridView2.DataSource = loadTable;
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    BuildTemperatureChart(loadTable);
                    UpdateMachineImage(selectedMachineId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных нагрузки: {ex.Message}");
                }
            }
            if (selectedMachineId != -1)
            {
                string averageTemp = MSI.GetMachineAverageTemperature(selectedMachineId);
                label_Sr_Znach_Temperature.Text = $"Средняя суточная температура шпинделя: {averageTemp} °C";
                try
                {
                    DataTable stateTable = MSI.GetMachineState(selectedMachineId);
                    dataGridView1.DataSource = stateTable;
                    dataGridView1.Columns["Важность"].Width = 20;
                    dataGridView1.Columns["Важность"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["Вид"].Width = 30;
                    dataGridView1.Columns["Вид"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["Код сообщения"].Width = 40;
                    dataGridView1.Columns["Код сообщения"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["Текст сообщения"].Width = 260;
                    dataGridView1.Columns["Текст сообщения"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["Время"].Width = 70;
                    dataGridView1.Columns["Время"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dataGridView1.Columns["Канал"].Width = 30;
                    dataGridView1.Columns["Канал"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных станка: {ex.Message}");
                }
            }
        }
        private void BuildTemperatureChart(DataTable loadData)
        {
            // 1. Полностью очищаем старые данные и серии на графике
            Grafik.Series.Clear();
            Grafik.ChartAreas.Clear();

            // 2. Создаем новую область графика и настраиваем оси
            ChartArea chartArea = new ChartArea("MainArea");

            // Настройка сетки и подписей для оси X (Время)
            chartArea.AxisX.Title = "Время суток (ч)";
            chartArea.AxisX.Interval = 2; // Показывать подпись каждые 2 часа, чтобы текст не слипался
            chartArea.AxisX.MajorGrid.LineColor = Color.LightGray; // Делаем сетку неяркой

            // Настройка сетки для оси Y (Температура)
            chartArea.AxisY.Title = "Температура (°C)";
            chartArea.AxisY.MajorGrid.LineColor = Color.LightGray;
            // Можно зафиксировать или автоматически определять масштаб оси Y:
            chartArea.AxisY.IsStartedFromZero = false; // График будет масштабироваться под реальные температуры (например, от 30 до 60 градусов)

            Grafik.ChartAreas.Add(chartArea);

            // 3. Создаем линию графика (Серию данных)
            Series series = new Series("Температура шпинделя");
            series.ChartType = SeriesChartType.Spline;      // Тип графика - линия
            series.BorderWidth = 3;                       // Толщина линии
            series.Color = Color.DodgerBlue;              // Красивый синий цвет линии
            series.XValueType = ChartValueType.String;    // По оси X у нас строки (например, '01:00')

            // 4. Наполняем серию точками из нашей таблицы данных
            foreach (DataRow row in loadData.Rows)
            {
                string hour = row["Время"].ToString();
                double temperature = Convert.ToDouble(row["Температура"]);

                // Добавляем точку на график (X - время, Y - температура)
                series.Points.AddXY(hour, temperature);
            }

            // 5. Добавляем настроенную серию на компонент Chart
            Grafik.Series.Add(series);

            // Необязательно: добавляем маркеры (точки) на изгибах линии
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 6;
            series.MarkerColor = Color.DarkBlue;
        }

        private void UpdateMachineImage(int machineId)
        {
            try
            {
                // 1. Формируем относительный путь к картинке: "images/1.png", "images/2.png" и т.д.
                // Переменная Application.StartupPath автоматически указывает на папку bin/Debug вашего проекта
                string imagePath = Path.Combine(Application.StartupPath, "stanki", $"{machineId}.png");

                // 2. Проверяем, существует ли файл физически на диске
                if (File.Exists(imagePath))
                {
                    // Перед загрузкой новой картинки освобождаем старую из памяти, если она была
                    if (pictureBox_Machine.Image != null)
                    {
                        pictureBox_Machine.Image.Dispose();
                    }

                    // Загружаем изображение из файла
                    pictureBox_Machine.Image = Image.FromFile(imagePath);
                    pictureBox_Machine.SizeMode = PictureBoxSizeMode.StretchImage;
                }
                else
                {
                    // Если файла нет (например, забыли перенести картинку) — убираем старое изображение
                    pictureBox_Machine.Image = null;
                    MessageBox.Show($"Файл не найден по пути: {imagePath}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось загрузить изображение станка: {ex.Message}");
            }
        }

        private void dataGridView1_RowPrePaint_1(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            //не раблтает
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //не надо
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0)
            {

                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                if (e.Value != null && e.Value != DBNull.Value)
                {
                    int imp = Convert.ToInt32(e.Value);

                    Brush indicatorBrush;
                    switch (imp)
                    {
                        case 0: indicatorBrush = Brushes.Blue; break;   // Info (Синий)
                        case 1: indicatorBrush = Brushes.Orange; break; // Warning (Оранжевый)
                        case 2: indicatorBrush = Brushes.Red; break;    // Error (Красный)
                        default: indicatorBrush = Brushes.Gray; break;
                    }

                    int rectWidth = 20;
                    int rectHeight = 20;


                    int rectX = e.CellBounds.X + (e.CellBounds.Width - rectWidth) / 2;
                    int rectY = e.CellBounds.Y + (e.CellBounds.Height - rectHeight) / 2;

                    Rectangle rect = new Rectangle(rectX, rectY, rectWidth, rectHeight);


                    e.Graphics.FillRectangle(indicatorBrush, rect);
                }

                e.Handled = true;
            }
        }
    }
}
