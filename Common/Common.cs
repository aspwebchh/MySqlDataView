using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Net;
using System.IO;

namespace MySqlDataView.Common {
    class Common {
        public static string RemoveLineBreak( string str ) {
            if( string.IsNullOrEmpty( str ) ) {
                return str;
            }
            str = str.Replace( "\n", "" );
            str = str.Replace( "\r", "" );
            return str;
        }

        public static string GetString( string str,  int len ) {
            if( string.IsNullOrEmpty( str ) ) {
                return "";
            } else {
                if( str.Length <= len ) {
                    return str;
                } else {
                    return str.Substring( 0, len ) + "…";
                }
            }
        }

        public static String FilterHtml( string htmlText ) {
            var result = Regex.Replace( htmlText, "<[^>]+>", "" );
            result = Regex.Replace( result, "&[^;]+;", "" );
            return result;
        }

        public static bool IsUrl( string strUrl ) {
            if( string.IsNullOrWhiteSpace( strUrl ) ) {
                return false;
            }
            return Regex.IsMatch( strUrl, @"http(s)?://[\S]+" );
        }

        public static string ReadUrlContent( string url ) {
            var request = WebRequest.Create( url ) as HttpWebRequest;
            request.Method = "GET";
            var response = request.GetResponse() as HttpWebResponse;
            using( var stream = response.GetResponseStream() ) {
                var streamReader = new StreamReader( stream );
                var content = streamReader.ReadToEnd();
                return content;
            }
        }

        public static Tuple<string,string> PathAndFile( string url ) {
            var pattern = @"[^\/]+$";
            var regex = new Regex( pattern );
            var match = regex.Match(url);
            var file = match.Value;
            var path = regex.Replace( url, "" );
            return Tuple.Create( path, file );
        }
    }
}
