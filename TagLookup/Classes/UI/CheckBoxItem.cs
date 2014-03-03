using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// class specific
using System.ComponentModel;
using System.Reflection;

namespace TagLookup.Classes.UI
{
    // This class is intended to be an item within an item collection for the 
    // listboxes of the primary form, which are intended to be filtering listboxes.

    // It inherits the INotifyPropertyChanged interface to possible extend the class
    // to allow for automatic filtration functionality via modification of the checkboxes'
    // checkmarks within the listboxes.

    // Currently, the user must must call the filter functions themselves
    public class CheckBoxItem : INotifyPropertyChanged
    {
        public CheckBoxItem() { }
        public CheckBoxItem( PropertyInfo propertyInfo )
        {
            Name = propertyInfo.Name;
            IsChecked = true;
        }
        public CheckBoxItem( string itemInfo )
        {
            Name = itemInfo;
            IsChecked = true;
        }
        private string name;
        private bool isChecked;
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                NotifyPropertyChanged( "IsChecked" );
            }
        }
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                NotifyPropertyChanged( "Name" );
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged( string strPropertyName )
        {
            if ( PropertyChanged != null )
                PropertyChanged( this, new PropertyChangedEventArgs( strPropertyName ) );
        }
    }
}
