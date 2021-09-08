namespace PVMaster
{
    partial class FormLogin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormLogin));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.comboBox1Branch = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.materialButton1 = new MaterialSkin.Controls.MaterialButton();
            this.radioButton1_Prijemky = new System.Windows.Forms.RadioButton();
            this.radioButton2_Vydejky = new System.Windows.Forms.RadioButton();
            this.radioButton3_Sklad = new System.Windows.Forms.RadioButton();
            this.radioButton4_Zamest = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(373, 125);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 14);
            this.label1.TabIndex = 0;
            this.label1.Text = "User";
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox1.Location = new System.Drawing.Point(410, 122);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(147, 22);
            this.textBox1.TabIndex = 1;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(349, 153);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 14);
            this.label2.TabIndex = 0;
            this.label2.Text = "Password";
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBox2.Location = new System.Drawing.Point(410, 148);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(147, 22);
            this.textBox2.TabIndex = 2;
            this.textBox2.UseSystemPasswordChar = true;
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyDown_1);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(90, 122);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(186, 104);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // comboBox1Branch
            // 
            this.comboBox1Branch.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1Branch.FormattingEnabled = true;
            this.comboBox1Branch.Location = new System.Drawing.Point(410, 175);
            this.comboBox1Branch.Name = "comboBox1Branch";
            this.comboBox1Branch.Size = new System.Drawing.Size(147, 21);
            this.comboBox1Branch.TabIndex = 22;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Calibri", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(361, 177);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 14);
            this.label3.TabIndex = 21;
            this.label3.Text = "Branch";
            // 
            // materialButton1
            // 
            this.materialButton1.AutoSize = false;
            this.materialButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.materialButton1.Depth = 0;
            this.materialButton1.DrawShadows = true;
            this.materialButton1.HighEmphasis = true;
            this.materialButton1.Icon = null;
            this.materialButton1.Location = new System.Drawing.Point(406, 266);
            this.materialButton1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.materialButton1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialButton1.Name = "materialButton1";
            this.materialButton1.Size = new System.Drawing.Size(147, 28);
            this.materialButton1.TabIndex = 23;
            this.materialButton1.Text = "Login";
            this.materialButton1.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.materialButton1.UseAccentColor = false;
            this.materialButton1.UseVisualStyleBackColor = true;
            this.materialButton1.Click += new System.EventHandler(this.materialButton1_Click);
            // 
            // radioButton1_Prijemky
            // 
            this.radioButton1_Prijemky.AutoSize = true;
            this.radioButton1_Prijemky.Checked = true;
            this.radioButton1_Prijemky.Location = new System.Drawing.Point(413, 203);
            this.radioButton1_Prijemky.Name = "radioButton1_Prijemky";
            this.radioButton1_Prijemky.Size = new System.Drawing.Size(67, 17);
            this.radioButton1_Prijemky.TabIndex = 24;
            this.radioButton1_Prijemky.TabStop = true;
            this.radioButton1_Prijemky.Text = "Příjemky";
            this.radioButton1_Prijemky.UseVisualStyleBackColor = true;
            // 
            // radioButton2_Vydejky
            // 
            this.radioButton2_Vydejky.AutoSize = true;
            this.radioButton2_Vydejky.Location = new System.Drawing.Point(491, 203);
            this.radioButton2_Vydejky.Name = "radioButton2_Vydejky";
            this.radioButton2_Vydejky.Size = new System.Drawing.Size(62, 17);
            this.radioButton2_Vydejky.TabIndex = 24;
            this.radioButton2_Vydejky.Text = "Výdejky";
            this.radioButton2_Vydejky.UseVisualStyleBackColor = true;
            // 
            // radioButton3_Sklad
            // 
            this.radioButton3_Sklad.AutoSize = true;
            this.radioButton3_Sklad.Location = new System.Drawing.Point(413, 230);
            this.radioButton3_Sklad.Name = "radioButton3_Sklad";
            this.radioButton3_Sklad.Size = new System.Drawing.Size(52, 17);
            this.radioButton3_Sklad.TabIndex = 24;
            this.radioButton3_Sklad.Text = "Sklad";
            this.radioButton3_Sklad.UseVisualStyleBackColor = true;
            // 
            // radioButton4_Zamest
            // 
            this.radioButton4_Zamest.AutoSize = true;
            this.radioButton4_Zamest.Location = new System.Drawing.Point(489, 230);
            this.radioButton4_Zamest.Name = "radioButton4_Zamest";
            this.radioButton4_Zamest.Size = new System.Drawing.Size(86, 17);
            this.radioButton4_Zamest.TabIndex = 24;
            this.radioButton4_Zamest.Text = "Zaměstnanci";
            this.radioButton4_Zamest.UseVisualStyleBackColor = true;
            // 
            // FormLogin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.ClientSize = new System.Drawing.Size(661, 340);
            this.Controls.Add(this.radioButton2_Vydejky);
            this.Controls.Add(this.radioButton4_Zamest);
            this.Controls.Add(this.radioButton3_Sklad);
            this.Controls.Add(this.radioButton1_Prijemky);
            this.Controls.Add(this.materialButton1);
            this.Controls.Add(this.comboBox1Branch);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "PVMaster v4a.08092021 Login";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLogin_FormClosed);
            this.Load += new System.EventHandler(this.FormLogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ComboBox comboBox1Branch;
        private System.Windows.Forms.Label label3;
        private MaterialSkin.Controls.MaterialButton materialButton1;
        private System.Windows.Forms.RadioButton radioButton1_Prijemky;
        private System.Windows.Forms.RadioButton radioButton2_Vydejky;
        private System.Windows.Forms.RadioButton radioButton3_Sklad;
        private System.Windows.Forms.RadioButton radioButton4_Zamest;
    }
}