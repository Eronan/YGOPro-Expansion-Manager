using System;
using System.Collections.Generic;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YGOPro_Expansion_Manager
{
    class Card
    {
        Bitmap _image;
        DataRow _datasRow;
        DataRow _textsRow;
        string _luaText;

        public Card(Stream zipStream, DataRow data, DataRow text)
        {
            using (zipStream)
            {

            }

            this._datasRow = data;
            this._textsRow = text;
        }

        public Bitmap cardImage { get { return this._image; } }
        public DataRow Datas { get { return this._datasRow; } }
        public DataRow Texts { get { return this._textsRow; } }
        public string LuaFile { get { return this._luaText; } }
    }
}
