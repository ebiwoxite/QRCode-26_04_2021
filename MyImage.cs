using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;


namespace ReedSolomon
{
    class MyImage
    {
        
        private string type;
        private int taille;
        private int tailleOffset;
        private int largeur;
        private int hauteur;
        private int bitparcouleur;
        private Pixel[,] imagePixel;
        private int padding;
        #region get/set
        public string Type
        {
            get { return type; }
        }
        public int Taille
        {
            get { return taille; }
        }
        public int TailleOffset
        {
            get { return tailleOffset; }
        }
        public int Largeur
        {
            get { return largeur; }
        }
        public int Hauteur
        {
            get { return hauteur; }
        }
        public int Bitparcouleur
        {
            get { return bitparcouleur; }
        }
        public Pixel[,] ImagePixel
        {
            get { return imagePixel; }
            set { imagePixel = value; }
        }
        #endregion



        public MyImage(int largeur, int hauteur)///crée une image à partir d'une hauteur et largeur donnée
        {
            if (largeur > 0 && hauteur > 0)
            {
                type = "BM";
                tailleOffset = 54;
                this.largeur = largeur;
                this.hauteur = hauteur;
                bitparcouleur = 24;
                int largeurmultiple4 = largeur * (bitparcouleur / 8);
                padding = 0;
                if ((largeur * (bitparcouleur / 8)) % 4 != 0)
                {
                    //rajoute un octet à la fin de la ligne tant que la largeur n'est pas multiple de 4 et retient le décalage (padding)
                    while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
                }
                taille = (hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding + tailleOffset;
                imagePixel = CreationMatricePixel(hauteur, largeur, 255);
            }
            else Console.WriteLine("taille invalide");

        }
        public MyImage(Pixel[,] imagecachee, Pixel[,] imagemontree)///crée une image à partir de deux images en cachant une image dans l'autre
        {
            if (imagecachee != null && imagemontree != null && imagemontree.Length != 0 && imagecachee.Length != 0)
            {
                type = "BM";
                tailleOffset = 54;

                hauteur = imagemontree.GetLength(0);
                largeur = imagemontree.GetLength(1);

                bitparcouleur = 24;
                int largeurmultiple4 = largeur * (bitparcouleur / 8);
                padding = 0;
                if ((largeur * (bitparcouleur / 8)) % 4 != 0)
                {
                    //rajoute un octet à la fin de la ligne tant que la largeur n'est pas multiple de 4 et retient le décalage (padding)
                    while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
                }
                taille = (hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding + tailleOffset;
                imagePixel = CreationMatricePixel(hauteur, largeur, 255);

                //int decalage = imagemontree.GetLength(0) - imagecachee.GetLength(0);
                double coefh = (double)(imagemontree.GetLength(0)) / (double)(imagecachee.GetLength(0));
                double coefl = (double)(imagemontree.GetLength(1)) / (double)(imagecachee.GetLength(1));
                imagecachee = ChangerTailleMatrice(coefh, coefl, imagecachee);
                if (imagecachee.GetLength(0) == imagemontree.GetLength(0) && imagemontree.GetLength(1) == imagecachee.GetLength(1))
                {
                    CrypterImage(imagecachee, imagemontree);
                }
                else
                {
                    Console.WriteLine("erreur dans le changement de taille des images (constructeur de MyImage)");
                }

            }
            else Console.WriteLine("images invalides");

        }
        public MyImage(string myfile)///récupère les informations de l'image bitmap à partir du nom du fichier
        {

            byte[] tab = File.ReadAllBytes(myfile);//récupère les byte du fichier
            byte[] t2 = new byte[2];
            byte[] t4 = new byte[4];
            #region definition caractéristiques image
            for (int m = 0; m < 2; m++)
            {
                t2[m] = tab[m];
            }
            type = ASCIIEncoding.ASCII.GetString(t2);

            for (int m = 2; m < 6; m++)
            {
                t4[m - 2] = tab[m];

            }
            taille = Convertir_Endian_To_Int(t4);

            for (int m = 10; m < 14; m++)
            {
                t4[m - 10] = tab[m];
            }
            tailleOffset = Convertir_Endian_To_Int(t4);

            for (int m = 18; m < 22; m++)
            {
                t4[m - 18] = tab[m];
            }
            largeur = Convertir_Endian_To_Int(t4);

            for (int m = 22; m < 26; m++)
            {
                t4[m - 22] = tab[m];
            }
            hauteur = Convertir_Endian_To_Int(t4);


            for (int m = 28; m < 30; m++)
            {
                t2[m - 28] = tab[m];

            }
            bitparcouleur = Convertir_Endian_To_Int(t2);
            #endregion
            #region remplissage matrice de pixel
            int y = tailleOffset;
            bool fini = false;
            int ligne = 0;
            int colonne = 0;
            imagePixel = new Pixel[hauteur, largeur];
            int largeurmultiple4 = largeur * (bitparcouleur / 8);//bitparcouleur/8 sert à avoir le nombre d'octet par pixel
            padding = 0;
            if ((largeur * (bitparcouleur / 8)) % 4 != 0)
            {
                //rajoute un octet à la fin de la ligne tant que la largeur n'est pas multiple de 4 et retient le décalage (padding)
                while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
            }
            do
            {
                if (colonne == largeur + padding)//quand on arrive à la fin de la ligne on remet les colonnes à zéro et on incrémente les lignes
                {
                    colonne = 0;
                    ligne++;
                }
                else
                {
                    if (y + 2 < tab.Length && ligne < hauteur)//on vérifie si on est arrivé à la fin du tableau
                    {
                        if (colonne < largeur)//tant qu'on est dans la largeur théorique (sans le padding) on récupère les octets 3 par 3
                        {
                            imagePixel[ligne, colonne] = new Pixel(tab[y + 2], tab[y + 1], tab[y]);
                            y = y + (bitparcouleur / 8);
                            colonne++;

                        }
                        else//si on est dans le padding on augmente y et colonne mais on ne remplit pas la matrice
                        {
                            colonne++;
                            y++;
                        }

                    }
                    else fini = true;//pour sortir des boucles
                }



            } while (fini == false);
            #endregion

        }


        public void From_Image_To_File(string file)///crée un fichier bitmap avec toutes les informations de MyImage
        {
            //création du tableau de byte qu'on va retourner et des tableaux de byte utiles
            byte[] retour = new byte[taille];
            byte[] t2 = new byte[2];//pour mettre les informations codées sur 2 byte
            byte[] t4 = new byte[4];//pour mettre les informations codées sur 4 byte
            byte[] img = ConvertirImageEnTByte();//récupère la matrice de pixel et la transforme en tableau de byte

            t2 = ASCIIEncoding.ASCII.GetBytes(type);//transforme un string en tableau de byte au format ASCII
            for (int i = 0; i < 2; i++)
            {
                retour[i] = t2[i];
            }

            t4 = Convertir_Int_To_Endian(taille);
            for (int i = 2; i < 6; i++)
            {
                retour[i] = t4[i - 2];
            }

            t4 = Convertir_Int_To_Endian(tailleOffset);
            for (int i = 10; i < 13; i++)
            {
                retour[i] = t4[i - 10];
            }

            retour[14] = 40;

            t4 = Convertir_Int_To_Endian(largeur);
            for (int i = 18; i < 22; i++)
            {
                retour[i] = t4[i - 18];
            }

            t4 = Convertir_Int_To_Endian(hauteur);
            for (int i = 22; i < 26; i++)
            {
                retour[i] = t4[i - 22];
            }

            retour[26] = 1;

            t2 = Convertir_Int_To_Endian(bitparcouleur);
            for (int i = 28; i < 30; i++)
            {
                retour[i] = t2[i - 28];
            }


            for (int i = tailleOffset; i < img.Length + tailleOffset; i++)
            {
                retour[i] = img[i - tailleOffset];
            }

            //crée le fichier avec le tableau rempli
            File.WriteAllBytes(file, retour);
        }

        #region TD3
        /// <summary>
        /// convertit une matrice de pixel en tableau de byte (prend en compte le cas où ce n'est pas multiple de 4) 
        /// </summary>
        /// <returns></returns>
        byte[] ConvertirImageEnTByte()
        {
            byte[] retour = new byte[(hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding];
            //initialisation des variables
            int colonne = 0;
            int ligne = 0;
            int i = 0;

            while (i < retour.Length)//parcourt tout le tableau de retour
            {
                if (colonne < largeur)//récupère les valeurs des byte codant chaque pixel 3 par 3
                {
                    retour[i] = imagePixel[ligne, colonne].B;
                    i++;
                    retour[i] = imagePixel[ligne, colonne].G;
                    i++;
                    retour[i] = imagePixel[ligne, colonne].R;
                    i++;
                    colonne++;
                }
                else//une fois arrivé au bout de la ligne on rajoute le padding si il y en a un on augmente la variable ligne de 1 et remet la variable colonne à 0
                {

                    if (padding != 0)
                    {
                        for (int index = 0; index < padding; index++)
                        {

                            retour[i] = 0;
                            i++;
                        }
                    }
                    ligne++;
                    colonne = 0;
                }
            }
            return retour;
        }
        /// <summary>
        /// Convertit un tableau de byte en format Little endian en entier 
        /// </summary>
        /// <param name="tab"></param>
        /// <returns></returns>
        int Convertir_Endian_To_Int(byte[] tab)
        {
            double n = -1;
            if (tab != null && tab.Length != 0)
            {
                n = 0;
                for (int i = 0; i < tab.Length; i++)
                {
                    n = n + tab[i] * Math.Pow(256d, i);
                }
            }
            return Convert.ToInt32(n);

        }
        /// <summary>
        ///convertit un entier en tableau de byte en format Little Endian 
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        byte[] Convertir_Int_To_Endian(int val)
        {
            if (val > 0)
            {
                byte[] tab = new byte[4];
                for (int i = 3; i >= 0; i--)
                {
                    tab[i] = Convert.ToByte(val / Convert.ToInt32(Math.Pow(256d, i)));
                    val = val % Convert.ToInt32(Math.Pow(256d, i));
                }
                return tab;
            }
            else return null;


        }
        /// <summary>
        /// inverse les couleurs de chaque pixel
        /// </summary>
        public void CouleurInverse()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    imagePixel[i, j].InverserCouleur();
                }
            }
        }
        /// <summary>
        /// applique un filtre de couleur nuances de gris à l'image
        /// </summary>
        public void CouleurNuancesDeGris()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    imagePixel[i, j].NuancesDeGris();
                }
            }
        }
        /// <summary>
        /// applique un filtre de couleur noir et blanc à l'image
        /// </summary>
        public void CouleurNoirBlanc()
        {
            for (int i = 0; i < hauteur; i++)
            {
                for (int j = 0; j < largeur; j++)
                {
                    imagePixel[i, j].NoirBlanc();
                }
            }
        }
        #region rotations basiques de l'image
        /// <summary>
        /// tourne l'image de 180° 
        /// </summary>
        void Image180()
        {
            Pixel[,] clone = cloneImage(imagePixel);
            for (int i = 0; i < imagePixel.GetLength(0); i++)
            {
                for (int j = 0; j < imagePixel.GetLength(1); j++)
                {
                    imagePixel[i, j] = clone[imagePixel.GetLength(0) - 1 - i, imagePixel.GetLength(1) - 1 - j];
                }
            }
        }
        /// <summary>
        ///tourne l'image de 90°
        /// </summary>
        public void Image90()
        {
            Pixel[,] clone = cloneImage(imagePixel);
            int temp = largeur;
            largeur = hauteur;
            hauteur = temp;
            int largeurmultiple4 = largeur * 3;
            padding = 0;
            while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
            imagePixel = new Pixel[hauteur, largeur];
            for (int i = 0; i < imagePixel.GetLength(0); i++)
            {
                for (int j = 0; j < imagePixel.GetLength(1); j++)
                {
                    imagePixel[i, j] = clone[j, imagePixel.GetLength(0) - 1 - i];
                }
            }
        }
        /// <summary>
        /// tourne l'image de 270°
        /// </summary>
        void Image270()
        {
            Pixel[,] clone = cloneImage(imagePixel);
            int temp = largeur;
            largeur = hauteur;
            hauteur = temp;
            int largeurmultiple4 = largeur * 3;
            padding = 0;
            while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
            imagePixel = new Pixel[hauteur, largeur];
            for (int i = 0; i < imagePixel.GetLength(0); i++)
            {
                for (int j = 0; j < imagePixel.GetLength(1); j++)
                {
                    imagePixel[i, j] = clone[imagePixel.GetLength(1) - 1 - j, i];
                }
            }
        }
        #endregion
        /// <summary>
        /// renverse l'image selon un axe horizontal
        /// </summary>
        public void MiroirHorizontal()
        {
            Pixel[,] clone = cloneImage(imagePixel);
            for (int i = 0; i < imagePixel.GetLength(0); i++)
            {
                for (int j = 0; j < imagePixel.GetLength(1); j++)
                {
                    imagePixel[i, j] = clone[imagePixel.GetLength(0) - 1 - i, j];
                }
            }
        }
        /// <summary>
        /// renverse l'image selon un axe vertical
        /// </summary>
        public void MiroirVertical()
        {
            Pixel[,] clone = cloneImage(imagePixel);
            for (int i = 0; i < imagePixel.GetLength(0); i++)
            {
                for (int j = 0; j < imagePixel.GetLength(1); j++)
                {
                    imagePixel[i, j] = clone[i, imagePixel.GetLength(1) - 1 - j];
                }
            }
        }
        /// <summary>
        /// renvoie une matrice de pixel pareille que celle qui a été donnée en entrée
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        Pixel[,] cloneImage(Pixel[,] t)
        {
            if (t != null && t.Length != 0)
            {
                Pixel[,] retour = new Pixel[t.GetLength(0), t.GetLength(1)];
                for (int i = 0; i < t.GetLength(0); i++)
                {
                    for (int j = 0; j < t.GetLength(1); j++)
                    {
                        retour[i, j] = new Pixel(t[i, j].R, t[i, j].G, t[i, j].B);

                    }
                }
                return retour;
            }
            else { Console.WriteLine("matrice invalide"); return null; }

        }
        /// <summary>
        ///agrandit ou rétrécit l'image en fonction du coefficient donné
        /// </summary>
        /// <param name="coefh"></param>
        /// <param name="coefl"></param>
        public void ChangerTaille(double coefh, double coefl)
        {
            ///coefh est le coefficient qui affecte la hauteur de l'image et coefl affecte la largeur
            if (coefh > 0 && coefl > 0)
            {
                //change la taille de la matrice de pixel
                hauteur = (int)(hauteur * coefh);
                largeur = (int)(largeur * coefl);
                if (largeur == 0 || hauteur == 0) { hauteur++; largeur++; }

                //on vérifie si c'est multiple de 4, si ça l'est pas on modifie le padding pour que la fonction From_Image_to_file marche bien
                int largeurmultiple4 = largeur * 3;
                padding = 0;
                while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
                //on modifie la taille de l'image
                taille = (hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding + tailleOffset;

                Pixel[,] clone = cloneImage(imagePixel);
                //on crée une nouvelle image avec la taille modifiée
                imagePixel = new Pixel[hauteur, largeur];

                //on parcoure la nouvelle image en allant chercher le pixel correspondant ([i/coef,j/coef])
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        try
                        {
                            imagePixel[i, j] = new Pixel(clone[(int)(i / coefh), (int)(j / coefl)].R, clone[(int)(i / coefh), (int)(j / coefl)].G, clone[(int)(i / coefh), (int)(j / coefl)].B);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e + " ");
                        }

                    }
                }
            }
            else Console.WriteLine("coefficient inférieur à 0");


        }
        /// <summary>
        /// effectue une rotation de l'image selon l'angle donné
        /// </summary>
        /// <param name="angle"></param>
        public void Rotation(double angle)
        {
            //on gère les cas où l'angle en entrée est >360, >90, <0 et on convertit en radians
            //notre fonction ne marche que pour un angle compris entre 0 et 90 donc on ramène à chaque fois l'angle dans cette fourchette
            angle = angle % 360;
            if (angle < 0) angle = 360 + angle;
            if (angle > 270) { Image270(); angle = angle - 270; }
            else if (angle > 180) { Image180(); angle = angle - 180; }
            else if (angle > 90) { Image90(); angle = angle - 90; }
            angle = (angle) * (Math.PI / 180);

            //on sauvegarde temporairement la hauteur et la largeur de l'image de base
            int htemp = hauteur;
            int ltemp = largeur;
            //on cherche la taille de l'image une fois modifiée
            for (int i = 0; i < htemp; i++)
            {
                for (int j = 0; j < ltemp; j++)
                {
                    if ((int)(Math.Sin(angle) * (ltemp - 1) + Math.Cos(angle) * i - Math.Sin(angle) * j) >= hauteur) hauteur = (int)(Math.Sin(angle) * (ltemp - 1) + Math.Cos(angle) * i - Math.Sin(angle) * j) + 1;
                    if ((int)(Math.Cos(angle) * j + Math.Sin(angle) * i) >= largeur) largeur = (int)(Math.Cos(angle) * j + Math.Sin(angle) * i) + 1;

                }
            }
            //on vérifie si c'est multiple de 4, si ça l'est pas on modifie le padding pour que la fonction From_Image_to_file marche bien
            int largeurmultiple4 = largeur * 3;
            padding = 0;
            while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }

            //comme la taille de l'image change, la taille du fichier change aussi
            taille = (hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding + tailleOffset;

            Pixel[,] clone = cloneImage(imagePixel);

            //on remplit la nouvelle image de pixels blancs
            imagePixel = CreationMatricePixel(hauteur, largeur, 255);
            //on parcourt l'image de départ (le clone) et on le met à la bonne position dans imagePixel
            for (int i = 0; i < htemp; i++)
            {
                for (int j = 0; j < ltemp; j++)
                {
                    try
                    {
                        imagePixel[(int)(Math.Sin(angle) * (ltemp - 1) + Math.Cos(angle) * i - Math.Sin(angle) * j), (int)(Math.Cos(angle) * j + Math.Sin(angle) * i)] = new Pixel(clone[i, j].R, clone[i, j].G, clone[i, j].B);
                    }
                    catch
                    {
                        Console.WriteLine("[" + i + "," + j + "] devient: [" + (int)(Math.Sin(angle) * (ltemp - 1) + i - Math.Sin(angle) * j) + "," + (int)(j + Math.Sin(angle) * i) + "]");
                    }

                }
            }

        }

        #endregion
        /// <summary>
        /// parcoure chaque pixel, crée une matrice de pixel composé des voisins du pixel (parcelle) et applique le noyau sur cette matrice pour trouver la nouvelle valeur du pixel
        /// </summary>
        /// <param name="noyau"></param>
        public void applicationfiltre(double[,] noyau)
        {
            if (noyau != null && noyau.Length != 0 && noyau.GetLength(1) == noyau.GetLength(0))
            {
                Pixel[,] clone = cloneImage(imagePixel);
                Pixel[,] parcelle = CreationMatricePixel(noyau.GetLength(0), noyau.GetLength(1), 0);
                int décalage = (noyau.GetLength(0) - 1) / 2;
                for (int i = 0; i < imagePixel.GetLength(0); i++)
                {
                    for (int j = 0; j < imagePixel.GetLength(1); j++)
                    {
                        for (int k = -décalage; k <= décalage; k++)
                        {
                            for (int l = -décalage; l <= décalage; l++)
                            {
                                parcelle[k + décalage, l + décalage].réinitialiserPixel(0);
                                if (i + k >= 0 && j + l >= 0 && k + i < imagePixel.GetLength(0) && j + l < imagePixel.GetLength(1))
                                {
                                    parcelle[k + décalage, l + décalage].R = clone[i + k, j + l].R;
                                    parcelle[k + décalage, l + décalage].G = clone[i + k, j + l].G;
                                    parcelle[k + décalage, l + décalage].B = clone[i + k, j + l].B;
                                }
                            }

                        }
                        double[] appnoyau = applicationNoyau(parcelle, noyau);
                        imagePixel[i, j].R = (byte)(appnoyau[0]);
                        imagePixel[i, j].G = (byte)(appnoyau[1]);
                        imagePixel[i, j].B = (byte)(appnoyau[2]);
                    }

                }
            }
            else Console.WriteLine("noyau invalide");


        }
        /// <summary>
        /// dessine une fractale au milieu de l'image
        /// </summary>
        /// <param name="c"></param>
        public void creationFractale(Complex c)
        {
            bool borné = true;
            int originex = (hauteur - 1) / 2;
            int originey = (largeur - 1) / 2;

            for (int i = -originex; i < originex; i++)
            {
                for (int j = -originey; j < originey; j++)
                {
                    borné = borne(new Complex((double)(j / (hauteur / 2d)), (double)(i / (hauteur / 2d))), 0, c);
                    if (borné == true) imagePixel[i + originex, j + originey].G = 0;
                }
            }
        }
        /// <summary>
        /// regarde si la suite z+1=z*z+c est borné
        /// </summary>
        /// <param name="z"></param>
        /// <param name="compteur"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        bool borne(Complex z, int compteur, Complex c)
        {

            //Console.Write("équation de la forme z^2 + c avec c=i:");
            if (compteur < 18)
            {
                if (z.Real < 2000 && z.Imaginary < 2000)
                {
                    z = (z * z) + c;


                    compteur++;
                    //Console.Write(z);
                    return borne(z, compteur, c);
                }
                else return false;
            }
            else return true;
        }
        /// <summary>
        /// retourne une matrice de pixel avec une taille donnée en entrée et remplis avec des pixels gris selon la valeur donnée
        /// </summary>
        /// <param name="hauteur"></param>
        /// <param name="largeur"></param>
        /// <param name="valeur"></param>
        /// <returns></returns>
        Pixel[,] CreationMatricePixel(int hauteur, int largeur, byte valeur)
        {
            if (hauteur > 0 && largeur > 0)
            {
                Pixel[,] retour = new Pixel[hauteur, largeur];
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        retour[i, j] = new Pixel(valeur, valeur, valeur);
                    }
                }
                return retour;
            }
            else return null;

        }
        /// <summary>
        /// crée un histogramme et le rajoute en dessous de l'image
        /// </summary>
        public void Histogramme()
        {
            #region création de l'histogramme
            int[] rouge = new int[256];
            int[] bleu = new int[256];
            int[] vert = new int[256];
            ///parcoure toute les valeurs de byte possibles, compte le nombre de byte rouge,vert et bleu associé à cette valeur et le met dans chaque tableau associé
            int compteurR = 0;
            int compteurG = 0;
            int compteurB = 0;
            for (int i = 0; i <= 255; i++)
            {
                foreach (Pixel p in imagePixel)
                {
                    ///la case i de chaque tableau contient le nombre de byte de couleur égaux à i
                    if (hauteur * largeur > 450000)///si l'image est très grande, on ne compte qu'une fois sur 500 pour limiter la mémoire utilisée
                    {
                        if (p.R == (byte)(i) && compteurR % 500 == 0) rouge[i]++;
                        else compteurR++;
                        if (p.G == (byte)(i) && compteurG % 500 == 0) vert[i]++;
                        else compteurG++;
                        if (p.B == (byte)(i) && compteurB % 500 == 0) bleu[i]++;
                        else compteurB++;
                    }
                    else
                    {
                        if (p.R == (byte)(i)) rouge[i]++;
                        if (p.G == (byte)(i)) vert[i]++;
                        if (p.B == (byte)(i)) bleu[i]++;
                    }

                }
            }


            ///cherche la valeur maximale des tableaux
            int max = rouge[maxTableau(rouge)];
            if (bleu[maxTableau(bleu)] > max) max = bleu[maxTableau(bleu)];
            if (vert[maxTableau(vert)] > max) max = vert[maxTableau(vert)];


            ///crée une matrice de pixel qui va contenir uniquement l'histogramme
            Pixel[,] histogramme = CreationMatricePixel(max, 256 * 3, 255);

            ///on dessine l'histogramme associé à chaque couleur
            for (int j = 0; j < 256; j++)
            {
                ///plus la valeur contenue dans la case j sera grande, plus la barre de pixel sera haute
                for (int i = 0; i < rouge[j]; i++)
                {
                    histogramme[i, j] = new Pixel(255, 0, 0);
                }
            }
            for (int j = 256; j < 512; j++)
            {
                for (int i = 0; i < vert[j - 256]; i++)
                {
                    histogramme[i, j] = new Pixel(0, 255, 0);
                }
            }
            for (int j = 512; j < histogramme.GetLength(1); j++)
            {
                for (int i = 0; i < bleu[j - 512]; i++)
                {
                    histogramme[i, j] = new Pixel(0, 0, 255);
                }
            }
            ///on ramène la hauteur de l'histogramme à 100 pour avoir la même taille pour toutes les images
            histogramme = ChangerTailleMatrice(100d / max, 1, histogramme);
            #endregion
            #region on fusionne l'histogramme qu'on vient de créer avec l'image originelle

            ///on veut mettre l'histogramme en dessous de l'image originelle donc on doit rajouter 100 pixels à hauteur
            hauteur = 100 + hauteur;
            ///la largeur doit être supérieur à la largeur de l'histogramme
            while (largeur < 768)
            {
                largeur++;
            }

            ///on ajuste les attributs de l'image
            int largeurmultiple4 = largeur * (bitparcouleur / 8);
            padding = 0;
            if ((largeur * (bitparcouleur / 8)) % 4 != 0)
            {
                //rajoute un octet à la fin de la ligne tant que la largeur n'est pas multiple de 4 et retient le décalage (padding)
                while (largeurmultiple4 % 4 != 0) { padding++; largeurmultiple4++; }
            }
            taille = (hauteur * largeur * (bitparcouleur / 8)) + hauteur * padding + tailleOffset;

            Pixel[,] clone = cloneImage(imagePixel);
            imagePixel = CreationMatricePixel(hauteur, largeur, 255);
            ///on remplit d'abord avec l'image de base
            for (int i = 0; i < clone.GetLength(0); i++)
            {
                for (int j = 0; j < clone.GetLength(1); j++)
                {
                    imagePixel[hauteur - 1 - i, j].R = clone[clone.GetLength(0) - 1 - i, j].R;
                    imagePixel[hauteur - 1 - i, j].G = clone[clone.GetLength(0) - 1 - i, j].G;
                    imagePixel[hauteur - 1 - i, j].B = clone[clone.GetLength(0) - 1 - i, j].B;
                }
            }
            ///on rajoute ensuite l'histogramme en dessous
            for (int i = 0; i < histogramme.GetLength(0); i++)
            {
                for (int j = 0; j < histogramme.GetLength(1); j++)
                {
                    imagePixel[i, j].R = histogramme[i, j].R;
                    imagePixel[i, j].G = histogramme[i, j].G;
                    imagePixel[i, j].B = histogramme[i, j].B;
                }
            }
            #endregion

        }
        /// <summary>
        /// pareil que ChangerTaille mais retourne une matrice de Pixel (utile dans histogramme) 
        /// </summary>
        /// <param name="coefh"></param>
        /// <param name="coefl"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Pixel[,] ChangerTailleMatrice(double coefh, double coefl, Pixel[,] source)
        {
            if (coefh > 0 && coefl > 0 && source != null && source.Length != 0)
            {
                //change la taille de la matrice de pixel
                int h = (int)(source.GetLength(0) * coefh);
                int l = (int)(source.GetLength(1) * coefl);
                if (l == 0 || h == 0) { h++; l++; }

                //on vérifie si c'est multiple de 4, si ça l'est pas on modifie le padding pour que la fonction From_Image_to_file marche bien

                Pixel[,] clone = cloneImage(source);
                //on crée une nouvelle image avec la taille modifiée
                source = new Pixel[h, l];

                //on parcoure la nouvelle image en allant chercher le pixel correspondant ([i/coef,j/coef])
                for (int i = 0; i < h; i++)
                {
                    for (int j = 0; j < l; j++)
                    {
                        try
                        {
                            source[i, j] = new Pixel(clone[(int)(i / coefh), (int)(j / coefl)].R, clone[(int)(i / coefh), (int)(j / coefl)].G, clone[(int)(i / coefh), (int)(j / coefl)].B);
                        }
                        catch (Exception e)
                        {
                            Console.Write(e + " ");
                        }

                    }
                }
                return source;
            }
            else
            {
                Console.WriteLine("coefficient inférieur à 0");
                return null;
            }


        }

        /// <summary>
        /// retourne l'index du maximum d'un tableau de int donné en entrée
        /// </summary>
        /// <param name="tableau"></param>
        /// <returns></returns>
        int maxTableau(int[] tableau)
        {
            if (tableau != null && tableau.Length != 0)
            {
                int retour = -1;//valeur de l'index du max temporaire
                int tempo = -1;//valeur maximale temporaire
                for (int n = 0; n < tableau.Length; n++)
                {
                    if (tableau[n] > tempo)
                    {
                        retour = n;
                        tempo = tableau[n];
                    }
                }
                return retour;
            }
            else return -1;

        }
        /// <summary>
        /// multiplie case par case deux matrices et fait la somme retourne un tableau d'entier
        /// </summary>
        /// <param name="parcelle"></param>
        /// <param name="noyau"></param>
        /// <returns></returns>
        static double[] applicationNoyau(Pixel[,] parcelle, double[,] noyau)
        {
            if (parcelle != null && noyau != null && parcelle.Length != 0 && noyau.Length != 0)
            {
                ///il faut retourner une valeur par couleur donc il y a 3 cases dans le tableau de retour
                double[] retour = new double[3];
                for (int i = 0; i < parcelle.GetLength(0); i++)
                {
                    for (int j = 0; j < parcelle.GetLength(1); j++)
                    {
                        retour[0] = (double)(retour[0] + parcelle[i, j].R * noyau[i, j]);
                        retour[1] = (double)(retour[1] + parcelle[i, j].G * noyau[i, j]);
                        retour[2] = (double)(retour[2] + parcelle[i, j].B * noyau[i, j]);

                    }
                }
                for (int i = 0; i < retour.Length; i++)///on majore par 255 et minore par 0 pour éviter que ça dépasse les bornes du byte
                {
                    if (retour[i] > 255) retour[i] = 255;
                    if (retour[i] < 0) retour[i] = 0;
                }
                return retour;
            }
            else return null;
        }

        /// <summary>
        /// cache imagecachée dans image montrée
        /// </summary>
        /// <param name="imagecachée"></param>
        /// <param name="imagemontrée"></param>
        void CrypterImage(Pixel[,] imagecachée, Pixel[,] imagemontrée)
        {
            if (hauteur == imagemontrée.GetLength(0) && largeur == imagemontrée.GetLength(1))
            {
                if (imagecachée.GetLength(0) == hauteur && imagecachée.GetLength(1) == largeur)
                {
                    for (int i = 0; i < hauteur; i++)
                    {
                        for (int j = 0; j < largeur; j++)
                        {
                            //try
                            {
                                imagePixel[i, j].R = nouvelleValeurCrypte(imagecachée[i, j].R, imagemontrée[i, j].R);
                                imagePixel[i, j].G = nouvelleValeurCrypte(imagecachée[i, j].G, imagemontrée[i, j].G);
                                imagePixel[i, j].B = nouvelleValeurCrypte(imagecachée[i, j].B, imagemontrée[i, j].B);
                            }
                        }
                    }
                }
            }

        }
        /// <summary>
        /// montre l'image qui était cachée dans l'image montrée
        /// </summary>
        public void DecrypterImage()
        {
            if (hauteur > 0 && largeur > 0)
            {
                Pixel[,] clone = cloneImage(imagePixel);
                for (int i = 0; i < hauteur; i++)
                {
                    for (int j = 0; j < largeur; j++)
                    {
                        imagePixel[i, j].R = nouvelleValeurDecrypte(clone[i, j].R);
                        imagePixel[i, j].G = nouvelleValeurDecrypte(clone[i, j].G);
                        imagePixel[i, j].B = nouvelleValeurDecrypte(clone[i, j].B);
                    }
                }
            }
        }
        /// <summary>
        /// remplace les 4 derniers bit de imagemontrée par les 4 premiers bits de imagecachée
        /// </summary>
        /// <param name="imageCachée"></param>
        /// <param name="imageMontrée"></param>
        /// <returns></returns>
        static byte nouvelleValeurCrypte(byte imageCachée, byte imageMontrée)
        {
            int[] tabCaché = nombreToBinaire(imageCachée);
            int[] tabMontré = nombreToBinaire(imageMontrée);
            int[] tabFinal = new int[8];
            for (int k = 0; k < 8; k++)
            {
                if (k < 4)
                {
                    tabFinal[k] = tabMontré[k];
                }
                else
                {
                    tabFinal[k] = tabCaché[k - 4];
                }
            }
            return binaireToNombre(tabFinal);
        }
        /// <summary>
        /// prend les 4 derniers bit de imagecryptée et retourne un byte avec les 4 premiers bit et 4 zéros derrières
        /// </summary>
        /// <param name="imageCryptée"></param>
        /// <returns></returns>
        static byte nouvelleValeurDecrypte(byte imageCryptée)
        {
            int[] tabCrypté = nombreToBinaire(imageCryptée);
            int[] tabFinal = new int[8];
            for (int k = 0; k < 8; k++)
            {
                if (k < 4)
                {
                    tabFinal[k] = tabCrypté[k + 4];
                }
                else
                {
                    tabFinal[k] = 0;
                }
            }
            return binaireToNombre(tabFinal);
        }
        /// <summary>
        /// prend un byte et retourne le tableau de 8 bit associé
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static int[] nombreToBinaire(byte A)
        {
            int I = A;
            int[] tab = { 0, 0, 0, 0, 0, 0, 0, 0 };
            int R = 128;
            int k = 0;
            while (I > 0 && k < 8)
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
        /// prend un tableau de bits et retourne le nombre associé
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static byte binaireToNombre(int[] A)
        {
            int D = 128;
            int R = 0;
            for (int i = 0; i < A.Length; i++)
            {
                R += A[i] * D;
                D = D / 2;
            }
            return (byte)(R);

        }
        public override string ToString()
        {
            return "l'image a un format " + type + " \n" +
                "elle fait " + largeur + " pixels de large et " + hauteur + " pixels de hauteur\n" +
                "sa taille en octets est de " + taille + "\n" +
                "ses pixels sont codés sur " + bitparcouleur + " bit";
        }
    }


}