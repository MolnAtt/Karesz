using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace Karesz
{
    partial class Form1
    {

        #region Robot

        static readonly int várakozás = 5;

        //pálya tartalom
        static readonly int üres = 0;
        static readonly int fal = 1;
        static readonly int nincs_pálya = -1;

        //irány
        static readonly int észak = 0;
        static readonly int kelet = 1;
        static readonly int dél = 2;
        static readonly int nyugat = 3;

        //forgásirány
        static readonly int jobbra = 1;
        static readonly int balra = -1;

        //színek                
        static readonly int fekete = 2;
        static readonly int piros = 3;
        static readonly int zöld = 4;
        static readonly int sárga = 5;

        static robot Karesz;
        static bool megy_a_robot = false;
        Random véletlen = new Random();

        void Lépj()
        {
            Thread.Sleep(várakozás);
            Karesz.Lép();
        }

        void Fordulj_jobbra()
        {
            Fordulj(jobbra);
        }

        void Fordulj_balra()
        {
            Fordulj(balra);
        }

        void Fordulj(int irány)
        {
            Thread.Sleep(várakozás);
            Karesz.Fordul(irány);
        }

        void Vegyél_fel_egy_kavicsot()
        {
            Karesz.Felvesz();
        }

        void Tegyél_le_egy_kavicsot()
        {
            Karesz.Lerak(fekete);
        }

        void Tegyél_le_egy_kavicsot(int szín)
        {
            Karesz.Lerak(szín);
        }

        bool Északra_néz()
        {
            return Merre_néz() == észak;
        }

        bool Délre_néz()
        {
            return Merre_néz() == dél;
        }

        bool Keletre_néz()
        {
            return Merre_néz() == kelet;
        }

        bool Nyugatra_néz()
        {
            return Merre_néz() == nyugat;
        }

        int Merre_néz()
        {
            return Karesz.MerreNéz();
        }

        bool Van_e_itt_kavics()
        {
            return Karesz.VanKavics();
        }

        int Mi_van_alattam()
        {
            return Karesz.MiVanItt();
        }

        bool Van_e_előttem_fal()
        {
            return Karesz.MiVanElőttem() == 1;
        }

        bool Kilépek_e_a_pályáról()
        {
            return Karesz.MiVanElőttem() == -1;
        }


        class robot
        {
            public robot(PictureBox pb, PictureBox rbforg, Label xl, Label yl, Label időlab, TextBox fdb, TextBox pdb, TextBox zdb, TextBox sdb, int x = 5, int y = 28, int irány = 0, string pálya = "")
            {
                this.x = x;
                this.y = y;
                this.irány = irány;
                this.kép = pb;
                this.idő = 0;

                this.xl = xl; this.yl = yl; this.fdb = fdb; this.pdb = pdb; this.zdb = zdb; this.sdb = sdb; this.időlab = időlab;
                this.robiforg = rbforg;

                for (int i = 0; i < 6; ++i)
                    this.kődb[i] = 0;

                this.kődb[fekete] = Convert.ToInt32(fdb.Text);
                this.kődb[piros] = Convert.ToInt32(pdb.Text);
                this.kődb[zöld] = Convert.ToInt32(zdb.Text);
                this.kődb[sárga] = Convert.ToInt32(sdb.Text);



                kareszkép[0] = Properties.Resources.Karesz0;
                kareszkép[1] = Properties.Resources.Karesz1;
                kareszkép[2] = Properties.Resources.Karesz2;
                kareszkép[3] = Properties.Resources.Karesz3;

                Betölt(pálya);
            }

            public void Lép()
            {
                Point v = new Point(0, 0);

                if (irány == észak)
                    v.Y = -1;
                else if (irány == kelet)
                    v.X = 1;
                else if (irány == dél)
                    v.Y = 1;
                else
                    v.X = -1;

                int ux = x + v.X;
                int uy = y + v.Y;

                if (ux >= 0 && ux < p.GetLength(0) && uy >= 0 && uy < p.GetLength(1) && p[ux, uy] != fal)
                {
                    x = ux;
                    y = uy;
                    ++idő;
                }
                else
                    MessageBox.Show("Nem tudok lépni!");

                kép.Refresh();
            }

            int modulo(int a, int b)
            {
                if (a < 0)
                    return b + a%b;
                else
                    return a%b;
 
            }

            public void Fordul(int forgásirány)
            {
                irány = modulo(irány + forgásirány , 4);
                ++idő;
                kép.Refresh();
            }

            public int MerreNéz()
            {
                return irány;
            }

            public void Lerak(int szín)
            {
                if (p[x, y] != üres)
                    MessageBox.Show("Nem tudom a kavicsot lerakni, mert van lerakva kavics!");
                else  if(kődb[szín]<=0)
                    MessageBox.Show("Nem tudom a kavicsot lerakni, mert nincs kavicsom!");
                else
                {
                    p[x, y] = szín;
                    --kődb[szín];
                    ++idő;
                }

                kép.Refresh();
            }

            public void Felvesz()
            {
                if (p[x, y] > fal)
                {
                    ++kődb[p[x, y]];
                    p[x, y] = üres;
                    ++idő;
                }
                else
                    MessageBox.Show("Nem tudom a kavicsot felvenni!");

                kép.Refresh();
            }

            public bool VanKavics()
            {
                return p[x,y]>fal;
            }

            public int MiVanItt()
            {
                return p[x, y];
            }

            public int MiVanElőttem()
            {
                Point v = new Point(0, 0);

                if (irány == észak)
                    v.Y = -1;
                else if (irány == kelet)
                    v.X = 1;
                else if (irány == dél)
                    v.Y = 1;
                else
                    v.X = -1;

                int ux = x + v.X;
                int uy = y + v.Y;

                if (ux >= 0 && ux < p.GetLength(0) && uy >= 0 && uy < p.GetLength(1))
                {
                    return p[ux, uy];
                }

                return -1;
            }

            public int Ultrahang_szenzor()
            {
                Point v = new Point(0, 0);

                if (irány == észak)
                    v.Y = -1;
                else if (irány == kelet)
                    v.X = 1;
                else if (irány == dél)
                    v.Y = 1;
                else
                    v.X = -1;

                int távolság = 0;
                int ux = x;
                int uy = y;

                while(ux >= 0 && ux < p.GetLength(0) && uy >= 0 && uy < p.GetLength(1) && p[ux,uy]!=fal)
                {
                    ux += v.X;
                    uy += v.Y;
                    ++távolság;
                }

                if (ux >= 0 && ux < p.GetLength(0) && uy >= 0 && uy < p.GetLength(1))
                    return nincs_pálya;

                return távolság;
            }

            int[,] p = new int[41, 31];

            int x, y;
            int irány = 0;
            int[] kődb = new int[6];
            int idő;

            PictureBox kép,robiforg;
            Bitmap[] kareszkép = new Bitmap[4];
            Color[] színek = {Color.White, Color.Brown, Color.Black, Color.Red, Color.Green, Color.Yellow};

            Label xl, yl, időlab; TextBox fdb, pdb, zdb, sdb;

            public void rajzol(PaintEventArgs e)
            {
                Brush üresbrush = new SolidBrush(színek[üres]);
                Brush falbrush = new SolidBrush(színek[fal]);                               

                Pen rácsvonal = new Pen(new SolidBrush(Color.Gray),1);

                int h = kép.Width / p.GetLength(0);

                for (int y = 1; y < p.GetLength(1); ++y)
                    e.Graphics.DrawLine(rácsvonal, 0, h*y-1, p.GetLength(0)*h, h*y-1);

                for (int x = 1; x < p.GetLength(0); ++x)
                    e.Graphics.DrawLine(rácsvonal, h*x - 1 , 0 ,  h*x -1 , p.GetLength(1)*h);

                for (int y = 0; y < p.GetLength(1); ++y)
                {
                    for (int x = 0; x < p.GetLength(0); ++x)
                    {
                        if (p[x, y] == fal)
                            e.Graphics.FillRectangle(falbrush, x * h, y * h, h, h);
                        else if (p[x, y] > fal)  //kő
                            e.Graphics.FillEllipse(new SolidBrush(színek[p[x,y]]) , x * h + 2, y * h + 2, h-4, h-4);
                    }
                }

                e.Graphics.DrawImageUnscaledAndClipped(kareszkép[irány], new Rectangle(this.x * h, this.y * h, h, h));

                xl.Text = this.x.ToString();
                yl.Text = this.y.ToString();
                időlab.Text = this.idő.ToString();

                if (megy_a_robot)
                {
                    fdb.Text = kődb[fekete].ToString();
                    pdb.Text = kődb[piros].ToString();
                    zdb.Text = kődb[zöld].ToString();
                    sdb.Text = kődb[sárga].ToString();
                }
                
                robiforg.BackgroundImage = kareszkép[irány];
            }

            public void Betölt(string fájlnév)
            {
                if (fájlnév.Length > 0)
                {
                    try
                    {
                        StreamReader f = new StreamReader(fájlnév);
                        for (int y = 0; y < p.GetLength(1); ++y)
                        {
                            string[] sor = f.ReadLine().Split('\t');
                            for (int x = 0; x < p.GetLength(0); ++x)
                                p[x, y] = Convert.ToInt32(sor[x]);
                        }
                        f.Close();
                    }
                    catch (FileNotFoundException e)
                    {
                        MessageBox.Show("Nincs meg a pálya!");
                    }
                }
                else
                {
                    for (int y = 0; y < p.GetLength(1); ++y)
                    {
                        for (int x = 0; x < p.GetLength(0); ++x)
                            p[x, y] = 0;
                    }
                }
                kép.Refresh();

            }          
        }

        public Form1()
        {
            InitializeComponent();
            Karesz = new robot(pictureBox1,pictureBox2,xl,yl,idől,feketedb,pirosdb,színdb,sárgadb);
        }

        private void Start_Click(object sender, EventArgs e)
        {
            int x = Convert.ToInt32(xl.Text);
            int y = Convert.ToInt32(yl.Text);
            Karesz = new robot(pictureBox1, pictureBox2, xl, yl, idől, feketedb, pirosdb, színdb, sárgadb, x, y, Karesz.MerreNéz(), pálya.Text);
            megy_a_robot = true;
            Start.Enabled = feketedb.Enabled = pirosdb.Enabled = színdb.Enabled = sárgadb.Enabled = pálya.Enabled = false;
            FELADAT();
            Start.Enabled = feketedb.Enabled = pirosdb.Enabled = színdb.Enabled = sárgadb.Enabled = pálya.Enabled =  true;
            megy_a_robot = false;
            MessageBox.Show("Vége!");
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Karesz.rajzol(e);
            panel1.Refresh();
            pictureBox2.Refresh();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            int h = pictureBox1.Width / 41;
            int x = e.X / h;
            int y = e.Y / h;
            Karesz = new robot(pictureBox1, pictureBox2, xl, yl,idől, feketedb, pirosdb, színdb, sárgadb, x, y, Karesz.MerreNéz(), pálya.Text);
            pictureBox1.Refresh();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Karesz.Fordul(jobbra);
        }


        private void Betölt_Click(object sender, EventArgs e)
        {
            Karesz.Betölt(pálya.Text);
        }

        #endregion

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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Start = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Betölt = new System.Windows.Forms.Button();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pálya = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.sárgadb = new System.Windows.Forms.TextBox();
            this.színdb = new System.Windows.Forms.TextBox();
            this.pirosdb = new System.Windows.Forms.TextBox();
            this.feketedb = new System.Windows.Forms.TextBox();
            this.sárgal = new System.Windows.Forms.Label();
            this.színl = new System.Windows.Forms.Label();
            this.pirosl = new System.Windows.Forms.Label();
            this.feketel = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.yl = new System.Windows.Forms.Label();
            this.xl = new System.Windows.Forms.Label();
            this.idől = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.Location = new System.Drawing.Point(13, 13);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(820, 620);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(847, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 20);
            this.label1.TabIndex = 6;
            this.label1.Text = "Pálya:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(850, 33);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 17);
            this.label2.TabIndex = 7;
            this.label2.Text = "31 sor";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label3.Location = new System.Drawing.Point(850, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 8;
            this.label3.Text = "41 oszlop";
            // 
            // Start
            // 
            this.Start.Location = new System.Drawing.Point(850, 90);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(99, 39);
            this.Start.TabIndex = 9;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.Start_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label4.Location = new System.Drawing.Point(835, 156);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(106, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "Karesz adatai";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(9, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 17);
            this.label5.TabIndex = 11;
            this.label5.Text = "x:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(9, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 17);
            this.label6.TabIndex = 12;
            this.label6.Text = "y:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.idől);
            this.panel1.Controls.Add(this.Betölt);
            this.panel1.Controls.Add(this.pictureBox2);
            this.panel1.Controls.Add(this.pálya);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.sárgadb);
            this.panel1.Controls.Add(this.színdb);
            this.panel1.Controls.Add(this.pirosdb);
            this.panel1.Controls.Add(this.feketedb);
            this.panel1.Controls.Add(this.sárgal);
            this.panel1.Controls.Add(this.színl);
            this.panel1.Controls.Add(this.pirosl);
            this.panel1.Controls.Add(this.feketel);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.yl);
            this.panel1.Controls.Add(this.xl);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Location = new System.Drawing.Point(839, 179);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(118, 454);
            this.panel1.TabIndex = 13;
            // 
            // Betölt
            // 
            this.Betölt.Location = new System.Drawing.Point(14, 386);
            this.Betölt.Name = "Betölt";
            this.Betölt.Size = new System.Drawing.Size(96, 23);
            this.Betölt.TabIndex = 28;
            this.Betölt.Text = "Pályát betölt";
            this.Betölt.UseVisualStyleBackColor = true;
            this.Betölt.Click += new System.EventHandler(this.Betölt_Click);
            // 
            // pictureBox2
            // 
            this.pictureBox2.BackgroundImage = global::Karesz.Properties.Resources.Karesz0;
            this.pictureBox2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.pictureBox2.Location = new System.Drawing.Point(31, 95);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(49, 49);
            this.pictureBox2.TabIndex = 27;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Click += new System.EventHandler(this.pictureBox2_Click);
            // 
            // pálya
            // 
            this.pálya.Location = new System.Drawing.Point(12, 360);
            this.pálya.Name = "pálya";
            this.pálya.Size = new System.Drawing.Size(100, 20);
            this.pálya.TabIndex = 26;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label9.Location = new System.Drawing.Point(11, 323);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(43, 17);
            this.label9.TabIndex = 25;
            this.label9.Text = "Pálya";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label8.Location = new System.Drawing.Point(11, 157);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(94, 17);
            this.label8.TabIndex = 24;
            this.label8.Text = "Kezében lévő";
            // 
            // sárgadb
            // 
            this.sárgadb.Location = new System.Drawing.Point(61, 284);
            this.sárgadb.Name = "sárgadb";
            this.sárgadb.Size = new System.Drawing.Size(49, 20);
            this.sárgadb.TabIndex = 23;
            this.sárgadb.Text = "1000";
            // 
            // színdb
            // 
            this.színdb.Location = new System.Drawing.Point(61, 255);
            this.színdb.Name = "színdb";
            this.színdb.Size = new System.Drawing.Size(49, 20);
            this.színdb.TabIndex = 22;
            this.színdb.Text = "1000";
            // 
            // pirosdb
            // 
            this.pirosdb.Location = new System.Drawing.Point(61, 227);
            this.pirosdb.Name = "pirosdb";
            this.pirosdb.Size = new System.Drawing.Size(49, 20);
            this.pirosdb.TabIndex = 21;
            this.pirosdb.Text = "1000";
            // 
            // feketedb
            // 
            this.feketedb.Location = new System.Drawing.Point(61, 198);
            this.feketedb.Name = "feketedb";
            this.feketedb.Size = new System.Drawing.Size(49, 20);
            this.feketedb.TabIndex = 20;
            this.feketedb.Text = "1000";
            // 
            // sárgal
            // 
            this.sárgal.AutoSize = true;
            this.sárgal.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.sárgal.ForeColor = System.Drawing.Color.Yellow;
            this.sárgal.Location = new System.Drawing.Point(14, 285);
            this.sárgal.Name = "sárgal";
            this.sárgal.Size = new System.Drawing.Size(44, 17);
            this.sárgal.TabIndex = 19;
            this.sárgal.Text = "sárga";
            // 
            // színl
            // 
            this.színl.AutoSize = true;
            this.színl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.színl.ForeColor = System.Drawing.Color.Green;
            this.színl.Location = new System.Drawing.Point(13, 256);
            this.színl.Name = "színl";
            this.színl.Size = new System.Drawing.Size(34, 17);
            this.színl.TabIndex = 18;
            this.színl.Text = "szín";
            // 
            // pirosl
            // 
            this.pirosl.AutoSize = true;
            this.pirosl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.pirosl.ForeColor = System.Drawing.Color.Red;
            this.pirosl.Location = new System.Drawing.Point(12, 228);
            this.pirosl.Name = "pirosl";
            this.pirosl.Size = new System.Drawing.Size(39, 17);
            this.pirosl.TabIndex = 17;
            this.pirosl.Text = "piros";
            // 
            // feketel
            // 
            this.feketel.AutoSize = true;
            this.feketel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.feketel.ForeColor = System.Drawing.Color.Black;
            this.feketel.Location = new System.Drawing.Point(14, 201);
            this.feketel.Name = "feketel";
            this.feketel.Size = new System.Drawing.Size(47, 17);
            this.feketel.TabIndex = 16;
            this.feketel.Text = "fekete";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label7.Location = new System.Drawing.Point(11, 174);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(94, 17);
            this.label7.TabIndex = 15;
            this.label7.Text = "kövek száma:";
            // 
            // yl
            // 
            this.yl.AutoSize = true;
            this.yl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.yl.Location = new System.Drawing.Point(30, 54);
            this.yl.Name = "yl";
            this.yl.Size = new System.Drawing.Size(18, 17);
            this.yl.TabIndex = 14;
            this.yl.Text = "x:";
            // 
            // xl
            // 
            this.xl.AutoSize = true;
            this.xl.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.xl.Location = new System.Drawing.Point(30, 37);
            this.xl.Name = "xl";
            this.xl.Size = new System.Drawing.Size(18, 17);
            this.xl.TabIndex = 13;
            this.xl.Text = "x:";
            // 
            // idől
            // 
            this.idől.AutoSize = true;
            this.idől.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.idől.Location = new System.Drawing.Point(38, 9);
            this.idől.Name = "idől";
            this.idől.Size = new System.Drawing.Size(16, 17);
            this.idől.TabIndex = 14;
            this.idől.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label10.Location = new System.Drawing.Point(10, 9);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(31, 17);
            this.label10.TabIndex = 29;
            this.label10.Text = "idő:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(966, 650);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.Start);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Karesz";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button Start;
        private Label label4;
        private Label label5;
        private Label label6;
        private Panel panel1;
        private Label sárgal;
        private Label színl;
        private Label pirosl;
        private Label feketel;
        private Label label7;
        private Label yl;
        private Label xl;
        private TextBox sárgadb;
        private TextBox színdb;
        private TextBox pirosdb;
        private TextBox feketedb;
        private Label label8;
        private TextBox pálya;
        private Label label9;
        private PictureBox pictureBox2;
        private Button Betölt;
        private Label idől;
        private Label label10;
    }
}

