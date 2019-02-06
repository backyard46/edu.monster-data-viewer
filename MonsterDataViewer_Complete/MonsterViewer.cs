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
        /// 表示押下時処理（TableAdapter利用）。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShow_Click(object sender, EventArgs e)
        {
            TableAdapterSearch(textCondition.Text);
        }

        /// <summary>
        /// 表示（ダメ）押下時処理。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonShowDame_Click(object sender, EventArgs e)
        {
            // テキストボックスの文字列を条件にして検索実行
            SimpleSearch(textCondition.Text);
        }

        /// <summary>
        /// 指定された文字列を検索キーワードにして、TableAdapterを利用してデータベースからデータを取得、表示する。
        /// 
        /// TableAdapterを使ってパラメーターによる条件指定を用いたSQLでデータを取得しているため、
        /// SQLインジェクションは起こらない。
        /// </summary>
        /// <param name="searchKey">検索条件文字列。</param>
        private void TableAdapterSearch(string searchKey)
        {
            if (searchKey.Trim().Length == 0)
            {
                // 条件が空の場合
                monsterTblAdapter.Fill(monsterDataSet.Monsters);
                gridMain.DataSource = monsterDataSet.Monsters;
            }
            else
            {
                // 条件が指定されている場合
                monsterTblAdapter.FillByTypes(monsterDataSet.Monsters, searchKey, searchKey);
                gridMain.DataSource = monsterDataSet.Monsters;
            }
        }

        /// <summary>
        /// 指定された文字列を検索キーワードにして単純な文字列結合でSQLを作成、データベースからデータを取得し、表示する。
        /// 
        /// SqlAdapterとSqlCommandで、SQLを文字列組み立てで作成している。
        /// パラメーターによる条件指定を行っていないので、SQLインジェクションが起こる。
        /// たとえば
        /// 　　「' OR 1=1; insert into monsters values(231,'謎モンスター','Riddle','なぞ',null,null,null); SELECT * FROM Monsters Where Type1='」
        /// を打ち込むと、エラーにはなるが新データは追加される。
        /// </summary>
        /// <param name="searchKey">検索条件文字列。</param>
        private void SimpleSearch(string searchKey)
        {
            string query = "SELECT * FROM Monsters ";
            if (searchKey.Trim().Length != 0)
            {
                // SQLインジェクション対策処理(単純置換)
                //string sanitizedText = searchKey.Replace("'", "''");
                //query += "WHERE Type1='" + sanitizedText + "' OR Type2='" + sanitizedText + "'";

                // SQLインジェクション未対策処理(文字列直結)
                //query += "WHERE Type1='" + searchKey + "' OR Type2='" + searchKey + "'";

                // SQLインジェクション対策処理(単純置換)+Unicode検索条件対策(Nプリフィックス)
                string sanitizedText = searchKey.Replace("'", "''");
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
