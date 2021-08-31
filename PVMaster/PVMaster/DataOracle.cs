using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
namespace PVMaster
{


    public class DataOracleMorava
    {
        private string connectSTR = @"User Id=TRANNGOCHAI;Password=t.20cvu;Data Source=10.70.20.133:1522/PMSSRV";

        private static DataOracleMorava instance;
        public static DataOracleMorava Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataOracleMorava();
                }
                return DataOracleMorava.instance;
            }

            private set { DataOracleMorava.instance = value; }
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


