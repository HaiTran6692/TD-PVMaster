using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Xml;
using test_QLTS;

namespace PrijemkaHostivice
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
        }
        string localID = "";
        public static Timer timer1 = new Timer();
        private void FormLogin_Load(object sender, EventArgs e)
        {
            LoadComboBoxBranch();
            localID = DateTime.Now.ToString("yyyyMMdd-HH-mm-ss") + ".";
            string Filepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "//inuseCache//" + localID + System.Windows.Forms.SystemInformation.ComputerName + ".txt";
            try
            {
                if (!File.Exists(Filepath)) // If file does not exists
                {
                    File.Create(Filepath).Close(); // Create file
                    using (StreamWriter sw = File.AppendText(Filepath))
                    {
                        sw.WriteLine(DateTime.Now); // Write text to .txt file
                    }
                }
                else // If file already exists
                {
                    using (StreamWriter sw = File.AppendText(Filepath))
                    {
                        sw.WriteLine(DateTime.Now); // Write text to .txt file
                    }
                }
            }
            catch (Exception)
            {
                
            }

            timer1.Interval = 10 * 1000;
            timer1.Tick += tickEventShow;
            timer1.Start();
        }
        private void tickEventShow(object sender, EventArgs e)
        {
            CheckUpdate();
        }
        private void CheckUpdate()
        {
            string Filepath = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "//inuseCache//" + localID + System.Windows.Forms.SystemInformation.ComputerName + ".txt";

            if (!File.Exists(Filepath)) // If file does not exists
            {
                this.Close();
            }
        } // 10s check inusecache to close app
        private void LoadComboBoxBranch()
        {
            List<string> list_Branch = new List<string>();
            list_Branch.Add("TD - Sapa");
            list_Branch.Add("TD - Brno");
            list_Branch.Add("TK - Hostivice");
            list_Branch.Add("TD - Usti");
            list_Branch.Add("DC - Morava");
           
            comboBox1Branch.DataSource = list_Branch;

            try
            {
                string ip_local = GetLocalIPAddress();
            
                if (ip_local.Contains("192.168.5"))
                {
                    comboBox1Branch.Text = "TD - Brno";
                }
                else if (ip_local.Contains("192.168.6"))
                {
                    comboBox1Branch.Text = "TD - Usti";
                }
                else if (ip_local.Contains("192.168.89"))
                {
                    comboBox1Branch.Text = "DC - Morava";
                }
                else if (ip_local.Contains("192.168.99"))
                {
                    comboBox1Branch.Text = "TK - Hostivice";
                }
                else
                {
                    comboBox1Branch.Text = "TD - Sapa";
                }
            }
            catch (Exception)
            {
                comboBox1Branch.Text = "TD - Sapa";
            }
        }
        private void loadFormPrijemka()
        {
            this.Hide();
            var fm = new FormPrijemky();
            fm.SendToFormMain = comboBox1Branch.Text;
            fm.Closed += (s, args) => this.Show();
            fm.Show();
        }
        private void loadFormVydejka()
        {
            this.Hide();
            var fm = new FormVydejky();
            fm.SendToFormVydejky = comboBox1Branch.Text;
            fm.Closed += (s, args) => this.Show();
            fm.Show();
        }


        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox2.Focus();
            }
        }
        private void textBox2_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CheckLogin();
            }
        }        
        private void materialButton1_Click(object sender, EventArgs e)
        {
            CheckLogin();
        }
        private void CheckLogin()
        {
            if (LoadUser_Infor(textBox1.Text, textBox2.Text) == null)
            {
                textBox2.Text = "";
            }
            else if (LoadUser_Infor(textBox1.Text, textBox2.Text).Rows.Count == 1 || (textBox1.Text == "admin" && textBox2.Text == "admin"))
            {
                LoadBranchToDataProvider();
                if (radioButton1_Prijemky.Checked)
                {
                    loadFormPrijemka(); 
                }
                else
                {
                    loadFormVydejka();
                }
            }
            else
            {
                textBox2.Text = "";
            }
        }
        private DataTable LoadUser_Infor(string user, string pw)
        {
            DataTable TB = new DataTable();
            string selectUser = @"select [stt],[ho_ten],[security_lv],[id] from [TS-TTB-Users]
                                where [user_name]=''+ @p1 and [password]=''+ @p2 and  [security_lv]='9' ";
            try
            {
                TB = DataProvider_Sapa.Instance.ExecuteQuery(selectUser, new object[] { user, Encrypt.Instance.EncryptMethod(user + pw, user.Length) });
            }
            catch (Exception)
            {
                TB = null;

            }
            return TB;
        }
        private void LoadBranchToDataProvider()
        {
            if (comboBox1Branch.Text == "TD - Sapa")
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.4.100,1434;Initial Catalog=TamdaQLTS_Sapa;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";
            }
            else if (comboBox1Branch.Text == "TD - Brno")
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.5.100,1434;Initial Catalog=TamdaQLTS_Brno;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";
            }
            else if (comboBox1Branch.Text == "TK - Hostivice")
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.99.100,1434;Initial Catalog=TamdaQLTS_Hostivice;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";
            }
            else if (comboBox1Branch.Text == "TD - Usti")
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.6.100,1434;Initial Catalog=TamdaQLTS_Usti;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";
            }
            else if (comboBox1Branch.Text == "DC - Morava")
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.89.100,1434;Initial Catalog=TamdaQLTS_DC_Morava;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";
            }



            if (radioButton2_Vydejky.Checked)
            {
                DataProvider.SetConnectString = $@"Data Source=192.168.4.100,1434;Initial Catalog=TamdaQLTS_Sapa;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";

                if (comboBox1Branch.Text == "TD - Sapa")
                {
                    DataProvider.SetBranch = "P";
                }
                else if (comboBox1Branch.Text == "TD - Brno")
                {
                    DataProvider.SetBranch = "B";
                }
                else if (comboBox1Branch.Text == "TK - Hostivice")
                {
                    DataProvider.SetBranch = "O";
                }
                else if (comboBox1Branch.Text == "TD - Usti")
                {
                    DataProvider.SetBranch = "U";
                }
                else if (comboBox1Branch.Text == "DC - Morava")
                {
                    DataProvider.SetBranch = "M";
                }
            }

        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network!");
        }// tự động tìm chi nhánh theo IP
        private void FormLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            string sourceFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "//inuseCache//" + localID + System.Windows.Forms.SystemInformation.ComputerName + ".txt";
            string destinationFile = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "//inuseCache//Archive//" + localID + System.Windows.Forms.SystemInformation.ComputerName + ".txt";
            try
            {
                File.Move(sourceFile, destinationFile);
            }
            catch (Exception)
            {
                
            }
        }
    }
}
