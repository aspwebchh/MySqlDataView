using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows;

namespace WebServiceCaller.Logic {
    public class XmlConfigParser {

        public static Config Parse( string xmlFilePath ) {
            var doc = new XmlDocument();
            doc.Load( xmlFilePath );

            var windowsGroup = GetGroups( doc );
            var products = GetProducts( doc );

            var config = new Config();
            config.WindowsGroup = windowsGroup;
            config.Products = products;
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
            return product;
        }
        #endregion;

        #region
        private static WindowGroup GetGroups( XmlDocument doc) {
            var groupElements = doc.GetElementsByTagName( "WindowGroup" );
            if( groupElements.Count > 0 ) {
                var unique = groupElements[ 0 ];
                return GetGroup( unique );
            } else {
                throw new XmlConfigParseError( "WindowGroup节点不存在" );
            }
        }

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
                var title = ele.GetAttribute( "Title" );
                group.Title = title;

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

            var window = new WindowObject();

            if( windowElement is XmlElement ) {
                var ele = windowElement as XmlElement;
                var title = ele.GetAttribute( "Title" );
                var type = ele.GetAttribute( "Type" );
                var tableName = ele.GetAttribute( "TableName" );
                var sortField = ele.GetAttribute( "SortField" );
                var sortMode = ele.GetAttribute( "SortMode" );
                window.Title = title;
                window.Type = WindowObject.GetType( type );
                window.TableName = tableName;
                window.SortField = sortField;
                window.SortMode = sortMode;
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

                if( item.DataType == WindowItemDataType.Map || item.DataType == WindowItemDataType.List) {
                    var childItemElements = windowItemElement.ChildNodes;
                    if( childItemElements.Count == 0 ) {
                        throw new XmlConfigParseError( "类型为Map或者List的Item节点必须存在子节点" );
                    }
                    foreach( XmlNode childItemElement in windowItemElement.ChildNodes ) {
                        item.Items.Add( GetWindowItem( childItemElement ) );
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
