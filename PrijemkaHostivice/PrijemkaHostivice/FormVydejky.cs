using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrijemkaHostivice
{
    public partial class FormVydejky : Form
    {
        public FormVydejky()
        {
            InitializeComponent();
        }

        private string sendToFormVydejky;

        public string SendToFormVydejky
        {
            get { return sendToFormVydejky; }
            set { sendToFormVydejky = value; }
        }
        private void FormVydejky_Load(object sender, EventArgs e)
        {
            this.Text = "PV_Report v2e.090721 Vydejky " + SendToFormVydejky.ToString();

            this.WindowState = FormWindowState.Maximized;


            dateTimePicker1from.Format = DateTimePickerFormat.Custom;
            dateTimePicker1from.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1from.Value = DateTime.Today.AddDays(-1);

            dateTimePicker2to.Format = DateTimePickerFormat.Custom;
            dateTimePicker2to.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2to.Value = DateTime.Today;

            LoadVydejky_od_cisloobj();
        }
        private void LoadVydejky_od_cisloobj()
        {
            string sqlLoadVydejky_od_cisloobj = $@" SELECT id 
                                                  ,order_id
                                                  ,CONVERT(DATETIME, STUFF(STUFF(STUFF(porizeno,13,0,':'),11,0,':'),9,0,' ')) AS Datum
                                                  ,odberatel
                                                  ,faktura
                                                  ,concat(pobocka,'-',text_pred,'-',user_id) as info
                                                  from [TDFaktury].[dbo].[FakVyd] fv
                                                  where pobocka = '{DataProvider.GetBranch}' 
                                                        and DATEDIFF(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104), cast(left(vystavena,8) as date))>=0                                                                      
                                                        and DATEDIFF(day,convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104), cast(left(vystavena,8) as date))<=0
                                                 union all 
                                                  SELECT  id                                    ,order_id
                                                                                                ,porizeno AS Datum
                                                                                                ,odb 
                                                                                                ,fa
                                                                                                ,concat(pob,'-',pok,'-',user_id) as info                                            
                                                                                                FROM [TDFaktury].[dbo].[Archiv]
			   								                                                    WHERE pob = '{DataProvider.GetBranch}' and fa=0                                     
                                                  and datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),porizeno)>=0
                                                  and datediff(day,convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104),porizeno)<=0
                                                  order by Datum ";
            DataTable TB = new DataTable();
            try
            {
                TB = DataProvider.Instance.ExecuteQuery(sqlLoadVydejky_od_cisloobj);
                dataGridView1.DataSource = null;
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Číslo výdejky";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Datum";
                    dataGridView1.Columns[3].HeaderText = "Název";
                    dataGridView1.Columns[4].HeaderText = "Faktura";
                    dataGridView1.Columns[5].HeaderText = "Pokladna";
                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    dataGridView1.Columns.Add(btn);
                    btn.HeaderText = " Details ";                   
                    btn.Text = " Detail ";
                    btn.Name = "btn";
                    btn.UseColumnTextForButtonValue = true;


                    for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                    {
                        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                    }
                    dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    //label3.Text = $"Rows count: {dataGridView1.Rows.Count}";
                }
                else
                {
                    //  MessageBox.Show("Không có đơn hàng nào!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(sqlLoadVydejky_od_cisloobj+ex.ToString());
            }

        }

        string t2 = "";
        string t1 = "";
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();
                row = dataGridView1.Rows[e.RowIndex];
                t2 = row.Cells[0].Value.ToString();
                t1 = row.Cells[1].Value.ToString();
                textBox2.Text = t2;
                textBox1.Text = t1;
            }
            catch (Exception)
            {
                t2 = "0";
                t1 = "0";
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 7)
            {
                try
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row = dataGridView1.Rows[e.RowIndex];
                    t2 = row.Cells[0].Value.ToString();
                    t1 = row.Cells[1].Value.ToString();
                    textBox2.Text = t2;
                    textBox1.Text = t1;
                    RunAsync();
                }
                catch (Exception)
                {

                }
            }
        }
        private void RunAsync()
        {
            crystalReportViewer1.ReportSource = null;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            progressBar1.MarqueeAnimationSpeed = 1;
            worker.RunWorkerAsync();
        }
    }
}
