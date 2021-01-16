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
        }
        List<InvoiceDetail> _List = new List<InvoiceDetail>();
        private void placeHolderTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TimTheoSoDon();
            }
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            TimTheoSoDon();
        }
        private void TimTheoSoDon()
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
                                            where oe_ncdefo = '{placeHolderTextBox1.Text}'
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
                        string obj = "Objednávka č."+placeHolderTextBox1.Text;
                        string prijemka = "PŘÍJEMKA: "+placeHolderTextBox1.Text;
                        string datumPrijmuD = TB_oracle.Rows[0][0].ToString();
                        Zen.Barcode.Code128BarcodeDraw bar1 = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;
                        bar1.Draw(placeHolderTextBox1.Text,40).Save(Application.StartupPath + "\\cache\\file_EAN_obj.png");

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

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.PrintReport();
        }
    }
}
