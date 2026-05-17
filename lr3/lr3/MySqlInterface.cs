using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lr3
{
    internal class MySqlInterface
    {
        private string connectionString = "Server=localhost;Database=dashboard;Uid=root;Pwd=root;";
        public void machineToolType(ComboBox box)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "SELECT id_mt, type FROM machine_tool_type";
                    MySqlDataAdapter da = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    box.DataSource = dt;
                    box.DisplayMember = "type";  
                    box.ValueMember = "id_mt";  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}");
            }
        }
        public void machineToolName(int typeId, ComboBox box)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    string query = "SELECT id_mtn, machine_tool_name FROM machine_tool_name WHERE id_mt = @id_mt";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id_mt", typeId);

                    MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    box.DataSource = dt;
                    box.DisplayMember = "machine_tool_name"; 
                    box.ValueMember = "id_mtn";          
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки станков: {ex.Message}");
            }
        }
        public DataTable GetMachineLoad(int machineId)
        {
            DataTable dt = new DataTable();
            string query = "SELECT hr AS 'Время', temp AS 'Температура' " +
                           "FROM machine_tool_load " +
                           "WHERE id_mtn = @id_mtn " +
                           "ORDER BY hr";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_mtn", machineId);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
        public DataTable GetMachineState(int machineId)
        {
            DataTable dt = new DataTable();
            string query = "SELECT imp AS 'Важность', " + 
                           "code AS 'Код сообщения', " +
                           "text AS 'Текст сообщения', " +
                           "type AS 'Вид', " +
                           "hr AS 'Время', " +
                           "channel AS 'Канал' " +
                           "FROM machine_tool_state " +
                           "WHERE id_mtn = @id_mtn " +
                           "ORDER BY hr DESC"; // Сортируем: сначала самые свежие сообщения

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_mtn", machineId);
                    using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
        public string GetMachineAverageTemperature(int machineId)
        {
            string avgTemp = "0";

            // Вычисляем среднее значение столбца temp для конкретного станка
            // и округляем до 1 знака после запятой
            string query = "SELECT ROUND(AVG(temp), 1) FROM machine_tool_load WHERE id_mtn = @id_mtn";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id_mtn", machineId);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar(); // Получаем одиночное число из БД

                        if (result != null && result != DBNull.Value)
                        {
                            avgTemp = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Если произойдет ошибка в БД, метод вернет "ошибка" вместо падения приложения
                        avgTemp = "ошибка";
                        Console.WriteLine($"Ошибка подсчета температуры: {ex.Message}");
                    }
                }
            }
            return avgTemp;
        }
    }
}
