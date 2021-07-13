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
    public partial class FormPrijemky : Form
    {
        private BackgroundWorker worker = null;
        CrystalReport_Prijemky cr12 = new CrystalReport_Prijemky();
        List<InvoiceDetail> _List = new List<InvoiceDetail>();
        public FormPrijemky()
        {
            InitializeComponent();
            progressBar1.Visible = false;
            worker = new BackgroundWorker();  
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
        }
        private string sendToFormMain;
        public string SendToFormMain
        {
            get { return sendToFormMain; }
            set { sendToFormMain = value; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Text = "PV_Report v2a.280521 "+ SendToFormMain.ToString();
            //this.Text = "PV_Report v2b.230621 "+ SendToFormMain.ToString();
            //this.Text = "PV_Report v2c.240621 " + SendToFormMain.ToString();
            //this.Text = "PV_Report v2d.290621 Prijemky " + SendToFormMain.ToString();
            
            this.Text = "PV_Report v3a.130721 Prijemky " + SendToFormMain.ToString();

            this.WindowState = FormWindowState.Maximized;
            dateTimePicker1from.Format = DateTimePickerFormat.Custom;
            dateTimePicker1from.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1from.Value = DateTime.Today.AddDays(-1);

            dateTimePicker2to.Format = DateTimePickerFormat.Custom;
            dateTimePicker2to.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2to.Value = DateTime.Today;

            if (SendToFormMain=="TK - Hostivice" || SendToFormMain == "DC - Morava")
            {
                LoadPrijemkyGold();
            }
            else
            {
                LoadPrijemky_od_cisloobj();
            }
            
        }
        private void LoadPrijemkyGold()
        {
            string sqlLoadPrijemky = $@"SELECT        --substr(es_adrqre, 5, 2) AS rampa,
                                                      oe_ncdefo AS prijemka, oe_ncdefo AS order_num,
                                                      oe_librs   AS dodavatel,
                                                      CASE WHEN ma_starr = 0 THEN
                                                                        'INITIALISE'
                                                           WHEN ma_starr = 1 THEN
                                                                        'ON DOCK'
                                                           WHEN ma_starr = 2 THEN
                                                                        'Kontroluje'
                                                           WHEN ma_starr = 3 THEN
                                                                        'HOTOVO!'
                                                           END as Status,
                                                       TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'DD/MM HH24:MI') AS Recep_date,
                                                       -- TO_CHAR(ma_datclo, 'DD/MM HH24:MI') AS Close_date, 
                                                       (trunc(24 *(SYSDATE - ma_dtrecr) - 24 *(trunc(SYSDATE - ma_dtrecr))))||'h:'||TO_CHAR(trunc(60 * 24 *(sysdate - ma_dtrecr)) - 60 *(trunc(24 *(SYSDATE - ma_dtrecr))),'00') AS celkem_cas,
                                                       --  ma_starr   AS status_num,
                                                        ml_numarr AS consignment_num,
                                                        ml_numorc AS stock_num,substr(es_adrqre, 5, 2) AS rampa
                                          FROM  tb_lrdv left join tb_erdv ON ml_numarr = ma_numarr
                                          LEFT join tb_erec ON ml_numorc = oe_numorc
                                          LEFT JOIN tb_eslrec ON ml_numarr = es_numarr
                                          WHERE ma_starr !=0 
                                                AND TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMdd') >= '{dateTimePicker1from.Value.ToString("yyyyMMdd")}'
                                                AND TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMdd') <= '{dateTimePicker2to.Value.ToString("yyyyMMdd")}'
                                          ORDER BY TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMDDHH24MISS') DESC";
            DataTable TB = new DataTable();
            try
            {
                if (SendToFormMain == "TK - Hostivice")
                {
                    TB = DataOracle.Instance.ExecuteQuery(sqlLoadPrijemky);
                }
                else
                {
                    TB = DataOracleMorava.Instance.ExecuteQuery(sqlLoadPrijemky);
                }
                
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Příjemka";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Dodavatel";
                    dataGridView1.Columns[3].HeaderText = "Stav";
                    dataGridView1.Columns[4].HeaderText = "Čas příjmu";
                    dataGridView1.Columns[5].HeaderText = "Čas celkem";
                    dataGridView1.Columns[6].HeaderText = "Consignment";
                    dataGridView1.Columns[7].HeaderText = "Stock";
                    dataGridView1.Columns[8].HeaderText = "Rampa";
                }
                else
                {
                    MessageBox.Show("Không có đơn hàng nào!");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Xảy ra lỗi");
            }

        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {           
            LoadReportDetail(textBox1.Text, textBox2.Text);           
        }       
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            crystalReportViewer1.ReportSource = cr12;
        }       
        private void LoadReportDetail(string inputOBJ, string inputPrijemka)
        {            
            _List.Clear();
            string sqlERP1 = $@"select 
                            format(p.created,'dd.MM.yyyy') as datum_prijmu --0
                            ,f.dodavatel--1
                            ,f.cislo--2
                            ,kod_zbozi--3
                            ,nazev--4
                            ,dph --5
                            ,convert(int,mnozstvi) as prijato --6
                            ,(select top 1 convert(int,pocetmj*baleni) from [OrdersManager].[dbo].[ObjVydPol] vp where charindex(vp.cisloobj,f.cisloobj,0)>0 and vp.kod_zbozi=p.kod_zbozi) as objednano   --7                    
                            from [OrdersManager].[dbo].[FakPol] p 
                            left join [OrdersManager].[dbo].[Faktury] f on p.cisloobj=f.cisloobj and p.casti=f.casti
                            where f.cislo='{inputPrijemka}'
                            order by  p.id ";

            string sqlERP_HOS = $@"select 
                            format(p.created,'dd.MM.yyyy') as datum_prijmu --0
                            ,f.dodavatel--1
                            ,f.cislo--2
                            ,kod_zbozi--3
                            ,nazev--4
                            ,dph --5
                            ,convert(int,mnozstvi) as prijato --6
                            ,(select top 1 convert(int,pocetmj*baleni) from [OrdersManager].[dbo].[ObjVydPol] vp where charindex(vp.cisloobj,f.cisloobj,0)>0 and vp.kod_zbozi=p.kod_zbozi) as objednano   --7                    
                            from [OrdersManager].[dbo].[FakPol] p 
                            left join [OrdersManager].[dbo].[Faktury] f on p.cisloobj=f.cisloobj and p.casti=f.casti
                            where p.cisloobj='{inputOBJ}'
                            order by  p.id ";
            DataTable TB_erp = new DataTable();
            try
            {
                if (SendToFormMain == "TK - Hostivice" || SendToFormMain == "DC - Morava")
                {
                    TB_erp = DataProvider.Instance.ExecuteQuery(sqlERP_HOS);
                }
                else
                {
                    TB_erp = DataProvider.Instance.ExecuteQuery(sqlERP1);
                }

                if (TB_erp.Rows.Count >= 1)
                {
                    for (int i = 0; i < TB_erp.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_erp.Rows[i][3].ToString(),
                            nazev = TB_erp.Rows[i][4].ToString(),
                            prjato = TB_erp.Rows[i][6].ToString(),
                            objednano = TB_erp.Rows[i][7].ToString()

                        });
                    }
                    int cisloDodavatele = int.Parse(TB_erp.Rows[0][1].ToString().Substring(1, TB_erp.Rows[0][1].ToString().Length - 1));
                    string sql1 = $@"SELECT cislodod, nazev, ico, dic, street, city, zip    
                                 FROM [TDF Database].dbo.dodavatele 
                                 WHERE CONVERT(int,cislodod)='{cisloDodavatele}'";

                    DataTable TB_dodavatel_erp = DataProvider.Instance.ExecuteQuery(sql1);
                    if (TB_dodavatel_erp.Rows.Count >= 1)
                    {
                        cr12.SetDataSource(_List);
                        string cislodod = "D" + TB_dodavatel_erp.Rows[0][0].ToString();
                        string nazev = TB_dodavatel_erp.Rows[0][1].ToString();
                        string ico = "IČ: " + TB_dodavatel_erp.Rows[0][2].ToString();
                        string dic = "DIČ: " + TB_dodavatel_erp.Rows[0][3].ToString();
                        string street = TB_dodavatel_erp.Rows[0][4].ToString();
                        string city = TB_dodavatel_erp.Rows[0][6].ToString() + " " + TB_dodavatel_erp.Rows[0][5].ToString();
                        string obj = "Objednávka č." + inputOBJ;
                        string prijemka = "PŘÍJEMKA: " + TB_erp.Rows[0][2].ToString();
                        string datumPrijmuD = TB_dodavatel_erp.Rows[0][0].ToString();
                        Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        bar1.Draw(inputOBJ, 40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");

                        cr12.SetParameterValue("EAN", Application.StartupPath + "\\cache\\file_EAN_obj.png"); //location  
                        cr12.SetParameterValue("prijemka", prijemka);
                        cr12.SetParameterValue("obj", obj);
                        cr12.SetParameterValue("Dodavatel", nazev);
                        cr12.SetParameterValue("DodavatelCislo", cislodod);
                        cr12.SetParameterValue("cityD", city);
                        cr12.SetParameterValue("streetD", street);
                        cr12.SetParameterValue("icD", ico);
                        cr12.SetParameterValue("dicD", dic);
                        cr12.SetParameterValue("dicD", dic);
                        cr12.SetParameterValue("datumPrijmuD", datumPrijmuD);
                        // crystalReportViewer1.Zoom(80);
                    } 
                }
                else
                {
                    string sqlOracle1 = $@"select --ol_nligof as stt
                                           -- ,oe_numorc as cislo_stock
                                            to_char(oe_dtlivp,'dd.MM.yyyy') as datum_prijmu --0
                                           -- ,oe_ncdefo as prijemka
                                            ,oe_fourn as cislo_dodavatel --1
                                            ,oe_librs as dodavatel --2
                                            ,ol_cproin as kod_zbozi --3
                                            ,ar_libpro as nazev_zbozi  --4
                                           -- ,ol_qteup
                                            ,nvl(ol_qteuvc, 0) as objednano --5
                                            ,nvl(ol_uvcrec, 0) as prijato  --6
                                            ,to_number(nvl(ol_uvcrec, 0)) - to_number(nvl(ol_qteuvc, 0)) as rozdil
                                            --,ol_datmod as datum_modify
                                            from tb_lrec tb1 left
                                            join tb_erec tb2 on tb1.ol_cincde = tb2.oe_ncdefo

                                       left
                                            join tb_art tb3 on tb1.ol_cproin = tb3.ar_cproin
                                            where oe_ncdefo = '{inputOBJ}'
                                            order by to_number(ol_nligof)";
                    DataTable TB_oracle = new DataTable();
                    try
                    {
                        if (SendToFormMain == "TK - Hostivice")
                        {
                            TB_oracle = DataOracle.Instance.ExecuteQuery(sqlOracle1);
                        }
                        else
                        {
                            TB_oracle = DataOracleMorava.Instance.ExecuteQuery(sqlOracle1);
                        }
                        

                        if (TB_oracle.Rows.Count < 1)
                        {
                            MessageBox.Show("Không có đơn hàng này!");
                        }
                        else
                        {
                            for (int i = 0; i < TB_oracle.Rows.Count; i++)
                            {
                                _List.Add(new InvoiceDetail
                                {
                                    kod_zbozi = TB_oracle.Rows[i][3].ToString(),
                                    nazev = TB_oracle.Rows[i][4].ToString(),
                                    objednano = TB_oracle.Rows[i][5].ToString(),
                                    prjato = TB_oracle.Rows[i][6].ToString()
                                });
                            }
                            int cisloDodavatele = int.Parse(TB_oracle.Rows[0][1].ToString().Substring(1, TB_oracle.Rows[0][1].ToString().Length - 1));
                            string sql1 = $@"SELECT cislodod, nazev, ico, dic, street, city, zip    
                                 FROM [TDF Database].dbo.dodavatele 
                                 WHERE CONVERT(int,cislodod)='{cisloDodavatele}'";

                            DataTable TB = DataProvider.Instance.ExecuteQuery(sql1);
                            if (TB.Rows.Count >= 1)
                            {
                                cr12.SetDataSource(_List);
                                string cislodod = "D" + TB.Rows[0][0].ToString();
                                string nazev = TB.Rows[0][1].ToString();
                                string ico = "IČ: " + TB.Rows[0][2].ToString();
                                string dic = "DIČ: " + TB.Rows[0][3].ToString();
                                string street = TB.Rows[0][4].ToString();
                                string city = TB.Rows[0][6].ToString() + " " + TB.Rows[0][5].ToString();
                                string obj = "Objednávka č." + inputOBJ;
                                string prijemka = "PŘÍJEMKA: G" + inputOBJ;
                                string datumPrijmuD = TB_oracle.Rows[0][0].ToString();
                                Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                                bar1.Draw(inputOBJ, 40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");

                                cr12.SetParameterValue("EAN", Application.StartupPath + "\\cache\\file_EAN_obj.png"); //location  
                                cr12.SetParameterValue("prijemka", prijemka);
                                cr12.SetParameterValue("obj", obj);
                                cr12.SetParameterValue("Dodavatel", nazev);
                                cr12.SetParameterValue("DodavatelCislo", cislodod);
                                cr12.SetParameterValue("cityD", city);
                                cr12.SetParameterValue("streetD", street);
                                cr12.SetParameterValue("icD", ico);
                                cr12.SetParameterValue("dicD", dic);
                                cr12.SetParameterValue("dicD", dic);
                                cr12.SetParameterValue("datumPrijmuD", datumPrijmuD);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Xảy ra lỗi");
                    }
                }                
            }
            catch (Exception ex)
            {
                 MessageBox.Show(ex.ToString());
            }

        }      
        private void LoadPrijemky_od_cisloobj(string cisloobj_input = "")
        {
            string sqlLoadPrijemky_od_cisloobj = $@"SELECT      
                                      f.cislo as Cislo_prijemky
                                      ,f.cisloobj as Cislo_objednavky --  ,f.[Casti]      
                                      ,FORMAT(ISNULL(f.datum_prijemky,f.[datakt]),'dd.MM.yyyy HH:mm') as Datum_cas_prijemky 
                                      ,d.Nazev,f.casti
                                      ,IIF([dodavatelske_cislo] like 'DL%',[dodavatelske_cislo],'') as Cislo_dodaciho_listu	  
	                                  ,IIF([dodavatelske_cislo] not like 'DL%',[dodavatelske_cislo],'') as Cislo_faktury	 
	                                  --,ISNULL(f.interni_cislo_prijemky, f.cisloobj) as [Index]
                                  FROM [OrdersManager].[dbo].[Faktury] f
                                  LEFT JOIN [TDF Database].dbo.dodavatele d  ON d.cislodod = f.dodavatel
                                  WHERE f.cisloobj like '%{cisloobj_input}%' 
                                  AND datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),ISNULL(f.datum_prijemky,f.[datakt]))>=0
                                  AND datediff(day,ISNULL(f.datum_prijemky,f.[datakt]),convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104))>=0
                                  ORDER by ISNULL(f.datum_prijemky,f.[datakt]) desc, f.cislo ,f.cisloobj  ";
            DataTable TB = new DataTable();
            try
            {
                TB = DataProvider.Instance.ExecuteQuery(sqlLoadPrijemky_od_cisloobj);
                dataGridView1.DataSource = null;
                dataGridView1.Rows.Clear();
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Číslo příjemky";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Datum příjmu";
                    dataGridView1.Columns[3].HeaderText = "Název";
                    dataGridView1.Columns[4].HeaderText = "Části";
                    dataGridView1.Columns[5].HeaderText = "Dodací list";
                    dataGridView1.Columns[6].HeaderText = "Faktura";
                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    dataGridView1.Columns.Add(btn);
                    btn.HeaderText = "Details";
                    btn.Text = "Detail";
                    btn.Name = "btn";
                    btn.UseColumnTextForButtonValue = true;
                    for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                    {
                        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    }
                    //dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    label3.Text = $"Rows count: {dataGridView1.Rows.Count}";
                }
                else
                {
                    //  MessageBox.Show("Không có đơn hàng nào!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void pictureBox4_Click(object sender, EventArgs e) // picture in
        {
            crystalReportViewer1.PrintReport();
        }
        private void pictureBox3_Click(object sender, EventArgs e) // picture tim kiem xanh la
        {
            //LoadPrijemky_od_cisloobj(placeHolderTextBox1.Text);
            //MessageBox.Show($"Có {dataGridView1.Rows.Count} đơn hàng");
        }
        private void pictureBox1_Click_1(object sender, EventArgs e) // picture tim kiem xanh duong
        {
            LoadPrijemky_od_cisloobj();
            MessageBox.Show($"Có {dataGridView1.Rows.Count} đơn hàng");
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 7)
            {
                try
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row = dataGridView1.Rows[e.RowIndex];
                    textBox2.Text = row.Cells[0].Value.ToString();
                    textBox1.Text = row.Cells[1].Value.ToString();                     
                    RunAsync();
                }
                catch (Exception)
                {

                }
            }
        }
        private void placeHolderTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadPrijemky_od_cisloobj(placeHolderTextBox1.Text);
            }
        }
        private void placeHolderTextBox1_TextChanged(object sender, EventArgs e)
        {
            //if (placeHolderTextBox1.TextLength >= 4)
            //{
            //    LoadPrijemky_od_cisloobj(placeHolderTextBox1.Text);
            //}
        }      
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    RunAsync();
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
   
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();
                row = dataGridView1.Rows[e.RowIndex];
                textBox2.Text = row.Cells[0].Value.ToString();
                textBox1.Text = row.Cells[1].Value.ToString();               
            }
            catch (Exception)
            {
                textBox2.Text = "0";
                textBox1.Text = "0";
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            RunAsync();
        }
        private void RunAsync()
        {
            if (!worker.IsBusy)
            {
                crystalReportViewer1.ReportSource = null;
                progressBar1.Visible = true;
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = 1;
                worker.RunWorkerAsync(); 
            }
        }
    }
}
