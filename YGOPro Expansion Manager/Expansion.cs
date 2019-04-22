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
            this.Data = cardData.Tables["data"];
            this.Text = cardData.Tables["texts"];
        }

        public string Name
        {
            get { return name; }
        }

        public DataTable Data { get; set; }
        public DataTable Text { get; set; }
    }
}
