using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows;

namespace WebServiceCaller.Logic {
    public class XmlConfigParser {
        public static List<WindowGroup> Parse( string xmlFilePath ) {

            var doc = new XmlDocument();
            doc.Load( xmlFilePath );
            return GetGroups( doc );
        }

        private static List<WindowGroup> GetGroups( XmlDocument doc) {
            var result = new List<WindowGroup>();

            var groupsElements = doc.GetElementsByTagName( "WindowGroups" );

            if( groupsElements.Count > 0 ) {
                var unique = groupsElements[ 0 ];
                foreach( XmlNode groupItemElement in unique ) {
                    result.Add( GetGroup( groupItemElement ) );
                }
            } else {
                throw new XmlConfigParseError( "WindowGroups节点不存在" );
            }
            return result;
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
                window.Title = title;
                window.Type = WindowObject.GetType( type );
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
                item.Name = name;
                item.Title = title;
                item.DataType = WindowItem.GetType( dataType );

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
    }
}
