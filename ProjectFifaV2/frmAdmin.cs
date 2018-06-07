using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Web;
using System.IO;

namespace ProjectFifaV2
{
    public partial class frmAdmin : Form
    {
        private DatabaseHandler dbh;
        private OpenFileDialog opfd;

        private string path;
        DataTable table;
        SqlDataAdapter dataAdapter;
        public frmAdmin()
        {
            dbh = new DatabaseHandler();
            table = new DataTable();
            this.ControlBox = false;
            InitializeComponent();
        }

        private void btnAdminLogOut_Click(object sender, EventArgs e)
        {
            frmLogin frmLogin = new frmLogin();
            txtQuery.Text = null;
            txtPath = null;
            dgvAdminData.DataSource = null;
            frmLogin.Show();
            Hide();
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            if (txtQuery.TextLength > 0)
            {
                ExecuteSQL(txtQuery.Text);
                txtQuery.Text = "";
            }
        }

        private void ExecuteSQL(string selectCommandText)
        {
            try
            {
                dbh.TestConnection();
                dataAdapter = new SqlDataAdapter(selectCommandText, dbh.GetCon());
                dataAdapter.Fill(table);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            dgvAdminData.DataSource = table;
            dataAdapter.Dispose();
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            txtPath.Text = null;
            
            string path = GetFilePath();

            if (CheckExtension(path, "csv"))
            {
                txtPath.Text = path;
            }
            else
            {
                MessageHandler.ShowMessage("The wrong filetype is selected.");
            }
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            if (!(txtPath.Text == null))
            {
                dbh.OpenConnectionToDB();
                using (var reader = new StreamReader(path))
                {
                    List<string> listA = new List<string>();
                    List<string> listB = new List<string>();

                    while (!reader.EndOfStream)
                    {
                        var lines = reader.ReadLine();
                        var line = lines.Replace("\"", "");
                        var values = line.Split(';');

                        listA.Add(values[0]);
                        listB.Add(values[1]);

                        if (!values[1].Contains("student_id") && !values[1].Contains("poule_id"))
                        {
                            foreach (string value in values)
                            {
                                dgvAdminData.Columns.Add("Column1", value);
                            }
                            while (!values[1].Contains("student_id") && !reader.EndOfStream)
                            {
                                lines = reader.ReadLine();
                                line = lines.Replace("\"", "").Replace("NULL", "0");
                                values = line.Split(';');
                                if(values[1].Contains("student_id"))
                                {
                                    break;
                                }
                                ToSQL(values, 1);
                                dgvAdminData.Rows.Add(values[0], values[1], values[2], values[3], values[4], values[5]);
                            }
                            

                        }
                        if (!values[1].Contains("poule_id") && !values[1].Contains("team_id_a"))
                        {
                            foreach (string value in values)
                            {
                                dataGridView1.Columns.Add("Column1", value);
                            }
                            while (!values[1].Contains("poule_id") && !reader.EndOfStream)
                            {
                                lines = reader.ReadLine();
                                line = lines.Replace("\"", "").Replace("NULL", "0");
                                values = line.Split(';');
                                if (values[1].Contains("poule_id"))
                                {
                                    break;
                                }
                                ToSQL(values, 2);
                                dataGridView1.Rows.Add(values[0], values[1], values[2], values[3], values[4], values[5]);
                            }
                        }
                        if (!values[1].Contains("team_id_a") && !values[1].Contains("student_id"))
                        {
                            foreach (string value in values)
                            {
                                dataGridView2.Columns.Add("Column1", value);
                            }
                            while (!values[1].Contains("team_id_a") && !reader.EndOfStream)
                            {
                                lines = reader.ReadLine();
                                line = lines.Replace("\"", "");
                                values = line.Split(';');
                                if (reader.EndOfStream)
                                {
                                    break;
                                }
                                ToSQL(values, 3);
                                dataGridView2.Rows.Add(values[0], values[1], values[2], values[3], values[4]);
                            }
                        }
                        break;


                    }
                }
                dbh.CloseConnectionToDB();
            }
            else
            {
                MessageHandler.ShowMessage("No filename selected.");
            }
        }

        private void ToSQL(string[] value, int i)
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();
            if (i == 1)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT TblGames ON " +
                        "INSERT INTO TblGames ([Game_id], [HomeTeam], [AwayTeam], [HomeTeamScore], [AwayTeamScore]) VALUES (@id, @HomeTeam, @AwayTeam, @HomeTeamScore, @AwayTeamScore) " +
                        "SET IDENTITY_INSERT TblGames OFF"))
                    {
                        cmd.Parameters.AddWithValue("id", value[0]);
                        cmd.Parameters.AddWithValue("HomeTeam", value[1]);
                        cmd.Parameters.AddWithValue("AwayTeam", value[2]);
                        cmd.Parameters.AddWithValue("HomeTeamScore", value[3]);
                        cmd.Parameters.AddWithValue("AwayTeamScore", value[4]);
                        cmd.Connection = dbh.GetCon();
                        cmd.ExecuteNonQuery();
                    }
                    return;
                }
                catch(Exception ex)
                {
                    using (SqlCommand cmd = new SqlCommand("UPDATE [TblGames] SET [HomeTeamScore] = @HomeTeamScore, [AwayTeamScore] = @AwayTeamScore WHERE [Game_id] = @id AND [HomeTeam] = @HomeTeam AND [AwayTeam] = @AwayTeam", dbh.GetCon()))
                    {
                        cmd.Parameters.AddWithValue("id", value[0]);
                        cmd.Parameters.AddWithValue("HomeTeam", value[1]);
                        cmd.Parameters.AddWithValue("AwayTeam", value[2]);
                        cmd.Parameters.AddWithValue("HomeTeamScore", value[3]);
                        cmd.Parameters.AddWithValue("AwayTeamScore", value[4]);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            if (i == 2)
            {
                try
                {
                    using (SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT TblPlayers ON " +
                        "INSERT INTO TblPlayers ([Player_id], [Name], [Surname], [GoalsScored], [Team_id]) VALUES (@id, @Name, @Surname, @GoalsScored, @Team_id) " +
                        "SET IDENTITY_INSERT TblPlayers OFF"))
                    {
                        cmd.Parameters.AddWithValue("id", value[0]);
                        cmd.Parameters.AddWithValue("Name", value[3]);
                        cmd.Parameters.AddWithValue("SurName", value[4]);
                        cmd.Parameters.AddWithValue("GoalsScored", 0);
                        cmd.Parameters.AddWithValue("Team_id", value[2]);
                        cmd.Connection = dbh.GetCon();
                        cmd.ExecuteNonQuery();
                        return;
                    }
                }
                catch(Exception ex)
                {
                    return;
                }
            }
             if (i == 3)
             {
                 try
                 {
                     using (SqlCommand cmd = new SqlCommand("SET IDENTITY_INSERT TblTeams ON " +
                         "INSERT INTO TblTeams ([Team_Id], [TeamName]) VALUES (@id, @Name) " +
                         "SET IDENTITY_INSERT TblTeams OFF"))
                     {
                         cmd.Parameters.AddWithValue("id", value[0]);
                         cmd.Parameters.AddWithValue("Name", value[2]);
                         cmd.Connection = dbh.GetCon();
                         cmd.ExecuteNonQuery();
                     }
                     return;
                 }
                 catch(Exception ex)
                 {
                    return;
                }
             }
            dbh.CloseConnectionToDB();
        }
        private string GetFilePath()
        {
            string filePath = "";
            opfd = new OpenFileDialog();

            opfd.Multiselect = false;

            if (opfd.ShowDialog() == DialogResult.OK)
            {
                filePath = opfd.FileName;
            }
            path = filePath;
            return filePath;
        }

        private bool CheckExtension(string fileString, string extension)
        {
            int extensionLength = extension.Length;
            int strLength = fileString.Length;

            string ext = null;
            if (fileString.Length >1)
            {
                ext = fileString.Substring(strLength - extensionLength, extensionLength);
            }
            else
            {
                MessageBox.Show("Choose a correct path.");
            }

            if (ext == extension)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ClearDatagrid(DataGridView dataGrid)
        {
            dataGrid.Rows.Clear();
            dataGrid.Columns.Clear();
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {               
                dataGridView1.Rows.Clear();
                

                dgvAdminData.DataSource = null;
                dgvAdminData.Refresh();

                ClearDatagrid(dataGridView1);
                ClearDatagrid(dataGridView2);
                ClearDatagrid(dgvAdminData);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
