using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using MySqlDataView.Common;


namespace MySqlDataView.Logic {
    public abstract class DatabaseType {
        protected Product product;
        public DatabaseType(Product product) {
            this.product = product;
        }

        public virtual void AppendFilterFormFieldTo(WrapPanel wrapPannel, UIContent uiContent) {

        }

        public virtual Tuple<bool, string> SetConnectionString( WrapPanel wrapPannel ) {
            return Tuple.Create( true, "" );
        }

        public virtual void ResetConnectionString() {

        }

        public virtual void EmptyListView( ListView listView ) {
           
        }

        public virtual void PageChange( Pager pager ) {

        }

    }

    public class DatabaseSingle : DatabaseType {
        public DatabaseSingle( Product product ) : base(product) {
            DbHelperMySqL.ConnectionString = product.ConnectionString;
        }

        public override Tuple<bool, string> SetConnectionString( WrapPanel wrapPannel ) {
            return Tuple.Create( true, "" );
        }

        public override void PageChange( Pager pager ) {
            pager.PageChange();
        }
    }

    public class DatabaseMultiple : DatabaseType {
        public DatabaseMultiple( Product product ) : base(product) {

        }

        public override void AppendFilterFormFieldTo( WrapPanel wrapPannel, UIContent uiContent ) {
            wrapPannel.Children.Add( uiContent.NewFilterFormField( DatabaseSelectorItem() ) );
        }

        public override Tuple<bool,string> SetConnectionString( WrapPanel wrapPannel ) {
            var connString = WhereGenerator.GetConnectionString( wrapPannel );
            if( string.IsNullOrEmpty( connString ) ) {
                return Tuple.Create( false, "未选择数据库" );
            }
            DbHelperMySqL.ConnectionString = connString;
            return Tuple.Create( true, string.Empty );
        }

        public override void ResetConnectionString() {
            DbHelperMySqL.ConnectionString = string.Empty;
        }

        public override void EmptyListView( ListView listView ) {
            listView.ItemsSource = null;
        }

        private WindowItem DatabaseSelectorItem() {
            var contents = product.ConnectionStrings.Select( item => {
                return new KeyVal<string, string>( item.Name, item.Value );
            } );
            var windowItem = new WindowItemForDatabase();
            windowItem.Title = "数据库";
            windowItem.Name = WindowItemForDatabase.FIELD_NAME;
            windowItem.MatchType = WindowItemMatchType.Equals;
            windowItem.ItemType = WindowItemType.FilterItem;
            windowItem.DataType = WindowItemDataType.String;
            windowItem.Contents.AddRange( contents );
            return windowItem;
        }
    }
}
