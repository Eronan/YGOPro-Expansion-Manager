using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YGOPro_Expansion_Manager
{
    class DirectoryExpansion : Expansion
    {
        //To be Removed
        Dictionary<Expansion, bool> checked_expansions = new Dictionary<Expansion, bool>();
        //List<string> checkedCards = new List<string>();
        public DirectoryExpansion(string name, DataSet database, Dictionary<Expansion, bool> checkedExpansions) : base(name, database)
        {
            checked_expansions = checkedExpansions;
        }

        public DirectoryExpansion(string name, DataSet database, ItemCollection allExpansions, List<string> includedExpansions) : base(name, database)
        {
            //Create all KeyValuePairs
            foreach (CheckBox checkBox in allExpansions)
            {
                Expansion expansion = (Expansion)checkBox.Tag;
                checked_expansions.Add(expansion, includedExpansions.Contains(expansion.Name));
            }
        }

        public Dictionary<Expansion, bool> ExpansionDictionary
        {
            get { return checked_expansions; }
            set { checked_expansions = value; }
        }

        public void Dispose()
        {
            if (base.Data != null) base.Data.Dispose();
            if (base.Text != null) base.Text.Dispose();
        }
    }
}
