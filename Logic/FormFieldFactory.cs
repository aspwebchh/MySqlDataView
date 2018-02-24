using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;
using WinForm = System.Windows.Forms;

namespace MySqlDataView.Logic {
    class FormFieldFactory {
        public static TextBlock TextBlock( WindowItem windowItem ) {
            var title = new TextBlock();
            title.Text = windowItem.Title;
            return title;
        }

        public static TextBox TextBox( WindowItem windowItem ) {
            var content = new TextBox();
            content.Name = windowItem.Name;
            return content;
        }


        public static DatePicker DatePicker( WindowItem windowItem ) {
            var datePicker = new DatePicker();
            datePicker.Name = windowItem.Name;
            return datePicker;
        }

        public static ComboBox ComboBox(WindowItem windowItem) {
            var dataSource = windowItem.Contents.Select( item => new {
                Name = item.GetKey(),
                Value = item.GetVal()
            } ).ToList();
            dataSource.Insert( 0, new {
                Name = "所有",
                Value = ""
            } );
            var content = new ComboBox();
            content.ItemsSource = dataSource;
            content.DisplayMemberPath = "Name";
            content.SelectedValuePath = "Value";
            content.Name = windowItem.Name;
            content.SelectedIndex = 0;
            return content;
        }
    }
}
