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

namespace YGOPro_Expansion_Manager
{
    /// <summary>
    /// Interaction logic for TransferWindow.xaml
    /// </summary>
    public partial class TransferWindow : Window, INotifyPropertyChanged
    {
        //Prevent Loading New Database
        bool hasChanges = false;

        //Expansion LIsts
        Expansion TableTo;
        Expansion TableFrom;
        List<CardItem> changesTo = new List<CardItem>();
        List<CardItem> listFrom = new List<CardItem>();

        public TransferWindow()
        {
            InitializeComponent();
            ListBox_TransTo.ItemsSource = changesTo;
        }

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

        private void SortListChanged()
        {
            //Sort List
            changesTo.Sort();
            //Refresh List
            ListBox_TransTo.Items.Refresh();
            //Prevent Loading new Database
            hasChanges = true;
        }

        private void Command_Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {

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
                    }
                }
            }

            //Fill ListBox with items
        }

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

        #region FilterFunctions
        private void TextBox_FilterTo_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //ListBox_TransTo.Items.Filter = new Predicate<object>();
            if (ListBox_TransTo == null || ListBox_TransTo.Items == null) return;
            ListBox_TransTo.Items.Filter = FilterToFunc;
        }

        private bool FilterToFunc(object arg)
        {
            CardItem card = (CardItem)arg;
            return (card.Name.Contains(SearchBox_NameTo.Text) || SearchBox_NameTo.Foreground != Brushes.Black)
                && (card.Code.ToString().Contains(SearchBox_CodeTo.Text) || SearchBox_CodeTo.Foreground != Brushes.Black);
        }

        private void SearchBox_IsKeyboardFocusedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            System.Windows.Controls.TextBox searchBox = (System.Windows.Controls.TextBox)sender;
            if (searchBox.IsKeyboardFocused)
            {
                if (searchBox.Foreground == Brushes.LightGray)
                {
                    searchBox.Text = "";
                    searchBox.Foreground = Brushes.Black;
                }
            }
            else
            {
                if (searchBox.Foreground == Brushes.Black && searchBox.Text == "")
                {
                    searchBox.Text = searchBox.Tag.ToString();
                    searchBox.Foreground = Brushes.LightGray;
                }
            }

        }
        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string propertyName, object sender)
        {
            if (PropertyChanged != null) PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
