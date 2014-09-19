using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ProjectOpen.Controllers
{
    public class Database
    {
        
        private OdbcConnection Conn;

        public Database()
        {
            //Conn = new System.Data.Odbc.OdbcConnection("DSN=Phoenix;UID=fnmiuse1;PWD=fnmiuse1;");
            Conn = new System.Data.Odbc.OdbcConnection("DSN=Phoenix_32;UID=fnmiuse1;PWD=fnmiuse1;");
            Conn.ConnectionTimeout = 300;
        }

        public bool openConnection()
        {
            try
            {
                Conn.Open();
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public bool closeConnection()
        {
            try
            { 
                Conn.Close();
            }
            catch(Exception ex)
            {
                return false;
            }
            finally
            {

            }

            return true;
        }


        public DataSet getDataset(string sql)
        {
            DataSet ds = new DataSet();

            try
            { 
                OdbcDataAdapter adapter = new OdbcDataAdapter(sql, Conn);

                adapter.Fill(ds, "dsResults");

                return ds;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                
            }

        }

        public bool executeSQL(string sql)
        {
            try
            {
                
                OdbcCommand comm = new OdbcCommand(sql, Conn);
                comm.CommandTimeout = 300;
                int res = comm.ExecuteNonQuery();

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {

            }
        }

        public bool executeSQL(List<string> sql)
        {
            try
            {
                
                foreach(string s in sql)
                {
                    OdbcCommand comm = new OdbcCommand(s, Conn);
                    comm.CommandTimeout = 300;
                    int res = comm.ExecuteNonQuery();
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {

            }
        }
    }
}