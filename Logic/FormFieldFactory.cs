using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebServiceCaller.Logic {
    class FormFieldFactory {
        public static TextBlock TextBlock( WindowItem windowItem ) {
            var title = new TextBlock();
            title.Text = windowItem.Title;
            title.Width = 100;
            return title;
        }

        public static TextBox TextBox( WindowItem windowItem ) {
            var content = new TextBox();
            content.Name = windowItem.Name;
            content.Width = 200;
            return content;
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
            content.Width = 200;
            return content;
        }
    }
}
