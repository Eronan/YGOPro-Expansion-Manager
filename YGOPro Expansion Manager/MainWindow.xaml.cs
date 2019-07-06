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
        #region Variables
        bool handleChecked = false;
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

        #region Loading Procedures
        private void LoadLocal()
        {
            //Create Local Expansion Folder
            if (!Directory.Exists(LOCAL_EXPANSIONS))
            {
                Directory.CreateDirectory(LOCAL_EXPANSIONS);
                return;
            }

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
                newBoxItem.Checked += CheckBox_Expansions_Checked;
                newBoxItem.Unchecked += CheckBox_Expansions_Checked;
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
                //Load Document
                XmlDocument document = new XmlDocument();
                document.Load(LOCAL_METADATA);

                //Get all Expansions in Directory Expansion
                XmlNode elem = document.GetElementsByTagName(expName)[0];
                //Return if nothing exists
                if (elem == null) return new List<string>();
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
            //Load Database Files
            ItemCollection mainExpansions = ListBox_Directory.Items;
            foreach (string dbPath in Directory.GetFiles(Properties.Settings.Default.Path, "*.cdb"))
            {
                string fileName = Path.GetFileNameWithoutExtension(dbPath);

                if (fileName == "cards-tf") continue;

                //Load Database
                DataSet expansionSet = LoadDatabase(dbPath);

                //Create Expansion Set
                DirectoryExpansion newExpansion = new DirectoryExpansion(fileName, expansionSet, ListBox_Local.Items, LoadMetaData(fileName));
                ListBoxItem newBoxItem = new ListBoxItem();
                newBoxItem.Content = fileName;
                newBoxItem.Tag = newExpansion;
                newBoxItem.Selected += ListBoxItem_Expansions_Selected;
                //Add Item to the ListBox
                mainExpansions.Add(newBoxItem);
            }

            //Set SelectedIndex to 0
            if (mainExpansions.Count > 0) ListBox_Directory.SelectedIndex = 0;
        }

        //Load Database
        private DataSet LoadDatabase(string dbPath)
        {
            DataSet dataSet;
            //Set up Connection
            using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + dbPath))
            {
                sqlConn.Open();
                dataSet = LoadDatabase(sqlConn);
            }

            //Return Information
            return dataSet;
        }

        private DataSet LoadDatabase(SQLiteConnection sqlConn)
        {
            DataSet dataSet = new DataSet();
            
            //Read from 'data' table
            SQLiteCommand sqlCmd = new SQLiteCommand("Select * from datas", sqlConn);
            SQLiteDataAdapter sqlAdapter = new SQLiteDataAdapter(sqlCmd);
            sqlAdapter.Fill(dataSet, "datas");

            //Read from 'text' table
            sqlAdapter.SelectCommand.CommandText = "Select * from texts";
            sqlAdapter.Fill(dataSet, "texts");

            sqlAdapter.Dispose();
            sqlCmd.Dispose();

            return dataSet;
        }
        #endregion

        #region ControlEvents
        //New Database
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
                        newBoxItem.Selected += ListBoxItem_Expansions_Selected;
                        ListBox_Directory.Items.Add(newBoxItem);

                        //Dispose
                        sqlCmd_Data.Dispose();
                        sqlCmd_Text.Dispose();
                        sqlConn.Close();
                    }
                }
            }
        }

        //Delete Database
        private void Button_DeleteFile_Click(object sender, RoutedEventArgs e)
        {
            //Confirm Deletion
            if (MessageBox.Show("Are you sure you want to delete this file?", "Delete", MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                //Get Selected Item
                ListBoxItem selectedItem = (ListBoxItem)ListBox_Directory.SelectedItem;
                if (selectedItem != null)
                {
                    DirectoryExpansion selectedExpansion = (DirectoryExpansion)selectedItem.Tag;
                    string filePath = Properties.Settings.Default.Path + "\\" + selectedItem.Content + ".cdb";
                    //Delete File
                    if (File.Exists(filePath)) File.Delete(filePath);

                    //Dispose Variables
                    selectedExpansion.Dispose();

                    //Remove from ListBox
                    ListBox_Directory.Items.Remove(selectedItem);
                }
            }
        }

        //Delete
        private void ListBox_Directory_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                //Delete Database
                Button_DeleteFile_Click(sender, e);
            }
        }

        //CheckBox is Checked
        private void CheckBox_Expansions_Checked(object sender, RoutedEventArgs e)
        {
            if (handleChecked) return;

            CheckBox checkBoxSender = (CheckBox)sender;
            Expansion checkBoxTag = (Expansion)checkBoxSender.Tag;
            //Get Selected Main Directory Expansion
            ListBoxItem selectedExpansion = (ListBoxItem) ListBox_Directory.SelectedItem;
            if (selectedExpansion != null)
            {
                //Set New Value for Key
                DirectoryExpansion directoryExpansion = (DirectoryExpansion) selectedExpansion.Tag;
                directoryExpansion.ExpansionDictionary[checkBoxTag] = (checkBoxSender.IsChecked == true);

                //Merge/Remove Tables
                /*//To be re-added when adding in card specific choices
                if (checkBoxSender.IsChecked == true) directoryExpansion.Merge(checkBoxTag);
                else directoryExpansion.Delete(checkBoxTag.Data.Rows);
                */
            }
        }

        private void ListBox_Local_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                //Check Box
                CheckBox checkSender = (CheckBox) ListBox_Local.SelectedItem;
                checkSender.IsChecked = !checkSender.IsChecked;
            }
        }

        //Expansion is Selected
        private void ListBoxItem_Expansions_Selected(object sender, RoutedEventArgs e)
        {
            ListBoxItem selectedItem = (ListBoxItem) sender;
            DirectoryExpansion directoryExpansion = (DirectoryExpansion) selectedItem.Tag;
            handleChecked = true;
            foreach (CheckBox checkBox in ListBox_Local.Items)
            {
                Expansion checkBoxExpansion = (Expansion) checkBox.Tag;
                checkBox.IsChecked = directoryExpansion.ExpansionDictionary[checkBoxExpansion];
            }
            handleChecked = false;
        }
        #endregion

        #region Commands
        //Create New Database Command
        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Press Button
            Button_NewFile_Click(sender, e);
        }

        //Save
        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            XmlDocument xmlDocument = new XmlDocument();
            XmlElement root = xmlDocument.CreateElement("Metadata");
            foreach (ListBoxItem listItem in ListBox_Directory.Items)
            {
                DirectoryExpansion directoryExpansion = (DirectoryExpansion)listItem.Tag;
                string filePath = Properties.Settings.Default.Path + "\\" + directoryExpansion.Name;
                //Remove Data Merging
                #region RemovableTableCreator
                directoryExpansion.Clear();
                XmlElement expansionElement = xmlDocument.CreateElement(directoryExpansion.Name);
                root.AppendChild(expansionElement);

                //
                using (FileStream zipStream = new FileStream(filePath + ".zip", FileMode.Create))
                {
                    using (ZipArchive directoryZip = new ZipArchive(zipStream, ZipArchiveMode.Update))
                    {
                        foreach (KeyValuePair<Expansion, bool> keyValuePair in directoryExpansion.ExpansionDictionary)
                        {
                            //Skip if no checked
                            if (!keyValuePair.Value) continue;
                            //Merge Tables if local is checked
                            string localPath = LOCAL_EXPANSIONS + @"\" + keyValuePair.Key.Name + ".zip";
                            using (FileStream localStream = new FileStream(localPath, FileMode.Open))
                            {
                                using (ZipArchive localZip = new ZipArchive(localStream, ZipArchiveMode.Read))
                                {
                                    foreach (ZipArchiveEntry entry in localZip.Entries)
                                    {
                                        Stream mainStream = directoryZip.CreateEntry(entry.FullName).Open();
                                        Stream tempStream = entry.Open();
                                        tempStream.CopyTo(mainStream);
                                        mainStream.Close();
                                        tempStream.Close();
                                    }

                                    //Garbage Collection
                                    localZip.Dispose();
                                }

                                //Close
                                localStream.Close();
                            }

                            //Create Metadata
                            directoryExpansion.Merge(keyValuePair.Key);
                            XmlElement addElement = xmlDocument.CreateElement("local");
                            addElement.InnerText = keyValuePair.Key.Name;
                            expansionElement.AppendChild(addElement);
                        }
                    }

                    zipStream.Close();
                }
                xmlDocument.AppendChild(root);
                xmlDocument.Save(LOCAL_METADATA);
                #endregion

                //DataBase
                if (!File.Exists(filePath + ".cdb")) continue;
                using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + filePath + ".cdb"))
                {
                    sqlConn.Open();

                    //Delete all Data from Tables
                    SQLiteCommand sqlCmd_data_delete = new SQLiteCommand("DELETE FROM datas", sqlConn);
                    SQLiteCommand sqlCmd_text_delete = new SQLiteCommand("DELETE FROM texts", sqlConn);
                    sqlCmd_data_delete.ExecuteNonQuery();
                    sqlCmd_text_delete.ExecuteNonQuery();

                    //Dispose of Commands
                    sqlCmd_data_delete.Dispose();
                    sqlCmd_text_delete.Dispose();
                    //Insert Command
                    for (int i = 0; i < directoryExpansion.RowCount; i++)
                    {
                        DataRow dataRow = directoryExpansion.Data.Rows[i];
                        //Insert Data
                        SQLiteCommand sqlCmd_data_insert = new SQLiteCommand("INSERT INTO datas " +
                            "VALUES (@id, @ot, @alias, @setcode, @type, @atk, @def, @level, @race, " +
                            "@attribute, @category)", sqlConn);
                        sqlCmd_data_insert.Parameters.Add("@id", DbType.Int64).Value = dataRow["id"];
                        sqlCmd_data_insert.Parameters.Add("@ot", DbType.Int32).Value = dataRow["ot"];
                        sqlCmd_data_insert.Parameters.Add("@alias", DbType.Int64).Value = dataRow["alias"];
                        sqlCmd_data_insert.Parameters.Add("@setcode", DbType.Int64).Value = dataRow["setcode"];
                        sqlCmd_data_insert.Parameters.Add("@type", DbType.Int64).Value = dataRow["type"];
                        sqlCmd_data_insert.Parameters.Add("@atk", DbType.Int64).Value = dataRow["atk"];
                        sqlCmd_data_insert.Parameters.Add("@def", DbType.Int64).Value = dataRow["def"];
                        sqlCmd_data_insert.Parameters.Add("@level", DbType.Int64).Value = dataRow["level"];
                        sqlCmd_data_insert.Parameters.Add("@race", DbType.Int64).Value = dataRow["race"];
                        sqlCmd_data_insert.Parameters.Add("@attribute", DbType.Int64).Value = dataRow["attribute"];
                        sqlCmd_data_insert.Parameters.Add("@category", DbType.Int64).Value = dataRow["category"];

                        //Execute Data Insert
                        sqlCmd_data_insert.ExecuteNonQuery();
                        sqlCmd_data_insert.Dispose();

                        //Insert Texts
                        DataRow textRow = directoryExpansion.Text.Rows[i];
                        SQLiteCommand sqlCmd_text_insert = new SQLiteCommand("INSERT INTO texts " +
                            "VALUES (@id, @name, @desc, @str1, @str2, @str3, @str4, @str5, @str6, " +
                            "@str7, @str8, @str9, @str10, @str11, @str12, @str13, @str14, @str15, @str16)", sqlConn);
                        sqlCmd_text_insert.Parameters.Add("@id", DbType.Int64).Value = textRow["id"];
                        sqlCmd_text_insert.Parameters.Add("@name", DbType.String).Value = textRow["name"];
                        sqlCmd_text_insert.Parameters.Add("@desc", DbType.String).Value = textRow["desc"];
                        for (int j = 1; j <= 16; j++)
                        {
                            //Add all String Parameters
                            sqlCmd_text_insert.Parameters.Add("@str" + j, DbType.String).Value = textRow["str" + j];
                        }

                        sqlCmd_text_insert.ExecuteNonQuery();
                        sqlCmd_text_insert.Dispose();
                    }

                    //Close Connection
                    sqlConn.Close();
                }
            }
            Mouse.OverrideCursor = null;
        }

        //Get Directory
        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetExpansionsPath();
        }
        #endregion

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

    }
}
