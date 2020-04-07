using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using WinForms = System.Windows.Forms;
using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Media;
using System.IO.Compression;
using System.Windows.Media.Imaging;

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
        System.Drawing.Image currentSource;
        BitmapImage imageStream;

        public TransferWindow()
        {
            InitializeComponent();
            ListBox_TransTo.ItemsSource = changesTo;
            imageStream = new BitmapImage();
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
                                    //Replace Files in Zip
                                }
                                else
                                {
                                    //Insert Row in Table
                                    //Save Files in Zip
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

                            int deleted = sqlCmd_deleteData.ExecuteNonQuery();
                            deleted = sqlCmd_deleteText.ExecuteNonQuery();
                            //Delete Items from Zip
                            var scriptFile = zipFileTo.GetEntry("script/c" + cardItem.Code + ".lua");
                            var picFile = zipFileTo.GetEntry("pics/" + cardItem.Code + ".pic");
                            if (scriptFile != null) scriptFile.Delete();
                            if (picFile != null) picFile.Delete();

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
                    i--;
                }
            }
            hasChanges = false;
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
