using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Linq.Expressions;
using System.IO;
using ConsoleApp2;

namespace ReedSolomon
{
    class Program
    {
        static void Main(string[] args)
        {

            QRcode o = new QRcode("heyy.... toi! tu sais que t bg?");
            TD5();

            //Encodage("hello world");

            
            Console.ReadLine();
        }
        static string ChoixImage()
        {

            string file = "lena.bmp";
            Console.WriteLine(
                "les choix disponible sont: coco, harold, lac, lapinou, lena, tajm, tortank et test.\n" +
                "tapez l'image que vous souhaitez:");
            string saisie = Console.ReadLine();
            switch (saisie)
            {
                case "coco":
                    file = "coco.bmp";
                    break;
                case "harold":
                    file = "harold.bmp";
                    break;
                case "lac":
                    file = "lac.bmp";
                    break;
                case "lapinou":
                    file = "lapinou.bmp";
                    break;
                case "lena":
                    file = "lena.bmp";
                    break;
                case "tajm":
                    file = "tajm.bmp";
                    break;
                case "tortank":
                    file = "tortank.bmp";
                    break;
                case "test":
                    file = "test3.bmp";
                    break;
                default:
                    Console.WriteLine("l'image séléctionnée par défaut est léna");
                    break;
            }
            return file;
        }
        static void fonctionEsilv()
        {
            Encoding u8 = Encoding.UTF8;
            string a = "hello world";
            string p = "HELLO WORLD";
            int iBC = u8.GetByteCount(p);
            byte[] byteellul = { 64, 182, 134, 86, 198, 198, 242, 7, 118, 247, 38, 198, 64, 236, 17, 236, 17, 236, 17 };
            byte[] bytethonky =     { 32, 91, 11, 120, 209, 114, 220, 77, 67, 64, 236, 17, 236, 17, 236, 17 };
            byte[] bytemoipresque = { 32, 91, 11, 120, 209, 114, 220, 77, 67, 64, 236, 17, 236, 17, 236, 17, 236, 17, 236 };
            byte[] bytesa = u8.GetBytes(p);
            //Console.WriteLine(binaireToNombre(bytesa));
            string b = "HELLO WORF";
            byte[] bytesb = u8.GetBytes(b);
            //byte[] result = ReedSolomonAlgorithm.Encode(bytesa, 7); 
            //Privilégiez l'écriture suivante car par défaut le type choisi est DataMatrix 
            byte[] result = ReedSolomonAlgorithm.Encode(byteellul, 7, ErrorCorrectionCodeType.QRCode);
            byte[] result1 = ReedSolomonAlgorithm.Decode(bytesb, result);
            Console.WriteLine("Voici ce qui a été envoyé à Reed Solomon:");
            foreach (byte val in byteellul) Console.Write(val + " ");
            Console.WriteLine();
            Console.WriteLine("ce qui a été retourné:");
            foreach (byte val in result) Console.Write(val + " ");
            //foreach (int u in retour3) Console.Write(u + " ");
            Console.WriteLine();

            Console.ReadLine();
        }
        public static List<int> Encodage(string message)
        {
            
            List<int> retour = new List<int>();
            int[] mode = { 0, 0, 1, 0 };
            int[] taille = nombreToBinaire(message.Length, 9);
            int[] messageBit = stringToBit(message);
            foreach (int i in mode) retour.Add(i);
            foreach (int i in taille) retour.Add(i);
            foreach (int i in messageBit) retour.Add(i);
            
            int compteur = 0;
            
            while (retour.Count+56 < 152 && compteur < 4)
            {
                retour.Add(0);
                compteur++;
            }
            compteur = 0;

            
            while (retour.Count % 8 != 0) retour.Add(0);
            int[] n236 = { 1, 1, 1, 0, 1, 1, 0, 0 };
            int[] n17 = { 0, 0, 0, 1, 0, 0, 0, 1 };
            int tailletempo = retour.Count;
            for (int i=0;i< (152 - tailletempo)/8 ;i++)
            {
                if(compteur%2==0)
                {
                    for (int j=0;j<8;j++)
                    {
                        retour.Add(n236[j]);
                    }
                    
                }
                else
                {
                    for(int j=0;j<8;j++)
                    {
                        retour.Add(n17[j]);
                    }
                }
                compteur++;

            }

            byte[] messageBytes = octetToInt(retour);
            byte[] correction = ReedSolomonAlgorithm.Encode(messageBytes, 7, ErrorCorrectionCodeType.QRCode);//pour hello world ça me donne pas la meme chose que le cdc
            retour.RemoveRange(95, 56);
            Console.WriteLine(retour.Count);
            for(int i=0;i<correction.Length;i++)
            {
                foreach(int o in nombrebyteToBinaire(correction[i],8))
                {
                    retour.Add(o);
                }
            }
            
            return retour;
        }
        public static void initialisationQRcodeV1()
        {
            MyImage retour = new MyImage(21,21);
            for(int i=0;i<3;i++)
            {
                retour.Image90();
                motifRechercheBasGauche(retour);
            }
            retour.ChangerTaille(10, 10);
            retour.From_Image_To_File("retour.bmp");
            Process.Start("retour.bmp");

        }
        static void motifRechercheBasGauche(MyImage retour)
        {
            if(retour!=null)
            {
                for (int i = 0; i < 7; i++)
                {
                    retour.ImagePixel[0, i] = new Pixel(0, 0, 0);
                    retour.ImagePixel[6, i] = new Pixel(0, 0, 0);
                    retour.ImagePixel[i, 0] = new Pixel(0, 0, 0);
                    retour.ImagePixel[i, 6] = new Pixel(0, 0, 0);

                }
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        retour.ImagePixel[2 + i, 2 + j] = new Pixel(0, 0, 0);
                    }
                }
            }
            
        }
        public static int[] stringToBit(string message)
        {
            int[] messagebit;
            if (message.Length != 0)
            {
                int longueurbit = 0;
                int longueur = 0;
                if (message.Length % 2 == 0)
                {
                    longueur = message.Length / 2;
                    longueurbit = (message.Length / 2) * 11;
                }
                else
                {
                    longueur = ((message.Length - 1) / 2) + 1;
                    longueurbit = ((message.Length - 1) / 2) * 11 + 6;
                }
                messagebit = new int[longueurbit];

                int[] lettreint = new int[longueur];

                int index = 0;
                for (int i = 0; i < longueur; i++)//convertit les paires de caractères en int
                {
                    if (index + 1 < message.Length)
                    {
                        lettreint[i] = charToInt(message[index]) * 45 + charToInt(message[index + 1]);
                    }
                    else lettreint[i] = charToInt(message[index]);
                    index = index + 2;
                }

                index = 0;
                for (int i = 0; i < longueur - 1; i++)//convertit les int en 11 bits
                {
                    int[] tempo = nombreToBinaire(lettreint[i], 11);
                    for (int j = 0; j < 11; j++)
                    {
                        messagebit[index] = tempo[j];
                        index++;
                    }
                }
                if (message.Length % 2 == 0)//si le message est pair la dernière est codée sur 11 bits
                {
                    int[] tempo = nombreToBinaire(lettreint[longueur - 1], 11);
                    for (int j = 0; j < 11; j++)
                    {
                        messagebit[index] = tempo[j];
                        index++;
                    }
                }
                else //sinon elle est codée sur 6 bits
                {
                    int[] tempo = nombreToBinaire(lettreint[longueur - 1], 6);
                    for (int j = 0; j < 6; j++)
                    {
                        messagebit[index] = tempo[j];
                        index++;
                    }
                }

            }
            else messagebit = null;
            return messagebit;
            
        }
        public static int charToInt(char caractere)
        {
            int sortie;
            string h = caractere.ToString().ToLower();
            caractere = Convert.ToChar(h);
            
            switch(caractere)
            {
                case '0':
                    sortie = 0;
                    break;
                case '1':
                    sortie = 1;
                    break;
                case '2':
                    sortie = 2;
                    break;
                case '3':
                    sortie = 3;
                    break;
                case '4':
                    sortie = 4;
                    break;
                case '5':
                    sortie = 5;
                    break;
                case '6':
                    sortie = 6;
                    break;
                case '7':
                    sortie = 7;
                    break;
                case '8':
                    sortie = 8;
                    break;
                case '9':
                    sortie = 9;
                    break;
                case 'a':
                    sortie = 10;
                    break;
                case 'b':
                    sortie = 11;
                    break;
                case 'c':
                    sortie = 12;
                    break;
                case 'd':
                    sortie = 13;
                    break;
                case 'e':
                    sortie = 14;
                    break;
                case 'f':
                    sortie = 15;
                    break;
                case 'g':
                    sortie = 16;
                    break;
                case 'h':
                    sortie = 17;
                    break;
                case 'i':
                    sortie = 18;
                    break;
                case 'j':
                    sortie = 19;
                    break;
                case 'k':
                    sortie = 20;
                    break;
                case 'l':
                    sortie = 21;
                    break;
                case 'm':
                    sortie = 22;
                    break;
                case 'n':
                    sortie = 23;
                    break;
                case 'o':
                    sortie = 24;
                    break;
                case 'p':
                    sortie = 25;
                    break;
                case 'q':
                    sortie = 26;
                    break;
                case 'r':
                    sortie = 27;
                    break;
                case 's':
                    sortie = 28;
                    break;
                case 't':
                    sortie = 29;
                    break;
                case 'u':
                    sortie = 30;
                    break;
                case 'v':
                    sortie = 31;
                    break;
                case 'w':
                    sortie = 32;
                    break;
                case 'x':
                    sortie = 33;
                    break;
                case 'y':
                    sortie = 34;
                    break;
                case 'z':
                    sortie = 35;
                    break;
                case ' ':
                    sortie = 36;
                    break;
                case '$':
                    sortie = 37;
                    break;
                case '%':
                    sortie = 38;
                    break;
                case '*':
                    sortie = 39;
                    break;
                case '+':
                    sortie = 40;
                    break;
                case '-':
                    sortie = 41;
                    break;
                case '.':
                    sortie = 42;
                    break;
                case '/':
                    sortie = 43;
                    break;
                case ':':
                    sortie = 44;
                    break;
                default:
                    sortie = -1;
                    break;
            }
            return sortie;
        }
        public static byte[] octetToInt(List<int> tab)
        {
            byte[] retour;
            if (tab != null && tab.Count != 0 && tab.Count % 8 == 0)
            {
                int index = 0;
                int[] tempo = new int[8];
                retour = new byte[tab.Count / 8];
                for (int i = 0; i < retour.Length; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        tempo[j] = tab[index];
                        index++;
                    }
                    retour[i] = (byte)(binaireToNombre(tempo));
                }
            }
            else
            {
                retour = null;
                Console.WriteLine(tab.Count);
            }
            return retour;
            
        }
        public static int[] nombreToBinaire(int A,int longueur)
        {
            int I = A;
            int[] tab = new int[longueur];
            int R = (int)(Math.Pow(2,longueur)/2);
            int k = 0;
            while (I > 0 && k < tab.Length)
            {

                if (I - R >= 0)
                {
                    tab[k] = 1;

                    I = I - R;
                }

                k++;
                R = R / 2;


            }
            return tab;

        }
        public static byte[] nombrebyteToBinaire(int A, int longueur)
        {
            int I = A;
            byte[] tab = new byte[longueur];
            int R = (int)(Math.Pow(2, longueur) / 2);
            int k = 0;
            while (I > 0 && k < tab.Length)
            {

                if (I - R >= 0)
                {
                    tab[k] = 1;

                    I = I - R;
                }

                k++;
                R = R / 2;


            }
            return tab;

        }
        public static void affichertab(int[] tab)
        {
            if (tab != null && tab.Length != 0)
            {
                foreach (int i in tab)
                {
                    Console.Write(i + " ");
                }
            }
            else Console.WriteLine("tableau vide");
        }
        static void TD5()
        {

            string file = ChoixImage();
            MyImage image = new MyImage(file);
            string retour = "retour.bmp";
            bool sortie = false;
            while (sortie == false)
            {

                int caseSwitch = SaisirMenu(("Que désirez-vous tester ?\n" +
                    " Inversion des couleurs:                   tapez 1\n" +
                    " Couleurs nuances de gris :                tapez 2\n" +
                    " Couleurs Noir et blanc :                  tapez 3\n" +
                    " Miroir horizontal :                       tapez 4\n" +
                    " Miroir vertical :                         tapez 5\n" +
                    " Changement de Taille :                    tapez 6\n" +
                    " Rotation :                                tapez 7\n" +
                    " Afficher les information de l'image :     tapez 8\n" +
                    " utiliser le filtre flou :                 tapez 9\n" +
                    " utiliser le filtre repoussage :           tapez 10\n" +
                    " utiliser le filtre detection des contours tapez 11\n" +
                    " utiliser le filtre renforcement des bords tapez 12\n" +
                    " utiliser le filtre flou de Gauss 5x5 :    tapez 13\n" +
                    " utiliser le masque flou 5x5 :             tapez 14\n" +
                    " créer une fractale de Julia :             tapez 15\n" +
                    " afficher la photo et son histogramme :    tapez 16\n" +
                    " cacher une image dans une autre :         tapez 17\n" +
                    "sortir :tapez -1 "));
                switch (caseSwitch)
                {
                    case 1:
                        image = new MyImage(file);
                        image.CouleurInverse();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 2:
                        image = new MyImage(file);
                        image.CouleurNuancesDeGris();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 3:
                        image = new MyImage(file);
                        image.CouleurNoirBlanc();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 4:
                        image = new MyImage(file);
                        image.MiroirHorizontal();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 5:
                        image = new MyImage(file);
                        image.MiroirVertical();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 6:
                        image = new MyImage(file);
                        double coef = SaisirDoubleTaille("De quel coefficient souhaitez-vous modifier l'image ? (réduit la taille si inférieur à 1)");
                        image.ChangerTaille(coef, coef);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 7:
                        image = new MyImage(file);
                        double angle = SaisirDouble("De quel angle souhaitez-vous que l'image pivote ? (tourne dans l'autre sens si négatif)");
                        image.Rotation(angle);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);

                        break;
                    case 8:
                        Console.Clear();
                        image = new MyImage(file);
                        Console.WriteLine(image.ToString());
                        Console.ReadLine();
                        break;
                    case 9:
                        image = new MyImage(file);
                        double[,] flou = { { 0.1111111, 0.1111111, 0.1111111 }, { 0.1111111, 0.1111111, 0.1111111 }, { 0.1111111, 0.1111111, 0.1111111 } };
                        image.applicationfiltre(flou);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 10:
                        image = new MyImage(file);
                        double[,] repoussage = { { -2, -1, 0 }, { -1, 1, 1 }, { 0, 1, 2 } };
                        image.applicationfiltre(repoussage);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 11:
                        image = new MyImage(file);
                        double[,] contour = { { -1, -1, -1 }, { -1, 8, -1 }, { -1, -1, -1 } };
                        image.applicationfiltre(contour);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);

                        break;
                    case 12:
                        image = new MyImage(file);
                        double[,] renforcementbords = { { 0, 0, 0 }, { -1, 1, 0 }, { 0, 0, 0 } };
                        image.applicationfiltre(renforcementbords);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);

                        break;
                    case 13:
                        image = new MyImage(file);
                        double[,] gauss = { { 1, 4, 6, 4, 1 }, { 4, 6, 24, 6, 4 }, { 6, 24, 36, 24, 6 }, { 4, 6, 24, 6, 4 }, { 1, 4, 6, 4, 1 } };
                        for (int i = 0; i < gauss.GetLength(0); i++)
                        {
                            for (int j = 0; j < gauss.GetLength(1); j++)
                            {
                                gauss[i, j] = 0.00390625 * gauss[i, j];
                            }
                        }
                        image.applicationfiltre(gauss);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);

                        break;
                    case 14:
                        image = new MyImage(file);
                        double[,] masquegauss = { { 1, 4, 6, 4, 1 }, { 4, 6, 24, 6, 4 }, { 6, 24, -476, 24, 6 }, { 4, 6, 24, 6, 4 }, { 1, 4, 6, 4, 1 } };
                        for (int i = 0; i < masquegauss.GetLength(0); i++)
                        {
                            for (int j = 0; j < masquegauss.GetLength(1); j++)
                            {
                                masquegauss[i, j] = -0.00390625 * masquegauss[i, j];
                            }
                        }
                        image.applicationfiltre(masquegauss);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 15:


                        bool sortie2 = false;
                        int largeur = 900;
                        int hauteur = 500;
                        while (sortie2 == false)
                        {
                            Console.Clear();
                            Console.WriteLine("taille actuelle {0}*{1} \n" +
                                "pour changer la taille tapez 1:\n" +
                                "pour revenir au menu précédent tapez 2:\n" +
                                "pour avancer tapez sur une autre touche.", hauteur, largeur);
                            string saisie2 = Console.ReadLine();

                            switch (saisie2)
                            {
                                case "1":
                                    largeur = SaisirIntTaille("De quelle largeur voulez-vous que l'image soit?");
                                    hauteur = SaisirIntTaille("De quelle hauteur voulez-vous que l'image soit?");
                                    Console.Clear();
                                    break;
                                case "2":
                                    sortie2 = true;
                                    break;
                                default:
                                    Console.WriteLine("une fractale de Julia suit l'equation z^2+c avec z et c des complexes, on peut modifier c pour avoir des fractales différentes\n" +
                                "les valeurs de c les plus esthetiques sont comprises entre -1-i et 1+i\n" +
                                "les valeurs de c qui marchent le mieux pour ce programme sont: -0.75, i, 0.39+0.6i et -1+0.2i");
                                    double reel = SaisirDoubleComplexe("saisir la partie réelle de c : (il faut mettre une virgule pour les décimales)");
                                    double imaginaire = SaisirDoubleComplexe("saisir la partie imaginaire de c : (il faut mettre une virgule pour les décimales)");
                                    Complex c = new Complex(reel, imaginaire);
                                    image = new MyImage(largeur, hauteur);
                                    image.creationFractale(c);
                                    image.From_Image_To_File(retour);
                                    Process.Start(retour);
                                    break;

                            }
                        }


                        break;
                    case 16:
                        Console.Clear();
                        image = new MyImage(file);
                        image.Histogramme();
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case 17:
                        Console.Clear();
                        Console.WriteLine("veuillez choisir l'image que vous voulez montrer");
                        string montree = ChoixImage();
                        Console.WriteLine("veuillez choisir l'image que vous voulez cacher");
                        string cachee = ChoixImage();
                        MyImage imagemontree = new MyImage(montree);
                        MyImage imagecachee = new MyImage(cachee);
                        MyImage imagefusionnee = new MyImage(imagecachee.ImagePixel, imagemontree.ImagePixel);
                        imagefusionnee.From_Image_To_File("Image_Cryptee.bmp");
                        Process.Start("Image_Cryptee.bmp");
                        imagefusionnee.DecrypterImage();
                        imagefusionnee.From_Image_To_File("Image_Decryptee.bmp");
                        Process.Start("Image_Decryptee.bmp");
                        break;
                    case 18:

                        //image.imagepixel = decouvreImage2(image.imagePixel);
                        image.From_Image_To_File(retour);
                        Process.Start(retour);
                        break;
                    case -1:
                        sortie = true;
                        break;


                }
                Console.Clear();
            }
        }
        
        public static int binaireToNombre(int[] A)
        {
            int D = 128;
            int R = 0;
            for (int i = 0; i < A.Length; i++)
            {
                R += A[i] * D;
                D = D / 2;
            }
            return R;

        }
        static void afficherMatrice(int[,] matrice)
        {
            for (int i = 0; i < matrice.GetLength(0); i++)
            {
                for (int j = 0; j < matrice.GetLength(1); j++)
                {
                    Console.Write(matrice[i, j] + " ");

                }
                Console.WriteLine();
            }
        }
        static void affichertableau(int[] tableau)
        {
            if (tableau != null && tableau.Length != 0)
            {
                for (int i = 0; i < tableau.Length; i++)
                {
                    Console.Write(tableau[i] + " ");
                }
            }
            else Console.WriteLine("tableau non valide");

        }
        #region saisir nombre
        public static int SaisirMenu(string message)
        {
            int n;
            bool parseOk;
            do
            {
                Console.WriteLine(message);
                parseOk = int.TryParse(Console.ReadLine(), out n);

            }
            while (parseOk == false || n < -1 || n > 20);
            return n;
        }
        public static double SaisirDouble(string message)
        {
            double n;
            bool parseOk;
            do
            {
                Console.WriteLine(message);
                parseOk = double.TryParse(Console.ReadLine(), out n);

            }
            while (parseOk == false);
            return n;
        }
        public static double SaisirDoubleTaille(string message)
        {
            double n;
            bool parseOk;
            do
            {
                Console.WriteLine(message);
                parseOk = double.TryParse(Console.ReadLine(), out n);

            }
            while (parseOk == false || n <= 0d);
            return n;
        }
        public static double SaisirDoubleComplexe(string message)
        {
            double n;
            bool parseOk;
            do
            {
                Console.WriteLine(message);
                parseOk = double.TryParse(Console.ReadLine(), out n);

            }
            while (parseOk == false || n < -1d || n > 1d);
            return n;
        }
        public static int SaisirIntTaille(string message)
        {
            int n;
            bool parseOk;
            do
            {
                Console.WriteLine(message);
                parseOk = int.TryParse(Console.ReadLine(), out n);

            }
            while (parseOk == false || n <= 0);
            return n;
        }
        #endregion
    }
}

    
    

  
