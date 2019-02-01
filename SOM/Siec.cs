using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SOM
{
    public class Siec
    {
        private PunktSieci[,] siec;
        private int xEl, yEl;
        private Line[,] pionowe, poziome;
        private Punkt[] zbiorTreningowy;
        private int iteracja;

        private double sila_przesuniecia;
        private double promien_zmian;
        private double poczatkowa_sila_przesuniecia;
        private double poczatkowy_promien_zmian;
        private double spadek_sily_przesuniecia;
        private double spadek_promienia;

        private double param_sily_przes;

        public int ElementyWPoziomie { get { return xEl; } }
        public int ElementyWPionie { get { return yEl; } }
        public PunktSieci[,] PunktySieci { get { return siec; } }
        public Punkt[] ZbiorTreningowy { get { return zbiorTreningowy; } set { zbiorTreningowy = value; } }
        public int Iteracja { get { return iteracja; } }
        public double PoczatkowaSilaPrzesuniecia { get { return poczatkowa_sila_przesuniecia; } set { poczatkowa_sila_przesuniecia = value; } }
        public double PoczatkowyPromienZmian { get { return poczatkowy_promien_zmian; } set { poczatkowy_promien_zmian = value; } }
        public double SpadekSilyPrzesuniecia { get { return spadek_sily_przesuniecia; } set { spadek_sily_przesuniecia = value; } }
        public double SpadekPromienia { get { return spadek_promienia; } set { spadek_promienia = value; } }
        public double SilaPrzesuniecia { get { return sila_przesuniecia; } }
        public double PromienZmian { get { return promien_zmian; } }

        public Siec(int x_el, int y_el, Canvas canvas, Punkt pozycja_poczatkowa, double delta_pozycji, Punkt[] zbiorTreningowy, double sila_przesuniecia, double promien_zmian, double spadek_sily_przesuniecia, double spadek_promienia)
        {
            this.spadek_sily_przesuniecia = spadek_sily_przesuniecia;
            this.spadek_promienia = spadek_promienia;
            this.zbiorTreningowy = zbiorTreningowy;
            this.sila_przesuniecia = poczatkowa_sila_przesuniecia = sila_przesuniecia;
            this.promien_zmian = poczatkowy_promien_zmian = promien_zmian;
            iteracja = 0;
            OblParamertFunkcjiSilyPrzesuniecia();

            xEl = x_el;
            yEl = y_el;
            siec = new PunktSieci[x_el, y_el];

            Random r = new Random();
            for (int i = 0; i < x_el; i++)
            {
                for (int j = 0; j < y_el; j++)
                {
                    double x = pozycja_poczatkowa.x + (r.NextDouble() * 2 - 1) * delta_pozycji;
                    double y = pozycja_poczatkowa.y + (r.NextDouble() * 2 - 1) * delta_pozycji;
                    siec[i, j] = new PunktSieci(x, y, i, j, this);
                }
            }

            ZainicjujLinie(canvas);
        }

        private void ZainicjujLinie(Canvas c)
        {
            if (xEl - 1 > 0)
            {
                poziome = new Line[xEl - 1, yEl];
                for (int i = 1; i < xEl; i++)
                {
                    for (int j = 0; j < yEl; j++)
                    {
                        poziome[i - 1, j] = new Line()
                        {
                            Stroke = Brushes.Black,
                            Fill = Brushes.Black,
                            StrokeThickness = 1,

                            X1 = siec[i, j].x,
                            Y1 = siec[i, j].y,
                            X2 = siec[i - 1, j].x,
                            Y2 = siec[i - 1, j].y
                        };

                        c.Children.Add(poziome[i - 1, j]);
                    }
                }
            }

            if (yEl - 1 > 0)
            {
                pionowe = new Line[xEl, yEl - 1];
                for (int j = 1; j < yEl; j++)
                {
                    for (int i = 0; i < xEl; i++)
                    {
                        pionowe[i, j - 1] = new Line()
                        {
                            Stroke = Brushes.Black,
                            Fill = Brushes.Black,
                            StrokeThickness = 1,
                            
                            X1 = siec[i, j].x,
                            Y1 = siec[i, j].y,
                            X2 = siec[i, j - 1].x,
                            Y2 = siec[i, j - 1].y
                        };

                        c.Children.Add(pionowe[i, j - 1]);
                    }
                }
            }
        }
        public void Rysuj()
        {
            //if (iteracja % 10 != 0)
            //    return;

            if (poziome != null)
            {
                for (int i = 1; i < xEl; i++)
                {
                    for (int j = 0; j < yEl; j++)
                    {
                        poziome[i - 1, j].X1 = siec[i, j].x;
                        poziome[i - 1, j].Y1 = siec[i, j].y;
                        poziome[i - 1, j].X2 = siec[i - 1, j].x;
                        poziome[i - 1, j].Y2 = siec[i - 1, j].y;
                    }
                }
            }

            if (pionowe != null)
            {
                for (int j = 1; j < yEl; j++)
                {
                    for (int i = 0; i < xEl; i++)
                    {
                        pionowe[i, j - 1].X1 = siec[i, j].x;
                        pionowe[i, j - 1].Y1 = siec[i, j].y;
                        pionowe[i, j - 1].X2 = siec[i, j - 1].x;
                        pionowe[i, j - 1].Y2 = siec[i, j - 1].y;
                    }
                }
            }
        }
        public void Wymaz(Canvas c)
        {
            if (poziome != null)
            {
                foreach (var l in poziome)
                    c.Children.Remove(l);
            }

            if (pionowe != null)
            {
                foreach (var l in pionowe)
                    c.Children.Remove(l);
            }
        }

        public void WykonajIteracje()
        {
            Punkt pTr = zbiorTreningowy[iteracja % zbiorTreningowy.Length];
            
            // Wybierz najbliższy
            PunktSieci zwyciezca = null;
            double odl_zwyciezcy = double.MaxValue;
            foreach (PunktSieci p in siec)
            {
                double odl_p = p.Odleglosc(pTr);

                if (odl_p < odl_zwyciezcy)
                {
                    zwyciezca = p;
                    odl_zwyciezcy = odl_p;
                }
            }

            // Wprowadzenie zmian
            PrzesunNeurony(pTr, zwyciezca);

            // Spadek wartości parametrów
            ObliczNowaSilePrzesuniecia();
            SpadekPromieniaZmian();

            iteracja += 1;
        }
        
        private void ObliczNowaSilePrzesuniecia()
        {
            sila_przesuniecia = Math.Pow(Math.E, -(iteracja / spadek_sily_przesuniecia)) * poczatkowa_sila_przesuniecia;
        }
        private void SpadekPromieniaZmian()
        {
            promien_zmian = Math.Pow(Math.E, -(iteracja / spadek_promienia)) * poczatkowy_promien_zmian;
        }
        private double FunkcjaZmian(double odleglosc)
        {
            const double b = 1.0;
            double a = (-b) / promien_zmian;

            double result = a * odleglosc + b;

            if (result > 0)
                return result;
            else
                return 0;
        }
        private double FunkcjaSilyPrzesuniecia(double odleglosc)
        {
            double gauss = Math.Exp(-Math.Pow(odleglosc, 2) / (2 * Math.Pow(poczatkowy_promien_zmian / 3, 2))) / Math.Sqrt(2 * Math.PI * Math.Pow(poczatkowy_promien_zmian / 3, 2));
            return gauss / param_sily_przes * sila_przesuniecia;
        }
        private void OblParamertFunkcjiSilyPrzesuniecia()
        {
            param_sily_przes = 1 / Math.Sqrt(2 * Math.PI * Math.Pow(poczatkowy_promien_zmian / 3, 2));
        }
        private void PrzesunNeurony(Punkt pTr, PunktSieci p, double odl = 0.0, bool wPrawo = true, bool wLewo = true, bool wGore = true, bool wDol = true)
        {
            if (FunkcjaZmian(odl) == 0)
                return;

            // Przesun
            PrzesunNeuron(pTr, p, odl);

            // Rozwiń sąsiadów
            PunktSieci s;
            // Rozwiń sąsiadów w pionie
            s = p.SasiadGorny();
            if (wGore && s != null)
                PrzesunNeurony(pTr, s, 1 + odl, false, false, true, false);
            
            s = p.SasiadDolny();
            if (wDol && s != null)
                PrzesunNeurony(pTr, s, 1 + odl, false, false, false, true);

            // Rozwiń sąsiadów w poziomie
            s = p.SasiadLewy();
            if (wLewo && s != null)
                PrzesunNeurony(pTr, s, 1 + odl, false, true, true, true);

            s = p.SasiadPrawy();
            if (wPrawo && s != null)
                PrzesunNeurony(pTr, s, 1 + odl, true, false, true, true);
        }
        private void PrzesunNeuron(Punkt pTr, Punkt p, double odl)
        {
            if (FunkcjaZmian(odl) == 0)
                return;
            
            double x = pTr.x - p.x;
            double y = pTr.y - p.y;

            // Przesuniecie
            p.x += FunkcjaSilyPrzesuniecia(odl) * x;
            p.y += FunkcjaSilyPrzesuniecia(odl) * y;
        }

        public class Punkt
        {
            public double x, y;
            
            public Punkt(double x, double y)
            {
                this.x = x;
                this.y = y;
            }

            public double Odleglosc(Punkt p)
            {
                return Math.Sqrt(Math.Pow(x - p.x, 2) + Math.Pow(y - p.y, 2));
            }
        }
        public class PunktSieci : Punkt
        {
            private int i, j;
            Siec s;

            public PunktSieci(double x, double y, int i, int j, Siec siec) : base(x, y)
            {
                this.i = i;
                this.j = j;
                s = siec;
            }

            public PunktSieci SasiadGorny()
            {
                if (j - 1 < 0)
                    return null;

                return s.PunktySieci[i, j - 1];
            }
            public PunktSieci SasiadDolny()
            {
                if (j + 1 >= s.ElementyWPionie)
                    return null;

                return s.PunktySieci[i, j + 1];
            }
            public PunktSieci SasiadLewy()
            {
                if (i - 1 < 0)
                    return null;

                return s.PunktySieci[i - 1, j];
            }
            public PunktSieci SasiadPrawy()
            {
                if (i + 1 >= s.ElementyWPoziomie)
                    return null;

                return s.PunktySieci[i + 1, j];
            }
        }
    }
}
