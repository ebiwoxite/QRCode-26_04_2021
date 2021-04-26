

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomon
{
    class Pixel
    {
        private byte r = 0;
        private byte g = 0;
        private byte b = 0;

        #region get/set
        public byte R
        {
            get { return r; }
            set { r = value; }
        }
        public byte G
        {
            get { return g; }
            set { g = value; }
        }
        public byte B
        {
            get { return b; }
            set { b = value; }
        }
        #endregion

        public Pixel(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
        /// <summary>
        /// inverse les couleurs
        /// </summary>
        public void InverserCouleur()
        {
            r = (byte)(255 - r);
            g = (byte)(255 - g);
            b = (byte)(255 - b);
        }
        /// <summary>
        /// met en nuance de gris
        /// </summary>
        public void NuancesDeGris()
        {
            int moyenne = Convert.ToByte((r + g + b) / 3);
            r = (byte)(moyenne);
            g = (byte)(moyenne);
            b = (byte)(moyenne);
        }
        /// <summary>
        /// met en noir et blanc
        /// </summary>
        public void NoirBlanc()
        {
            int moyenne = (byte)((r + g + b) / 3);
            if (moyenne < 126) moyenne = 0;
            else moyenne = 255;
            r = (byte)(moyenne);
            g = (byte)(moyenne);
            b = (byte)(moyenne);
        }
        /// <summary>
        /// met le pixel en gris selon la valeur donnée
        /// </summary>
        /// <param name="valeur"></param>
        public void réinitialiserPixel(byte valeur)
        {
            r = valeur;
            g = valeur;
            b = valeur;
        }
        public override string ToString()

        {
            return r + " " + g + " " + b;
        }

    }
}