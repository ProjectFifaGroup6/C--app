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

namespace ProjectFifaV2
{
    public partial class frmAdmin : Form
    {
        private DatabaseHandler dbh;
        private OpenFileDialog opfd;

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

                dbh.CloseConnectionToDB();
            }
            else
            {
                MessageHandler.ShowMessage("No filename selected.");
            }
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

            return filePath;
        }

        private bool CheckExtension(string fileString, string extension)
        {
            int extensionLength = extension.Length;
            int strLength = fileString.Length;

            string ext = fileString.Substring(strLength - extensionLength, extensionLength);

            if (ext == extension)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {

                table.Clear();
                dgvAdminData.DataSource = null;
                dgvAdminData.Refresh();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
