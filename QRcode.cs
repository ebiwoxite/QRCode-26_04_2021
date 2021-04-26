using ReedSolomon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Data.SqlClient;

namespace ConsoleApp2
{
    
    class QRcode
    {
        private MyImage QR;
        private int version;
        private int tailleTotal;
        private int tailleEC;
        private List<int> messageCode;
        private int[] masque;
        /// <summary>
        /// constructeur
        /// </summary>
        /// <param name="message"></param>
        public QRcode(string message)
        {
            if (message.Length != 0 && message.Length <= 47 && message != "") 
            {
                masque = new int[] { 1, 1, 1, 0, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0 };
                if(message.Length<=25)
                {
                    version = 1;
                    tailleEC = 7;
                    tailleTotal = 152;
                    QR = new MyImage(21, 21);
                }
                else
                {
                    version = 2;
                    tailleEC = 10;
                    tailleTotal = 272;
                    QR = new MyImage(25, 25);                    
                }
                messageCode = Encodage(message);
                foreach (int i in messageCode) Console.Write(i);

                initialisationQRcode();
                Remplissage();
                QR.ChangerTaille(10, 10);
                QR.From_Image_To_File("retour.bmp");
                Process.Start("retour.bmp");
            }
            else Console.WriteLine("message invalide");
        }
        /// <summary>
        /// transforme le message en suite de bit avec le code de correction d'erreur
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        List<int> Encodage(string message)
        {
            
            List<int> retour = new List<int>();
            int[] mode = { 0, 0, 1, 0 };
            int[] taille = nombreToBinaire(message.Length, 9);
            int[] messageBit = stringToBit(message);
            foreach (int i in mode) retour.Add(i);
            foreach (int i in taille) retour.Add(i);
            foreach (int i in messageBit) retour.Add(i);

            int compteur = 0;

            while (retour.Count + (tailleEC*8) < tailleTotal && compteur < 4)
            {
                retour.Add(0);
                compteur++;
            }
            compteur = 0;


            while (retour.Count % 8 != 0) retour.Add(0);
            int[] n236 = { 1, 1, 1, 0, 1, 1, 0, 0 };
            int[] n17 = { 0, 0, 0, 1, 0, 0, 0, 1 };
            int tailletempo = retour.Count;
            for (int i = 0; i < (tailleTotal - tailletempo) / 8; i++)
            {
                if (compteur % 2 == 0)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        retour.Add(n236[j]);
                    }

                }
                else
                {
                    for (int j = 0; j < 8; j++)
                    {
                        retour.Add(n17[j]);
                    }
                }
                compteur++;

            }

            byte[] messageBytes = listeBitToTabByte(retour);
            byte[] correction = ReedSolomonAlgorithm.Encode(messageBytes, tailleEC, ErrorCorrectionCodeType.QRCode);
            //retour.RemoveRange(tailleTotal-(tailleEC*8), tailleEC*8);
            
            for (int i = 0; i < correction.Length; i++)
            {
                foreach (int o in nombrebyteToBinaire(correction[i], 8))
                {
                    retour.Add(o);
                }
            }
            if (version==2)
            {
                for (int i=0;i<7;i++)
                {
                    retour.Add(0);
                }
            }
            Console.WriteLine(retour.Count);
            return retour;
        }
        /// <summary>
        /// ajoute les motifs de recherche dans les coins et les motifs d'alignement 
        /// </summary>
        void initialisationQRcode()
        {
            for (int i = 0; i < 3; i++)
            {
                QR.Image90();
                motifRechercheBasGauche();
            }
            QR.ImagePixel[7,8] = new Pixel(0, 0, 0);
            for (int i = 8; i < QR.Hauteur - 8; i = i + 2)
            {
                QR.ImagePixel[i, 6] = new Pixel(0, 0, 0);
                QR.ImagePixel[QR.Hauteur - 7, i] = new Pixel(0, 0, 0);
            }


            if(version==2)
            {
                QR.ImagePixel[6,18] = new Pixel(0, 0, 0);
                
                for (int i=0;i<5;i++)
                {
                    QR.ImagePixel[4, 16 + i] = new Pixel(0, 0, 0);
                    QR.ImagePixel[4 + i, 16] = new Pixel(0, 0, 0);
                    QR.ImagePixel[8, 16 + i] = new Pixel(0, 0, 0);
                    QR.ImagePixel[4 + i, 20] = new Pixel(0, 0, 0);
                }
            }
            

        }
        /// <summary>
        /// remplit le QR code avec le message codé en suivant le bon chemin et applique le masque
        /// </summary>
        void Remplissage()
        {
            int ligne;
            int colonne;          
            int index = 0;
            
            bool[,] emplacementValide = rechercheEmplacementValide();
            
            ligne=0;
            colonne = QR.Largeur - 1;
            int sens = 1;
            
            /*for(int i=0;i<messageCode.Count;i++)
            {
                messageCode[i] = 0;
            }*/
            while (index+1 < messageCode.Count && colonne != 0)
            {
                
                if (ligne >= 0 && ligne < QR.Hauteur )
                {
                    if (emplacementValide[ligne, colonne - 1] == true && emplacementValide[ligne, colonne] == true)
                    {
                        QR.ImagePixel[ligne, colonne] = nouvelleValeur(messageCode[index],ligne,colonne);
                        index++;
                        QR.ImagePixel[ligne, colonne - 1] = nouvelleValeur(messageCode[index],ligne,colonne-1);
                        index++;
                        
                    }                   
                    else if (emplacementValide[ligne, colonne - 1] == true && emplacementValide[ligne, colonne] == false)
                    {
                        QR.ImagePixel[ligne, colonne - 1] = nouvelleValeur(messageCode[index],ligne,colonne-1);
                        index++;
                        
                    }
                }
                else
                {
                    sens = -sens;
                    if (colonne == 8) colonne = colonne - 3;
                    else colonne = colonne - 2;
                }
                ligne = ligne + sens; 
            }
            applicationMasque();
            

        }
        /// <summary>
        /// prend en entrée un bit
        /// si il est egal a 1 et que le masque le permet ça retourne un pixel noir
        /// si il est égal a 0 et que le masque le permet ça retourne un pixel blanc
        /// </summary>
        /// <param name="index"></param>
        /// <param name="ligne"></param>
        /// <param name="colonne"></param>
        /// <returns></returns>
        Pixel nouvelleValeur(int valeur,int ligne,int colonne)
        {
            bool inverser = (ligne + colonne) % 2 == 0;//application du masque 0
            Pixel retour;
            if (valeur == 1)
            {
                if (inverser==true)
                {
                    retour = new Pixel(255, 255, 255);
                }
                else
                {
                    retour = new Pixel(0, 0, 0);
                }
                 
            }
            else
            {
                if (inverser == true)
                {
                    retour = new Pixel(0, 0, 0);
                }
                else
                {
                    retour = new Pixel(255, 255, 255);
                }
            }
            if (colonne == 66&&valeur==1) retour = new Pixel(255, 0, 0);//permet de visualiser avec une autre couleur là ou on place les pixels
            if (colonne == 66 && valeur == 0) retour = new Pixel(0, 255, 0);
            return retour;
        }
        
        /// <summary>
        /// retourne une matrice de bool avec en true les emplacements valide
        /// les emplacements valides sont les emplacements du QR code où on peut placer les données
        /// </summary>
        /// <returns></returns>
        bool[,] rechercheEmplacementValide()
        {
            bool[,] emplacementValide = new bool[QR.Hauteur, QR.Hauteur];
            bool basgauche = false;
            bool hautgauche = false;
            bool hautdroite = false;
            bool alignement = false;
            bool synchro = false;
            int compteur = 0;
            for (int colonne = 0; colonne < QR.Hauteur; colonne++)
            {
                for (int ligne = 0; ligne < QR.Hauteur; ligne++)
                {
                    basgauche = ligne < 8 && colonne < 9;
                    hautgauche = ligne > QR.Hauteur - 10 && colonne < 9;
                    hautdroite = ligne > QR.Hauteur - 10 && colonne > QR.Hauteur - 9;
                    synchro = colonne == 6 || ligne == QR.Hauteur - 7;
                    if (version == 2)
                    {
                        alignement = (ligne > 3 && ligne < 9) && (colonne > 15 && colonne < 21);
                    }
                    if (basgauche == false && hautgauche == false && hautdroite == false && synchro == false && alignement == false)
                    {
                        emplacementValide[ligne, colonne] = true;
                        compteur++;
                    }
                    else emplacementValide[ligne, colonne] = false;
                }
            }
            Console.WriteLine("le compteur est a " + compteur);
            return emplacementValide;
        }
        
        /// <summary>
        /// rajoute le masque aux emplacements prévus à cet effet
        /// </summary>
        void applicationMasque()
        {
            for (int i = 0; i < 7;i++)
            {
                QR.ImagePixel[i, 8] = nouvelleValeur(masque[i],0,1);
                if (i<6)
                {
                    QR.ImagePixel[QR.Hauteur - 9, i] = nouvelleValeur(masque[i],0,1);
                }
                else
                {
                    QR.ImagePixel[QR.Hauteur - 9, i+1] = nouvelleValeur(masque[i], 0, 1);
                }
            }
            for (int i = 7; i < 15; i++)
            {
                QR.ImagePixel[QR.Hauteur-9, QR.Hauteur - 15+i] = nouvelleValeur(masque[i], 0, 1);
                if (i < 9)
                {
                    QR.ImagePixel[QR.Hauteur - 16+i, 8] = nouvelleValeur(masque[i], 0, 1);
                }
                else
                {
                    QR.ImagePixel[QR.Hauteur - 15+i, 8] = nouvelleValeur(masque[i], 0, 1);
                }
            }
        }
        /// <summary>
        /// dessine le motif de recherche en bas à gauche de l'image
        /// </summary>
        void motifRechercheBasGauche()
        {
            if (QR != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    QR.ImagePixel[0, i] = new Pixel(0, 0, 0);
                    QR.ImagePixel[6, i] = new Pixel(0, 0, 0);
                    QR.ImagePixel[i, 0] = new Pixel(0, 0, 0);
                    QR.ImagePixel[i, 6] = new Pixel(0, 0, 0);

                }
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        QR.ImagePixel[2 + i, 2 + j] = new Pixel(0, 0, 0);
                    }
                }
            }

        }

        /// <summary>
        /// encode le message en alphanumerique
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        static int[] stringToBit(string message)
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

        /// <summary>
        /// associe chaque caractère supporté par l'alphanumérique à un nombre
        /// </summary>
        /// <param name="caractere"></param>
        /// <returns></returns>
        public static int charToInt(char caractere)
        {
            int sortie;
            string h = caractere.ToString().ToLower();
            caractere = Convert.ToChar(h);

            switch (caractere)
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
                    sortie = 39;
                    break;
            }
            return sortie;
        }

        /// <summary>
        /// transforme un byte en binaire d'une longueur donnée
        /// </summary>
        /// <param name="A"></param>
        /// <param name="longueur"></param>
        /// <returns></returns>
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

        /// <summary>
        /// transforme une liste de bit en tableau de byte
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        public static byte[] listeBitToTabByte(List<int> tab)
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
        /// <summary>
        /// transforme un tableau de bit en entier
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        static int binaireToNombre(int[] A)
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
        /// <summary>
        /// transforme un entier en tableau de bit d'une longueur donnée
        /// </summary>
        /// <param name="A"></param>
        /// <param name="longueur"></param>
        /// <returns></returns>
        public static int[] nombreToBinaire(int A, int longueur)
        {
            int I = A;
            int[] tab = new int[longueur];
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
    }
}
