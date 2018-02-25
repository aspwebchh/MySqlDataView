using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Controls;
using System.Windows;

namespace MySqlDataView.Logic {
    public class XmlConfigParser {

        public static Config Parse( string xmlFilePath ) {
            var doc = new XmlDocument();
            doc.Load( xmlFilePath );

            var config = new Config();
            config.Products = GetProducts( doc );
            return config;
        }

        #region
        private static List<Product> GetProducts( XmlDocument doc ) {
            var products = new List<Product>();
            var productsElement = doc.GetElementsByTagName( "Products" );
            if( productsElement.Count > 0 ) {
                var childNodes = productsElement[ 0 ].ChildNodes;
                if( childNodes.Count == 0 ) {
                    throw new XmlConfigParseError( "Product节点不存在" );
                }
                foreach( XmlNode ele in childNodes ) {
                    products.Add( GetProduct( ele ) );
                }
            } else {
                throw new XmlConfigParseError( "Products节点不存在" );
            }
            return products;
        }

        private static Product GetProduct( XmlNode node ) {
            var ele = node as XmlElement;
            var name = ele.GetAttribute( "Name" );
            var connectionString = ele.GetAttribute( "ConnectionString" );
            var id = ele.GetAttribute( "ID" );

            var product = new Product();
            product.ID = id;
            product.Name = name;
            product.ConnectionString = connectionString;

            if( node.ChildNodes.Count > 0 ) {
                var uniqueGroupElement = node.ChildNodes[ 0 ] as XmlElement;
                product.WindowGroup = GetGroup( uniqueGroupElement );
            } else {
                 throw new XmlConfigParseError( "WindowGroup节点不存在" );
            }

            return product;
        }
        #endregion;

        #region
        private static WindowGroup GetGroup(XmlNode groupElement) {
            if( groupElement.Name != "WindowGroup" ) {
                throw new XmlConfigParseError( "WindowGroup节点名称不正确" );
            }

            var group = new WindowGroup();

            if( groupElement.ChildNodes.Count == 0 ) {
                throw new XmlConfigParseError( "WindowGroup必须存在子节点" );
            }

            if(  groupElement is XmlElement) {
                var ele = groupElement as XmlElement;
                foreach( XmlNode windowElement in groupElement.ChildNodes ) {
                    group.Items.Add( GetWindow( windowElement ) );
                }
                return group;
            } else {
                throw new XmlConfigParseError( "WindowGroup节点非Element节点" );
            }
        }

        private static WindowObject GetWindow( XmlNode windowElement ) {
            if( windowElement.Name != "Window" ) {
                throw new XmlConfigParseError( "Window节点名称不正确" );
            }

            const string ATTR_TITLE = "Title";
            const string ATTR_TYPE = "Type";
            const string ATTR_TABLENAME = "TableName";
            const string ATTR_SORTFIELD = "SortField";
            const string ATTR_SORTMODE = "SortMode";
            const string ATTR_UNIQUEID = "UniqueID";

            var window = new WindowObject();

            if( windowElement is XmlElement ) {
                var ele = windowElement as XmlElement;

                if( !ele.HasAttribute( ATTR_TITLE ) ) {
                    throw new XmlConfigParseError( ATTR_TITLE+ "属性必须设置" );
                }
                if( !ele.HasAttribute( ATTR_TYPE ) ) {
                    throw new XmlConfigParseError( ATTR_TYPE + "属性必须设置" );
                }
                if( !ele.HasAttribute( ATTR_TABLENAME ) ) {
                    throw new XmlConfigParseError( ATTR_TABLENAME + "属性必须设置" );
                }
                if( !ele.HasAttribute( ATTR_SORTFIELD ) ) {
                    throw new XmlConfigParseError( ATTR_SORTFIELD + "属性必须设置" );
                }
                if( !ele.HasAttribute( ATTR_SORTMODE ) ) {
                    throw new XmlConfigParseError( ATTR_SORTMODE + "属性必须设置" );
                }
                if( !ele.HasAttribute( ATTR_UNIQUEID ) ) {
                    throw new XmlConfigParseError( ATTR_UNIQUEID + "属性必须设置" );
                }

                var title = ele.GetAttribute( ATTR_TITLE );
                var type = ele.GetAttribute( ATTR_TYPE );
                var tableName = ele.GetAttribute( ATTR_TABLENAME );
                var sortField = ele.GetAttribute( ATTR_SORTFIELD );
                var sortMode = ele.GetAttribute( ATTR_SORTMODE );
                var uniqueID = ele.GetAttribute( ATTR_UNIQUEID );
                
                window.Title = title;
                window.Type = WindowObject.GetType( type );
                window.TableName = tableName;
                window.SortField = sortField;
                window.SortMode = sortMode;
                window.UniqueID = uniqueID;
                if( windowElement.ChildNodes.Count == 0 ) {
                    throw new XmlConfigParseError( "Window节点必须存在子节点" );
                }
                foreach( XmlNode itemElement in windowElement.ChildNodes ) {
                    var windowItem = GetWindowItem( itemElement );
                    window.Items.Add( windowItem );
                }
            } else {
                throw new XmlConfigParseError( "Window节点非Element节点" );
            }

            return window;
        }

        private static WindowItem GetWindowItem( XmlNode windowItemElement ) {
            if( windowItemElement.Name != "Item" ) {
                throw new XmlConfigParseError( "Item节点名称不正确" );
            }
            var item = new WindowItem();
            if( windowItemElement is XmlElement ) {
                var ele = windowItemElement as XmlElement;
                var name = ele.GetAttribute( "Name" );
                var title = ele.GetAttribute( "Title" );
                var dataType = ele.GetAttribute( "DataType" );
                var itemType = ele.GetAttribute( "ItemType" );
                var matchType = ele.GetAttribute("MatchType");
                item.Name = name;
                item.Title = title;
                item.DataType = WindowItem.GetDataType( dataType );
                item.ItemType = WindowItem.GetItemType( itemType );
                item.MatchType = WindowItem.GetMatchType(matchType, item.ItemType);

                var childItemElements = windowItemElement.ChildNodes;
                if( childItemElements.Count > 0 ) {
                    foreach( XmlElement childItem in childItemElements ) {
                        var strName = childItem.GetAttribute( "Name" );
                        var strValue = childItem.GetAttribute( "Value" );
                        item.Contents.Add( new KeyVal<string, string>( strName, strValue ) );
                    }
                }
            } else {
                throw new XmlConfigParseError( "Item节点非Element节点" );
            }
            return item;
        }

        #endregion
    }
}
