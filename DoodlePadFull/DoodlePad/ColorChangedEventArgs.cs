using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

/**************************************************************
/      DoodlePad
/      Version 1.2 Full
/      Last Mod: January 17, 2010 
/	  
/      Author: David Isbitski (DaveDev Productions)
/	   Blog: http://blogs.msdn.com/davedev
/      Twitter: http://twitter.com/theDaveDev
/      Web: http://about.me/davedev
/      Git: http://github.com/disbitski
/      Email: disbitski@hotmail.com
/
**************************************************************/

namespace SilverlightColorPicker
{
    public class ColorChangedEventArgs : RoutedEventArgs
    {
        internal ColorChangedEventArgs(Color selectedColor)
	    {
            this.SelectedColor = selectedColor;
	    }

        public Color SelectedColor
        {
            get;
            private set;
        }
    }
}
