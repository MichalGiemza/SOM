using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SOM
{
    enum ZbiorTreningowyListItems
    {
        prostokat,
        romb,
        elipsa,
        trojkat
    }

    public partial class MainWindow : Window
    {
        Siec s;
        DispatcherTimer timer;
        bool symulacjaTrwa;
        UIElement zbTreningowyRys;
        int xEl = 10, yEl = 10;
        int xMargin = 0, yMargin = 0;
        double deltaPozPocz = 20.0;
        Siec.Punkt[] zbTreningowyPunkty;
        double silaPrzesuniecia = 0.2;
        double promienZmian = 6.0;
        double spadekSilyPrzesuniecia = 5050;
        double spadekPromienia = 5050;
        UIElement[] zbTrenPunktyRys;
        ZbiorTreningowyListItems typZbioru = ZbiorTreningowyListItems.prostokat;

        bool rysujPunktyTrenDebug = false;

        public MainWindow()
        {
            InitializeComponent();

            canvas.Loaded += SetMargins;

            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(TimerTick);
            timer.Interval = new TimeSpan(1000);
            
            resetButton.Click += ResetButton_Click;
            WprowadzZmianyWSieci(Przebudowa: true);

            zbTreningowyList.SelectionChanged += ZbTreningowyList_SelectionChanged;
            ZbTreningowyList_SelectionChanged(null, null);

            startButton.Click += ToggleSimulation;

            rysZbTrenCheckbox.Checked += RysZbTrenCheckbox_Checked;
            rysZbTrenCheckbox.Unchecked += RysZbTrenCheckbox_Unchecked;
            rysZbTrenCheckbox.IsChecked = true;
            
            marginesXControl.ValueChanged += MarginesXControl_ValueChanged;
            marginesYControl.ValueChanged += MarginesYControl_ValueChanged;

            neuronyPoziomControl.Value = xEl;
            neuronyPoziomControl.ValueChanged += NeuronyPoziomControl_ValueChanged;
            neuronyPionControl.Value = yEl;
            neuronyPionControl.ValueChanged += NeuronyPionControl_ValueChanged;

            deltaPozPoczControl.Value = deltaPozPocz;
            deltaPozPoczControl.ValueChanged += DeltaPozPoczControl_ValueChanged;

            PoczSilPrzesControl.Value = silaPrzesuniecia;
            PoczSilPrzesControl.ValueChanged += PoczSilPrzesControl_ValueChanged;
            PoczPromZmianControl.Value = promienZmian;
            PoczPromZmianControl.ValueChanged += PoczPromZmianControl_ValueChanged1;

            SpadekSilyControl.ValueChanged += SpadekSilyControl_ValueChanged;
            SpadekSilyIndicator.Content = "Exp( - iteracja / " + spadekSilyPrzesuniecia.ToString() + " )";
            SpadekPromieniaControl.ValueChanged += SpadekPromieniaControl_ValueChanged;
            SpadekPromieniaIndicator.Content = "Exp( - iteracja / " + spadekPromienia.ToString() + " )";
            
            symulacjaTrwa = false;
            
            s.Rysuj();
        }
        
        private void SetMargins(object sender, RoutedEventArgs e)
        {
            xMargin = (int)(canvas.ActualWidth / 20.0 * marginesXControl.Value);
            yMargin = (int)(canvas.ActualHeight / 20.0 * marginesXControl.Value);
            WprowadzZmianyWSieci(ModZbTrening: true);
        }
        
        private void DeltaPozPoczControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            deltaPozPocz = (double)e.NewValue;

            WprowadzZmianyWSieci(Przebudowa: true);
        }

        private void PoczSilPrzesControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            silaPrzesuniecia = (double)e.NewValue;

            WprowadzZmianyWSieci(ModPoczSilyPrzes: true);
        }

        private void PoczPromZmianControl_ValueChanged1(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            promienZmian = (double)e.NewValue;

            WprowadzZmianyWSieci(ModPoczPromZmian: true);
        }
        
        private void MarginesXControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            xMargin = (int)(canvas.ActualWidth / 20.0 * e.NewValue);

            WprowadzZmianyWSieci(ModZbTrening: true);
        }

        private void MarginesYControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            yMargin = (int)(canvas.ActualHeight / 20.0 * e.NewValue);

            WprowadzZmianyWSieci(ModZbTrening: true);
        }

        private void SpadekSilyControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            spadekSilyPrzesuniecia = e.NewValue;

            SpadekSilyIndicator.Content = "Exp( - iteracja / " + spadekSilyPrzesuniecia.ToString("0") + " )";

            WprowadzZmianyWSieci(ModSpadkuSilyPrzes: true);
        }

        private void SpadekPromieniaControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            spadekPromienia = e.NewValue;

            SpadekPromieniaIndicator.Content = "Exp( - iteracja / " + spadekPromienia.ToString("0") + " )";

            WprowadzZmianyWSieci(ModSpadkuProm: true);
        }
        
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (symulacjaTrwa)
                ToggleSimulation(null, null);

            WprowadzZmianyWSieci(Przebudowa: true);
        }

        private void ZbTreningowyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            typZbioru = (ZbiorTreningowyListItems)zbTreningowyList.SelectedIndex;
            
            WprowadzZmianyWSieci(ModZbTrening: true);
        }

        private void NeuronyPoziomControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            xEl = (int)e.NewValue;

            WprowadzZmianyWSieci(Przebudowa: true);
        }

        private void NeuronyPionControl_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            yEl = (int)e.NewValue;

            WprowadzZmianyWSieci(Przebudowa: true);
        }

        private void RysZbTrenCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (zbTreningowyRys == null)
            {
                rysZbTrenCheckbox.IsChecked = false;
                return;
            }

            zbTreningowyList.IsEnabled = true;
            marginesXControl.IsEnabled = true;
            marginesYControl.IsEnabled = true;

            canvas.Children.Insert(0, zbTreningowyRys);

            if (rysujPunktyTrenDebug)
            {
                zbTrenPunktyRys = new UIElement[zbTreningowyPunkty.Length];
                for (int i = 0; i < zbTreningowyPunkty.Length; i++)
                    zbTrenPunktyRys[i] = PunktTreningowyRys(zbTreningowyPunkty[i].x, zbTreningowyPunkty[i].y);

                foreach (var p in zbTrenPunktyRys)
                    canvas.Children.Add(p);
            }
        }

        private void RysZbTrenCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            zbTreningowyList.IsEnabled = false;
            marginesXControl.IsEnabled = false;
            marginesYControl.IsEnabled = false;

            canvas.Children.Remove(zbTreningowyRys);

            if (rysujPunktyTrenDebug)
            {
                foreach (var p in zbTrenPunktyRys)
                    canvas.Children.Remove(p);
            }
        }

        private void ToggleSimulation(object sender, RoutedEventArgs e)
        {
            if (symulacjaTrwa == false)
            {
                // Start simulation
                symulacjaTrwa = true;
                timer.Start();
                startButton.Content = "Stop";
                neuronyPionControl.IsEnabled = false;
                neuronyPoziomControl.IsEnabled = false;
                deltaPozPoczControl.IsEnabled = false;
            }
            else
            {
                // Stop simulation
                symulacjaTrwa = false;
                timer.Stop();
                startButton.Content = "Start";
                neuronyPionControl.IsEnabled = true;
                neuronyPoziomControl.IsEnabled = true;
                deltaPozPoczControl.IsEnabled = true;
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            s.WykonajIteracje();
            s.Rysuj();
            OdswiezWskazniki();
        }

        private void OdswiezWskazniki()
        {
            IteracjaIndicator.Content = s.Iteracja.ToString();
            SilaPrzesIndicator.Content = s.SilaPrzesuniecia.ToString("0.0000");
            PromZmianIndicator.Content = s.PromienZmian.ToString("0.00");
        }

        private UIElement ProstokatRys(int XMargin, int YMargin)
        {
            Rectangle r = new Rectangle();
            r.Height = canvas.Height - YMargin * 2;
            r.Width = canvas.Width - XMargin * 2;
            r.Margin = new Thickness(XMargin, YMargin, 0, 0);
            r.Fill = Brushes.LightGray;

            return r;
        }

        private UIElement RombRys(int XMargin, int YMargin)
        {
            Polygon r = new Polygon();

            r.Points.Add(new Point(canvas.Width / 2, YMargin));
            r.Points.Add(new Point(canvas.Width - xMargin, canvas.Height / 2));
            r.Points.Add(new Point(canvas.Width / 2, canvas.Height - YMargin));
            r.Points.Add(new Point(xMargin, canvas.Height / 2));
                
            r.Fill = Brushes.LightGray;

            return r;
        }

        private UIElement ElipsaRys(int XMargin, int YMargin)
        {
            Ellipse r = new Ellipse();
            r.Height = canvas.Height - YMargin * 2;
            r.Width = canvas.Width - XMargin * 2;
            r.Margin = new Thickness(XMargin, YMargin, 0, 0);
            r.Fill = Brushes.LightGray;

            return r;
        }

        private UIElement TrojkatRys(int XMargin, int YMargin)
        {
            Polygon r = new Polygon();

            r.Points.Add(new Point(canvas.Width / 2, YMargin));
            r.Points.Add(new Point(canvas.Width - xMargin, canvas.Height - yMargin));
            r.Points.Add(new Point(xMargin, canvas.Height - yMargin));

            r.Fill = Brushes.LightGray;

            return r;
        }

        private Siec.Punkt Srodek()
        {
            return new Siec.Punkt(canvas.Width / 2, canvas.Height / 2);
        }

        private int WielkoscZbioruTreningowego(int xEl, int yEl)
        {
            return xEl * yEl * 10 < 1000 ? 1000 : xEl * yEl * 10;
        }

        private UIElement PunktTreningowyRys(double x, double y)
        {
            Ellipse r = new Ellipse();
            r.Height = 4;
            r.Width = 4;
            r.Margin = new Thickness(x - 2, y - 2, 0, 0);
            r.Fill = Brushes.Gray;

            return r;
        }

        private void WprowadzZmianyWSieci(bool ModZbTrening = false, bool ModPoczSilyPrzes = false, bool ModPoczPromZmian = false, bool ModSpadkuProm = false, bool ModSpadkuSilyPrzes = false, bool Przebudowa = false)
        {
            // Czy przebudować całą sieć od nowa?
            if (Przebudowa)
            {
                if (s != null && canvas != null)
                    s.Wymaz(canvas);
                
                s = new Siec(xEl, yEl, canvas, Srodek(), deltaPozPocz, zbTreningowyPunkty, silaPrzesuniecia, promienZmian, spadekSilyPrzesuniecia, spadekPromienia);

                OdswiezWskazniki();
                return;
            }

            // Przygotowanie zmian
            if (ModZbTrening)
            {
                if (rysZbTrenCheckbox.IsChecked == true) // Rysunek zbioru
                    RysZbTrenCheckbox_Unchecked(null, null);

                switch (typZbioru)
                {
                    case ZbiorTreningowyListItems.prostokat:
                        zbTreningowyRys = ProstokatRys(xMargin, yMargin);
                        zbTreningowyPunkty = ZbiorTreningowy.Prostokat(canvas.Width, canvas.Height, xMargin, yMargin, WielkoscZbioruTreningowego(xEl, yEl));
                        break;
                    case ZbiorTreningowyListItems.romb:
                        zbTreningowyRys = RombRys(xMargin, yMargin);
                        zbTreningowyPunkty = ZbiorTreningowy.Romb(canvas.Width, canvas.Height, xMargin, yMargin, WielkoscZbioruTreningowego(xEl, yEl));
                        break;
                    case ZbiorTreningowyListItems.elipsa:
                        zbTreningowyRys = ElipsaRys(xMargin, yMargin);
                        zbTreningowyPunkty = ZbiorTreningowy.Elipsa(canvas.Width, canvas.Height, xMargin, yMargin, WielkoscZbioruTreningowego(xEl, yEl));
                        break;
                    case ZbiorTreningowyListItems.trojkat:
                        zbTreningowyRys = TrojkatRys(xMargin, yMargin);
                        zbTreningowyPunkty = ZbiorTreningowy.Trojkat(canvas.Width, canvas.Height, xMargin, yMargin, WielkoscZbioruTreningowego(xEl, yEl));
                        break;
                    default:
                        break;
                }
            }

            // Wstrzymanie symulacji jeśli trwa
            bool poprzedniStanSymulacji = symulacjaTrwa;
            if (symulacjaTrwa)
                ToggleSimulation(null, null);

            // Wprowadzenie zmian
            if (ModZbTrening)
            {
                s.ZbiorTreningowy = zbTreningowyPunkty;

                if (rysZbTrenCheckbox.IsChecked == true) // Rysunek zbioru
                    RysZbTrenCheckbox_Checked(null, null);
            }
            
            if (ModPoczSilyPrzes)
                s.PoczatkowaSilaPrzesuniecia = silaPrzesuniecia;

            if (ModPoczPromZmian)
                s.PoczatkowyPromienZmian = promienZmian;

            if (ModSpadkuProm)
                s.SpadekPromienia = spadekPromienia;

            if (ModSpadkuSilyPrzes)
                s.SpadekSilyPrzesuniecia = spadekSilyPrzesuniecia;

            // Wznowienie symulacji jeśli wcześniej trwała
            if (poprzedniStanSymulacji == true)
                ToggleSimulation(null, null);
            
            OdswiezWskazniki();
        }
    }
}
