using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Data_Ran;
using static Data_Ran.DR;

namespace ISTQB_QA                  //ha nem 40 kérdést akarsz próbálni, hanem csak többet annál, minden progressBar++-t ki kell kapcsolni, mert 40 után leáll
{
    public partial class ISTQBmain : Form
    {

        public List<QA> QAList = new List<QA>();        //ez először az összes kérdés lesz - aztán átíródik csak 40 randomra
        public int QSzám=0;                             //ezzel nézi, hanyadik kérdésnél járunk és ez alapján kerül kiírásra a label_szamlalo is
        private int wasRight = 0;                       //számolja, hányszor találta el a felhasználó a helyes választ a futás alatt
        public int timeleft= 4500;                      //75perc
        public ISTQBmain()
        {
            InitializeComponent();
        }

        private void ISTQBmain_Load(object sender, EventArgs e)                                 //ez az On_Load
        {
            timer1.Start();                                                                     //elindul a timer

            string firstLine = "";                                                              //tartja a páratlan sort (ami mindig kérdés)
            foreach (var line in File.ReadAllLines(@"QA.txt").Where(filine=> filine!=""))       //végigpörgeti a QA.txt-t úgy, hogy minden üres sort kihagy
            {
                if (line.Contains("[Q]"))                                                       //ha a sorban [Q] van, akkor az kérdés sor lesz, ez megy a "firstline"-ba
                {
                    firstLine = line;  
                }
                else
                {                                                                               //ha nincs benne Q, akkor az egy answer sor, amit hozzáfüzünk egy |-al elválasztva az Q sorhoz
                    QAList.Add(new QA(firstLine + "|" + line));                                 //aztán meghívom a QA típus építőjét és új elemként odaadom neki ezt a sort - hozzáadva a QAListához
                }                                                                               //mire átmegy az építőn ebből kérdés, lista, válasz, kép, stb minden kijön szétválogatva
            }

            QAList = RandomQuestions(QAList);                           //megkérem, hogy a már kész, minden elemet tartalmazó QAListát helyettesítse azzal, amit majd a RandomQ visszaad - itt az elemszám 40re csökken
            ToDefault();                                                //alaphelyzetbe állítom a felületet (rádiógombok, színek, méretek)
            NextQuestion();                                             //meghívom a következő kérdést (akár az elsőt is)
            
        }

        #region TOVÁBB gomb és OKÉ gomb
        private void buttonOK_Click(object sender, EventArgs e)     //oké gomb, ami csak rossz válasz esetén látható
        {

            ToDefault();            //meghívja a visszaállítót
            QSzám++;                //növeli a questszámot
            NextQuestion();         //behívja a következő kérdést
        }

        private void button_tovabb_Click(object sender, EventArgs e)
        {
            if (progressBar.Value<39)                                                        //míg a progressBar értéke nem éri el az utolsó előttit, addig:
            {
                bool isTheUserRight = CTRLcheck(QAList[QSzám].rightAnswer.Count);            //meghívom az ellenőrzőt, ami megnézi, h a felhasználó melyik rádiógombot választotta mielőtt tovább gombra nyomott

                if (isTheUserRight)                                                          //ha true értéke lett a fenti vizsgálatnak
                {
                    QSzám++;                                                                 //növelem a question sorszámát (ami a labelt is növeli majd)
                    if (QSzám<QAList.Count-1)                                                //míg nem értük el a lista utolsó indexét
                    {
                        NextQuestion();                                                      //újrahívom a következő kérdést
                    }
                    else
                    {
                            EndQ();                                                          //ha elértem az utolsó indexet, elmegyek a program végére
                    }
                }
                else                                    //ha a user elrontotta a választ, az ellenőrzés false-t dobott vissza:
                {               
                    labelFalse.Visible = true;                                               //megjeleníti a szöveget a syllabusszal
                    if (QAList[QSzám].isSyllabus)                                            //ha volt syllabus hivatkozás:
                    {
                        labelFalse.Text = "A válaszod helytelen volt. A syllabus itt hivatkozik rá: " + QAList[QSzám].syllabus; //beírja azt,
                    }
                    else                                                                     //ha nem volt hivatkozás:
                    {
                        labelFalse.Text = "A válaszod helytelen volt. És azt sem tudom, hol van ez a syllabusban...";           //csak kiírja h nem tudja
                    }
                
                    UserFail(QAList[QSzám].rightAnswer.Count);                               //meghívja a hiba esetén értesítendő személyt... átadja nekiaz aktuális helyes válaszok számát
                }
                progressBar.Value++;                                                         //növeli a progress bart eredménytől függetlenül
            }
            else                                      //ha a progressbar majdnem betelt (itt valami számolási hibám van azért ilyen bonyolult), akkor vége a feladatnak
            {
                progressBar.Value = 40;               //kitölti a bar utolsó szakaszát
                EndQ();                               //és a program végére küld.
            }
        }
        #endregion

        #region check the checkers --eldönti, hogy a user input helyes volt-e
        private bool CTRLcheck(int valasz)                                              //váaszlehetőségek számát adja fel (radio vagy check)
        {
            List<char> ans = new List<char>();                                          //létrehoz a lehetségesválaszoknak egy listát
            
            if (valasz>1)                                                               //ha checkbox
            {
                foreach (CheckBox ctrl in panelCheck.Controls.OfType<CheckBox>())       //megnézi a checkboxok közül
                {
                    if (ctrl.Checked)                                                   //melyek vannak bepipálva
                    {
                        ans.Add(ctrl.Name.Last());                                      //és a bepipált boxok nevének utolsó betűjét hozzáadja az ans listához (checkA, checkB...)
                    }
                }

            }
            else                                                                        //ha ez radiogombos feladat:
            {   
                foreach (RadioButton ctrl in panelRadio.Controls.OfType<RadioButton>()) //megnézi, hogy a radiogombok közül
                {
                    if (ctrl.Checked)                                                   //melyik van bejelölve
                    {
                        ans.Add(ctrl.Name.Last());                                      //és a nevét hozzáadja az ans listához
                    }                    
                }
            }                                                           

            if (QAList[QSzám].rightAnswer.SequenceEqual(ans))            //HA az ans lista elemei (tartalma) megegyetik a helyes válaszok elemeivel               
            {
                //Console.WriteLine("true");                          
                wasRight++;                                              //akkor a helyes válasz számlálót feljebb veszi és
                return true;                                             //az ellenőrzés TRUE értéket fog visszaadni a hívónak
            }
            else
            {
                //Console.WriteLine("false");
                return false;                                           //ha nem egyeztek a listák, a hívó a FALSE választ kapja
            }
        }
        #endregion

        #region HIBÁS válasz
        private void UserFail(int valasz)                                                   //ha hibás válasz érkezett a felhasználótól, ez fut le
        {
            Random rnd = new Random();                                                      //randomot készítek
            if (valasz > 1)                                                               //ha a válaszlehetőségek száma nagyobb volt, mint egy (azaz többválasztós volt)
            {
                foreach (CheckBox c in panelCheck.Controls.OfType<CheckBox>())          //a checkboxokat tároló panel összes checkboxát megnézem
                {
                    if (QAList[QSzám].rightAnswer.Any(i => i == c.Name.Last()))           //HA a box nevének utolsó karaktere egyezik a helyes válasszal (checkA, checkB...)
                    {
                        c.BackColor = Color.Green;                                      //az adott boxot színezze zöldre, a betűit meg fehérre
                        c.ForeColor = Color.White;
                    }
                }
            }
            else                                                                        //ha 1 féle helyes válasz volt, akkor az radio gobos kérdés
            {
                foreach (RadioButton c in panelRadio.Controls.OfType<RadioButton>())    //minden radiogombot, ami a radio panelben van megnéz
                {
                    if (QAList[QSzám].rightAnswer.Any(i => i.Equals(c.Name.Last())))    //ha a radiogomb nevének a vége egyezik a helyes megoldás betűjével (radA,radB...)
                    {
                        c.BackColor = Color.Green;                                      //az adott radiot zöldre színezi, a betűit fehérre
                        c.ForeColor = Color.White;
                    }
                }
            }

            button_tovabb.Visible = false;                  //közben láthatatlanná teszi a Tovább gombot ÉS                     
            buttonOK.Visible = true;                        //a helyére láthatóvá teszi az Oké-t (mivel a 2nek különböző funkciói vannak)
            pictureBox2.Image = Image.FromFile(Path.Combine("Pictures\\", "userfail" + rnd.Next(1, 5) + ".gif")); //a kép helyére random generált gifet tesz összefűzéssel
        }
        #endregion

        #region next Qestion - következő kérdést adja be
        private void NextQuestion()
        {
            ToDefault();                                                                            //az ablakot defaultra állítja
            label_szamlalo.Text = (QSzám+1) + "/40"+" ("+wasRight+")";                              //a számláló labelen módosítja az állást az aktuális questszám és eddig eltalált válaszok alapján
            pictureBox2.Image = Image.FromFile(Path.Combine("Pictures\\", QAList[QSzám].picture));  //berakja az aktuális kérdéshez tartozó képet
            
            if (QAList[QSzám].rightAnswer.Count>1)          //ha több válaszlehetőség van, mint 1 (azaz checkbox)
            {
                panelCheck.Visible = true;                  //eltűnteti a rádiógombokat és megjeleníti a checkboxokat
                panelRadio.Visible = false;
               
                checkA.Text = QAList[QSzám].answers[0];     //a megfelelő helyxre eírja a kérdéshez tartozó válaszlehetőségeket
                checkB.Text = QAList[QSzám].answers[1];
                checkC.Text = QAList[QSzám].answers[2];
                checkD.Text = QAList[QSzám].answers[3];
            }
            else                                            //ha egy válasz lehet (radio)
            {
                panelCheck.Visible = false;                 //eltűnteti a checkboxokat (ha ne adj isten lettek volna) és megjeleníti a radiokat
                panelRadio.Visible = true;
                
                radA.Text = QAList[QSzám].answers[0];       //a kérdéshez tartozó válaszokat beírja a radiogombok mellé
                radB.Text = QAList[QSzám].answers[1];
                radC.Text = QAList[QSzám].answers[2];
                radD.Text = QAList[QSzám].answers[3];
            }

            string lab = "";                                                        //készít egy ideiglenes stringet, amihez
            foreach (var item in QAList[QSzám].mainQszoveg)                         //megnézi az aktuális kérdéshez tartozó main szöveg minden elemét (Q,L)
            {
                if (item.Contains("[L]"))                                           //ha az adott elem [L] lista megjelöléssel rendelkezik
                {
                    int index = Convert.ToInt32(item.Substring(3)) - 1;             //levágja a megjelölést, így egy számot kap (ami a lista sorszáma volt), -1el megkapja az indexét az adott listának a listák között ( [L]1, [L]2 ...)
                    foreach (var listitem in QAList[QSzám].list[index].Split('~'))  //list[index]-en tárolt strrtinget végiglapozza a ~ jelek mentén ideiglenesen vágva
                    {
                        lab += "     " + listitem + "\n";                           //hozzáad a labhoz 1 tabot, az aktuális listaelemet és egy sortörést
                    }
                    lab += "\n";                                                    //minden lista után entert üt a lab-ba
                }
                //else if (item.Contains("[I]"))                      //HA LENNE IDÉZET... 
                //{
                //    lab += "\n" + item.Substring(3) + "\n";
                //}
                else
                {
                    lab += item + "\n";                                             //ha nem tartalmazza a [L] jelölőt az elem, akk az csak a Q része, ezt új sor kíséretében csapja hozzá a lab-hoz
                }
            }
           
            label_Question.Text = lab;                                              //kiírja a lab szövegét a Question labelbe.
        }
        #endregion

        #region todefault - visszaállít minden controlt alaphelyzetbe
        private void ToDefault()
        {
            button_tovabb.Visible = true;           //alapból a tovább gomb látható, az Oké nem
            buttonOK.Visible = false;
           
                foreach (RadioButton c in panelRadio.Controls.OfType<RadioButton>())    //alapból minden checkbox checkeletlen, control háttérszínű, fekete betűkkel
                {
                    c.Checked = false;
                    c.BackColor = SystemColors.Control;
                    c.ForeColor = Color.Black;
                }
          
                foreach (CheckBox c in panelCheck.Controls.OfType<CheckBox>())          //minden radio jelöletlen, control háttérszínű, fekete betűkkel
            {
                    c.Checked = false;
                    c.BackColor = SystemColors.Control;
                    c.ForeColor = Color.Black;
                }
           
            labelFalse.Visible = false;                   //a hiba label, a syllabussal rejtett
        }
        #endregion

        #region end - program vége
        public void EndQ()
        {

            button_tovabb.Visible = false;                  //leveszi a tovább gombot
            panelCheck.Visible = false;                     //elveszi a válaszpaneleket
            panelRadio.Visible = false;
            pictureBox2.Image = Image.FromFile(@"Pictures\finished.png");  //kirakja a kép helyére a csillagot
            double jovot = ((double)wasRight / 40) * 100;                   //kiszámolja a %-ot
            if (timeleft == 0)                                //HA force-léptettek ide, mert az idő lejárt:
            {
                label_Question.Text = "Sajnos lejárt az időd!\n \n40-ből " + wasRight + " volt helyes a válaszaid közül, ami " + String.Format("{0:0.00}", jovot) + "%.";
            }
            else                                            //ha maradt idő, de elfogyott a kérdés:
            {
                label_Question.Text = "Gratulálok! A végére értél!\n\n40-ből " + wasRight + " volt helyes a válaszaid közül, ami " + String.Format("{0:0.00}", jovot) + "%.";
            }
        }
        #endregion
        private void timer1_Tick(object sender, EventArgs e)        //az időzítő tickere - mindenmásodpercben tickel
        {
            if (timeleft > 0)                                       //ha az idő nem járt még le
            {
                if (timeleft<60)                                            //HA kevesebb időm van, mint 60 sec
                {
                   
                    label_timer.ForeColor = Color.Red;                      //az időt kiíró label színe pirosra vált és megjeleníti az időt másodpercenként
                    label_timer.Text = (int)(timeleft) + " secounds";
                }
                else                                                        //ha még több van hátra, mint egy perc, csak percenként vált időt és percben méri, színe fekete
                {                   
                    label_timer.Text = (int)(timeleft/60) + " minutes";
                }
                timeleft = timeleft - 1;                            //amíg van idő, minden tick végén 1-el csökkenti a hátralevő secek számát
            }
            else                                                    //ha lejárt az idő:
            {
                timer1.Stop();                                      //megállítja a timert
                label_timer.Text = "Time's up!";                    //kiírja az óra helyére, h lehjárt az idő               
                EndQ();                                             //és force-léptet a program végére
            }
        }
        
    }
}
