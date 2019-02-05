using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace MonsterDataViewer
{
    public partial class MonsterViewer : Form
    {
        public MonsterViewer()
        {
            InitializeComponent();
        }


        /// <summary>
        /// 閉じるボタン押下時処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        /// <summary>
        /// 表示押下時処理。
        /// </summary>
        /// <remarks>
        /// TableAdapterを使ってパラメーターによる条件指定を用いたSQLでデータを取得しているため、
        /// SQLインジェクションは起こらない。
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShow_Click(object sender, EventArgs e)
        {
            if (textCondition.Text.Trim().Length == 0)
            {
                monsterTblAdapter.Fill(monsterDataSet.Monsters);
                gridMain.DataSource = monsterDataSet.Monsters;
            }
            else
            {
                monsterTblAdapter.FillByTypes(monsterDataSet.Monsters, textCondition.Text, textCondition.Text);
                gridMain.DataSource = monsterDataSet.Monsters;
            }
            
        }


        /// <summary>
        /// 表示（ダメ）押下時処理。
        /// </summary>
        /// <remarks>
        /// SqlAdapterとSqlCommandで、SQLを文字列組み立てで作成している。
        /// パラメーターによる条件指定を行っていないので、SQLインジェクションが起こる。
        /// たとえば
        /// 　　「' OR 1=1; insert into monsters values(231,'謎モンスター','Riddle','なぞ',null,null,null); SELECT * FROM Monsters Where Type1='」
        /// を打ち込むと、エラーにはなるが新データは追加される。
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShowDame_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM Monsters ";
            if (textCondition.Text.Trim().Length != 0)
            {
                // SQLインジェクション対策処理(単純置換)
                //string sanitizedText = textCondition.Text.Replace("'", "''");
                //query += "WHERE Type1='" + sanitizedText + "' OR Type2='" + sanitizedText + "'";

                // SQLインジェクション未対策処理(文字列直結)
                //query += "WHERE Type1='" + textCondition.Text + "' OR Type2='" + textCondition.Text + "'";

                // SQLインジェクション対策処理(単純置換)+Unicode検索条件対策(Nプリフィックス)
                string sanitizedText = textCondition.Text.Replace("'", "''");
                query += "WHERE Type1=N'" + sanitizedText + "' OR Type2=N'" + sanitizedText + "'";

            }

            string connectionString = "Data Source=SIGMA-WSV009;Initial Catalog=koushi;User ID=koushi_admin;Password=koushi_admin";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand command = new SqlCommand();
            DataSet ds = new DataSet();
            using (SqlDataAdapter adapter = new SqlDataAdapter())
            {
                command.Connection = con;
                command.CommandText = query;
                adapter.SelectCommand = command;

                con.Open();
                SqlDataReader reader = command.ExecuteReader();

                reader.Read();

                try
                {
                    adapter.Fill(ds);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("条件指定に誤りがあるか、データベースへの接続に問題があります。");
                }
            }
            // 先にほかのデータバインドが行われたことを想定して、いったんDataSourceをリセットする
            gridMain.DataSource = null;

            // MSDNにはテーブル1つのDataSetであればDataMember指定は不要とあるが、うまく表示されないので指定。
            // なお、DataMember指定なしで、～.DataSource = (DataTable)ds.Tables[0] でもいける。
            gridMain.DataMember = ds.Tables[0].TableName;
            gridMain.DataSource = ds;
        }


        /// <summary>
        /// クリアボタン押下時処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            gridMain.DataSource = null;
        }
    }
}
