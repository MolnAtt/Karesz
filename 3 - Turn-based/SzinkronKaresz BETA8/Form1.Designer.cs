using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using System.Reflection;
//using System.Threading;

// Karesz OOP-bban, több robothoz előkészítve
namespace Karesz
{
	partial class Form1
	{

		#region Konstansok

		//pályatartalom
		const int üres = 0;
		const int fal = 1;
		const int nincs_pálya = -1;
		const int láva = 7;

		//forgásirány
		const int jobbra = 1;
		const int szemben = 0;
		const int balra = -1;

		//színek				
		const int fekete = 2;
		const int piros = 3;
		const int zöld = 4;
		const int sárga = 5;
		const int hó = 6;
		const int víz = 8;

		// lövésekhez
		const int maxtöltődés = 10;
		#endregion

		#region Statikus objektumok
		// robotok
		static List<Robot> robotlista = new List<Robot>();
		static List<Lövedék> lövedéklista = new List<Lövedék>();
		static List<Robot> haldoklók = new List<Robot>();
		static List<Lövedék> koppanók = new List<Lövedék>();
		/*{
			new Robot("Karesz", new Bitmap[4]
					{
						Properties.Resources.Karesz0,
						Properties.Resources.Karesz1,
						Properties.Resources.Karesz2,
						Properties.Resources.Karesz3
					},
				new Vektor(5,28),
				new Vektor(0,-1),
				new int[5]{100, 20, 20, 20, 0}
				)
		};*/
		static Óra óra = new Óra();
		static string fájlnév = "";
		static Random véletlen = new Random();
		private static Pálya pálya = new Pálya();
		private static MonitorPanel monitorpanel = new MonitorPanel();
		#endregion

		#region Form-komponensek 
		private OnOffGomb startgomb = new OnOffGomb
		{
			Location = new Point(pálya.Size.Width + 20, 10),
			Margin = new Padding(4, 4, 4, 4),
			Name = "startgomb",
			Size = new Size(monitorpanel.Size.Width, 48),
			TabIndex = 9,
			UseVisualStyleBackColor = true
		};
		private Label pályafelirat = new Label
		{
			AutoSize = true,
			Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
			Location = new Point(20 + pálya.Size.Width, pálya.Size.Height - 50),
			Margin = new Padding(4, 0, 4, 0),
			Name = "pályafelirat",
			Size = new Size(50, 20),
			TabIndex = 25,
			Text = "Pálya:",
		};
		private TextBox pályaválasztó = new TextBox
		{
			Location = new Point(20 + pálya.Size.Width + 55, pálya.Size.Height - 50),
			Margin = new Padding(4, 4, 4, 4),
			Name = "pálya",
			Size = new Size(monitorpanel.Size.Width - 55, 30),
			TabIndex = 26,
		};
		private Button Betölt = new Button
		{
			Location = new Point(20 + pálya.Size.Width, pálya.Size.Height - 20),
			Margin = new Padding(4, 4, 4, 4),
			Name = "Betölt",
			Size = new Size(monitorpanel.Size.Width, 30),
			TabIndex = 28,
			Text = "Pályát betölt",
			UseVisualStyleBackColor = true
		};

		#endregion

		class OnOffGomb : Button
		{ 
			private bool bekapcsolva = false;
			public OnOffGomb()
			{
				this.Click += OnOffGomb_Click;
				Frissít();
			}

			private void Frissít()
			{
				if (bekapcsolva) { Text = "Stop"; } else { Text = "Start"; }
			}
			private void OnOffGomb_Click(object sender, EventArgs e)
			{
				bekapcsolva = !bekapcsolva;
				Frissít();
			}
			public bool Be(){ return bekapcsolva;}
		}
		class Óra : Timer
		{
			private int idő = 0;
			public int GetIdő() { return idő; }
			public Óra()
			{
				Interval = 50;
				this.Tick += Óra_Tick;
			}

			private void Óra_Tick(object sender, EventArgs e)
			{
				idő++;
				foreach (Robot robot in robotlista)robot.MűködikEgyet(); 
				foreach (Lövedék lövedék in lövedéklista) lövedék.Repül(); 
				foreach (Robot robot in robotlista)
				{
					foreach (Lövedék lövedék in lövedéklista)
					{
						// ha egy helyen vannak vagy nem egy helyen vannak, de át kellett menniük egymáson
						if (robot.GetHely().Egybeesik(lövedék.GetHely()) || robot.FrontálisanÜtközik(lövedék))
						{
							haldoklók.Add(robot);
							koppanók.Add(lövedék);
						}
					}
				}
				for (int i = 0; i < robotlista.Count(); i++)
				{
					for (int j = i+1; j < robotlista.Count; j++)
					{
						// ha egy helyen vannak vagy nem egy helyen vannak, de át kellett menjenek egymáson, hogy ide kerüljenek.
						if (robotlista[i].GetHely().Egybeesik(robotlista[j].GetHely()) || robotlista[i].FrontálisanÜtközik(robotlista[j]))
						{
							haldoklók.Add(robotlista[i]);
							haldoklók.Add(robotlista[j]);
						}
					}
				}

				foreach (Robot robot in haldoklók) robotlista.Remove(robot); 
				foreach (Lövedék lövedék in koppanók)
				{
					lövedéklista.Remove(lövedék);
					pálya.LegyenItt(lövedék.GetHely(), lövedék.GetSzín());
				}
				koppanók.Clear();
				pálya.Refresh();
				monitorpanel.Frissít();
				//monitorpanel.debug("", robotlista.Count + " robot van");
			}
		}

		class MonitorPanel : Panel
		{
			public Robot megfigyeltrobot;
			public List<string> debuglista = new List<string>();
			Button Robotváltó = new Button
			{
				Location = new Point(20, 20),
				Margin = new Padding(4, 4, 4, 4),
				Name = "Robotváltó",
				TabIndex = 28,
				Text = "Robotváltás",
				UseVisualStyleBackColor = true
			};
			Label robotcímke = new Label
			{
				AutoSize = true,
				Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(20, 70),
				Margin = new Padding(4, 0, 4, 0),
				Name = "robotcímke",
				Size = new Size(132, 25),
				TabIndex = 10,
				Text = "Robot adatai"
			};
			Label koordináták = new Label
			{
				AutoSize = true,
				Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(5, 150),
				Margin = new Padding(4, 0, 4, 0),
				Name = "koordináták",
				Size = new Size(18, 20),
				TabIndex = 13,
				Text = "Koord.: (5;28)"
			};
			Label idől = new Label
			{
				AutoSize = true,
				Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(5, 190),
				Margin = new Padding(4, 0, 4, 0),
				Name = "idől",
				Size = new Size(18, 20),
				TabIndex = 14,
				Text = "Eltelt idő: 0 s"
			};
			Label hől = new Label
			{
				AutoSize = true,
				Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(5, 170),
				Margin = new Padding(4, 0, 4, 0),
				Name = "idől",
				Size = new Size(18, 20),
				TabIndex = 14,
				Text = "Hő: "
			};
			Label kőszámfelirat = new Label
			{
				AutoSize = true,
				Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(25, 290),
				Margin = new Padding(4, 0, 4, 0),
				Name = "kőszámfelirat",
				Size = new Size(111, 20),
				TabIndex = 15,
				Text = "Zsebeiben lévő \nkövek száma:",
			};
			public TextBox[] kőmutatók = new TextBox[5]
				{
					new TextBox{ Name = "feketedb" },
					new TextBox{ Name = "pirosdb" },
					new TextBox{ Name = "zölddb" },
					new TextBox{ Name = "sárgadb" },
					new TextBox{ Name = "hódb" }
				};
			Label[] kőfeliratok = new Label[5]
				{
					new Label{ Name="fekete", Text = "fekete", ForeColor = Color.Black},
					new Label{ Name="piros", Text = "piros", ForeColor = Color.Red},
					new Label{ Name="zöld", Text = "zöld", ForeColor = Color.Green},
					new Label{ Name="sárga", Text = "sárga", ForeColor = Color.Yellow},
					new Label{ Name="hógolyó", Text = "hógolyó", ForeColor = Color.White}
				};
			PictureBox NagyítottKép = new PictureBox
			{
				BackgroundImage = global::Karesz.Properties.Resources.Karesz0,
				BackgroundImageLayout = ImageLayout.Stretch,
				Location = new Point(40, 200),
				Margin = new Padding(4, 4, 4, 4),
				Name = "NagyítottKép",
				Size = new Size(80, 80),
				TabIndex = 27,
				TabStop = false,
			};
			Panel debugmonitorpanel = new Panel
			{
				BackColor = SystemColors.ControlLight,
				Margin = new Padding(4, 4, 4, 4),
				Name = "debugmonitorpanel",
				TabIndex = 13
			};
			Label debugmonitor = new Label
			{
				AutoSize = false, //FontFamily.GenericMonospace
				Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238))),
				Location = new Point(5, 5),
				Margin = new Padding(4, 0, 4, 0),
				Name = "debugmonitor",
				TabIndex = 15,
				Text = "Felirat arra, hogy mi történik itt",
			};
			public void debug(string info, string str)
			{
				debuglista.Add(info + str);
				string sum = "";
				for (int i = Math.Max(debuglista.Count - 5, 0); i < debuglista.Count; i++)
				{
					sum += debuglista[i] + "\n";
				}
				debugmonitor.Text = sum;
				//monitorpanel.Frissít();
			}

			public MonitorPanel()
			{
				BackColor = SystemColors.ControlDark;
				Location = new Point(pálya.Size.Width + 20, 70);
				Margin = new Padding(4, 4, 4, 4);
				Name = "monitorpanel";
				Size = new Size(160, pálya.Size.Height - 130);
				Robotváltó.Size = new Size(Size.Width - 40, 30);
				TabIndex = 13;
				koordináták.Location = new Point(10, robotcímke.Location.Y + 40);
				idől.Location = new Point(koordináták.Location.X, koordináták.Location.Y + 30);
				hől.Location = new Point(koordináták.Location.X, idől.Location.Y + 30);


				Controls.Add(Robotváltó);
				Controls.Add(koordináták);
				Controls.Add(idől);
				Controls.Add(hől);
				Controls.Add(kőszámfelirat);
				Controls.Add(robotcímke);
				Controls.Add(NagyítottKép);
				Controls.Add(debugmonitorpanel);
				debugmonitorpanel.Controls.Add(debugmonitor);
				((System.ComponentModel.ISupportInitialize)(NagyítottKép)).BeginInit();
				((System.ComponentModel.ISupportInitialize)(NagyítottKép)).EndInit();




				for (int i = 0; i < 5; i++)
				{
					// Labelek
					kőfeliratok[i].AutoSize = true;
					kőfeliratok[i].Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(238)));
					kőfeliratok[i].Location = new Point(10, kőszámfelirat.Location.Y + 50 + i * 25);    // ezért forciklus!
					kőfeliratok[i].Margin = new Padding(4, 0, 4, 0);
					kőfeliratok[i].Size = new Size(54, 20);
					kőfeliratok[i].TabIndex = 16;
					Controls.Add(kőfeliratok[i]);

					// Textboxok
					kőmutatók[i].Location = new Point(90, kőszámfelirat.Location.Y + 50 + i * 25);  // ezért forciklus!
					kőmutatók[i].Margin = new Padding(4, 4, 4, 4);
					kőmutatók[i].Name = "feketedb";
					kőmutatók[i].Size = new Size(64, 22);
					kőmutatók[i].TabIndex = 20;
					kőmutatók[i].Text = "10";
					Controls.Add(kőmutatók[i]);
				}
				debugmonitorpanel.Location = new Point(10, kőmutatók[4].Location.Y + 30);
				debugmonitorpanel.Size = new Size(Size.Width - 20, 135);
				debugmonitor.Size = new Size(debugmonitorpanel.Size.Width - 10, debugmonitorpanel.Height - 10);

				NagyítottKép.Click += new System.EventHandler(NagyítottKép_Click);
				Robotváltó.Click += new System.EventHandler(Robotváltó_Click);
			}
			private void Robotváltó_Click(object sender, EventArgs e)
			{
				if (robotlista.Count > 0)
				{
					megfigyeltrobot = robotlista[(robotlista.IndexOf(megfigyeltrobot) + 1) % robotlista.Count]; 
				}
				monitorpanel.Frissít();
			}
			private void NagyítottKép_Click(object sender, EventArgs e) { megfigyeltrobot.Fordul(jobbra); pálya.Refresh(); monitorpanel.Frissít(); }
			public void Enged(bool szabade) { foreach (TextBox szövegdoboz in kőmutatók) { szövegdoboz.Enabled = szabade; } }
			public void Frissít()
			{
				idől.Text = "Eltelt idő: " + óra.GetIdő() + " s";
				if (robotlista.Count>0)
				{
					robotcímke.Text = megfigyeltrobot.Neve() + " adatai";
					koordináták.Text = "Koord: (" + megfigyeltrobot.GetHely().X + ";" + megfigyeltrobot.GetHely().Y + ")";
					hől.Text = "Hő: " + megfigyeltrobot.Hőmérő() + " fok";
					for (int szín = 0; szín < 5; szín++) { kőmutatók[szín].Text = megfigyeltrobot.Mennyi(2 + szín).ToString(); }
					NagyítottKép.BackgroundImage = megfigyeltrobot.Iránykép();
					Refresh();
				}
			}
		}

		/// <summary>
		/// A Pálya egy olyan PictureBox-ból származtatott class, amely azon túl, hogy megrajzolja magát pályának,
		/// a rendelkezésre álló robotokat is felrajzolja a pályára. Rendelkezik továbbá azokkal a függvényekkel, amelyek
		/// kiszolgálják a robotok szenzorait és cselekvéseit. Illetve itt helyezkedik el a fájlból pályát betöltő metódus is.
		/// </summary>
		class Pálya : PictureBox
		{
			public const int X = 41;
			public const int Y = 31;
			public const int LX = 20;   // ha fix léptékkel akarunk pályát
			public const int LY = 20;   // ha fix léptékkel akarunk pályát
			public Vektor lépték;
			private Brush[] tollkészlet = new SolidBrush[9];
			private Pen vonalzósceruza = new Pen(new SolidBrush(Color.Gray), 1);
			private Color[] színek = { Color.DarkOliveGreen, Color.Brown, Color.Black, Color.Red, Color.Green, Color.Yellow, Color.White, Color.Orange, Color.Blue };
			private int[,] tábla = new int[X, Y];
			private int[,] hőtábla = new int[X, Y];
			public Pálya()
			{
				for (int i = üres; i < víz+1; i++)
				{
					tollkészlet[i] = new SolidBrush(színek[i]);
				}
				lépték = new Vektor(LX, LY);
				//new Vektor(Width / X, Height / Y);
				Width = 270+X * lépték.X; // maradékos osztás hibái miatt
				Height = 150+Y * lépték.Y;

				BackColor = Color.White;
				Location = new Point(10, 10);
				Margin = new Padding(4, 4, 4, 4);
				Name = "Pálya";
				Size = new Size(Width, Height);
				TabIndex = 0;
				TabStop = false;
			}
			/// <summary>
			/// Megnézi, hogy értelmezhető-e a pályán az adott pont.
			/// </summary>
			/// <param name="V">A pont, amiről megvizsgáljuk, hogy rajta van-e a pályán vagy sem</param>
			/// <returns>Igaz, ha rajta van, hamis, ha nincs.</returns>
			public bool BenneVan(Vektor V) { return 0 <= V.X && V.X < X && 0 <= V.Y && V.Y < Y; }
			/// <summary>
			/// Visszaadja egy vektorral megadott ponton lévő entitás kódját.
			/// </summary>
			/// <param name="P">A vizsgálandó pozíció</param>
			/// <returns>Az itt lévő entitás kódja.</returns>
			public int MiVanItt(Vektor P)
			{
				if (BenneVan(P)) { return tábla[P.X, P.Y]; }
				else return -1;
			}
			public int Hőmérséklet(Vektor P)
			{
				if (BenneVan(P)) { return hőtábla[P.X, P.Y]; }
				else return -1;
			}
			/// <summary>
			/// Felülírja a pálya egy adott pontját azzal az értékkel, amit megadunk.
			/// </summary>
			/// <param name="P"> Az itt lévő dolgot írja át</param>
			/// <param name="ez">Erre írja át</param>
			public void LegyenItt(Vektor P, int ez) { tábla[P.X, P.Y] = ez; }
			/// <summary>
			/// Ellenőrzi, hogy van-e kavics az adott pozíción.
			/// </summary>
			/// <param name="P">A vizsgálandó pozíció</param>
			/// <returns>igaz, ha van, hamis, ha nincs.</returns>
			public bool VanKavics(Vektor P) { return MiVanItt(P) > fal; }
			/// <summary>
			/// Lerajzol mindent a pályán, amit csak lehetséges. Robotokat is beleértve.
			/// </summary>
			/// <param name="vászon"></param>
			/// <param name="e"></param>
			public void Rajz(PictureBox vászon, PaintEventArgs e)
			{
				e.Graphics.FillRectangle(tollkészlet[üres], 0, 0 , Width, Height);
				for (int y = 1; y < Y; ++y) e.Graphics.DrawLine(vonalzósceruza, 0, y * lépték.Y - 1, X * lépték.X, y * lépték.Y - 1);   // vízszintes vonalak
				for (int x = 1; x < X; ++x) e.Graphics.DrawLine(vonalzósceruza, x * lépték.X - 1, 0, x * lépték.X - 1, Y * lépték.Y);    // függőleges vonalak
				for (int y = 0; y < Y; ++y)
				{
					for (int x = 0; x < X; ++x)
					{
						switch (tábla[x, y])
						{
							//case üres: e.Graphics.FillRectangle(tollkészlet[üres], x * lépték.X+1, y * lépték.Y+1, lépték.X-2, lépték.Y-2); break;
							case fal: e.Graphics.FillRectangle(tollkészlet[fal], x * lépték.X, y * lépték.Y, lépték.X, lépték.Y); break;
							case láva: e.Graphics.FillRectangle(tollkészlet[láva], x * lépték.X, y * lépték.Y, lépték.X, lépték.Y); break;
							case víz: e.Graphics.FillRectangle(tollkészlet[víz], x * lépték.X, y * lépték.Y, lépték.X, lépték.Y); break;
							case fekete: e.Graphics.FillEllipse(tollkészlet[tábla[x, y]], x * lépték.X + 2, y * lépték.Y + 2, lépték.X - 4, lépték.Y - 4); break;
							case piros: e.Graphics.FillEllipse(tollkészlet[tábla[x, y]], x * lépték.X + 2, y * lépték.Y + 2, lépték.X - 4, lépték.Y - 4); break;
							case zöld: e.Graphics.FillEllipse(tollkészlet[tábla[x, y]], x * lépték.X + 2, y * lépték.Y + 2, lépték.X - 4, lépték.Y - 4); break;
							case sárga: e.Graphics.FillEllipse(tollkészlet[tábla[x, y]], x * lépték.X + 2, y * lépték.Y + 2, lépték.X - 4, lépték.Y - 4); break;
							case hó:  e.Graphics.FillRectangle(tollkészlet[hó], x * lépték.X+1, y * lépték.Y+1, lépték.X-2, lépték.Y-2); break;
						}
					}
				}
				foreach (Robot robot in robotlista)
				{
					e.Graphics.DrawImageUnscaledAndClipped(robot.Iránykép(), new Rectangle(robot.GetHely().X * lépték.X, robot.GetHely().Y * lépték.Y, lépték.X, lépték.Y));
				}
				foreach (Lövedék lövedék in lövedéklista)
				{
					//MessageBox.Show("rajzolódik!");

					e.Graphics.FillEllipse(tollkészlet[6], lövedék.GetHely().X * lépték.X + 2, lövedék.GetHely().Y * lépték.Y + 2, lépték.X - 4, lépték.Y - 4);
				}
			}
			/// <summary>
			/// A pályaválasztó textboxban szereplő fájlt betölti és újrarajzolja ennek megfelelően a pályát. Üres string esetén üres pályát ad.
			/// </summary>
			/// <param name="fájlnév"></param>
			public void Betölt(string leolvasottfájlnév)
			{
				if (leolvasottfájlnév.Length > 0)
				{
					fájlnév = leolvasottfájlnév;
				}

				if (fájlnév.Length > 0)
				{
					try
					{
						StreamReader f = new StreamReader(fájlnév+".txt");
						for (int y = 0; y < Y; ++y)
						{
							string[] sor = f.ReadLine().Split('\t');
							for (int x = 0; x < X; ++x)
								tábla[x, y] = Convert.ToInt32(sor[x]);
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
				// a szélére -1-et rakunk, a belsejébe nullát.
				//	for (int x = 0; x < X; x++) tábla[x, 0] = -1;
				//	for (int x = 0; x < X; x++) tábla[x, Y - 1] = -1;
				//	for (int y = 1; y < Y - 1; y++) tábla[0, y] = -1;
				//	for (int y = 1; y < Y - 1; y++) tábla[X - 1, y] = -1;
					for (int y = 0; y < Y; ++y)
					{
						for (int x = 0; x < X; ++x)
							tábla[x, y] = 0;
					}
				}

				Hőtérképezés();
				Refresh();
			}
			private bool VanELáva()
			{
				bool megvan = false;
				for (int i = 0; i < X && !megvan; i++)
					for (int j = 0; j < Y && !megvan; j++)
						if (tábla[i, j] == 7) megvan = true;
				return megvan;
			}
			private void Melegedés(int i, int j)
			{
				List<int> környezetihőmérséklet = new List<int>();
				if (0 < i && hőtábla[i - 1, j] > 200) környezetihőmérséklet.Add(hőtábla[i - 1, j]);
				if (0 < j && hőtábla[i, j - 1] > 200) környezetihőmérséklet.Add(hőtábla[i, j - 1]);
				if (i < X - 1 && hőtábla[i + 1, j] > 200) környezetihőmérséklet.Add(hőtábla[i + 1, j]);
				if (j < Y - 1 && hőtábla[i, j + 1] > 200) környezetihőmérséklet.Add(hőtábla[i, j + 1]);
				if (környezetihőmérséklet.Count > 0)
				{
					int kihívó = környezetihőmérséklet.Max() - 200;
					if (hőtábla[i, j] < kihívó) hőtábla[i, j] = kihívó;
				}
			}
			private void Hőtérképezés()
			{
				if (VanELáva())
				{// A láva 1000 fokos... ("Inicializálás")
					for (int i = 0; i < X; i++)
					{
						for (int j = 0; j < Y; j++)
						{
							hőtábla[i, j] = (tábla[i, j] == 7 ? 1000 : 0);
						}
					}
					//... és minden szomszédos mezőn 200 fokkal hűvösebb. Tehát 4-szer (1000->800->600->400->200) végigmegyünk, hogy a felmelegedést update-eljük.
					for (int k = 0; k < ((1000 / 200) - 1); k++)
					{
						for (int i = 0; i < X; i++)
						{
							for (int j = 0; j < Y; j++)
							{
								Melegedés(i, j);
							}
						}
					}
				}
			}
		}

		public class Vektor
		{
			/// <summary>
			/// vektor koordinátái
			/// </summary>
			public int X, Y;
			/// <summary>
			/// új vektor megadása egy másik vektor által (másolás)
			/// </summary>
			/// <param name="V"></param>
			public Vektor(Vektor V) { X = V.X; Y = V.Y; }
			/// <summary>
			/// Vektor megadása koordinátákkal
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			public Vektor(int x, int y) { X = x; Y = y; }
			/// <summary>
			/// Két pont közt menő vektor megadása
			/// </summary>
			/// <param name="P"></param>
			/// <param name="Q"></param>
			public Vektor(Vektor P, Vektor Q) { X = P.X - Q.X; Y = P.Y - Q.Y; }
			/// <summary>
			/// Vektor hosszának a négyzetének a kiszámítása
			/// </summary>
			/// <returns></returns>
			public bool Egybeesik(Vektor P) { return X==P.X && Y==P.Y; }
			public int HosszN() { return Matek.Négyzet(X) + Matek.Négyzet(Y); }
			/// <summary>
			/// Vektor hosszának a kiszámítása
			/// </summary>
			/// <returns></returns>
			public int Hossz() { return (int)Math.Sqrt(HosszN()); }
			public double HosszD() { return Math.Sqrt(HosszN()); }
			/// <summary>
			/// Vektor forgatása pozitív, azaz az óramutató járásával ellenkező irányba
			/// </summary>
			public void ForgNeg() { int temp = X; X = -Y; Y = temp; }// ez most azért működik fordítva, mert fordítva van a koordinátarendszer is.
																	 /// <summary>
																	 /// Vektor forgatása negatív, azaz az óramutató járásával megegyező irányba.
																	 /// </summary>
			public void ForgPoz() { int temp = Y; Y = -X; X = temp; }
			/// <summary>
			/// A vektorhoz hozzáadunk egy másik vektort.
			/// </summary>
			/// <param name="P"></param>
			public void Add(Vektor V) { X += V.X; Y += V.Y; }
			public Vektor Plusz(Vektor V) { return new Vektor(X + V.X, Y + V.Y); }
			public Vektor Minusz(Vektor V) { return new Vektor(X - V.X, Y - V.Y); }
			/// <summary>
			/// A vektort megszorozzuk egy skalárral
			/// </summary>
			/// <param name="V"></param>
			public void Ska(int a) { X *= a; Y *= a; }
			/// <summary>
			/// A vektorból kivonunk egy másik vektort.
			/// </summary>
			/// <param name="V"></param>
			public void Rem(Vektor V) { X -= V.X; Y -= V.Y; }
			/// <summary>
			/// A négy égtáj fele mutató irányvektort lekódoljuk egy int-be. 
			/// 0: észak
			/// 1: kelet
			/// 2: dél
			/// 3: nyugat
			/// </summary>
			/// <returns></returns>
			public int ToInt() { return Y == -1 ? 0 : (X == 1 ? 1 : (Y == 1 ? 2 : 3)); }
			/// <summary>
			/// Az adott pont egy másik ponttól való távolságának négyzetét adja meg.
			/// </summary>
			/// <param name="Q"></param>
			/// <returns></returns>
			public int TavN(Vektor Q) { return Matek.Négyzet(X - Q.X) + Matek.Négyzet(Y - Q.Y); }
		}

		/// <summary>
		/// Bármilyen számokkal kapcsolatos nemtriviális matematikai műveletet ide lehet beírni. Eredetileg itt voltak a vektorműveletek is, de aztán átpakoltam a Vektor classba.
		/// </summary>
		static class Matek
		{
			/// <summary>
			/// Egy számot négyzetre emel
			/// </summary>
			/// <param name="x"></param>
			/// <returns></returns>
			public static int Négyzet(int x) { return x * x; }
		}

		class Lövedék
		{
			protected int szín = piros;
			protected Vektor H;
			protected Vektor I;
			public Lövedék(Robot robot, int inszín=hó)
			{
				szín = inszín;
				H = robot.GetHely();
				I = robot.GetIrány();
			}
			public Vektor GetHely() { return new Vektor(H); }
			public Vektor GetIrány() { return new Vektor(I); }
			public int GetSzín() { return szín; }
			public void Repül()
			{
				H.Add(I);
				if (pálya.MiVanItt(H) == -1 || pálya.MiVanItt(H) == fal)
				{
					H.Rem(I);
					koppanók.Add(this);
				}
			}

		}
		class Robot
		{
			protected string név;
			protected Vektor H;
			protected Vektor I = new Vektor(0, 1);
			protected int[] kődb;
			protected Bitmap[] képkészlet;
			/// <summary>
			/// Létrehoz egy új robotot a megadott névvel, képkészlettel, pozícióval, iránnyal és induló kövek számával.
			/// </summary>
			/// <param name="adottnév">A robot neve</param>
			/// <param name="képek">A képkészlet a Resources mappából</param>
			/// <param name="Pozíció">indulási pozíció</param>
			/// <param name="Irány">kezdőirány</param>
			/// <param name="indulókövek">induláskor a zsebeiben lévő kövek száma</param>
			public Robot(string adottnév, Bitmap[] képek, Vektor Pozíció, Vektor Irány, int[] indulókövek)
			{
				név = adottnév;
				H = Pozíció;
				I = Irány;
				képkészlet = képek;
				kődb = indulókövek;
			}

			private int töltődés = maxtöltődés;
			protected int MikorLőhetek(){return töltődés;}

			public bool Van_e_előttem_fal = false;
			public bool Kilépek_e_a_pályáról = false;
			public int Szemközti_fal_távolsága =-1;
			public int Hőmérséklet = -1;
			public bool Előre_megy = false;
			public bool Hátra_megy = false;
			public bool Jobbra_forog = false;
			public bool Balra_forog = false;
			public int lerakandó_kő = fekete;
			public bool Követ_rak_le = false;
			public bool Felveszem_ami_itt_van = false;
			public bool Hógolyó_tart_felém = false;
			public bool Hógolyó_fog_elmenni_a_jobb_fülem_mellett = false;
			public bool Hógolyó_fog_elmenni_a_bal_fülem_mellett = false;

			protected void Visszajelzés(string üzenet="")
			{
				monitorpanel.debug(DebugInfo(), üzenet);
			}

			protected void Motorok(int b, int e, int j)
			{
				Balra_forog = b == 1;
				Előre_megy = e == 1;
				Jobbra_forog = j == 1;
			}
			protected void Motorok(int b, int e, int j, int h)
			{
				Motorok(b, e, j);
				Hátra_megy = h == 1;
			}
			protected void Leáll()
			{
				Előre_megy = false;
				Hátra_megy = false;
				Jobbra_forog = false;
				Balra_forog = false;
				Követ_rak_le = false;
				Felveszem_ami_itt_van = false;
			}

			protected void Motor(int b, int j)
			{
				if (b > 0) { Előre_megy = j >= 0; Balra_forog = false; Jobbra_forog = j <= 0; Hátra_megy = false; }
				else if (b == 0) { Előre_megy = j > 0; Balra_forog = j > 0; Jobbra_forog = j < 0; Hátra_megy = j < 0; }
				else if (b < 0) { Előre_megy = false; Balra_forog = j >= 0; Jobbra_forog = false; Hátra_megy = j <= 0; }
			}

			protected int HógolyóSzenzor(int sáv) // balra = -1, szembe = 0, jobbra = 1
			{ IEnumerable<Lövedék> szembejövők =
					lövedéklista
						.Where
						(g =>
							Math.Abs(g.GetIrány().ToInt() - I.ToInt()) == 2 // a mozgó lövedék ellentétes irányba mozog, mint amerre a robot épp néz
							&& (I.ToInt() % 2 == 0
								? g.GetHely().X + sáv * I.Y == H.X && Math.Sign(g.GetHely().Y - H.Y) != Math.Sign(g.GetIrány().Y - I.Y) // iránytól függően azonos oszlopban vagy azonos sorban vannak, és emellett Karesz a lövedék irányában helyezkedik el.
								: g.GetHely().Y - sáv * I.X == H.Y && Math.Sign(g.GetHely().X - H.X) != Math.Sign(g.GetIrány().X - I.X)
								)
						);
				if (szembejövők.Count() != 0)
				{
					return szembejövők.Min(g => g.GetHely().Minusz(H).Hossz()-1);
				}
				else return -1;
			}

			protected virtual void KÖR(){}
			public void MűködikEgyet()
			{
				// Szenzorokból bejövő adatok:

				Van_e_előttem_fal = MiVanElőttem() == fal;
				Kilépek_e_a_pályáról = (MiVanElőttem() == -1);
				Szemközti_fal_távolsága = UltrahangSzenzor();
				Hőmérséklet = Hőmérő();
				Hógolyó_tart_felém = HógolyóSzenzor(0)>-1;
				Hógolyó_fog_elmenni_a_bal_fülem_mellett = HógolyóSzenzor(-1)>-1;
				Hógolyó_fog_elmenni_a_jobb_fülem_mellett = HógolyóSzenzor(1)>-1;

				// Ezt programozza a diák:
				KÖR();

				// Mozgatások: 
				if (Felveszem_ami_itt_van) Felvesz();
				if (Követ_rak_le) Lerak(lerakandó_kő);
				if (Előre_megy) Lép();
				if (Hátra_megy) Tolat();
				if (Jobbra_forog) Fordul(jobbra);
				if (Balra_forog) Fordul(balra);

				if (töltődés > 0) töltődés--;
			}

			/// <summary>
			/// Létrehoz egy új robotot a megadott névvel és induló kövek számával az 5,28 helyen északra nézvén.
			/// </summary>
			/// <param name="adottnév">A robot neve</param>
			/// <param name="indulókövek">induláskor a zsebeiben lévő kövek száma</param>
			public Robot(string adottnév, int[] indulókövek)
			{
				név = adottnév;
				H = new Vektor(5, 28);
				I = new Vektor(0,-1);
				képkészlet = new Bitmap[4]
					{
						Properties.Resources.Karesz0,
						Properties.Resources.Karesz1,
						Properties.Resources.Karesz2,
						Properties.Resources.Karesz3
					};
				kődb = indulókövek;
				robotlista.Add(this);
			}

			/// <summary>
			/// Létrehoz egy új üres zsebű kék robotot a megadott névvel az 5,28 helyen északra nézvén.
			/// </summary>
			/// <param name="adottnév">A robot neve</param>
			public Robot(string adottnév)
			{
				név = adottnév;
				H = new Vektor(5, 28);
				I = new Vektor(0, -1);
				képkészlet = new Bitmap[4]
					{
						Properties.Resources.Karesz0,
						Properties.Resources.Karesz1,
						Properties.Resources.Karesz2,
						Properties.Resources.Karesz3
					};
				kődb = new int[5] { 0, 0, 0, 0, 0 };
				robotlista.Add(this);
			}
			public Robot()
			{
				név = "robot-"+robotlista.Count;
				H = new Vektor(5, 28);
				I = new Vektor(0, -1);
				képkészlet = new Bitmap[4]
					{
						Properties.Resources.Karesz0,
						Properties.Resources.Karesz1,
						Properties.Resources.Karesz2,
						Properties.Resources.Karesz3
					};
				kődb = new int[5] { 0, 0, 0, 0, 0 };
//				robotlista.Add(this);
			}
			/// <summary>
			/// Elhelyezi a Robotot a megadott helyre.
			/// </summary>
			/// <param name="x"></param>
			/// <param name="y"></param>
			public string Neve() { return név; }
			public void Teleport(int x, int y) { H.X = x; H.Y = y; }
			/// <summary>
			/// Visszaadja a robot koordinátáit.
			/// </summary>
			/// <returns></returns>
			public Vektor GetHely() { return new Vektor(H); }
			/// <summary>
			/// Frontálisan ütközik két robot vagy egy hógolyó és egy robot, ha visszaléptetve őket az előző helyükre, a mostani pozíciókat kapjuk.
			/// </summary>
			/// <returns></returns>
			public bool FrontálisanÜtközik(Robot robot) { return H.Minusz(I).Egybeesik(robot.H) && robot.H.Minusz(robot.I).Egybeesik(H); }
			public bool FrontálisanÜtközik(Lövedék golyó) { return H.Minusz(I).Egybeesik(golyó.GetHely()) && golyó.GetHely().Minusz(golyó.GetIrány()).Egybeesik(H); }
			/// <summary>
			/// Visszaadja a robot irányát
			/// </summary>
			/// <returns></returns>
			public Vektor GetIrány() { return new Vektor(I); }
			/// <summary>
			/// Megadja, hogy az adott színből mennyi köve van a robotnak.
			/// </summary>
			/// <param name="szín"></param>
			/// <returns></returns>
			public int Mennyi(int szín) { return kődb[szín - 2]; }
			/// <summary>
			/// Lépteti a robotot a megfelelő irányba.
			/// </summary>

			private string DebugInfo()
			{
				return "(" + óra.GetIdő() + ";"+H.X+","+H.Y+") "+név;
			}

			private void Lép()
			{
				if (MiVanElőttem() == -1 || MiVanElőttem() == fal)
				{
					if (MiVanElőttem() == -1) monitorpanel.debug(DebugInfo(), " kizuhant");
					if (MiVanElőttem() == fal) monitorpanel.debug(DebugInfo(), " falba ment");
					haldoklók.Add(this);
				}
				else { H.Add(I); }
			}

			/// <summary>
			/// Lépteti a robotot az ellenkező irányba.
			/// </summary>
			private void Tolat()
			{
				H.Rem(I); // Ahova lépni készül.
				if (!pálya.BenneVan(H) || pálya.MiVanItt(H) == fal) 
				{
					if (!pálya.BenneVan(H)) monitorpanel.debug(DebugInfo(), " kizuhant");
					if (pálya.MiVanItt(H) == fal) monitorpanel.debug(DebugInfo(), " falba ment");
					haldoklók.Add(this);
					H.Add(I); 
				}
			}
			/// <summary>
			/// Elforgatja a robotot a megadott irányban. (Csak normális irányokra reagál.)
			/// </summary>
			/// <param name="forgásirány"></param>
			public void Fordul(int forgásirány){if (forgásirány == balra) I.ForgPoz(); else I.ForgNeg();}
			/// <summary>
			/// Visszaadja a sebességvektor számkódját, ami a képek kezeléséhez kell.
			/// </summary>
			/// <returns></returns>
			public Bitmap Iránykép() { return képkészlet[I.ToInt()]; }
			/// <summary>
			/// Lerakja az adott színű követ a pályán a robot helyére.
			/// </summary>
			/// <param name="szín"></param>
			private void Lerak(int szín = fekete)
			{
				if (pálya.MiVanItt(H) != üres)
					monitorpanel.debug(DebugInfo() ,": Itt már van");
				else if (kődb[szín - 2] <= 0)
					monitorpanel.debug(DebugInfo() ,": Elfogyott");
				else
				{
					pálya.LegyenItt(H, szín);
					--kődb[szín - 2];
				}
			}
			/// <summary>
			/// Felveszi azt, amin éppen áll -- feltéve ha az nem fal, stb.
			/// </summary>
			private void Felvesz()
			{
				if (pálya.MiVanItt(H) > fal)
				{
					++kődb[pálya.MiVanItt(H) - 2];
					pálya.LegyenItt(H, üres);
				}
//				else monitorpanel.debug(DebugInfo() , ": itt nincs kő");
			}
			/// <summary>
			/// Megadja, hogy kavicson áll-e a robot.
			/// </summary>
			/// <returns></returns>
			protected bool VanKavics() { return pálya.MiVanItt(H) > fal; }
			/// <summary>
			/// Megadja, hogy min áll a robot
			/// </summary>
			/// <returns></returns>
			public int MiVanItt() { return pálya.MiVanItt(H); }
			/// <summary>
			/// Megadja, hogy mi van a robot előtt
			/// </summary>
			/// <returns></returns>
			private int MiVanElőttem()
			{
				/*  1, ha fal van előtte
				 * -1, ha kilép a pályáról;*/

				H.Add(I); // Ahova lépni készül.
				if (pálya.BenneVan(H))
				{
					int result = pálya.MiVanItt(H);
					H.Rem(I);
					return result;
				}
				else { H.Rem(I); return -1; }
			}
			protected int UltrahangSzenzor()
			{
				int d = 0;
				Vektor J = new Vektor(H);
				while (
						pálya.MiVanItt(J) != -1 
					&&	pálya.MiVanItt(J) != 1 
					&&  robotlista.Where(r => r.GetHely().Egybeesik(J) && r!=this).Count()==0
					)
				{
					J.Add(I);
					d++;
				}
				if (pálya.MiVanItt(J) == -1)
				{ return -1; }
				else 
				{ return d; }
			}
			public int Hőmérő() { return pálya.Hőmérséklet(H); }
			protected void Dob(int szín = hó)
			{
				if (kődb[szín - 2] > 0 && töltődés == 0)
				{
					--kődb[szín - 2];
					lövedéklista.Add(new Lövedék(this, szín));
					töltődés = maxtöltődés;
				}
				else if (kődb[szín - 2] == 0) Visszajelzés("out of ammo");
				else Visszajelzés("charging: " + töltődés.ToString());
			}
		}


		public Form1()
		{
			INICIALIZÁLÁS();
			monitorpanel.megfigyeltrobot = robotlista[0];

			((System.ComponentModel.ISupportInitialize)(pálya)).BeginInit();
			monitorpanel.SuspendLayout();
			SuspendLayout();

			// Controlok
			Controls.Add(Betölt);
			Controls.Add(pályaválasztó);
			Controls.Add(pályafelirat);

			// Form1
			AutoScaleDimensions = new SizeF(8F, 16F);
			AutoScaleMode = AutoScaleMode.Font;
			AutoSize = true;
			ClientSize = new Size(monitorpanel.Size.Width+pálya.Size.Width+35, pálya.Size.Height+15);
			Controls.Add(monitorpanel);
			Controls.Add(startgomb);
			Controls.Add(pálya);
			Margin = new Padding(4, 4, 4, 4);
			Name = "Form1";
			Text = "Karesz2";
			((System.ComponentModel.ISupportInitialize)(pálya)).EndInit();
			monitorpanel.ResumeLayout(false);
			monitorpanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();

			// Eseménykezelés
			pálya.Paint += new PaintEventHandler(pálya_Paint);
			pálya.MouseDown += new MouseEventHandler(pálya_MouseDown);
			Betölt.Click += new System.EventHandler(Betölt_Click);
			startgomb.Click += new System.EventHandler(startgomb_Click);

			pálya.Betölt(pályaválasztó.Text);
		}

		/// <summary>
		/// Végrehajtja a feladatot
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void startgomb_Click(object sender, EventArgs e)
		{
			if (startgomb.Be()) { óra.Start(); } else { óra.Stop(); }
			monitorpanel.Enged(!startgomb.Be());
		}

		/// <summary>
		/// Lerajzolja a Pályánál található függvénnyel.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void pálya_Paint(object sender, PaintEventArgs e){pálya.Rajz(pálya, e);}

		/// <summary>
		/// Ha valaki a pályára kattint, akkor Kareszt leteszi oda.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected void pálya_MouseDown(object sender, MouseEventArgs e)
		{
			pálya.Refresh();
			monitorpanel.Frissít();
			if (robotlista.Count>0) monitorpanel.megfigyeltrobot.Teleport(e.X/pálya.lépték.X, e.Y/ pálya.lépték.Y);
			pálya.Refresh();
			monitorpanel.Frissít();
		}

		protected void Betölt_Click(object sender, EventArgs e){pálya.Betölt(pályaválasztó.Text);} // ITTEN kellene még dolgozni, mert hogy most nem működik az inicializálás


		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected System.ComponentModel.IContainer components = null;

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
	}
}