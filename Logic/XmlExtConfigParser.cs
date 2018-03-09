using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;


namespace MySqlDataView.Logic {
    public class XmlExtConfigParser {
        private static List<KeyVal<string, string>> Parse( XDocument doc ) {
            var root = doc.Element( "Items" );
            if( root == null ) {
                throw new XmlConfigParseError( "Items节点不存在" );
            }
            var items = doc.Descendants( "Item" );
            if( items.Count() == 0 ) {
                throw new XmlConfigParseError( "Item节点不存在" );
            }
            try {
                var result = from element in doc.Descendants( "Item" )
                             select new KeyVal<string, string>(
                                 element.Attribute( "Name" ).Value,
                                 element.Attribute( "Value" ).Value );
                return result.ToList();
            } catch( NullReferenceException ) {
                throw new XmlConfigParseError( "Item节点Name属性或者Value属性不存在" );
            } catch( Exception e ) {
                throw new XmlConfigParseError( e.Message );
            }
        }

        public static List<KeyVal<string, string>> Parse( string xmlFilePath ) {
            var doc = XDocument.Load( xmlFilePath );
            return Parse( doc );
        }

        public static List<KeyVal<string, string>> ParseUrl( string url ) {
            var doc = XDocument.Load( url );
            return Parse( doc );
        }
    }
}