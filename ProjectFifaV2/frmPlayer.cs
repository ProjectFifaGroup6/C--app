﻿using System;
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

        int[] Finished = new int[20];

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
                    if (c.GetType() == typeof(NumericUpDown))
                    {
                        if (((NumericUpDown)(c)).ReadOnly == true)
                        {
                            return;
                        }
                        else if (((NumericUpDown)(c)).ReadOnly == false)
                        {
                            ((NumericUpDown)(c)).Text = "0";
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
                if(Int32.Parse(dataRowAway["AwayTeamScore"].ToString()) > 0 | Int32.Parse(dataRowHome["HomeTeamScore"].ToString()) > 0)
                {
                   CheckPrediction(dataRowHome["Teamname"].ToString(), dataRowAway["TeamName"].ToString(),Int32.Parse(dataRowAway["AwayTeamScore"].ToString()), Int32.Parse(dataRowHome["HomeTeamScore"].ToString()), i);
                }
                lstItem.SubItems.Add(dataRowAway["TeamName"].ToString());
                lvOverview.Items.Add(lstItem);
            }
        }
        public void CheckPrediction(string HomeTeam, string AwayTeam, int AwayScore, int HomeScore, int index)
        {
            dbh.TestConnection();
            dbh.OpenConnectionToDB();

            int Home_id = 0;
            int Away_id = 0;
            int Game_id = 0;
            int Count = 0;
            using (SqlCommand cmd = new SqlCommand("SELECT Team_Id FROM TblTeams WHERE TeamName = @HomeTeam", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("HomeTeam", HomeTeam);
                Home_id = (int)cmd.ExecuteScalar();
            }
            using (SqlCommand cmd = new SqlCommand("SELECT Team_Id FROM TblTeams WHERE TeamName = @AwayTeam", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("AwayTeam", AwayTeam);
                Away_id = (int)cmd.ExecuteScalar();
            }
            using (SqlCommand cmd = new SqlCommand("SELECT Game_id FROM TblGames WHERE HomeTeam = @HomeTeam AND AwayTeam = @AwayTeam", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("HomeTeam", Home_id);
                cmd.Parameters.AddWithValue("AwayTeam", Away_id);
                Game_id = (int)cmd.ExecuteScalar();
            }

            using (SqlCommand cmd = new SqlCommand("SELECT Count(*) FROM TblPredictions WHERE Game_id = @Game_id AND PredictedHomeScore = @HomeScore AND PredictedAwayScore = @AwayScore AND User_id = @uid", dbh.GetCon()))
            {
                cmd.Parameters.AddWithValue("Game_id", Game_id);
                cmd.Parameters.AddWithValue("HomeScore", HomeScore);
                cmd.Parameters.AddWithValue("AwayScore", AwayScore);
                cmd.Parameters.AddWithValue("uid", GetUID());
                Count = (int)cmd.ExecuteScalar();

            }
            Finished[index] = 1;
            if (Count == 1)
            {
                int rewarded = 0;
                using (SqlCommand cmd = new SqlCommand("SELECT [rewarded] FROM tblPredictions WHERE Game_id = @game_id AND User_id = @uid", dbh.GetCon()))
                {
                    cmd.Parameters.AddWithValue("game_id", Game_id);
                    cmd.Parameters.AddWithValue("uid", GetUID());
                    rewarded = (int)cmd.ExecuteScalar();
                }
                if (rewarded == 0)
                {
                    MessageBox.Show("You Have Correctly Predicted Game : " + HomeTeam + " VS " + AwayTeam);
                    using (SqlCommand cmd = new SqlCommand("UPDATE TblPredictions SET rewarded = 1 WHERE Game_id = @game_id AND User_id = @uid", dbh.GetCon()))
                    {
                        cmd.Parameters.AddWithValue("game_id", Game_id);
                        cmd.Parameters.AddWithValue("uid", GetUID());
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand("UPDATE TblUsers SET score = score + 1 WHERE id = @uid", dbh.GetCon()))
                    {
                        cmd.Parameters.AddWithValue("uid", GetUID());
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            dbh.CloseConnectionToDB();
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
                    object x = cmd.ExecuteScalar();
                    if (x != null)
                    {
                        HomePred = (int)cmd.ExecuteScalar();
                    }
                }
                using (SqlCommand cmd = new SqlCommand("SELECT PredictedAwayScore FROM [TblPredictions] WHERE user_id = @uid AND game_id = @game", dbh.GetCon()))
                {
                    cmd.Parameters.AddWithValue("uid", GetUID());
                    cmd.Parameters.AddWithValue("game", dataRowAway["Game_id"].ToString());
                    object z = cmd.ExecuteScalar();
                    if (z != null)
                    {
                        AwayPred = (int)cmd.ExecuteScalar();
                    }
                }

                int rewarded = 0;
                using (SqlCommand cmd = new SqlCommand("SELECT rewarded FROM [TblPredictions] WHERE Game_id = @game_id AND User_id = @uid", dbh.GetCon()))
                {
                    cmd.Parameters.AddWithValue("game_id", dataRowAway["Game_id"].ToString());
                    cmd.Parameters.AddWithValue("uid", GetUID());
                    object y = cmd.ExecuteScalar();
                    if (y != null)
                    {
                        rewarded = (int)cmd.ExecuteScalar();
                    }
                }
                Label GameOver = new Label();
                Label lblHomeTeam = new Label();
                Label lblAwayTeam = new Label();
                NumericUpDown txtHomePred = new NumericUpDown();
                NumericUpDown txtAwayPred = new NumericUpDown();

                lblHomeTeam.TextAlign = ContentAlignment.BottomRight;
                lblHomeTeam.Text = dataRowHome["TeamName"].ToString();
                lblHomeTeam.Location = new Point(20, txtHomePred.Bottom + (i * 30));
                lblHomeTeam.AutoSize = true;

                txtHomePred.Text = HomePred.ToString();
                txtHomePred.Location = new Point(lblHomeTeam.Width, lblHomeTeam.Top - 3);
                txtHomePred.Width = 40;
                txtHomePred.Name = "txtHome" + i;
                txtHomePred.Minimum = 0;
                txtHomePred.Maximum = 20;

                txtAwayPred.Text = AwayPred.ToString();
                txtAwayPred.Location = new Point(txtHomePred.Width + lblHomeTeam.Width, txtHomePred.Top);
                txtAwayPred.Width = 40;
                txtAwayPred.Name = "txtAway" + i;
                txtAwayPred.Minimum = 0;
                txtAwayPred.Maximum = 20;

                if (rewarded == 1 || Finished[i] == 1)
                {
                    txtAwayPred.Hide();
                    txtHomePred.Hide();
                    GameOver.Text = "Finished";
                    GameOver.Location = new Point(txtHomePred.Location.X, txtHomePred.Location.Y);
                }
                lblAwayTeam.Text = dataRowAway["TeamName"].ToString();
                lblAwayTeam.Location = new Point(txtHomePred.Width + lblHomeTeam.Width + txtAwayPred.Width, txtHomePred.Top + 3);
                lblAwayTeam.AutoSize = true;

                pnlPredCard.Controls.Add(lblHomeTeam);
                pnlPredCard.Controls.Add(txtHomePred);
                pnlPredCard.Controls.Add(txtAwayPred);
                pnlPredCard.Controls.Add(lblAwayTeam);
                pnlPredCard.Controls.Add(GameOver);
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
                if (c.GetType() == typeof(NumericUpDown))
                {
                    if(((NumericUpDown)(c)).Name.Contains("txtAway")| ((NumericUpDown)(c)).Name.Contains("txtHome"))
                    {
                        int index = 0;
                        if(((NumericUpDown)(c)).Name.Contains("txtHome"))
                        {
                            index = Int32.Parse(((NumericUpDown)(c)).Name.Replace("txtHome", ""));
                            HomeScore[index] = Int32.Parse(((NumericUpDown)(c)).Text);
                        }
                        else if(((NumericUpDown)(c)).Name.Contains("txtAway"))
                        {
                            index = Int32.Parse(((NumericUpDown)(c)).Name.Replace("txtAway", ""));
                            AwayScore[index] = Int32.Parse(((NumericUpDown)(c)).Text);
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
                    MessageBox.Show("All Prediction Added!");
                    ((NumericUpDown)(c)).ReadOnly = true;
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
                if (c.GetType() == typeof(NumericUpDown))
                {
                    ((NumericUpDown)(c)).ReadOnly = false;
                }
            }
        }
    }
}
