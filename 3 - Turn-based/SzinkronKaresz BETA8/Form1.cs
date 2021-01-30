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
	public partial class Form1 : Form
	{
		/* Innentől lehet úgy robotokat létrehozni. 
		 * Az INICIALIZÁLÁS metódusban alapjáratban egy SajátRobot típusú robotot hoz létre a program.
		 * Ha szeretnél saját nevet a robottípusodnak, a "SajátRobot"-ra futtass le egy teljes dokumentumos cserét (CTRL+H).
		 * 1) Ha több robotot szeretnél létrehozni ugyanabból a robottípusból, akkor elég az INICIALIZÁLÁS metódust bővíteni. 
		 * 2) Ha több robotot szeretnél létrehozni más és más robottípusból, 
		 *		akkor a "class SajátRobot : Robot {...}" részt kell copy-pastelgetni, és a SajátRobot nevű részt átírni.
		 *		az ezeknek megfelelő robotokat pedig a megfelelő típus szerint létrehozni az Inicializálásban.
		 */
		void INICIALIZÁLÁS()
		{
			// Itt lehet deklarálni és létrehozni a robotokat.
			//Golyószóró Gyuszi = new Golyószóró("Golyószóró");
//			Golyószóró Terminátor = new Golyószóró("Terminátor");
			Felderítő Gyuszi = new Felderítő("Gyuszi");
			// Setup:
			óra.Interval = 20;      // itt a default gyorsaság. Minél kisebb, annál gyorsabb.
			fájlnév = "palya03";    // itt a default pálya, amit betölt a program az induláskor
		}
		// Innentől lehet új robotokat létrehozni. Ha egyazon típusú robotból kell több, a
		class Felderítő : Robot
		{
			public Felderítő(string adottnév) // ehhez nem kell hozzányúlni.
			{
				név = adottnév;
				H = new Vektor(35, 5);
				int ri = 0;
				I = new Vektor(ri == 0 || ri == 2 ? 0 : ri == 1 ? 1 : -1,
								ri == 1 || ri == 3 ? 0 : ri == 0 ? 1 : -1);
				képkészlet = new Bitmap[4]
					{
						Properties.Resources.Karesz0,
						Properties.Resources.Karesz1,
						Properties.Resources.Karesz2,
						Properties.Resources.Karesz3
					};
				kődb = new int[5] { 0, 0, 0, 0, 1000 };
				robotlista.Add(this);
			}
			// Innentől lehet változókat bevezetni és eljárásokat írni. 

			// A KÖR metódusban leírtakat futtatja le körönként a program.
			protected override void KÖR()
			{
				int balrajön = HógolyóSzenzor(balra);
				int szembejön = HógolyóSzenzor(szemben);
				int jobbrajön = HógolyóSzenzor(jobbra);


				Visszajelzés("(" + balrajön.ToString() + "," + szembejön.ToString() + "," + jobbrajön.ToString() + ")");
			}
		}
	}
}