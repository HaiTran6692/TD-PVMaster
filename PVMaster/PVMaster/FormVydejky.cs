using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVMaster
{
    public partial class FormVydejky : Form
    {
        private BackgroundWorker worker_left = null;
        private BackgroundWorker worker_right = null;
        List<InvoiceDetail> _List = new List<InvoiceDetail>();

        DataTable TB_left = new DataTable();
        CrystalReport_Vydejky cr_vydejky = new CrystalReport_Vydejky();
        string odb_numb = "";
        string datum_vydejky = "";
        string informace_kasa = "";


        private string sendToFormVydejky;
        public string SendToFormVydejky
        {
            get { return sendToFormVydejky; }
            set { sendToFormVydejky = value; }
        }
        public FormVydejky()
        {
            InitializeComponent();
            progressBar_left.Visible = false;
            worker_left = new BackgroundWorker();
            worker_left.WorkerReportsProgress = true;
            worker_left.DoWork += worker_left_DoWork;
            worker_left.RunWorkerCompleted += worker_left_RunWorkerCompleted;


            progressBar_right.Visible = false;
            worker_right = new BackgroundWorker();
            worker_right.WorkerReportsProgress = true;
            worker_right.DoWork += worker_right_DoWork;
            worker_right.RunWorkerCompleted += worker_right_RunWorkerCompleted;



        }
        void worker_left_DoWork(object sender, DoWorkEventArgs e)
        {
            LoadVydejky();
        }
        void worker_left_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridView1.DataSource = null;
            progressBar_left.Visible = false;
            if (TB_left.Rows.Count > 0)
            {
                dataGridView1.Columns.Clear();
                dataGridView1.DataSource = TB_left;
                dataGridView1.Columns[0].HeaderText = "Číslo výdejky";
                dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                dataGridView1.Columns[2].HeaderText = "Datum";
                dataGridView1.Columns[3].HeaderText = "Odberatel";
                dataGridView1.Columns[4].HeaderText = "Faktura";
                dataGridView1.Columns[5].HeaderText = "Informace";
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
                label3.Text = $"Rows count: {dataGridView1.Rows.Count}";
            }
            else
            {
                //  MessageBox.Show("Không có đơn hàng nào!");
            }
        }
        void worker_right_DoWork(object sender, DoWorkEventArgs e)
        {
            Load_VydejkyDetail(textBox1.Text, textBox2.Text);
        }
        void worker_right_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar_right.Visible = false;
            crystalReportViewer1.ReportSource = cr_vydejky;
        }
        private void FormVydejky_Load(object sender, EventArgs e)
        {
            //this.Text = "PVMaster v2e.090721 Vydejky " + SendToFormVydejky.ToString();
            this.Text = "PVMaster v2f.250821 Vydejky " + SendToFormVydejky.ToString(); // thêm thông tin informace pokladna, cislo uctenka

            this.WindowState = FormWindowState.Maximized;
            dateTimePicker1from.Format = DateTimePickerFormat.Custom;
            dateTimePicker1from.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1from.Value = DateTime.Today.AddDays(-1);
            dateTimePicker2to.Format = DateTimePickerFormat.Custom;
            dateTimePicker2to.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2to.Value = DateTime.Today;

            progressBar_left.Visible = true;
            progressBar_left.Style = ProgressBarStyle.Marquee;
            progressBar_left.MarqueeAnimationSpeed = 1;
            worker_left.RunWorkerAsync();



        }
        private void LoadVydejky()
        {
            string sqlLoadVydejky = $@" SELECT id 
                                                  ,order_id
                                                  ,CONVERT(DATETIME, STUFF(STUFF(STUFF(porizeno,13,0,':'),11,0,':'),9,0,' ')) AS Datum
                                                  ,case when len(isnull(odberatel,'0'))=0 then '0' else isnull(odberatel,'0') end  AS odberatel
                                                  ,faktura,concat('Faktura: ',faktura,' - ',text_pred) as info
                                                  --,concat('Faktura: ',faktura,' - user: ',user_id,'- ',text_pred) as info
                                                  from [TDFaktury].[dbo].[FakVyd] fv
                                                  where pobocka = '{DataProvider.GetBranch}' 
                                                        and DATEDIFF(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104), cast(left(vystavena,8) as date))>=0                                                                      
                                                        and DATEDIFF(day,convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104), cast(left(vystavena,8) as date))<=0
                                                 union all 
                                                  SELECT  id                                    ,order_id
                                                                                                ,porizeno AS Datum
                                                                                                ,case when len(isnull(odb,'0'))=0 then '0' else isnull(odb,'0') end  AS odberatel
                                                                                                ,fa,concat(N'Učtenka: ',ic,' - Pokladna:',pok) as info            
                                                                                               -- ,concat(N'Učtenka: ',ic,' - user: ',user_id,'- Pokladna:',pok) as info                                            
                                                                                                FROM [TDFaktury].[dbo].[Archiv]
			   								                                                    WHERE pob = '{DataProvider.GetBranch}' and fa=0                                     
                                                  and datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),porizeno)>=0
                                                  and datediff(day,convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104),porizeno)<=0
                                                  order by Datum ";
            try
            {
                TB_left = DataProvider_Sapa.Instance.ExecuteQuery(sqlLoadVydejky);
            }
            catch (Exception ex)
            {
                MessageBox.Show(sqlLoadVydejky + ex.ToString());
            }

        }
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();
                row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells[0].Value.ToString();
                textBox2.Text = row.Cells[4].Value.ToString();
                odb_numb = row.Cells[3].Value.ToString();
                datum_vydejky = row.Cells[2].Value.ToString();
                informace_kasa = row.Cells[5].Value.ToString();
             
            }
            catch (Exception)
            {
                textBox1.Text = "0";
                textBox2.Text = "0";
                odb_numb = "0";
                datum_vydejky = "";
                informace_kasa="";
            }
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 6)
            {
                try
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row = dataGridView1.Rows[e.RowIndex];
                    textBox1.Text = row.Cells[0].Value.ToString();
                    textBox2.Text = row.Cells[4].Value.ToString();
                    odb_numb = row.Cells[3].Value.ToString();
                    datum_vydejky = row.Cells[2].Value.ToString();
                    informace_kasa = row.Cells[5].Value.ToString();
                    RunAsyn_Right();
                }
                catch (Exception)
                {
                    textBox1.Text = "0";
                    textBox2.Text = "0";
                    odb_numb = "0";
                    datum_vydejky = "";
                    informace_kasa = "";
                }
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!worker_left.IsBusy)
            {
                progressBar_left.Visible = true;
                progressBar_left.Style = ProgressBarStyle.Marquee;
                progressBar_left.MarqueeAnimationSpeed = 1;
                worker_left.RunWorkerAsync();
            }
            else
            {
                MessageBox.Show("Still running!");
            }
        }
        private void Load_VydejkyDetail(string vydejka_cislo, string faktura_cislo)
        {
            _List.Clear();
            string sql_ArchivPol = $@"SELECT [kod_zbozi]     
                                           ,[nazev]
                                           ,[mnozstvi]
                                           ,[pc_bez]
                                            
                                      FROM [TDFaktury].[dbo].[ArchivPol]
                                      where archiv_id='{vydejka_cislo}' and ISNUMERIC(kod_zbozi)=1 ";

            string sql_FakVydPol = $@"SELECT fvp.[kod_zbozi]
	                                      ,[nazev]
                                          ,[ks]
                                          ,[cena_j]
                                      FROM [TDFaktury].[dbo].[FakVydPol] fvp
                                      left join [TDF Database].dbo.CENIK ck on fvp.kod_zbozi=ck.kod_zbozi
                                      where faktura='{faktura_cislo}'
                                      and len(fvp.kod_zbozi)>1 
                                      order by fvp.id";
            DataTable TB_erp = new DataTable();
            try
            {
                if (int.Parse(faktura_cislo) > 0)
                {
                    TB_erp = DataProvider_Sapa.Instance.ExecuteQuery(sql_FakVydPol);
                }
                else
                {
                    TB_erp = DataProvider_Sapa.Instance.ExecuteQuery(sql_ArchivPol);
                }

                if (TB_erp.Rows.Count < 1)
                {
                    //MessageBox.Show("Không có đơn hàng này!");
                }
                else
                {
                    decimal celkem_V = 0;
                    decimal celkem_m = 0;

                    for (int i = 0; i < TB_erp.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_erp.Rows[i][0].ToString(),
                            nazev = TB_erp.Rows[i][1].ToString(),
                            prjato = TB_erp.Rows[i][2].ToString(),
                            objednano = TB_erp.Rows[i][2].ToString(),
                            cena_bez= TB_erp.Rows[i][3].ToString()
                        });
                        celkem_V += Load_V_kus(TB_erp.Rows[i][0].ToString(), TB_erp.Rows[i][2].ToString());
                        celkem_m += Load_m_kus(TB_erp.Rows[i][0].ToString(), TB_erp.Rows[i][2].ToString());
                    }

                    string sql_get_odb = $@"SELECT 
                                            cislo
                                            ,[nazev]
                                            ,[ico]
                                            ,[dic]
                                            ,[ulice]
                                            ,concat([psc],' ',[obec])
                                          FROM [CustomerManagement].[dbo].[tmpCustomers] where cislo= '{odb_numb}'";
                    DataTable TB_odb = new DataTable();
                    try
                    {
                        TB_odb = DataProvider.Instance.ExecuteQuery(sql_get_odb);
                    }
                    catch (Exception)
                    {

                    }
                    string cislodod = "";
                    string nazev = "";
                    string ico = "";
                    string dic = "";
                    string street = "";
                    string city = "";
                    string obj = "";
                    string prijemka = "";
                    string datumPrijmuD = "";

                    if (int.Parse(faktura_cislo) > 0 && TB_odb.Rows.Count > 0)
                    {                       
                        cislodod = TB_odb.Rows[0][0].ToString();
                        nazev = TB_odb.Rows[0][1].ToString();
                        ico = "IČ: " + TB_odb.Rows[0][2].ToString();
                        dic = "DIČ: " + TB_odb.Rows[0][3].ToString();
                        street = TB_odb.Rows[0][4].ToString();
                        city = TB_odb.Rows[0][5].ToString();
                        obj = "Objednávka č." + vydejka_cislo;
                        prijemka = "VÝDEJKA: " + vydejka_cislo;
                        datumPrijmuD = datum_vydejky;

                        cr_vydejky.SetDataSource(_List);
                        Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        bar1.Draw(vydejka_cislo, 40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");
                        cr_vydejky.SetParameterValue("EAN", Application.StartupPath + "\\cache\\file_EAN_obj.png"); //location  
                        cr_vydejky.SetParameterValue("prijemka", prijemka);
                        cr_vydejky.SetParameterValue("obj", obj);
                        cr_vydejky.SetParameterValue("Dodavatel", nazev);
                        cr_vydejky.SetParameterValue("DodavatelCislo", cislodod);
                        cr_vydejky.SetParameterValue("cityD", city);
                        cr_vydejky.SetParameterValue("streetD", street);
                        cr_vydejky.SetParameterValue("icD", ico);
                        cr_vydejky.SetParameterValue("dicD", dic);
                        cr_vydejky.SetParameterValue("dicD", dic);
                        cr_vydejky.SetParameterValue("datumPrijmuD", datumPrijmuD);
                        cr_vydejky.SetParameterValue("celk_m", celkem_m.ToString("0.000"));
                        cr_vydejky.SetParameterValue("celk_V", celkem_V.ToString("0.000"));
                        cr_vydejky.SetParameterValue("informace_kasa", informace_kasa);
                    }
                    else
                    {
                        cislodod = "...";
                        nazev = "Koncový zakazník";
                        ico = "IČ: ...";
                        dic = "DIČ: ...";
                        street = "..."; 
                        city = "...";
                        obj = "Objednávka č." + vydejka_cislo;
                        prijemka = "VÝDEJKA: " + vydejka_cislo;
                        datumPrijmuD = datum_vydejky;

                        cr_vydejky.SetDataSource(_List);
                        Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        bar1.Draw(vydejka_cislo, 40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");
                        cr_vydejky.SetParameterValue("EAN", Application.StartupPath + "\\cache\\file_EAN_obj.png"); //location  
                        cr_vydejky.SetParameterValue("prijemka", prijemka);
                        cr_vydejky.SetParameterValue("obj", obj);
                        cr_vydejky.SetParameterValue("Dodavatel", nazev);
                        cr_vydejky.SetParameterValue("DodavatelCislo", cislodod);
                        cr_vydejky.SetParameterValue("cityD", city);
                        cr_vydejky.SetParameterValue("streetD", street);
                        cr_vydejky.SetParameterValue("icD", ico);
                        cr_vydejky.SetParameterValue("dicD", dic);
                        cr_vydejky.SetParameterValue("dicD", dic);
                        cr_vydejky.SetParameterValue("datumPrijmuD", datumPrijmuD);
                        cr_vydejky.SetParameterValue("celk_m", celkem_m.ToString("0.000"));
                        cr_vydejky.SetParameterValue("celk_V", celkem_V.ToString("0.000"));
                        cr_vydejky.SetParameterValue("informace_kasa", informace_kasa);
                    }



                    // crystalReportViewer1.Zoom(80);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void RunAsyn_Right()
        {
            if (!worker_right.IsBusy)
            {
                crystalReportViewer1.ReportSource = null;
                progressBar_right.Visible = true;
                progressBar_right.Style = ProgressBarStyle.Marquee;
                progressBar_right.MarqueeAnimationSpeed = 1;
                worker_right.RunWorkerAsync();
            }
        }
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    RunAsyn_Right();
                }
                catch (Exception)
                {

                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                try
                {
                    DialogResult dialogResult = MessageBox.Show("Bạn có muốn xuất dữ liệu ra file Excel không?", "Cảnh báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button3);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ExportDTGVToExcel.Instance.ExportToExcel(dataGridView1);
                    }
                }
                catch (Exception)
                {

                }
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            RunAsyn_Right();
        }
        private decimal Load_V_kus(string mnb, string slg)
        {
            decimal _load_V_kus = 0;
            string sql1 = $@"Select  [kod_zbozi]
                                  ,[sirka_kus]*[hloubka_kus]*[vyska_kus] as V_kus
                                  ,[vaha_kus] as m_kus     
                              FROM [TamdaHostivice].[dbo].[MASTER_DATA] where [kod_zbozi]='{mnb}' ";
            try
            {
                DataTable TB = DataProvider_Hostivice.Instance.ExecuteQuery(sql1);
                if (TB.Rows.Count > 0)
                {
                    _load_V_kus = decimal.Parse(TB.Rows[0][1].ToString()) * decimal.Parse(slg) / 1000 / 1000 / 1000;
                }
                else
                {
                    _load_V_kus = decimal.Parse(slg) / 977; //97.7mm            
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return _load_V_kus;
        }
        private decimal Load_m_kus(string mnb, string slg)
        {
            decimal _load_m_kus = 0;
            string sql1 = $@"Select  [kod_zbozi]
                                  ,[sirka_kus]*[hloubka_kus]*[vyska_kus] as m_kus
                                  ,[vaha_kus] as m_kus     --gram
                              FROM [TamdaHostivice].[dbo].[MASTER_DATA] where [kod_zbozi]='{mnb}' ";
            try
            {
                DataTable TB = DataProvider_Hostivice.Instance.ExecuteQuery(sql1);
                if (TB.Rows.Count > 0)
                {
                    _load_m_kus = decimal.Parse(TB.Rows[0][2].ToString()) * decimal.Parse(slg) / 1000;
                }
                else
                {
                    _load_m_kus = decimal.Parse(slg) * 97 / 1000; //97gram  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return _load_m_kus;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
           crystalReportViewer1.PrintReport();
        }
    }
}
