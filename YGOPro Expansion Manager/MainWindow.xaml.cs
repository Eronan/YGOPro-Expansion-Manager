using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.IO.Compression;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Data;
using System.Data.SQLite;
using System.Xml;
using System.Xml.Linq;
//using System.Windows.Shapes;
using WinForms = System.Windows.Forms;

namespace YGOPro_Expansion_Manager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants
        public const string LOCAL_EXPANSIONS = @".\expansions";
        public const string LOCAL_METADATA = "metadata.xml";
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //Set Up Local Folders
            while (Properties.Settings.Default.Path == "") SetExpansionsPath();

            //Load Local Expansions
            LoadLocal();
            LoadMain();
            //State Checking
        }

        private void LoadLocal()
        {
            ItemCollection localExpansions = ListBox_Local.Items;
            foreach (string dbPath in Directory.GetFiles(LOCAL_EXPANSIONS, "*.cdb"))
            {
                string fileName = Path.GetFileNameWithoutExtension(dbPath);

                //Load Database
                DataSet expansionSet = LoadDatabase(dbPath);

                //Create a ListBoxItem
                Expansion newExpansion = new Expansion(fileName, expansionSet);
                CheckBox newBoxItem = new CheckBox();
                newBoxItem.Content = fileName;
                newBoxItem.Tag = newExpansion;
                //newBoxItem.IsThreeState = true;

                //Add CheckBox to ListBox
                localExpansions.Add(newBoxItem);
            }
        }

        //Load XML File
        private List<string> LoadMetaData(string expName)
        {
            List<string> includedExpansions = new List<string>();

            try
            {

                XmlDocument document = new XmlDocument();
                document.Load(LOCAL_METADATA);

                XmlElement elem = document.GetElementById(expName);
                foreach (XmlNode xmlNode in elem.ChildNodes)
                {
                    if (xmlNode.Name == "local") includedExpansions.Add(xmlNode.InnerText);
                }
            }
            catch (IOException e)
            {
                Console.Write(e.Message);
            }

            return includedExpansions;
        }

        //Load Data from Directory
        private void LoadMain()
        {
            //Load XML File

            //Load Database Files
            ItemCollection mainExpansions = ListBox_Directory.Items;
            foreach (string dbPath in Directory.GetFiles(Properties.Settings.Default.Path, "*.cdb"))
            {
                string fileName = Path.GetFileNameWithoutExtension(dbPath);

                //Load Database
                DataSet expansionSet = LoadDatabase(dbPath);

                //Create Expansion Set
                DirectoryExpansion newExpansion = new DirectoryExpansion(fileName, expansionSet, ListBox_Local.Items, LoadMetaData(fileName));
                ListBoxItem newBoxItem = new ListBoxItem();
                newBoxItem.Content = newExpansion;
                newBoxItem.Tag = newExpansion;

                //Add Item to the ListBox
                mainExpansions.Add(newBoxItem);
            }
        }

        //Set Expansions Folder Path
        private string SetExpansionsPath()
        {
            //Start Folder Browser Dialog
            using (var directoryDialog = new WinForms.FolderBrowserDialog())
            {
                directoryDialog.Description = "Please select the YGOPro expansions folder.";

                if (directoryDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    Properties.Settings.Default.Path = directoryDialog.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }

            //Return Path
            return Properties.Settings.Default.Path;
        }

        //Load Database
        private DataSet LoadDatabase(string dbPath)
        {
            DataSet dataSet;
            using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + dbPath))
            {
                dataSet = LoadDatabase(sqlConn);
            }

            return dataSet;
        }

        private DataSet LoadDatabase(SQLiteConnection sqlConn)
        {
            DataSet dataSet = new DataSet();
            try
            {
                sqlConn.Open();
                //Read from 'data' table
                SQLiteCommand sqlCmd = new SQLiteCommand("Select * from data", sqlConn);
                SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(sqlCmd);
                sqlAdapter.Fill(dataSet, "data");

                //Read from 'text' table
                sqlAdapter.SelectCommand.CommandText = "Select * from texts";
                sqlAdapter.Fill(dataSet, "texts");

                sqlAdapter.Dispose();
                sqlCmd.Dispose();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return dataSet;
        }

        private void Button_NewFile_Click(object sender, RoutedEventArgs e)
        {
            InputBox inputBox = new InputBox("What do you want to name the database?");
            if (inputBox.ShowDialog() == true)
            {
                //Get File Name
                string fileName = Path.GetFileNameWithoutExtension(inputBox.Input);
                //Get File Path
                string filePath = Properties.Settings.Default.Path + "\\" + fileName + ".cdb";
                if (File.Exists(filePath))
                {
                    MessageBox.Show("A file with that name already exists!", "Cannot Save",
                        MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
                else
                {
                    //Create Tables
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

                        //Create List Box Item
                        ListBoxItem newBoxItem = new ListBoxItem();
                        newBoxItem.Content = fileName;
                        newBoxItem.Tag = new DirectoryExpansion(fileName, LoadDatabase(sqlConn), ListBox_Local.Items, new List<string>());
                        ListBox_Directory.Items.Add(newBoxItem);
                    }
                }
            }
        }
    }
}
