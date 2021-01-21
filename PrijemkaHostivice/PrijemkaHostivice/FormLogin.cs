using System;
using System.Data;
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
        private void FormLogin_Load(object sender, EventArgs e)
        {
            textBox1.Focus();
        }

        private void loadFormMain()
        {
            if (LoadSttUser(textBox1.Text, textBox2.Text) != null)
            {
                this.Hide();
                var fm = new FormMain();
                fm.Closed += (s, args) => this.Close();
                fm.Show();
            }
            else
            {
                textBox2.Clear();
                textBox2.Focus();
            }
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
                loadFormMain();
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            loadFormMain();
        } //buton login
        private string LoadSttUser(string user, string pw)
        {
            string stt = null;
            string selectUser = @"select * from [TamdaQLTS_Hostivice].[dbo].[TS-TTB-Users]
                                where user_name=''+ @p1 and password=''+ @p2 ";
            try
            {
                DataTable TB = new DataTable();
                TB = DataProvider.Instance.ExecuteQuery(selectUser, new object[] { user, Encrypt.Instance.EncryptMethod(user + pw, user.Length) });
                int rowc = TB.Rows.Count;
                if (rowc == 1)
                {
                    stt = TB.Rows[0][3].ToString();
                }
                else
                {
                    stt = null;
                }
            }
            catch (Exception)
            {
                stt = null;

            }
            return stt;
        }

    }
}
