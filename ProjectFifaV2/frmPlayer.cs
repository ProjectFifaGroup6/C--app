using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ProjectFifaV2
{
    public partial class frmPlayer : Form
    {
        private Form frmRanking;
        private DatabaseHandler dbh;
        List<TextBox> txtBoxList;



        public frmPlayer(Form frm, string un)
        {
            this.ControlBox = false;
            frmRanking = frm;
            dbh = new DatabaseHandler();
            InitializeComponent();
            if (DisableEditButton())
            {
                btnEditPrediction.Enabled = false;
            }
            ShowResults();
            ShowScoreCard();
            foreach (Control c in pnlPredCard.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                {
                    ((TextBox)(c)).ReadOnly = true;
                }
            }
            this.Text = "Welcome " + un;
        }

        private void btnLogOut_Click(object sender, EventArgs e)
        {
            frmLogin frmLogin = new frmLogin();
            Hide();
            frmLogin.Show();
        }

        private void btnShowRanking_Click(object sender, EventArgs e)
        {
            frmRanking.Show();
        }

        private void btnClearPrediction_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to clear your prediction?", "Clear Predictions", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (result.Equals(DialogResult.OK))
            {
                foreach (Control c in pnlPredCard.Controls)
                {
                    if (c.GetType() == typeof(TextBox))
                    {
                        if (((TextBox)(c)).ReadOnly == true)
                        {
                            return;
                        }
                        else if (((TextBox)(c)).ReadOnly == false)
                        {
                            ((TextBox)(c)).Text = "0";
                        }
                    }
                }
            }
        }

        private bool DisableEditButton()
        {
            bool hasPassed;
            //This is the deadline for filling in the predictions
            DateTime deadline = new DateTime(2019, 06, 12);
            DateTime curTime = DateTime.Now;
            int result = DateTime.Compare(deadline, curTime);

            if (result < 0)
            {
                hasPassed = true;
            }
            else
            {
                hasPassed = false;
            }

            return hasPassed;
        }

        private void ShowResults()
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();

            DataTable hometable = dbh.FillDT("SELECT tblTeams.TeamName, tblGames.HomeTeamScore FROM tblGames INNER JOIN tblTeams ON tblGames.HomeTeam = tblTeams.Team_ID");
            DataTable awayTable = dbh.FillDT("SELECT tblTeams.TeamName, tblGames.AwayTeamScore FROM tblGames INNER JOIN tblTeams ON tblGames.AwayTeam = tblTeams.Team_ID");

            dbh.CloseConnectionToDB();

            for (int i = 0; i < hometable.Rows.Count; i++)
            {
                DataRow dataRowHome = hometable.Rows[i];
                DataRow dataRowAway = awayTable.Rows[i];
                ListViewItem lstItem = new ListViewItem(dataRowHome["TeamName"].ToString());
                lstItem.SubItems.Add(dataRowHome["HomeTeamScore"].ToString());
                lstItem.SubItems.Add(dataRowAway["AwayTeamScore"].ToString());
                lstItem.SubItems.Add(dataRowAway["TeamName"].ToString());
                lvOverview.Items.Add(lstItem);
            }
        }
        public int GetUID()
        {
            int uid;
            using (SqlCommand cmd = new SqlCommand("SELECT Id FROM [TblUsers] WHERE Username = @user", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("user", frmLogin.getuser());
                uid = (int)cmd.ExecuteScalar();
            }
            return (uid);
        }
        private void ShowScoreCard()
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();
            int HomePred = 0;
            int AwayPred = 0;
            DataTable hometable = dbh.FillDT("SELECT tblTeams.TeamName FROM tblGames INNER JOIN tblTeams ON tblGames.HomeTeam = tblTeams.Team_ID");
            DataTable awayTable = dbh.FillDT("SELECT tblTeams.TeamName, tblGames.Game_id FROM tblGames INNER JOIN tblTeams ON tblGames.AwayTeam = tblTeams.Team_ID");
            dbh.CloseConnectionToDB();
            for (int i = 0; i < hometable.Rows.Count; i++)
            {
                dbh.OpenConnectionToDB();
                DataRow dataRowHome = hometable.Rows[i];
                DataRow dataRowAway = awayTable.Rows[i];

                if (dataRowAway["Game_id"] != null)
                {
                    game_id[i] = dataRowAway["Game_id"].ToString();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT PredictedHomeScore FROM [TblPredictions] WHERE user_id = @uid AND game_id = @game", dbh.GetCon()))
                {
                    cmd.Parameters.AddWithValue("uid", GetUID());
                    cmd.Parameters.AddWithValue("game", dataRowAway["Game_id"].ToString());

                    HomePred = (int)cmd.ExecuteScalar();
                }
                using (SqlCommand cmd = new SqlCommand("SELECT PredictedAwayScore FROM [TblPredictions] WHERE user_id = @uid AND game_id = @game", dbh.GetCon()))
                {
                    cmd.Parameters.AddWithValue("uid", GetUID());
                    cmd.Parameters.AddWithValue("game", dataRowAway["Game_id"].ToString());

                    AwayPred = (int)cmd.ExecuteScalar();
                }

                Label lblHomeTeam = new Label();
                Label lblAwayTeam = new Label();
                TextBox txtHomePred = new TextBox();
                TextBox txtAwayPred = new TextBox();

                lblHomeTeam.TextAlign = ContentAlignment.BottomRight;
                lblHomeTeam.Text = dataRowHome["TeamName"].ToString();
                lblHomeTeam.Location = new Point(20, txtHomePred.Bottom + (i * 30));
                lblHomeTeam.AutoSize = true;

                txtHomePred.Text = HomePred.ToString();
                txtHomePred.Location = new Point(lblHomeTeam.Width, lblHomeTeam.Top - 3);
                txtHomePred.Width = 40;
                txtHomePred.Name = "txtHome" + i;

                txtAwayPred.Text = AwayPred.ToString();
                txtAwayPred.Location = new Point(txtHomePred.Width + lblHomeTeam.Width, txtHomePred.Top);
                txtAwayPred.Width = 40;
                txtAwayPred.Name = "txtAway" + i;

                lblAwayTeam.Text = dataRowAway["TeamName"].ToString();
                lblAwayTeam.Location = new Point(txtHomePred.Width + lblHomeTeam.Width + txtAwayPred.Width, txtHomePred.Top + 3);
                lblAwayTeam.AutoSize = true;

                pnlPredCard.Controls.Add(lblHomeTeam);
                pnlPredCard.Controls.Add(txtHomePred);
                pnlPredCard.Controls.Add(txtAwayPred);
                pnlPredCard.Controls.Add(lblAwayTeam);
                //ListViewItem lstItem = new ListViewItem(dataRowHome["TeamName"].ToString());
                //lstItem.SubItems.Add(dataRowHome["HomeTeamScore"].ToString());
                //lstItem.SubItems.Add(dataRowAway["AwayTeamScore"].ToString());
                //lstItem.SubItems.Add(dataRowAway["TeamName"].ToString());
                //lvOverview.Items.Add(lstItem);
                dbh.CloseConnectionToDB();
            }
        }
        public string[] game_id = new string[20];
        public int[] AwayScore = new int[20];
        public int[] HomeScore = new int[20];
        private void btnAddPrediction_Click(object sender, EventArgs e)
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();

            foreach (Control c in pnlPredCard.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                {
                    if(((TextBox)(c)).Name.Contains("txtAway")| ((TextBox)(c)).Name.Contains("txtHome"))
                    {
                        int index = 0;
                        if(((TextBox)(c)).Name.Contains("txtHome"))
                        {
                            index = Int32.Parse(((TextBox)(c)).Name.Replace("txtHome", ""));
                            HomeScore[index] = Int32.Parse(((TextBox)(c)).Text);
                        }
                        else if(((TextBox)(c)).Name.Contains("txtAway"))
                        {
                            index = Int32.Parse(((TextBox)(c)).Name.Replace("txtAway", ""));
                            AwayScore[index] = Int32.Parse(((TextBox)(c)).Text);
                        }
                        if(game_id[index] != null)
                        {
                            if (Exist(game_id[index]))
                            {
                                dbh.OpenConnectionToDB();
                                using (SqlCommand cmd = new SqlCommand("UPDATE [TblPredictions] SET PredictedHomeScore = @Home , PredictedAwayScore = @Away WHERE Game_id = @Game AND User_id = @uid", dbh.GetCon()))
                                {
                                    cmd.Parameters.AddWithValue("Home", HomeScore[index]);
                                    cmd.Parameters.AddWithValue("Away", AwayScore[index]);
                                    cmd.Parameters.AddWithValue("Game", game_id[index]);
                                    cmd.Parameters.AddWithValue("uid", GetUID());
                                    cmd.ExecuteNonQuery();
                                }
                                dbh.CloseConnectionToDB();
                            }
                            else if(!Exist(game_id[index]))
                            {
                                dbh.OpenConnectionToDB();
                                using (SqlCommand cmd = new SqlCommand("INSERT INTO [TblPredictions](User_id, Game_id, PredictedHomeScore, PredictedAwayScore) VALUES (@uid, @Game, @Home, @Away)", dbh.GetCon()))
                                {
                                    cmd.Parameters.AddWithValue("Home", HomeScore[index]);
                                    cmd.Parameters.AddWithValue("Away", AwayScore[index]);
                                    cmd.Parameters.AddWithValue("Game", game_id[index]);
                                    cmd.Parameters.AddWithValue("uid", GetUID());
                                    cmd.ExecuteNonQuery();
                                }
                                dbh.CloseConnectionToDB();
                            }
                            
                        }
                    }
                    ((TextBox)(c)).ReadOnly = true;
                }
            }
            
            dbh.CloseConnectionToDB();
        }
        public bool Exist(string Game_Id)
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();

            bool exist = false;
            using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM [TblPredictions] WHERE User_Id = @Username AND game_id = @game_id", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("Username", GetUID());
                cmd.Parameters.AddWithValue("game_id", Int32.Parse(Game_Id));
                exist = (int)cmd.ExecuteScalar() > 0;
            }
            dbh.CloseConnectionToDB();
            return exist; 
        }

        private void btnEditPrediction_Click(object sender, EventArgs e)
        {
            foreach (Control c in pnlPredCard.Controls)
            {
                if (c.GetType() == typeof(TextBox))
                {
                    ((TextBox)(c)).ReadOnly = false;
                }
            }
        }
    }
}
