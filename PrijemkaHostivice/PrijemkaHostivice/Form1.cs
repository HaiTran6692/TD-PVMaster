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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            dateTimePicker1from.Format = DateTimePickerFormat.Custom;
            dateTimePicker1from.CustomFormat = "dd.MM.yyyy";
            dateTimePicker1from.Value = DateTime.Today;

            dateTimePicker2to.Format = DateTimePickerFormat.Custom;
            dateTimePicker2to.CustomFormat = "dd.MM.yyyy";
            dateTimePicker2to.Value = DateTime.Today;

            
            LoadPrijemky();
        }
        List<InvoiceDetail> _List = new List<InvoiceDetail>();
        private void placeHolderTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TimTheoSoDon(placeHolderTextBox1.Text);
            }
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            TimTheoSoDon(placeHolderTextBox1.Text);
        }
        private void TimTheoSoDon(string inputOBJ)
        {
            crystalReportViewer1.ReportSource = null;
            _List.Clear();
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
                TB_oracle = DataOracle.Instance.ExecuteQuery(sqlOracle1);
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
                        CrystalReport2 cr12 = new CrystalReport2();
                        cr12.SetDataSource(_List);                       
                        string cislodod = "D" + TB.Rows[0][0].ToString();
                        string nazev = TB.Rows[0][1].ToString();
                        string ico = "IČ: " + TB.Rows[0][2].ToString();
                        string dic = "DIČ: " + TB.Rows[0][3].ToString();
                        string street = TB.Rows[0][4].ToString();
                        string city = TB.Rows[0][6].ToString() + " " + TB.Rows[0][5].ToString();
                        string obj = "Objednávka č."+inputOBJ;
                        string prijemka = "PŘÍJEMKA: "+inputOBJ;
                        string datumPrijmuD = TB_oracle.Rows[0][0].ToString();
                        Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        bar1.Draw(inputOBJ,40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");

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
                       
                        crystalReportViewer1.ReportSource = cr12;
                    }
                }
            }
            catch (Exception )
            {
                MessageBox.Show("Xảy ra lỗi") ;
            }

        }    
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.PrintReport();
        }
        private void LoadPrijemky()
        {
            string sqlLoadPrijemky = $@"SELECT        --substr(es_adrqre, 5, 2) AS rampa,
                                                      oe_ncdefo AS order_num,
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
                                                        ml_numorc AS stock_num
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
                TB = DataOracle.Instance.ExecuteQuery(sqlLoadPrijemky);
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.DataSource = TB;                    
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
        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            LoadPrijemky();
            MessageBox.Show($"Có {dataGridView1.Rows.Count} đơn hàng");
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TimTheoSoDon(textBox1.Text);
        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();               
                row = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = row.Cells[0].Value.ToString();               
            }
            catch (Exception)
            {

            }
        }
    }
}
