using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void Merge(Expansion expansion)
        {
            this.Data.Merge(expansion.Data);
            this.Text.Merge(expansion.Text);
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
}
