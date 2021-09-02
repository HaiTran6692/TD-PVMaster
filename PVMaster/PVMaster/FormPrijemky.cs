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
    public partial class FormPrijemky : Form
    {
        private BackgroundWorker worker = null;
        CrystalReport_Prijemky cr12 = new CrystalReport_Prijemky();
        List<InvoiceDetail> _List = new List<InvoiceDetail>();
        string datumPrijmu = "";
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
        public string SendToFormPrijemka
        {
            get { return sendToFormMain; }
            set { sendToFormMain = value; }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //this.Text = "PVMaster v2a.280521 "+ SendToFormMain.ToString();
            //this.Text = "PVMaster v2b.230621 "+ SendToFormMain.ToString();
            //this.Text = "PVMaster v2c.240621 " + SendToFormMain.ToString();
            //this.Text = "PVMaster v2d.290621 Prijemky " + SendToFormMain.ToString();
            //this.Text = "PVMaster v3b.140721 Prijemky " + SendToFormMain.ToString();

            this.Text = "PVMaster v3c.170721 Prijemky " + SendToFormPrijemka.ToString();
            this.WindowState = FormWindowState.Maximized;
            dateTimePicker1from.Format = DateTimePickerFormat.Custom;
            dateTimePicker1from.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1from.Value = DateTime.Today.AddDays(0);

            dateTimePicker2to.Format = DateTimePickerFormat.Custom;
            dateTimePicker2to.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2to.Value = DateTime.Today;

            if (SendToFormPrijemka=="TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
            {
                LoadPrijemkyGold();
            }
            else
            {
                LoadPrijemkyBarco();
            }
            
        } 
        private void LoadPrijemkyGold(string cislObj ="")
        {
            string sqlLoadPrijemky = $@"SELECT        oe_ncdefo AS prijemka,
                                                      oe_ncdefo AS order_num,
                                                      oe_librs   AS dodavatel,
                                                      CASE WHEN ma_starr = 0 THEN 'INITIALISE'
                                                           WHEN ma_starr = 1 THEN 'ON DOCK'
                                                           WHEN ma_starr = 2 THEN 'Kontroluje'
                                                           WHEN ma_starr = 3 THEN 'HOTOVO!' END as Status,
                                                      TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'DD.MM.YYYY HH24:MI') AS Recep_date,                                                     
                                                      ml_numarr AS consignment_num,                                                       
                                                      substr(es_adrqre, 5, 2) AS rampa
                                          FROM  tb_lrdv left join tb_erdv ON ml_numarr = ma_numarr
                                          LEFT join tb_erec ON ml_numorc = oe_numorc
                                          LEFT JOIN tb_eslrec ON ml_numarr = es_numarr
                                          WHERE ma_starr !=0 
                                                AND TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMdd') >= '{dateTimePicker1from.Value.ToString("yyyyMMdd")}'
                                                AND TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMdd') <= '{dateTimePicker2to.Value.ToString("yyyyMMdd")}'
                                                AND oe_ncdefo LIKE '%{cislObj}%'
                                          ORDER BY TO_CHAR(nvl(ma_dtrecr, ma_datrec), 'YYYYMMDDHH24MISS') DESC";
            DataTable TB = new DataTable();
            try
            {
                if (SendToFormPrijemka == "TK - Hostivice")
                {
                    TB = DataOracle.Instance.ExecuteQuery(sqlLoadPrijemky);
                }
                else
                {
                    TB = DataOracleMorava.Instance.ExecuteQuery(sqlLoadPrijemky);
                }

                dataGridView1.Columns.Clear();
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Příjemka";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Dodavatel";
                    dataGridView1.Columns[3].HeaderText = "Stav";
                    dataGridView1.Columns[4].HeaderText = "Čas příjmu";
                    dataGridView1.Columns[5].HeaderText = "Consignment";
                    dataGridView1.Columns[6].HeaderText = "Rampa";
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
                    label3.Text = $"Počet objednávek: {dataGridView1.Rows.Count}";
                }
                else
                {
                    MessageBox.Show("Počet objednávek: 0");
                    label3.Text = "Počet objednávek: 0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Počet objednávek: 0");
                label3.Text = "Počet objednávek: 0";
            }

        }
        private void LoadPrijemkyBarco(string cisloObj="")
        {
            string sqlLoadPrijemkyBarco = $@"SELECT 
                                           [ObjednavkaID] as Prijemka --0
	                                      ,[ObjednavkaID] as Objednavka--1
	                                      ,[nazev]--2
	                                      ,case when ReceivingDate is null then 'Kontroluje' else 'HOTOVO!' end as Stav_prijmu--3   
                                          ,Format(isnull(ReceivingDate, datum),'dd.MM.yyyy HH:mm') as Receiving_Date--4 
                                          ,[Stav]       --5
                                          ,[DokladID]--6                                               
                                      FROM [OrdersManager].[dbo].[ViewBarcoReceiving] 
                                      WHERE datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),[datum])>=0
                                        AND datediff(day,convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104),[datum])<=0
                                        AND [ObjednavkaID] like '%{cisloObj}%'
                                      ORDER by isnull(ReceivingDate, datum) desc";
            DataTable TB = new DataTable();
            try
            {
                TB = DataProvider.Instance.ExecuteQuery(sqlLoadPrijemkyBarco);
                dataGridView1.Columns.Clear();
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Příjemka";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Dodavatel";
                    dataGridView1.Columns[3].HeaderText = "Stav";
                    dataGridView1.Columns[4].HeaderText = "Čas příjmu";
                    dataGridView1.Columns[5].HeaderText = "Stav č.";
                    dataGridView1.Columns[6].HeaderText = "DokladID";
                   
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
                    label3.Text = $"Počet objednávek: {dataGridView1.Rows.Count}";
                }
                else
                {
                    MessageBox.Show("Počet objednávek: 0");
                    label3.Text = "Počet objednávek: 0";
                }
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("Počet objednávek: 0");
                label3.Text = "Počet objednávek: 0";
            }


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
                if (TB.Rows.Count>0)
                {
                    _load_V_kus = decimal.Parse(TB.Rows[0][1].ToString()) * decimal.Parse(slg) / 1000 / 1000 / 1000;
                }
                else
                {
                    _load_V_kus =decimal.Parse(slg)/977; //97.7mm            
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
                    _load_m_kus =  decimal.Parse(slg)*97/1000; //97gram  
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return _load_m_kus;
        }
        private string LoadCenaVyd(string inputOBJ, string input_mnb)
        {
            string _cena = "0";
            string sqlCenaObjPri = $@"SELECT   [cena],[cisloobj],[kod_zbozi]                                              
                                      FROM [OrdersManager].[dbo].[ObjVydPol]
                                      where cisloobj='{inputOBJ}' and [kod_zbozi]='{input_mnb}'";
            try
            {
                DataTable TB = DataProvider.Instance.ExecuteQuery(sqlCenaObjPri);
                if (TB.Rows.Count>0)
                {
                    _cena = TB.Rows[0][0].ToString();
                }
                else
                {
                    _cena = "0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                _cena = "0";
            }
            return _cena;

        }
        private void LoadDetailGold(string inputOBJ)
        {
            string sqlOracle1 = $@"select    to_char(oe_dtlivp,'dd.MM.yyyy') as datum_prijmu --0
                                           -- oe_ncdefo as prijemka--0
                                            ,oe_fourn as cislo_dodavatel --1
                                            ,oe_librs as dodavatel --2
                                            ,ol_cproin as kod_zbozi --3
                                            ,ar_libpro as nazev_zbozi  --4                                         
                                            ,nvl(ol_qteuvc, 0) as objednano --5
                                            ,nvl(ol_uvcrec, 0) as prijato  --6
                                            ,to_number(nvl(ol_uvcrec, 0)) - to_number(nvl(ol_qteuvc, 0)) as rozdil
                                           
                                            from tb_lrec tb1 left join tb_erec tb2 on tb1.ol_cincde = tb2.oe_ncdefo
                                                             left join tb_art tb3 on tb1.ol_cproin = tb3.ar_cproin
                                            where oe_ncdefo = '{inputOBJ}'
                                            order by to_number(ol_nligof)";            
            DataTable TB_gold = new DataTable();
            try
            {
                if (SendToFormPrijemka == "TK - Hostivice")
                {
                    TB_gold = DataOracle.Instance.ExecuteQuery(sqlOracle1);                    
                }
                else
                {
                    TB_gold = DataOracleMorava.Instance.ExecuteQuery(sqlOracle1);
                }


                if (TB_gold.Rows.Count < 1)
                {
                    //MessageBox.Show("Không có đơn hàng này!");
                }
                else
                {
                    decimal celkem_V = 0;
                    decimal celkem_m = 0;

                    for (int i = 0; i < TB_gold.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_gold.Rows[i][3].ToString(),
                            nazev = TB_gold.Rows[i][4].ToString(),
                            objednano = TB_gold.Rows[i][5].ToString(),
                            prjato = TB_gold.Rows[i][6].ToString(),
                            cena_bez = LoadCenaVyd(inputOBJ, TB_gold.Rows[i][3].ToString())      
                        }) ;
                        celkem_V += Load_V_kus(TB_gold.Rows[i][3].ToString(), TB_gold.Rows[i][6].ToString());
                        celkem_m += Load_m_kus(TB_gold.Rows[i][3].ToString(), TB_gold.Rows[i][6].ToString());
                    }
                    int cisloDodavatele = int.Parse(TB_gold.Rows[0][1].ToString().Substring(1, TB_gold.Rows[0][1].ToString().Length - 1));
                    string sql1 = $@"SELECT cislodod, nazev, ico, dic, street, city, zip    
                                 FROM [TDF Database].dbo.dodavatele 
                                 WHERE CONVERT(int,cislodod)={cisloDodavatele} ";

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
                        cr12.SetParameterValue("datumPrijmuD", datumPrijmu);
                        cr12.SetParameterValue("celk_hm", celkem_m.ToString("0.000"));
                        cr12.SetParameterValue("celk_objem", celkem_V.ToString("0.000"));
                        string _td_infor_name = @"Dodavatel:
		                                                     TAMDA FOODS s.r.o.
                                                             Libušská 319/126
		                                                     142 00 PRAHA 411-LIBUŠ";
                        cr12.SetParameterValue("TD_infor_name", _td_infor_name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void LoadDetailBarco(string inputOBJ)
        {
            string loadDetailBarco = $@"SELECT
                        (SELECT TOP(1)[dodavatel] FROM[OrdersManager].[dbo].[ObjVyd] where cisloobj = vp.cisloobj) as cislo_dodavatel --0
                        ,cisloobj --1
                        ,kod_zbozi --2
                        ,nazev --3
                        ,convert(int,pocetmj*baleni) as objednano --4
                        ,isnull(Quantity, 0) as prijato --5
                        ,MeasureUnit,FORMAT(er.ReceivingDate,'dd.MM.yyyy') as datumPrijmu
                        ,LocationID
                          FROM[OrdersManager].[dbo].[ObjVydPol] vp
                        left join TamdaBarco.dbo.ExpReceiving er on vp.cisloobj = er.OrderID
                        left join TamdaBarco.dbo.ExpReceivingItem eri on er.ID = eri.IDReceiving and vp.kod_zbozi = eri.ArticleID
                        where vp.cisloobj = '{inputOBJ}'";          
            DataTable TB_barco = new DataTable();
            try
            {
                TB_barco = DataProvider.Instance.ExecuteQuery(loadDetailBarco); 

                if (TB_barco.Rows.Count < 1)
                {
                  // MessageBox.Show("Không có đơn hàng này!");
                }
                else
                {
                    decimal celkem_V = 0;
                    decimal celkem_m = 0;

                    for (int i = 0; i < TB_barco.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_barco.Rows[i][2].ToString(),
                            nazev = TB_barco.Rows[i][3].ToString(),
                            objednano = TB_barco.Rows[i][4].ToString(),
                            prjato = TB_barco.Rows[i][5].ToString(),
                            cena_bez = LoadCenaVyd(inputOBJ, TB_barco.Rows[i][2].ToString())
                        });
                        celkem_V += Load_V_kus(TB_barco.Rows[i][2].ToString(), TB_barco.Rows[i][5].ToString());
                        celkem_m += Load_m_kus(TB_barco.Rows[i][2].ToString(), TB_barco.Rows[i][5].ToString());
                    }
                    int cisloDodavatele = int.Parse(TB_barco.Rows[0][0].ToString());
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
                        string prijemka = "PŘÍJEMKA: B" + inputOBJ;
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
                        cr12.SetParameterValue("datumPrijmuD", datumPrijmu);
                        cr12.SetParameterValue("celk_hm", celkem_m.ToString("0.000"));
                        cr12.SetParameterValue("celk_objem", celkem_V.ToString("0.000"));
                        string _td_infor_name = @"Dodavatel:
		                                                     TAMDA FOODS s.r.o.
                                                             Libušská 319/126
		                                                     142 00 PRAHA 411-LIBUŠ";
                        cr12.SetParameterValue("TD_infor_name", _td_infor_name);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void LoadReportDetail(string inputOBJ, string inputPrijemka)
        {
            // ktra checkInvoice đã làm xong chưa 
            // nếu xong rồi thì lấy dữ liệu sau checkInvoice
            // nếu chưa xong thì lấy thẳng dữ liệu GOLD hoặc Barco

            _List.Clear();
            string sqlERP_Sapa = $@"select 
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
                            where f.cisloobj='{inputPrijemka}'
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
                if (SendToFormPrijemka == "TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
                {
                    TB_erp = DataProvider.Instance.ExecuteQuery(sqlERP_HOS);
                }
                else
                {
                    TB_erp = DataProvider.Instance.ExecuteQuery(sqlERP_Sapa);
                }

                if (TB_erp.Rows.Count >= 1)
                {
                    decimal celkem_V = 0;
                    decimal celkem_m = 0;

                    for (int i = 0; i < TB_erp.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_erp.Rows[i][3].ToString(),
                            nazev = TB_erp.Rows[i][4].ToString(),
                            objednano = TB_erp.Rows[i][7].ToString(),
                            prjato = TB_erp.Rows[i][6].ToString(),
                            cena_bez = LoadCenaVyd(inputOBJ, TB_erp.Rows[i][3].ToString())
                    }) ;
                        celkem_V += Load_V_kus(TB_erp.Rows[i][3].ToString(), TB_erp.Rows[i][6].ToString());
                        celkem_m += Load_m_kus(TB_erp.Rows[i][3].ToString(), TB_erp.Rows[i][6].ToString());
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
                        cr12.SetParameterValue("datumPrijmuD", datumPrijmu);
                        cr12.SetParameterValue("celk_hm", celkem_m.ToString("0.000"));
                        cr12.SetParameterValue("celk_objem", celkem_V.ToString("0.000"));
                        string _td_infor_name = @"Dodavatel:
		                                                     TAMDA FOODS s.r.o.
                                                             Libušská 319/126
		                                                     142 00 PRAHA 411-LIBUŠ";
                        cr12.SetParameterValue("TD_infor_name", _td_infor_name);

                        // crystalReportViewer1.Zoom(80);
                    }
                }
                else
                {
                    if (SendToFormPrijemka == "TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
                    {
                        LoadDetailGold(inputOBJ);
                    }
                    else
                    {
                        LoadDetailBarco(inputOBJ);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }       
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {           
            LoadReportDetail(textBox1.Text, textBox2.Text);           
        }       
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            progressBar1.Visible = false;
            if (cr12.Parameter_Dodavatel!= null)
            {
                crystalReportViewer1.ReportSource = cr12;
            }
        } 
        private void pictureBox4_Click(object sender, EventArgs e) // picture in
        {
            crystalReportViewer1.PrintReport();
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
                    datumPrijmu = row.Cells[4].Value.ToString();
                    RunDetail_Async();
                }
                catch (Exception)
                {

                }
            }
        }       
        private void placeHolderTextBox1_TextChanged(object sender, EventArgs e)
        {
            //if (placeHolderTextBox1.TextLength >= 4)
            //{
            //    LoadPrijemkyBarco(placeHolderTextBox1.Text);
            //}
        }      
        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    RunDetail_Async();
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
                datumPrijmu = row.Cells[4].Value.ToString();
            }
            catch (Exception)
            {
                textBox2.Text = "0";
                textBox1.Text = "0";
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            RunDetail_Async();
        }
        private void RunDetail_Async()
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
        private void pictureBox3_Click(object sender, EventArgs e) //tìm kiếm theo ngày
        {
            if (SendToFormPrijemka == "TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
            {
                LoadPrijemkyGold();
            }
            else
            {
                LoadPrijemkyBarco();
            }
            MessageBox.Show($"Počet objednávek: {dataGridView1.Rows.Count}");
        }
        private void pictureBox1_Click_1(object sender, EventArgs e) // tìm kiếm theo cislo obj
        {
            if (placeHolderTextBox1.TextLength>=5)
            {
                if (SendToFormPrijemka == "TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
                {
                    LoadPrijemkyGold(placeHolderTextBox1.Text);
                }
                else
                {
                    LoadPrijemkyBarco(placeHolderTextBox1.Text);
                } 
            }
            else
            {
                MessageBox.Show("Zadejte min. 5 čísel!");
            }
        }
        private void placeHolderTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Enter)
            {
                if (placeHolderTextBox1.TextLength >= 5)
                {
                    if (SendToFormPrijemka == "TK - Hostivice" || SendToFormPrijemka == "DC - Morava")
                    {
                        LoadPrijemkyGold(placeHolderTextBox1.Text);
                    }
                    else
                    {
                        LoadPrijemkyBarco(placeHolderTextBox1.Text);
                    }
                }
                else
                {
                    MessageBox.Show("Zadejte min. 5 čísel!");
                }
            }
        }

        //string sqlLoadPrijemkyBarco_old = $@"SELECT      
        //                          f.cislo as Cislo_prijemky
        //                          ,f.cisloobj as Cislo_objednavky --  ,f.[Casti]      
        //                          ,FORMAT(ISNULL(f.datum_prijemky,f.[datakt]),'dd.MM.yyyy HH:mm') as Datum_cas_prijemky 
        //                          ,d.Nazev,f.casti
        //                          ,IIF([dodavatelske_cislo] like 'DL%',[dodavatelske_cislo],'') as Cislo_dodaciho_listu	  
        //                       ,IIF([dodavatelske_cislo] not like 'DL%',[dodavatelske_cislo],'') as Cislo_faktury	 
        //                       --,ISNULL(f.interni_cislo_prijemky, f.cisloobj) as [Index]
        //                      FROM [OrdersManager].[dbo].[Faktury] f
        //                      LEFT JOIN [TDF Database].dbo.dodavatele d  ON d.cislodod = f.dodavatel
        //                      WHERE --f.cisloobj like '%{cisloobj_input}%' AND 
        //                    datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),ISNULL(f.datum_prijemky,f.[datakt]))>=0
        //                      AND datediff(day,ISNULL(f.datum_prijemky,f.[datakt]),convert(datetime,'{dateTimePicker2to.Value.ToString("dd.MM.yyyy")}',104))>=0
        //                      ORDER by ISNULL(f.datum_prijemky,f.[datakt]) desc, f.cislo ,f.cisloobj";

    }
}
