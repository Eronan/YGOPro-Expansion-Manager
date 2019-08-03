using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace YGOPro_Expansion_Manager
{
    /// <summary>
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window
    {
        Expansion transferTo;
        Expansion transferFrom;


        public TransferWindow()
        {
            InitializeComponent();
        }

        private void Command_Create_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Use Winforms' SaveFile Dialog
            using (WinForms.SaveFileDialog saveDialog = new WinForms.SaveFileDialog())
            {
                if (saveDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string filePath = saveDialog.FileName;

                    SQLiteConnection.CreateFile(filePath);
                    using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + filePath + ";"))
                    {
                        sqlConn.Open();
                        string execSql_Data = "CREATE TABLE 'datas' ('id' integer,'ot' integer, 'alias' integer," +
                            "'setcode' integer, 'type' integer, 'atk' integer, 'def' integer, 'level' integer," +
                            "'race' integer, 'attribute' integer, 'category' integer, PRIMARY KEY('id'))";
                        string execSql_Text = "CREATE TABLE 'texts'('id' integer, 'name' text, 'desc' text," +
                            "'str1' text, 'str2' text, 'str3' text, 'str4' text, 'str5' text, 'str6' text," +
                            "'str7' text, 'str8' text, 'str9' text, 'str10' text, 'str11' text, 'str12' text," +
                            "'str13' text, 'str14' text, 'str15' text, 'str16' text, PRIMARY KEY('id'))";

                        SQLiteCommand sqlCmd_Data = new SQLiteCommand(execSql_Data, sqlConn);
                        sqlCmd_Data.ExecuteNonQuery();

                        SQLiteCommand sqlCmd_Text = new SQLiteCommand(execSql_Text, sqlConn);
                        sqlCmd_Text.ExecuteNonQuery();

                        //Create Expansion
                        if (transferTo != null) transferTo.Dispose();
                        transferTo = OpenDatabase(sqlConn, filePath);

                        //Dispose
                        sqlCmd_Data.Dispose();
                        sqlCmd_Text.Dispose();
                        sqlConn.Close();
                    }
                }
            }

            //Fill ListBox with items
        }

        private void Command_Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            using (WinForms.OpenFileDialog openDialog = new WinForms.OpenFileDialog())
            {
                if (openDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string filePath = openDialog.FileName;

                    //Open Database
                    using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + filePath + ";"))
                    {
                        sqlConn.Open();

                        //Create Expansion
                        if (transferTo != null) transferTo.Dispose();
                        transferTo = OpenDatabase(sqlConn, filePath);
                    }
                }
            }

            //Fill ListBox with items
        }

        private static Expansion OpenDatabase(SQLiteConnection sqlConn, string filePath)
        {
            DataSet expansionSet = new DataSet();

            //Read from 'data' table
            SQLiteCommand sqlCmd = new SQLiteCommand("Select * from datas", sqlConn);
            SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(sqlCmd);
            sqlAdapter.Fill(expansionSet, "datas");

            //Read from 'text' table
            sqlAdapter.SelectCommand.CommandText = "Select * from texts";
            sqlAdapter.Fill(expansionSet, "texts");

            //Create new Expansion Class
            Expansion Loaded = new Expansion(filePath, expansionSet);

            sqlAdapter.Dispose();
            sqlCmd.Dispose();

            return Loaded;
        }

        private void FillListFrom()
        {
            //Fill ListBox based off of transferFrom
        }

        private void FillListTo()
        {
            //Fill ListBox based off of transferTo
        }

        private void Command_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Command_Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //MessageBox.Show("This is the load function");
        }
    }
}
