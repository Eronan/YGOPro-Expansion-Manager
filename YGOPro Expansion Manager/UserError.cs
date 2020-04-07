using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace YGOPro_Expansion_Manager
{
    public enum ErrorType
    {
        Error = 0,
        Warning = 1,
        Info = 2
    }
    public class UserError
    {
        ErrorType _errorType;
        Icon _icon;
        string _message;
        string _source;
        MethodBase _targetSite;

        public UserError(string message, string source, MethodBase targetSite, ErrorType errorType = ErrorType.Error)
        {
            this._errorType = errorType;
            this._message = message;
            this._source = source;
            this._targetSite = targetSite;

            switch (errorType)
            {
                case ErrorType.Error:
                    _icon = SystemIcons.Error;
                    break;
                case ErrorType.Warning:
                    _icon = SystemIcons.Warning;
                    break;
                case ErrorType.Info:
                    _icon = SystemIcons.Information;
                    break;
            }

            IconSource = Imaging.CreateBitmapSourceFromHIcon(_icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public UserError(Exception e, ErrorType errorType = ErrorType.Error) : this(e.Message, e.Source, e.TargetSite, errorType) { }

        public ErrorType ErrorType
        {
            get { return _errorType; }
        }

        public string Message
        {
            get { return _message; }
        }

        public string Source
        {
            get { return _source; }
        }

        public MethodBase TargetSite
        {
            get { return _targetSite; }
        }

        public Icon ErrorIcon
        {
            get { return _icon; }
        }

        public System.Windows.Media.ImageSource IconSource { get; set; }
    }
}
