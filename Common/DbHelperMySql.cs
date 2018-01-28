using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using MySql.Data.MySqlClient;

namespace WebServiceCaller.Common {
    /// <summary>
    /// 数据访问抽象基础类
    /// Copyright (C) Maticsoft
    /// </summary>
    public abstract class DbHelperMySqL {
        //数据库连接字符串(web.config来配置)，可以动态更改connectionString支持多数据库.		
        public static string connectionString = "Data Source=chhblog.com;User ID=x;Password=x;DataBase=x;allow zero datetime=true;Port=3306;Use Procedure Bodies=false;Charset=utf8;";
        public DbHelperMySqL() {
        }

        #region 公用方法
        /// <summary>
        /// 得到最大值
        /// </summary>
        /// <param name="FieldName"></param>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static int GetMaxID( string FieldName, string TableName ) {
            string strsql = "select max(" + FieldName + ")+1 from " + TableName;
            object obj = GetSingle( strsql );
            if( obj == null ) {
                return 1;
            } else {
                return int.Parse( obj.ToString() );
            }
        }
        /// <summary>
        /// 是否存在
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public static bool Exists( string strSql ) {
            object obj = GetSingle( strSql );
            int cmdresult;
            if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                cmdresult = 0;
            } else {
                cmdresult = int.Parse( obj.ToString() );
            }
            if( cmdresult == 0 ) {
                return false;
            } else {
                return true;
            }
        }
        /// <summary>
        /// 是否存在（基于MySqlParameter）
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="cmdParms"></param>
        /// <returns></returns>
        public static bool Exists( string strSql, List<MySqlParameter> cmdParms ) {
            object obj = GetSingle( strSql, cmdParms );
            int cmdresult;
            if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                cmdresult = 0;
            } else {
                cmdresult = int.Parse( obj.ToString() );
            }
            if( cmdresult == 0 ) {
                return false;
            } else {
                return true;
            }
        }
        #endregion

        #region  执行简单SQL语句
        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlByCon( string SQLString, string strConnection ) {
            using( MySqlConnection connection = new MySqlConnection( strConnection ) ) {
                using( MySqlCommand cmd = new MySqlCommand( SQLString, connection ) ) {
                    try {
                        connection.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql( string SQLString ) {
            return ExecuteSqlByCon( SQLString, connectionString );
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle( string SQLString, params MySqlParameter[] cmdParms ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand() ) {
                    try {
                        PrepareCommand( cmd, connection, null, SQLString, cmdParms );
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        throw e;
                    }
                }
            }
        }

        public static int ExecuteSqlByTime( string SQLString, int Times ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand( SQLString, connection ) ) {
                    try {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        int rows = cmd.ExecuteNonQuery();
                        return rows;
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran( List<String> SQLStringList ) {
            using( MySqlConnection conn = new MySqlConnection( connectionString ) ) {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                MySqlTransaction tx = conn.BeginTransaction();
                cmd.Transaction = tx;
                try {
                    int count = 0;
                    for( int n = 0; n < SQLStringList.Count; n++ ) {
                        string strsql = SQLStringList[ n ];
                        if( strsql.Trim().Length > 1 ) {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                } catch {
                    tx.Rollback();
                    return 0;
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql( string SQLString, string content ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                MySqlCommand cmd = new MySqlCommand( SQLString, connection );
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter( "@content", SqlDbType.NText );
                myParameter.Value = content;
                cmd.Parameters.Add( myParameter );
                try {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                } catch( MySql.Data.MySqlClient.MySqlException e ) {
                    throw e;
                } finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 执行带一个存储过程参数的的SQL语句。
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <param name="content">参数内容,比如一个字段是格式复杂的文章，有特殊符号，可以通过这个方式添加</param>
        /// <returns>影响的记录数</returns>
        public static object ExecuteSqlGet( string SQLString, string content ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                MySqlCommand cmd = new MySqlCommand( SQLString, connection );
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter( "@content", SqlDbType.NText );
                myParameter.Value = content;
                cmd.Parameters.Add( myParameter );
                try {
                    connection.Open();
                    object obj = cmd.ExecuteScalar();
                    if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                        return null;
                    } else {
                        return obj;
                    }
                } catch( MySql.Data.MySqlClient.MySqlException e ) {
                    throw e;
                } finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }
        /// <summary>
        /// 向数据库里插入图像格式的字段(和上面情况类似的另一种实例)
        /// </summary>
        /// <param name="strSQL">SQL语句</param>
        /// <param name="fs">图像字节,数据库的字段类型为image的情况</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSqlInsertImg( string strSQL, byte[] fs ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                MySqlCommand cmd = new MySqlCommand( strSQL, connection );
                MySql.Data.MySqlClient.MySqlParameter myParameter = new MySql.Data.MySqlClient.MySqlParameter( "@fs", SqlDbType.Image );
                myParameter.Value = fs;
                cmd.Parameters.Add( myParameter );
                try {
                    connection.Open();
                    int rows = cmd.ExecuteNonQuery();
                    return rows;
                } catch( MySql.Data.MySqlClient.MySqlException e ) {
                    throw e;
                } finally {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle( string SQLString ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand( SQLString, connection ) ) {
                    try {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        public static object GetSingle( string SQLString, int Times ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand( SQLString, connection ) ) {
                    try {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }
        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader( string strSQL ) {
            MySqlConnection connection = new MySqlConnection( connectionString );
            MySqlCommand cmd = new MySqlCommand( strSQL, connection );
            try {
                connection.Open();
                MySqlDataReader myReader = cmd.ExecuteReader( CommandBehavior.CloseConnection );
                return myReader;
            } catch( MySql.Data.MySqlClient.MySqlException e ) {
                throw e;
            }

        }
        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query( string SQLString, string strConnection ) {
            using( MySqlConnection connection = new MySqlConnection( strConnection ) ) {
                DataSet ds = new DataSet();
                try {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter( SQLString, connection );
                    command.Fill( ds, "ds" );
                } catch( MySql.Data.MySqlClient.MySqlException ex ) {
                    throw new Exception( ex.Message );
                }
                return ds;
            }
        }

        public static DataSet Query( string SQLString ) {
            return Query( SQLString, connectionString );
        }

        public static DataSet Query( string SQLString, int Times ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                DataSet ds = new DataSet();
                try {
                    connection.Open();
                    MySqlDataAdapter command = new MySqlDataAdapter( SQLString, connection );
                    command.SelectCommand.CommandTimeout = Times;
                    command.Fill( ds, "ds" );
                } catch( MySql.Data.MySqlClient.MySqlException ex ) {
                    throw new Exception( ex.Message );
                }
                return ds;
            }
        }



        #endregion

        #region 执行带参数的SQL语句

        /// <summary>
        /// 执行SQL语句，返回影响的记录数
        /// </summary>
        /// <param name="SQLString">SQL语句</param>
        /// <returns>影响的记录数</returns>
        public static int ExecuteSql( string SQLString, List<MySqlParameter> cmdList ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand() ) {
                    try {
                        PrepareCommand( cmd, connection, null, SQLString, cmdList.ToArray() );
                        int rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        throw e;
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTran( Hashtable SQLStringList ) {
            using( MySqlConnection conn = new MySqlConnection( connectionString ) ) {
                conn.Open();
                using( MySqlTransaction trans = conn.BeginTransaction() ) {
                    MySqlCommand cmd = new MySqlCommand();
                    try {
                        //循环
                        foreach( DictionaryEntry myDE in SQLStringList ) {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            PrepareCommand( cmd, conn, trans, cmdText, cmdParms );
                            int val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    } catch {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }


        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">SQL语句的哈希表（key为sql语句，value是该语句的MySqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity( Hashtable SQLStringList ) {
            using( MySqlConnection conn = new MySqlConnection( connectionString ) ) {
                conn.Open();
                using( MySqlTransaction trans = conn.BeginTransaction() ) {
                    MySqlCommand cmd = new MySqlCommand();
                    try {
                        int indentity = 0;
                        //循环
                        foreach( DictionaryEntry myDE in SQLStringList ) {
                            string cmdText = myDE.Key.ToString();
                            MySqlParameter[] cmdParms = (MySqlParameter[])myDE.Value;
                            foreach( MySqlParameter q in cmdParms ) {
                                if( q.Direction == ParameterDirection.InputOutput ) {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand( cmd, conn, trans, cmdText, cmdParms );
                            int val = cmd.ExecuteNonQuery();
                            foreach( MySqlParameter q in cmdParms ) {
                                if( q.Direction == ParameterDirection.Output ) {
                                    indentity = Convert.ToInt32( q.Value );
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    } catch {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle( string SQLString, List<MySqlParameter> cmdParms ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                using( MySqlCommand cmd = new MySqlCommand() ) {
                    try {
                        PrepareCommand( cmd, connection, null, SQLString, cmdParms.ToArray() );
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if( ( Object.Equals( obj, null ) ) || ( Object.Equals( obj, System.DBNull.Value ) ) ) {
                            return null;
                        } else {
                            return obj;
                        }
                    } catch( MySql.Data.MySqlClient.MySqlException e ) {
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行查询语句，返回MySqlDataReader ( 注意：调用该方法后，一定要对MySqlDataReader进行Close )
        /// </summary>
        /// <param name="strSQL">查询语句</param>
        /// <returns>MySqlDataReader</returns>
        public static MySqlDataReader ExecuteReader( string SQLString, params MySqlParameter[] cmdParms ) {
            MySqlConnection connection = new MySqlConnection( connectionString );
            MySqlCommand cmd = new MySqlCommand();
            try {
                PrepareCommand( cmd, connection, null, SQLString, cmdParms );
                MySqlDataReader myReader = cmd.ExecuteReader( CommandBehavior.CloseConnection );
                cmd.Parameters.Clear();
                return myReader;
            } catch( MySql.Data.MySqlClient.MySqlException e ) {
                throw e;
            }
            //			finally
            //			{
            //				cmd.Dispose();
            //				connection.Close();
            //			}	

        }

        /// <summary>
        /// 执行查询语句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查询语句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query( string SQLString, List<MySqlParameter> parameters ) {
            using( MySqlConnection connection = new MySqlConnection( connectionString ) ) {
                MySqlCommand cmd = new MySqlCommand();
                PrepareCommand( cmd, connection, null, SQLString, parameters.ToArray() );
                using( MySqlDataAdapter da = new MySqlDataAdapter( cmd ) ) {
                    DataSet ds = new DataSet();
                    try {
                        da.Fill( ds, "ds" );
                        cmd.Parameters.Clear();
                    } catch( MySql.Data.MySqlClient.MySqlException ex ) {
                        throw new Exception( ex.Message );
                    }
                    return ds;
                }
            }
        }


        private static void PrepareCommand( MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms ) {
            if( conn.State != ConnectionState.Open )
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if( trans != null )
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if( cmdParms != null ) {


                foreach( MySqlParameter parameter in cmdParms ) {
                    if( ( parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input ) &&
                        ( parameter.Value == null ) ) {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add( parameter );
                }
            }
        }

        #endregion

        #region 存储过程
        /// <summary>
        /// 执行存储过程，返回SqlDataReader ( 注意：调用该方法后，一定要对SqlDataReader进行Close )
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlDataReader</returns>
        public static MySqlDataReader RunProcedure( string storedProcName, List<MySqlParameter> parameters ) {
            using( MySqlConnection conn = new MySqlConnection( connectionString ) ) {

                MySqlDataReader returnReader;
                conn.Open();
                MySqlCommand command = BuildQueryCommand( conn, storedProcName, parameters );
                command.CommandType = CommandType.StoredProcedure;
                returnReader = command.ExecuteReader( CommandBehavior.CloseConnection );
                return returnReader;
            }
        }
        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <param name="tableName">DataSet结果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure( string storedProcName, List<MySqlParameter> parameters, string tableName, string strConnection ) {
            using( MySqlConnection connection = new MySqlConnection( strConnection ) ) {
                DataSet dataSet = new DataSet();
                connection.Open();

                MySqlDataAdapter sqlDA = new MySqlDataAdapter();
                sqlDA.SelectCommand = BuildQueryCommand( connection, storedProcName, parameters );
                sqlDA.Fill( dataSet, tableName );
                connection.Close();
                return dataSet;
            }
        }

        public static DataSet RunProcedure( string storedProcName, List<MySqlParameter> parameters, string tableName ) {
            return RunProcedure( storedProcName, parameters, tableName, connectionString );
        }


        /// <summary>
        /// 构建 SqlCommand 对象(用来返回一个结果集，而不是一个整数值)
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="storedProcName">存储过程名</param>
        /// <param name="parameters">存储过程参数</param>
        /// <returns>SqlCommand</returns>
        private static MySqlCommand BuildQueryCommand( MySqlConnection connection, string storedProcName, List<MySqlParameter> parameters ) {
            MySqlCommand command = new MySqlCommand( storedProcName, connection );
            command.CommandType = CommandType.StoredProcedure;
            foreach( MySqlParameter parameter in parameters ) {
                if( parameter != null ) {
                    // 检查未分配值的输出参数,将其分配以DBNull.Value.
                    if( ( parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input ) &&
                        ( parameter.Value == null ) ) {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add( parameter );
                }
            }

            return command;
        }
        #endregion

    }
}
