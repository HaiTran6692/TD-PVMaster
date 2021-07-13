using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
namespace PrijemkaHostivice
{


    public class DataOracle
    {
        private string connectSTR = @"User Id=NGOCHAI;Password=x.19zda;Data Source=192.168.99.248,1521/PMS";

        private static DataOracle instance;
        public static DataOracle Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataOracle();
                }
                return DataOracle.instance;
            }

            private set { DataOracle.instance = value; }
        }
        public DataTable ExecuteQuery(String query)
        {
            DataTable data = new DataTable();
            using (OracleConnection con = new OracleConnection(connectSTR))
            {
                con.Open();
                OracleCommand cm1 = con.CreateCommand();
                cm1.CommandText = query;
                OracleDataReader reader = cm1.ExecuteReader();
                data.Load(reader);
                con.Close();
                con.Dispose();
            }
            return data;
        }
    }
}


