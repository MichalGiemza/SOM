using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOM
{
    public static class ZbiorTreningowy
    {
        public static Siec.Punkt[] Prostokat(double maxX, double maxY, int margX, int margY, int ilosc)
        {
            Random r = new Random();
            Siec.Punkt[] result = new Siec.Punkt[ilosc];

            for (int i = 0; i < ilosc; i++)
            {
                result[i] = new Siec.Punkt(r.NextDouble() * (maxX - 2 * margX) + margX, r.NextDouble() * (maxY - 2 * margY) + margY);
            }

            return result;
        }

        public static Siec.Punkt[] Romb(double maxX, double maxY, int margX, int margY, int ilosc)
        {
            Random r = new Random();
            Siec.Punkt[] result = new Siec.Punkt[ilosc];
            Siec.Punkt p1, p2, p3, p4;

            p1 = new Siec.Punkt(maxX / 2, margY);
            p2 = new Siec.Punkt(maxX - margX, maxY / 2);
            p3 = new Siec.Punkt(maxX / 2, maxY - margY);
            p4 = new Siec.Punkt(margX, maxY / 2);


            for (int i = 0; i < ilosc; i++)
            {
                bool zawieraSie;
                do
                {
                    result[i] = new Siec.Punkt(r.NextDouble() * (maxX - 2 * margX) + margX, r.NextDouble() * (maxY - 2 * margY) + margY);

                    zawieraSie = false;
                    zawieraSie |= PunktWTrojkacie(result[i], p1, p2, p4); // Górna część rombu
                    zawieraSie |= PunktWTrojkacie(result[i], p2, p3, p4); // Dolna część rombu

                } while (zawieraSie == false);
            }

            return result;
        }

        public static Siec.Punkt[] Trojkat(double maxX, double maxY, int margX, int margY, int ilosc)
        {
            Random r = new Random();
            Siec.Punkt[] result = new Siec.Punkt[ilosc];
            Siec.Punkt p1, p2, p3;

            p1 = new Siec.Punkt(maxX / 2, margY);
            p2 = new Siec.Punkt(maxX - margX, maxY - margY);
            p3 = new Siec.Punkt(margX, maxY - margY);

            for (int i = 0; i < ilosc; i++)
            {
                do
                {
                    result[i] = new Siec.Punkt(r.NextDouble() * (maxX - 2 * margX) + margX, r.NextDouble() * (maxY - 2 * margY) + margY);

                } while (PunktWTrojkacie(result[i], p1, p2, p3) == false);
            }

            return result;
        }

        public static Siec.Punkt[] Elipsa(double maxX, double maxY, int margX, int margY, int ilosc)
        {
            Random r = new Random();
            Siec.Punkt[] result = new Siec.Punkt[ilosc];
            Siec.Punkt e;

            e = new Siec.Punkt(maxX / 2, maxY / 2);

            for (int i = 0; i < ilosc; i++)
            {
                do
                {
                    result[i] = new Siec.Punkt(r.NextDouble() * (maxX - 2 * margX) + margX, r.NextDouble() * (maxY - 2 * margY) + margY);

                } while (PunktWElipsie(result[i], e, (maxX - 2 * margX) / 2, (maxY - 2 * margY) / 2) == false);
            }

            return result;
        }
        
        private static double PunktProsta(Siec.Punkt pt, Siec.Punkt p1, Siec.Punkt p2)
        {
            return (pt.x - p2.x) * (p1.y - p2.y) - (p1.x - p2.x) * (pt.y - p2.y);
        }

        private static bool PunktWTrojkacie(Siec.Punkt pt, Siec.Punkt p1, Siec.Punkt p2, Siec.Punkt p3)
        {
            bool result = false;

            result |= PunktProsta(pt, p1, p2) < 0.0;
            result |= PunktProsta(pt, p2, p3) < 0.0;
            result |= PunktProsta(pt, p3, p1) < 0.0;

            return result == false;
        }

        private static bool PunktWElipsie(Siec.Punkt pt, Siec.Punkt e, double rx, double ry)
        {
            return Math.Pow(pt.x - e.x, 2) / Math.Pow(rx, 2) + Math.Pow(pt.y - e.y, 2) / Math.Pow(ry, 2) <= 1;
        }
    }
}
