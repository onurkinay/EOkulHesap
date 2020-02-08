using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HtmlAgilityPack;
using Windows.UI.Popups;
using System.Text.RegularExpressions;
using Windows.Storage;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace EOkulHesap
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<string> dersDizisi = new List<string>();

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try {
                StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile dersKredileri = await temporaryFolder.GetFileAsync("ders.txt");
                

            }
            catch {

                await (new MessageDialog("İlk açılışa hoşgeldiniz. E-Okul'a giriş yaparak ÖNCE ders kredilerini girmeniz gerekmektedir.")).ShowAsync();
            }
        }

        private void donem_Click(object sender, RoutedEventArgs e)
        {
            if (one.Visibility == Visibility.Visible)
            {

                two.Visibility = Visibility.Visible;
                one.Visibility = Visibility.Collapsed;

            }
            else if (two.Visibility == Visibility.Visible)
            {
                one.Visibility = Visibility.Visible;
                two.Visibility = Visibility.Collapsed;

            }

        }

        private async void hesapla_Click(object sender, RoutedEventArgs e)
        {
            try {
                var doc = new HtmlDocument();
                doc.LoadHtml(await web.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" }));

                StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;
                StorageFile dersKredileri = await temporaryFolder.GetFileAsync("ders.txt");
                String krediler = await FileIO.ReadTextAsync(dersKredileri);



                int[] katSayilar = krediler.Split(',').Select(n => Convert.ToInt32(n)).ToArray();

                var node = doc.DocumentNode.Element("html");

                string donemKodu = "";

                

                if (one.Visibility == Visibility.Visible)
                {
                    donemKodu = "tblNotlarIDonem";
                }

                else if (two.Visibility == Visibility.Visible) { donemKodu = "tblNotlarIIDonem"; }

                var donemler = node.ChildNodes["body"].Descendants("table").Where(o => o.GetAttributeValue("id", "").IndexOf(donemKodu) != -1).First();

                var inputs = donemler.Descendants("tr").Where(o => o.GetAttributeValue("bgcolor", "")
                                       == "#ffffff" || o.GetAttributeValue("bgcolor", "") == "#f5f5f5").ToList();

            
                int toplam = 0;
                for (int b = 0; b < katSayilar.Length; b++)
                {
                    toplam += katSayilar[b];
                }

                int i = 0; double sonuc = 0; string NotString; double Not;

                foreach (var item in inputs)
                {
                    if (katSayilar.Length > i)
                    {
                        var ders = item.Descendants("td");
                        if (ders.First().InnerText != "")
                        {
                            NotString = ders.Where(x => x.GetAttributeValue("class", "") == "header4").First().InnerText;
                            Not = Convert.ToDouble(NotString);
                            sonuc += (katSayilar[i] * Not);

                        }
                        i++;

                    }
                }
                

                await (new MessageDialog("Ortalamanız: " + Math.Round((sonuc / toplam),2).ToString())).ShowAsync();
                // 
            }catch { await (new MessageDialog("Derslerin kredilerini girdiğinizden emin olunuz")).ShowAsync(); }
        }

        private void dersGir_Click(object sender, RoutedEventArgs e)
        {
            
            Frame.Navigate(typeof(DersGir), dersDizisi.ToArray());
        }

        private async void web_LoadCompleted(object sender, NavigationEventArgs e)
        {

        }

        private async void web_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
          StorageFolder temporaryFolder = ApplicationData.Current.TemporaryFolder;

            if (args.Uri.AbsoluteUri.IndexOf("IOV02002") != -1)
            {
                dersDizisi = new List<string>();
                cmbar.Visibility = Visibility.Visible;


                var doc = new HtmlDocument();
                doc.LoadHtml(await web.InvokeScriptAsync("eval", new string[] { "document.documentElement.outerHTML;" }));

                var node = doc.DocumentNode.Element("html");

                var donemler = node.ChildNodes["body"].Descendants("table").Where(o => o.GetAttributeValue("id", "").IndexOf("tblNotlarIDonem") != -1).First();

                var inputs = donemler.Descendants("tr").Where(o => o.GetAttributeValue("bgcolor", "")
                                       == "#ffffff" || o.GetAttributeValue("bgcolor", "") == "#f5f5f5").ToList();


                int i = 0;
                foreach (var item in inputs)
                {
                    string kontrol = "";
                    var ders = item.Descendants("td");

                    if (!dersDizisi.Contains(ders.First().InnerText))
                    {
                        if (ders.First().InnerText != "")
                        {
                            string Ders = ders.First().InnerText;
                            dersDizisi.Add(Ders);
                        }

                    }
                    else break;
                    
                }

            
            }else { cmbar.Visibility = Visibility.Collapsed; }
        }
    }
}
