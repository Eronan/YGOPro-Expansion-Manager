using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;

namespace YGOPro_Expansion_Manager
{
    /// <summary>
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window
    {
        //Prevent Loading New Database
        bool hasChanges = false;

        //Expansion LIsts
        Expansion TableTo;
        Expansion TableFrom;
        List<CardItem> changesTo = new List<CardItem>();
        List<CardItem> listFrom = new List<CardItem>();

        //Global Variables
        string fromFilePath;
        string toFilePath;

        public TransferWindow()
        {
            InitializeComponent();
            ListBox_TransTo.ItemsSource = changesTo;
        }

        //Clear/Create New 'changesTo' List
        private void InitializeChangesList()
        {
            //Clear Changes
            changesTo.Clear();

            //Add Card List to Changes
            foreach (DataRow dr in TableTo.Text.Rows)
            {
                //Add Cards to List as "Original"
                changesTo.Add(new CardItem(Convert.ToInt64(dr["id"]), dr["name"].ToString()));
            }
            ListBox_TransTo.Items.Refresh();
        }

        private void SortListChanged()
        {
            //Sort List
            changesTo.Sort();
            //Refresh List
            ListBox_TransTo.Items.Refresh();
            //Prevent Loading new Database
            hasChanges = true;
        }

        #region Commands
        private void Command_Create_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Use Winforms' SaveFile Dialog
            using (WinForms.SaveFileDialog saveDialog = new WinForms.SaveFileDialog())
            {
                saveDialog.InitialDirectory = Properties.Settings.Default.Expansions;
                saveDialog.Filter = "Card Database (*.cdb)|*.cdb";
                if (saveDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string filePath = saveDialog.FileName;
                    toFilePath = filePath;
                    //Save Settings
                    Properties.Settings.Default.Expansions = Path.GetDirectoryName(filePath);
                    Properties.Settings.Default.Save();
                    //Show Database Name
                    Label_DatabaseTo.Text = Path.GetFileName(filePath);

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
                        if (TableTo != null) TableTo.Dispose();
                        TableTo = OpenDatabase(sqlConn, filePath);

                        //Clear or Create new 'changesTo' List
                        InitializeChangesList();

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
                //Set Up OpenFileDialog
                openDialog.InitialDirectory = Properties.Settings.Default.Expansions;
                openDialog.Filter = "Card Database (*.cdb)|*.cdb";
                //Open FileDialog
                if (openDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string filePath = openDialog.FileName;
                    toFilePath = filePath;
                    //Save Settings
                    Properties.Settings.Default.Expansions = Path.GetDirectoryName(filePath);
                    Properties.Settings.Default.Save();
                    //Show Database Name
                    Label_DatabaseTo.Text = Path.GetFileName(filePath);

                    //Open Database
                    using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + filePath + ";"))
                    {
                        sqlConn.Open();

                        //Create Expansion
                        if (TableTo != null) TableTo.Dispose();
                        TableTo = OpenDatabase(sqlConn, filePath);

                        //Clear or Create new 'changesTo' List
                        InitializeChangesList();

                        hasChanges = false;
                    }
                }
            }

            //Fill ListBox with items
        }

        private void Command_Load_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Prevent Loading new Database if changes exist
            if (hasChanges)
            {
                MessageBox.Show("Please save or cancel changes before Loading a new Database.",
                    "Please Save Changes", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Use Winforms' OpenFileDialog
            using (WinForms.OpenFileDialog openDialog = new WinForms.OpenFileDialog())
            {
                //Set Up OpenFileDialog
                openDialog.InitialDirectory = Properties.Settings.Default.Path;
                openDialog.Filter = "Card Database (*.cdb)|*.cdb";
                //Open FileDialog
                if (openDialog.ShowDialog() == WinForms.DialogResult.OK)
                {
                    string filePath = openDialog.FileName;
                    fromFilePath = filePath;
                    //Save Settings
                    Properties.Settings.Default.Path = Path.GetDirectoryName(filePath);
                    Properties.Settings.Default.Save();
                    //Show Database Name
                    Label_DatabaseFrom.Text = Path.GetFileName(filePath);

                    //Open Database
                    using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + filePath + ";"))
                    {
                        sqlConn.Open();

                        //Create Expansion
                        if (TableFrom != null) TableFrom.Dispose();
                        TableFrom = OpenDatabase(sqlConn, filePath);

                        //Add Card List to Changes
                        listFrom.Clear();
                        foreach (DataRow dr in TableFrom.Text.Rows)
                        {
                            //Add Cards to List as "Not Original"
                            listFrom.Add(new CardItem(Convert.ToInt64(dr["id"]), dr["name"].ToString(), false));
                        }

                        ListBox_TransFrom.ItemsSource = listFrom;
                        ListBox_TransFrom.Items.Refresh();
                    }
                }
            }

            //Fill ListBox with items
        }

        private void Command_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //Set up SQL Connection
            using (SQLiteConnection sqlConn = new SQLiteConnection("Data Source=" + toFilePath))
            {
                sqlConn.Open();

                int startOfExtension = toFilePath.Length - 4;


                //Set Up Zip File Connection to "To Zip File"
                using (var zipFileTo = ZipFile.Open(GetZipFilePath(toFilePath), ZipArchiveMode.Update))
                {
                    foreach (CardItem cardItem in changesTo)
                    {
                        if (cardItem.IsNew)
                        {
                            //Set Up Zip File Connection to "From Zip File"
                            using (var zipFileFrom = ZipFile.Open(GetZipFilePath(fromFilePath), ZipArchiveMode.Read))
                            {
                                if (cardItem.IsOriginal)
                                {
                                    //Update Row in Table
                                    //Commands
                                    SQLiteCommand sqlCmd_updateData = new SQLiteCommand("UPDATE datas " +
                                        "SET ot=@format, alias=@alias, setcode=@setcode, type=@type, atk=@atk, def=@def, " +
                                        "level=@level, race=@race, attribute=@attribute, category=@category " +
                                        "WHERE id=@code", sqlConn);
                                    SQLiteCommand sqlCmd_updateText = new SQLiteCommand("UPDATE texts " +
                                        "SET name=@name, desc=@desc, " +
                                        "str1=@str1, str2=@str2, str3=@str3, str4=@str4, str5=@str5, str6=@str6, " +
                                        "str7=@str7, str8=@str8, str9=@str9, str10=@str10, str11=@str11, str12=@str12, " +
                                        "str13=@str13, str14=@str14, str15=@str15, str16=@str16 " +
                                        "WHERE id=@code", sqlConn);

                                    //Initialize Parameters
                                    AddParametersFromExpansion(sqlCmd_updateData, sqlCmd_updateText, TableFrom, cardItem.Code);

                                    //Execute
                                    sqlCmd_updateData.ExecuteNonQuery();
                                    sqlCmd_updateText.ExecuteNonQuery();

                                    //Images and Scripts
                                    DeleteFilesFromZipArchive(zipFileTo, cardItem.Code);
                                    MoveFilesToZip(zipFileTo, zipFileFrom, cardItem.Code);
                                }
                                else
                                {
                                    //Insert Row in Table
                                    SQLiteCommand sqlCmd_insertData = new SQLiteCommand("INSERT INTO datas (id , ot, alias, " +
                                        "setcode, type, atk, def, level, race, attribute, category) " +
                                        "VALUES (@code, @format, @alias, @setcode, @type, @atk, @def, " +
                                        "@level, @race, @attribute, @category)", sqlConn);
                                    SQLiteCommand sqlCmd_insertText = new SQLiteCommand("INSERT INTO texts (id, name, desc, " +
                                        "str1, str2, str3, str4, str5, str6, str7, str8, str9, str10, str11, str12, " +
                                        "str13, str14, str15, str16) " +
                                        "VALUES (@code, @name, @desc, " + 
                                        "@str1, @str2, @str3, @str4, @str5, @str6, @str7, @str8, @str9, @str10, @str11, @str12, " +
                                        "@str13, @str14, @str15, @str16)", sqlConn);

                                    //Instantiate Parameters
                                    AddParametersFromExpansion(sqlCmd_insertData, sqlCmd_insertText, TableFrom, cardItem.Code);

                                    //Execute
                                    sqlCmd_insertData.ExecuteNonQuery();
                                    sqlCmd_insertText.ExecuteNonQuery();

                                    //Images and Scripts
                                    DeleteFilesFromZipArchive(zipFileTo, cardItem.Code);    //Delete If Necessary
                                    MoveFilesToZip(zipFileTo, zipFileFrom, cardItem.Code);  //Move
                                }
                            }
                        }
                        else if (cardItem.IsDeleted)
                        {
                            //Delete Row from Table
                            SQLiteCommand sqlCmd_deleteData = new SQLiteCommand("DELETE FROM datas WHERE id=@code", sqlConn);
                            SQLiteCommand sqlCmd_deleteText = new SQLiteCommand("DELETE FROM texts WHERE id=@code", sqlConn);
                            sqlCmd_deleteData.Parameters.Add("@code", DbType.Int64).Value = cardItem.Code;
                            sqlCmd_deleteText.Parameters.Add("@code", DbType.Int64).Value = cardItem.Code;

                            sqlCmd_deleteData.ExecuteNonQuery();
                            sqlCmd_deleteText.ExecuteNonQuery();

                            //Delete Files
                            DeleteFilesFromZipArchive(zipFileTo, cardItem.Code);

                            sqlCmd_deleteData.Dispose();
                            sqlCmd_deleteText.Dispose();
                        }
                    }
                }

                //Create Expansion
                if (TableTo != null) TableTo.Dispose();
                TableTo = OpenDatabase(sqlConn, toFilePath);

                //Clear or Create new 'changesTo' List
                InitializeChangesList();

                hasChanges = false;
                //Close Connection
                sqlConn.Close();
            }

            foreach (CardItem card in listFrom)
            {
                card.IsNew = false;
                card.IsDeleted = false;
            }
        }

        private static void MoveFilesToZip(ZipArchive archiveTo, ZipArchive archiveFrom, long code)
        {
            //Get Paths
            string scriptPath = "script/c" + code + ".lua";
            string picsPath = "pics/" + code + ".jpg";

            //Script File
            if (archiveFrom.GetEntry(scriptPath) != null)
            {
                //Create Streams
                var scriptStream = archiveTo.CreateEntry(scriptPath).Open();    //'To' Zip File
                var fromScriptStream = archiveFrom.GetEntry(scriptPath).Open(); //'From' Zip File

                //Copy
                fromScriptStream.CopyTo(scriptStream);
                //Close Streams
                scriptStream.Close();
                fromScriptStream.Close();
            }

            //Picture File
            if (archiveTo.GetEntry(picsPath) != null)
            {
                //Create Streams
                var picStream = archiveTo.CreateEntry(picsPath).Open();     //'To' Zip File
                var fromPicsStream = archiveFrom.GetEntry(picsPath).Open(); //'From' Zip File

                //Copy
                fromPicsStream.CopyTo(picStream);
                //Close Streams
                picStream.Close();
                fromPicsStream.Close();
            }
        }

        private static void AddParametersFromExpansion(SQLiteCommand dataCmd, SQLiteCommand textCmd, Expansion expansion, long code)
        {
            //Add Code to Parameters
            dataCmd.Parameters.Add("@code", DbType.Int64).Value = code;
            textCmd.Parameters.Add("@code", DbType.Int64).Value = code;

            //Set Up Data Parameters
            DataRow dataRow = expansion.Data.Select("id=" + code)[0];
            dataCmd.Parameters.Add("@format", DbType.Int32).Value = dataRow["ot"];
            dataCmd.Parameters.Add("@alias", DbType.Int64).Value = dataRow["alias"];
            dataCmd.Parameters.Add("@setcode", DbType.Int64).Value = dataRow["setcode"];
            dataCmd.Parameters.Add("@type", DbType.Int64).Value = dataRow["type"];
            dataCmd.Parameters.Add("@atk", DbType.Int64).Value = dataRow["atk"];
            dataCmd.Parameters.Add("@def", DbType.Int64).Value = dataRow["def"];
            dataCmd.Parameters.Add("@level", DbType.Int64).Value = dataRow["level"];
            dataCmd.Parameters.Add("@race", DbType.Int64).Value = dataRow["race"];
            dataCmd.Parameters.Add("@attribute", DbType.Int64).Value = dataRow["attribute"];
            dataCmd.Parameters.Add("@category", DbType.Int64).Value = dataRow["category"];

            //Set Up Text Parameters
            DataRow textRow = expansion.Text.Select("id=" + code)[0];
            textCmd.Parameters.Add("@name", DbType.String).Value = textRow["name"];
            textCmd.Parameters.Add("@desc", DbType.String).Value = textRow["desc"];
            textCmd.Parameters.Add("@str1", DbType.String).Value = textRow["str1"];
            textCmd.Parameters.Add("@str2", DbType.String).Value = textRow["str2"];
            textCmd.Parameters.Add("@str3", DbType.String).Value = textRow["str3"];
            textCmd.Parameters.Add("@str4", DbType.String).Value = textRow["str4"];
            textCmd.Parameters.Add("@str5", DbType.String).Value = textRow["str5"];
            textCmd.Parameters.Add("@str6", DbType.String).Value = textRow["str6"];
            textCmd.Parameters.Add("@str7", DbType.String).Value = textRow["str7"];
            textCmd.Parameters.Add("@str8", DbType.String).Value = textRow["str8"];
            textCmd.Parameters.Add("@str9", DbType.String).Value = textRow["str9"];
            textCmd.Parameters.Add("@str10", DbType.String).Value = textRow["str10"];
            textCmd.Parameters.Add("@str11", DbType.String).Value = textRow["str11"];
            textCmd.Parameters.Add("@str12", DbType.String).Value = textRow["str12"];
            textCmd.Parameters.Add("@str13", DbType.String).Value = textRow["str13"];
            textCmd.Parameters.Add("@str14", DbType.String).Value = textRow["str14"];
            textCmd.Parameters.Add("@str15", DbType.String).Value = textRow["str15"];
            textCmd.Parameters.Add("@str16", DbType.String).Value = textRow["str16"];
        }

        private static bool DeleteFilesFromZipArchive(ZipArchive zipArchive, long code)
        {
            //Delete Items from Zip
            var scriptFile = zipArchive.GetEntry("script/c" + code + ".lua");
            var picFile = zipArchive.GetEntry("pics/" + code + ".jpg");
            if (scriptFile != null) scriptFile.Delete();
            if (picFile != null) picFile.Delete();

            return scriptFile != null || picFile != null;
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

        private static string GetZipFilePath(string databasePath)
        {
            int startOfExtension = databasePath.Length - 4;
            return databasePath.Remove(startOfExtension, 4).Insert(startOfExtension, ".zip");
        }
        #endregion

        #region Buttons
        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            //Get Selected Item
            CardItem selectedCard = (CardItem) ListBox_TransTo.SelectedItem;
            //Remove from List if it is not already in Database Originally
            if (!selectedCard.IsOriginal)
            {
                changesTo.Remove(selectedCard);
                selectedCard.IsNew = false;
            }
            else selectedCard.IsDeleted = true;
            //Prevent Loading new Database
            SortListChanged();
        }

        private void Button_Transfer_Click(object sender, RoutedEventArgs e)
        {
            //Check if Database has been Opened (Initialized)
            if (changesTo.Count == 0)
            {
                MessageBox.Show("There is no database opened.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Make Sure Item is Selected
            if (ListBox_TransFrom.SelectedItem == null) return;

            //Add New Card to Changes
            CardItem selectedCard = (CardItem)ListBox_TransFrom.SelectedItem;
            int index = changesTo.FindIndex(item => item.Code == selectedCard.Code);
            //Determine whether to Update or Add card
            if (index != -1)
            {
                if (!changesTo[index].IsNew)
                {
                    //Set the Change as Replacement IsNew = true & IsDeleted = true
                    changesTo[index].IsNew = true;
                    changesTo[index].IsDeleted = true;
                }
            }
            else
            {
                changesTo.Add(selectedCard); //Add New Card to List
                ListBox_TransTo.Items.Refresh();
                selectedCard.IsNew = true;
            }

            //Refresh List
            SortListChanged();
        }

        private void Button_TransferAll_Click(object sender, RoutedEventArgs e)
        {
            //Check if Database has been Opened (Initialized)
            if (changesTo.Count == 0)
            {
                MessageBox.Show("There is no database opened.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            //Add New Card to Changes
            foreach (CardItem card in listFrom)
            {
                //Add only Filtered Cards: Skip if doesn't fulfill Search Conditions
                if (!FilterFunc(card, SearchBox_NameFrom, SearchBox_CodeFrom)) continue;
                //Check if Card already exists in changesTo
                int index = changesTo.FindIndex(item => item.Code == card.Code);

                if (index != -1)
                {
                    if (!changesTo[index].IsNew)
                    {
                        //Set the Change as Replacement IsNew = true & IsDeleted = true
                        changesTo[index].IsNew = true;
                        changesTo[index].IsDeleted = true;
                    }
                }
                else
                {
                    changesTo.Add(card); //Add New Card to List
                    ListBox_TransTo.Items.Refresh();
                    card.IsNew = true;
                }

                //Refresh List
                ListBox_TransTo.Items.Refresh();
            }

            //Refresh Lists
            ListBox_TransFrom.Items.Refresh(); ;
            //Prevent Loading new Database
            SortListChanged();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < changesTo.Count; i++)
            {
                CardItem card = changesTo[i];
                if (card.IsOriginal)
                {
                    card.IsDeleted = false;
                    card.IsNew = false;
                }
                else
                {
                    changesTo.RemoveAt(i);
                    card.IsNew = false;
                    i--;
                }
            }
            ListBox_TransTo.Items.Refresh();
            ListBox_TransFrom.Items.Refresh();
            hasChanges = false;
        }

        private void MenuItem_About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("YGOPro Expansion Manager\n" + 
                "Creator: Eronan\n" + 
                "Version: 2.0.0\n",
                "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region FilterFunctions
        private void TextBox_FilterTo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //Check Initialization
            if (ListBox_TransTo == null || ListBox_TransTo.Items == null) return;
            //Filter Items based on Function
            ListBox_TransTo.Items.Filter = new Predicate<object>(item =>
                FilterFunc((CardItem)item, SearchBox_NameTo, SearchBox_CodeTo)
            );
        }

        private void TextBox_FilterFrom_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //ListBox_TransTo.Items.Filter = new Predicate<object>();
            if (ListBox_TransFrom == null || ListBox_TransFrom.Items == null) return;
            ListBox_TransFrom.Items.Filter = new Predicate<object>(item =>
                FilterFunc((CardItem)item, SearchBox_NameFrom, SearchBox_CodeFrom)
            );
        }

        private bool FilterFunc(CardItem card, System.Windows.Controls.TextBox nameBox, System.Windows.Controls.TextBox codeBox)
        {
            return (card.Name.Contains(nameBox.Text) || nameBox.Foreground != Brushes.Black)
                && (card.Code.ToString().Contains(codeBox.Text) || codeBox.Foreground != Brushes.Black);
        }

        private void SearchBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.TextBox searchBox = (System.Windows.Controls.TextBox)sender;
            if (searchBox.IsKeyboardFocused)
            {
                if (searchBox.Foreground == Brushes.LightGray)
                {
                    searchBox.Foreground = Brushes.Black;
                    searchBox.Text = "";
                }
            }
            else
            {
                if (searchBox.Foreground == Brushes.Black && searchBox.Text == "")
                {
                    searchBox.Foreground = Brushes.LightGray;
                    searchBox.Text = searchBox.Tag.ToString();
                }
            }

        }
        #endregion

        #region CardInformation Functions
        private void ListBox_TransTo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_TransTo.SelectedItem == null) return;
            CardItem selectedCard = (CardItem)ListBox_TransTo.SelectedItem;
            if (selectedCard.IsNew) LoadFromCard(selectedCard, fromFilePath);
            else LoadFromCard(selectedCard, toFilePath);
        }

        private void ListBox_TransFrom_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_TransFrom.SelectedItem == null) return;
            LoadFromCard((CardItem)ListBox_TransFrom.SelectedItem, fromFilePath);
        }

        
        private void LoadFromCard(CardItem card, string databasePath)
        {
            ///<summary>
            ///[Card Types?] Monster Type?/Attribute?
            ///[Level] ATK/DEF PendScaleL/PendScaleR
            ///Card Text
            ///</summary>

            //Get Zip File Path
            string zipFileName = GetZipFilePath(databasePath);

            //if (Image_SelCard.Source != null) Image_SelCard.Source;
            using (var zipFile = ZipFile.OpenRead(zipFileName))
            {
                var picEntry = zipFile.GetEntry("pics/" + card.Code + ".jpg");
                if (picEntry != null)
                {
                    using (var zipStream = picEntry.Open())
                    using (var memoryStream = new MemoryStream())
                    {
                        if (Image_SelCard.Source != null) ((BitmapImage)Image_SelCard.Source).StreamSource.Close();
                        zipStream.CopyTo(memoryStream); // here
                        memoryStream.Position = 0;

                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = memoryStream;
                        bitmap.EndInit();

                        Image_SelCard.Source = bitmap;
                    }
                }
            }
        }
        #endregion
    }
}
