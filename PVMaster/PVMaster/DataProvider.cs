using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PVMaster
{
    public class DataProvider
    {
        private static string connectSTR = "";
        private static string branch = "";
        public static string SetConnectString
        {
            get { return connectSTR; }
            set { connectSTR = value; }
        }
        public static string SetBranch
        {            
            set { branch = value; }
        }
        public static string GetBranch
        {
            get { return branch; }
        }        

    private static DataProvider instance;
        public static DataProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataProvider();
                }
                return DataProvider.instance;
            }

            private set { DataProvider.instance = value; }
        }
        public DataTable ExecuteQuery(String query, object[] parametr = null)
        {

            DataTable data = new DataTable();
            using (SqlConnection con1 = new SqlConnection(connectSTR))
            {
                con1.Open();
                SqlCommand cm1 = new SqlCommand(query, con1);
                if (parametr != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (String item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            cm1.Parameters.AddWithValue(item, parametr[i]);
                            i++;
                        }
                    }
                }
                SqlDataAdapter adapter = new SqlDataAdapter(cm1);
                adapter.Fill(data);
                con1.Close();
            }
            return data;
        }
        public int ExecuteNonQuery(String query, object[] parametr = null)
        {
            int data = 0;
            using (SqlConnection con1 = new SqlConnection(connectSTR))
            {
                con1.Open();
                SqlCommand cm1 = new SqlCommand(query, con1);
                if (parametr != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (String item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            cm1.Parameters.AddWithValue(item, parametr[i]);
                            i++;
                        }
                    }

                }
                data = cm1.ExecuteNonQuery();
                con1.Close();
            }
            return data;
        }
        public object ExecuteScalar(String query, object[] parametr = null)
        {

            object data = new DataTable();
            using (SqlConnection con1 = new SqlConnection(connectSTR))
            {
                con1.Open();
                SqlCommand cm1 = new SqlCommand(query, con1);
                if (parametr != null)
                {
                    string[] listPara = query.Split(' ');
                    int i = 0;
                    foreach (String item in listPara)
                    {
                        if (item.Contains('@'))
                        {
                            cm1.Parameters.AddWithValue(item, parametr[i]);
                            i++;
                        }
                    }
                }
                data = cm1.ExecuteScalar();
                con1.Close();
            }
            return data;
        }
    }
}
