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
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            //this.WindowState = FormWindowState.Maximized;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
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
                TimTheoSoDon(placeHolderTextBox1.Text, placeHolderTextBox2.Text);
            }
        }
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            TimTheoSoDon(placeHolderTextBox1.Text, placeHolderTextBox2.Text);
        }
        private void TimTheoSoDon(string inputOBJ, string inputPrijemka)
        {
            crystalReportViewer1.ReportSource = null;
            _List.Clear();
            string sqlERP1 = $@"select 
                            format(p.created,'dd.MM.yyyy') as datum_prijmu --0
                            ,f.dodavatel--1
                            ,f.cislo--2
                            ,kod_zbozi--3
                            ,nazev--4
                            ,dph --5
                            ,convert(int,mnozstvi) as prijato --6
                            from [OrdersManager].[dbo].[FakPol] p 
                            left join [OrdersManager].[dbo].[Faktury] f on p.cisloobj=f.cisloobj
                            where p.cisloobj='{inputOBJ}' and f.cislo='{inputPrijemka}'
                            order by  p.id ";
            DataTable TB_erp = new DataTable();
            try
            {
                TB_erp = DataProvider.Instance.ExecuteQuery(sqlERP1);
                if (TB_erp.Rows.Count < 1)
                {
                    MessageBox.Show("Không có đơn hàng này!");
                }
                else
                {
                    for (int i = 0; i < TB_erp.Rows.Count; i++)
                    {
                        _List.Add(new InvoiceDetail
                        {
                            kod_zbozi = TB_erp.Rows[i][3].ToString(),
                            nazev = TB_erp.Rows[i][4].ToString(),
                            prjato = TB_erp.Rows[i][5].ToString(),
                            objednano = TB_erp.Rows[i][6].ToString() //dph
                        });
                    }
                    int cisloDodavatele = int.Parse(TB_erp.Rows[0][1].ToString().Substring(1, TB_erp.Rows[0][1].ToString().Length - 1));
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
                        string obj = "Objednávka č." + inputOBJ;
                        string prijemka = "PŘÍJEMKA: " + inputPrijemka;
                        string datumPrijmuD = TB_erp.Rows[0][0].ToString();
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

                        crystalReportViewer1.ReportSource = cr12;
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Xảy ra lỗi");
            }

        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            crystalReportViewer1.PrintReport();
        }
        private void LoadPrijemky()
        {
            string sqlLoadPrijemky = $@"SELECT      
                                      f.cislo as Cislo_prijemky
                                      ,f.cisloobj as Cislo_objednavky --  ,f.[Casti]      
                                      ,FORMAT(ISNULL(f.datum_prijemky,f.[datakt]),'dd.MM.yyyy HH:mm') as Datum_cas_prijemky 
                                      ,d.Nazev
                                      ,IIF([dodavatelske_cislo] like 'DL%',[dodavatelske_cislo],'') as Cislo_dodaciho_listu	  
	                                  ,IIF([dodavatelske_cislo] not like 'DL%',[dodavatelske_cislo],'') as Cislo_faktury	 
	                                  ,ISNULL(f.interni_cislo_prijemky, f.cisloobj) as [Index]
                                  FROM [OrdersManager].[dbo].[Faktury] f
                                  LEFT JOIN [TDF Database].dbo.dodavatele d  ON d.cislodod = f.dodavatel
                                  WHERE  datediff(day,convert(datetime,'{dateTimePicker1from.Value.ToString("dd.MM.yyyy")}',104),ISNULL(f.datum_prijemky,f.[datakt]))>=0
                                  ORDER by f.cislo ,f.cisloobj  ";
            DataTable TB = new DataTable();
            try
            {
                TB = DataProvider.Instance.ExecuteQuery(sqlLoadPrijemky);
                if (TB.Rows.Count > 0)
                {
                    dataGridView1.DataSource = TB;
                    dataGridView1.Columns[0].HeaderText = "Číslo příjemky";
                    dataGridView1.Columns[1].HeaderText = "Číslo objednávky";
                    dataGridView1.Columns[2].HeaderText = "Datum příjmu";
                    dataGridView1.Columns[3].HeaderText = "Název";
                    dataGridView1.Columns[4].HeaderText = "Dodací list";
                    dataGridView1.Columns[5].HeaderText = "Faktura";
                    dataGridView1.Columns[6].HeaderText = "Index";

                    for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                    {
                        dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                    }
                    dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;


                }
                else
                {
                    MessageBox.Show("Không có đơn hàng nào!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }
        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            LoadPrijemky();
            MessageBox.Show($"Có {dataGridView1.Rows.Count} đơn hàng");
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TimTheoSoDon(textBox1.Text, textBox2.Text);
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

            }
        }
    }
}
