using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace YGOPro_Expansion_Manager
{
    class Expansion
    {
        //List<string> CardCodes = new List<string>();
        string name;
        
        public Expansion(string expansionName, DataSet cardData)
        {
            this.name = expansionName;
            this.Data = cardData.Tables["datas"];
            this.Text = cardData.Tables["texts"];
        }

        public string Name
        {
            get { return name; }
        }

        //Find Card Code
        public int FindIndex(long code)
        {
            int length = Data.Rows.Count;
            for (int i = 0; i < length; i++)
            {
                if (Data.Rows[i]["id"].Equals(code)) return i;
            }
            return -1;
        }

        public void Merge(Expansion expansion)
        {
            this.Data.Merge(expansion.Data);
            this.Text.Merge(expansion.Text);
        }

        //Delete
        public void Delete(DataRowCollection collection)
        {
            int length = Data.Rows.Count;
            int removed = 0;
            for (int i = 0; i < length; i++)
            {
                long dataValue = Convert.ToInt64(Data.Rows[i]["id"]);
                long textValue = Convert.ToInt64(Text.Rows[i]["id"]);

                if (dataValue == textValue && collection.Find(dataValue)["id"] != null)
                {
                    Delete(i);
                    removed++;
                }

                if (removed == collection.Count) break;
            }
        }

        public void Delete(long[] codes)
        {
            int length = Data.Rows.Count;
            int removed = 0;
            for (int i = 0; i < length; i++)
            {
                long dataValue = Convert.ToInt64(Data.Rows[i]["id"]);
                long textValue = Convert.ToInt64(Text.Rows[i]["id"]);

                if (dataValue == textValue && codes.Contains(dataValue))
                {
                    Delete(i);
                    removed++;
                }

                if (removed == codes.Length) break;
            }
        }

        public void Delete(long code)
        {
            int length = Data.Rows.Count;
            for (int i = 0; i < length; i++)
            {
                if (Data.Rows[i]["id"].Equals(code) && Text.Rows[i]["id"].Equals(code))
                {
                    Delete(i);
                    break;
                }
            }
        }

        public void Delete(int index)
        {
            Data.Rows.RemoveAt(index);
            Text.Rows.RemoveAt(index);
        }

        //Clear Tables
        public void Clear()
        {
            Data.Rows.Clear();
            Text.Rows.Clear();
        }

        public int RowCount
        {
            get { return Data.Rows.Count; }
        }

        public void Dispose()
        {
            //Garbage Collection
            Data.Dispose();
            Text.Dispose();
            name = null;
        }

        public DataTable Data { get; set; }
        public DataTable Text { get; set; }
    }

    public class CardItem : INotifyPropertyChanged, IComparable<CardItem>
    {
        bool _newCard;
        bool _deleted;

        public CardItem(long code, string name, bool isOriginal = true)
        {
            this.Code = code;
            this.Name = name;
            this._newCard = false;
            this._deleted = false;
            this.IsOriginal = isOriginal;
        }

        public long Code { get; set; }
        public string Name { get; set; }
        public bool IsOriginal { get; set; }
        public bool IsNew {
            get { return _newCard; }
            set
            {
                _newCard = value;
                OnPropertyChanged("IsNew");
            }
        }
        public bool IsDeleted {
            get { return _deleted; }
            set {
                _deleted = value;
                OnPropertyChanged("IsDeleted");
            }
        }

        public CardItem()
        {
            IsNew = false;
            IsDeleted = false;
        }

        public string NameFromTextRow(DataRow dataRow)
        {
            Name = dataRow["name"].ToString();
            return Name;
        }

        public int CompareTo(CardItem other)
        {
            if (other.IsNew == this.IsNew)
            {
                return other.IsDeleted.CompareTo(this.IsDeleted);
            }
            else
            {
                return other.IsNew.CompareTo(this.IsNew);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
