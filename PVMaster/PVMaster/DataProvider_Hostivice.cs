﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;



namespace PVMaster
{
    public class DataProvider_Hostivice
    {
        private static string connectSTR = $@"Data Source=192.168.99.100,1434;Initial Catalog=TamdaSW;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";

        private static DataProvider_Hostivice instance;
        public static DataProvider_Hostivice Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DataProvider_Hostivice();
                }
                return DataProvider_Hostivice.instance;
            }

            private set { DataProvider_Hostivice.instance = value; }
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
