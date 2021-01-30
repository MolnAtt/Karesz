using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lori
{
	partial class Form1
	{
		

		class PV
		{
			public double A, L;
			public PV(PV v) { (A, L) = (v.A, v.L); }
			public PV(V v) { (A, L) = (Math.Atan2(v.Y, v.X),v.Length()); }
			public PV(double a, double l) { (A, L) = (a, l); }
			public V ToDescartes() { return new V(this); }
			public void Rotate(double a){ A += a; }
			public double Length() { return L; }
			public double Length2() { return L*L; }
			public static PV operator +(PV v, PV w) { return new PV(new V(v) + new V(w)); }
			public static PV operator -(PV v, PV w) { return new PV(new V(v) - new V(w)); }
			public static PV operator *(PV v, double x) { return new PV(v.A, v.L*x); }
			public static PV operator *(double x, PV v) { return v * x; }
			public static PV operator /(PV v, double x) { return new PV(v.A, v.L/x); }
			public static double operator *(PV v, PV w) { return new V(v) * new V(w); }
			public void Plus(PV v) { PV r = new PV(this + v); (A, L) = (r.A, r.L); }
			public void Minus(PV v) { PV r = new PV(this - v); (A, L) = (r.A, r.L); }
			public void Times(double x) { L *= x; }
			public static double Distance(PV v, PV w) 
			{ 
				// koszinusztétellel gyorsabb lenne, de nem hiszem, hogy használjuk majd.
				return (new V(v) - new V(w)).Length(); 
			}
			public static double Distance2(PV v, PV w) { return (new V(v) - new V(w)).Length2(); }

		}

		class V
		{
			public double X, Y;
			public V(V v) { (X, Y) = (v.X, v.Y); }
			public V(PV v) 
			{
				double l = v.Length();
				(X, Y) = (l * Math.Cos(v.A), l * Math.Sin(v.A));
			}
			public V(double x, double y) { (X, Y) = (x, y); }
			public PV ToPolar(){return new PV(this);}
			public void Forgat(double a) 
			{ // ez valószínű most még nem lesz jó, mert fejjel lefele van a koordinátarendszer!
				X = Math.Cos(a) * X - Math.Sin(a) * Y;
				Y = Math.Sin(a) * X + Math.Cos(a) * Y;
			}
			public double Length2() { return X*X + Y*Y; }
			public double Length() { return Math.Sqrt(Length2());}
			public static V operator +(V v, V w) { return new V(v.X + w.X, v.Y + w.Y); }
			public static V operator -(V v, V w) { return new V(v.X - w.X, v.Y - w.Y); }
			public static V operator +(V v, PV w) { V pw = new V(w); return new V(v.X + pw.X, v.Y + pw.Y); }
			public static V operator -(V v, PV w) { V pw = new V(w); return new V(v.X - pw.X, v.Y - pw.Y); }
			public static V operator +(PV v, V w) { return w + v; }
			public static V operator -(PV v, V w) { return w - v; }
			public static V operator *(V v, double x) { return new V(v.X * x, v.Y * x); }
			public static V operator *(double x, V v) { return v*x; }
			public static V operator /(V v, double x) { return new V(v.X / x, v.Y / x); }
			public static double operator *(V v, V w) { return v.X * w.X + v.Y * w.Y; }
			public void Plus(V v) { X += v.X; Y += v.Y; }
			public void Minus(V v) { X -= v.X; Y -= v.Y; }
			public void Plus(PV v) { X += v.L * Math.Cos(v.A); Y += v.L * Math.Sin(v.A); }
			public void Minus(PV v) { X -= v.L * Math.Cos(v.A); Y -= v.L * Math.Sin(v.A); }
			public void Times(double x) { X *= x; Y *= x; }
			public static double Distance(V v, V w) { return (v - w).Length(); }
			public static double Distance2(V v, V w) { return (v - w).Length2(); }
			public static double Distance(V v, PV w) { return (v - w).Length(); }
			public static double Distance2(V v, PV w) { return (v - w).Length2(); }
			public static double Distance(PV v, V w) { return (v - w).Length(); }
			public static double Distance2(PV v, V w) { return (v - w).Length2(); }
		}

		class Robot
		{
			public static List<Robot> lista = new List<Robot>();
			private string név;
			/// <summary>
			/// robot double pozíciója
			/// </summary>
			private V H;
			/// <summary>
			/// robot double iránya
			/// </summary>
			private PV I;
			private Bitmap kép;
			/// <summary>
			/// Létrehoz egy új robotot a megadott névvel, képpel, pozícióval és iránnyal.
			/// </summary>
			/// <param name="adottnév">A robot neve</param>
			/// <param name="kép">A kép a Resources mappából</param>
			/// <param name="Pozíció">indulási pozíció</param>
			/// <param name="Irány">kezdőirány</param>
			public Robot(string név, Bitmap kép, V H, PV I)
			{
				this.név = név;
				this.kép = kép;
				this.H = H;
				this.I = I;
			}

			/// <summary>
			/// Létrehoz egy új robotot a megadott névvel default helyen északra nézvén.
			/// </summary>
			/// <param name="adottnév">A robot neve</param>
			public Robot(string név)
			{
				this.név = név;
				H = new V(400,300);
				I = new PV(90,1);
				kép = Properties.Resources.Lori;
				lista.Add(this);
			}

			public string Neve() { return név; }
			public void Teleport(double x, double y) { H.X = x; H.Y = y; }
			/// <summary>
			/// Visszaadja a robot koordinátáit.
			/// </summary>
			/// <returns></returns>
			public V HolVan() { return new V(H); }

		}

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
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.SystemColors.Window;
			this.pictureBox1.Location = new System.Drawing.Point(12, 12);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(800, 600);
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1006, 721);
			this.Controls.Add(this.pictureBox1);
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		public Form1()
		{
			InitializeComponent();
		}
		#endregion

		private System.Windows.Forms.PictureBox pictureBox1;


	}
}

