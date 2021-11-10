using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ISTQB_QA;

namespace Data_Ran
{
    public class DR
    {
        #region DATA   ---- minden kérdés és válasz QA osztályú
        public class QA                                                 // minden QA TÍPUSÚ adatnak az alábbi tulajdonságai vannak:
        {
            public List<string> mainQszoveg = new List<string>();       // a kérdés részei
            public string picture;                                      // a kép neve
            public List<string> list = new List<string>();              // lista (amiben minden listaelem egy string, egy-egy darabolatlan lista)
            public List<string> answers = new List<string>();           // a válaszlehetőségek (ebből veszi a radio és ckeck az adatait)
            public List<char> rightAnswer = new List<char>();           // karakterlista a helyes válaszok betűjével
            public bool isPicture;                                      // volt-e kép a feladatlleírásban gyárilag?
            public string syllabus;                                     // syllabus hely
            public bool isSyllabus;                                     // volt-e syllabus hivatkozás gyárilag?

            public QA(string twoLines)                                  //típus építő (ami egy stringként kapja meg az kérdés- és válasz sort |-al elválasztva)
            {
                
                List<string> Q = twoLines.Split('|').ElementAt(0).Split('#').ToList();      //ez a kérdéslista, a string első fele a |-tól, feldarabolva #-ek mentén (minden Q,L,P,S)                           
                string[] A = twoLines.Split('|').ElementAt(1).Split('#');                   //ez a válasz tömb

                foreach (var item in Q)                                                     //végiglapozom a kérdéslistát
                {
                    switch (item.Substring(0, 3))                                           //amit vizsgálok, hogy a listaelelm első 3 karaktere mi: [Q] [L] [P] [S]
                    {
                        case "[Q]":
                            mainQszoveg.Add(item.Substring(3));                             //ha Q, akkor az main szöveg egyéb formázás nélkül
                            break; 
                        case "[L]":
                            list.Add(item.Substring(3));                                    //ha L, akkor a TÍPUS listához hozzáadom az egész [L] utáni darabot (~-kel együtt)
                            mainQszoveg.Add("[L]" + (list.Count).ToString());               //és a  main szöveghez hozzáfűzöm pl: [L]1 , ahol az 1 a lista lista hossza volt (ebből lesz az index)       
                            break;
                        case "[S]":
                            syllabus = item.Substring(3);                                   //ha S, a syllabus megkapja azt a szövegdarabot, és igaz lesz h van syllabus hivatkozása
                            isSyllabus = true;
                            break;
                        case "[P]":
                            picture = item.Substring(3);                                    //ha P, a képhez bekerül a képnév és igaz lesz, h volt eredetileg hozzá kép (nem kell generálni)
                            isPicture = true;
                            break;
                    }
                    if (!isPicture)                                                //ha nem volt kép (isPicture=false (c#ban a default érték false))   
                    {
                        Random rnd = new Random();                                 //készítek egy randomot
                        picture = "noimg"+rnd.Next(10,24)+".jpg";                  //és a kép neve az lesz, h noimage+randomszám
                    }
                }

                for (int i = 0; i < A.Length; i++)                       //ez rendezi a válaszokat (amiket az A nevű tömbbe raktam fent)
                {
                    char betu = 'A';                                     //A betűről indulok (ami asszem értékre 65 idk)
                    if (A[i].Contains("*"))                              //ha a tömb i. eleme tartalmazza a *-ot (azaz helyes válasz)
                    {
                        rightAnswer.Add((char)(betu+i));                 //a helyes válaszokat tartalmazó listába tegye be az aktuális betűt megnövelve az i értékével (A+0=A, A+1=B stb)
                        answers.Add(A[i].Substring(1));                  //és a választ a csillag nélkül tegye be a válaszokat tartalmazó listába
                    }
                    else
                    {
                        answers.Add(A[i]);                               //ha a válasz alapjáraton hibás (nincs benne csillag), akkor csak tegye be a válaszlistába
                    }
                }
            }
        }
        #endregion

        #region random quest set       -- ez készíti el a 40 db-ból álló készletet a teljes QA.txtből                   ..... ebben kell a while (idx.Count < 40)  40-et kisebbre vagy nagyobbra venni ha 1 vagy akárhány kérdést akarsz
        public static List<QA> RandomQuestions(List<QA> FullQA)     //ezt az On_Load esemény hívja meg, várja a txt fájlból beolvasott 200+elemű kérdéslistát, melynek típusa QA
        {
            List<QA> QuestSet = new List<QA>();                     //ez az ideiglenes lista (szintén QA típusú!), amibe gyűjteni fogja a kérdéseket (max 40 dbot)  
            Random rnd = new Random();                              //random
            HashSet<int> idx = new HashSet<int>();                  //Hashset, ami a kiválasztott indexeket fogja gyűjteni

            while (idx.Count < 40)                                  //míg a Hassetben nincs 40db elem, nem hagyhatja abba
            {
                int index = rnd.Next(0, FullQA.Count);              //dandom szám 0 és max kérdésszám között
                idx.Add(index);                                     //megkísérli hozzáadni a HashSethez ---> bool eredményű művelet előzi meg. Ha volt már benne ilyen elem, nem kerül hozzáadásra.
            }

            foreach (var i in idx)                                  //a már kész HashSetet lapozza végig
            {
                QuestSet.Add(FullQA[i]);                            //hozzáadja a full listából a HashSetben tárolt indexű kérdéseket a kérdésgyűjteményhez
            }

            //StreamWriter Sec_Wendy = new StreamWriter(@"latestQSet.txt");       //SECretary WENDY segíti a hibakeresést, de ki kell kapcsolni majd
            //foreach (var item in QuestSet)
            //{
            //    Sec_Wendy.WriteLine(item.mainQszoveg.First() + "    |   " + item.rightAnswer.First() + "    |   " +item.isPicture.ToString()+ "    |   " + item.picture);
            //}
            //Sec_Wendy.Close();

            return QuestSet;                                        //visszadobja az On_Load eventnek a 40elemű random válogatást
        }
        #endregion


    }
}
