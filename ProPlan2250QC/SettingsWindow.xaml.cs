using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using System.Windows.Markup;
using System.Printing;
using System.Drawing.Printing;

namespace ProPlan2250QC
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        //private const int GWL_STYLE = -16;
        //private const int WS_SYSMENU = 0x80000;

        //[DllImport("user32.dll", SetLastError = true)]
        //private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        //[DllImport("user32.dll")]

        //private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        Settings settings;

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            this.settings = settings;
            this.DataContext = this.settings;
        }


        private void SavePresetsAsXml()
        {
            XmlDocument projectXml = new XmlDocument();

            if (!File.Exists("Presets.xml"))
            {
                projectXml.LoadXml("<presets />");
                projectXml.Save("Presets.xml");
            }

            string id = settings.SelectedPreset;

            projectXml.Load("Presets.xml");

            projectXml = XmlLibrary.ReplacePreset(projectXml, id, settings);

            projectXml.Save("Presets.xml");
        }

        private void SaveSettingsAsXml()
        {
            XmlDocument settingsXml = new XmlDocument();

            if (!File.Exists("Settings.xml"))
            {
                settingsXml.LoadXml("<application />");
                settingsXml.Save("Settings.xml");
            }
            settingsXml.Load("Settings.xml");

            settingsXml = XmlLibrary.WriteSettings(settingsXml, settings);

            settingsXml.Save("Settings.xml");
        }

        private void btnSaveAndClose_Click(object sender, RoutedEventArgs e)
        {
            SavePresetsAsXml();
            SaveSettingsAsXml();
            this.DialogResult = true;

            ayarlar.Default.level100max = txt1.Text;
            ayarlar.Default.rpm100max = txt2.Text;
            ayarlar.Default.rpm100min = txt3.Text;
            ayarlar.Default.rpm50max = txt4.Text;
            ayarlar.Default.rpm50min = txt5.Text;
            ayarlar.Default.level50max = txt6.Text;
            ayarlar.Default.watt100max = txt7.Text;
            ayarlar.Default.watt100min = txt8.Text;
            ayarlar.Default.watt50max = txt9.Text;
            ayarlar.Default.watt50min = txt10.Text;
            ayarlar.Default.comPort = txt11.Text;
            ayarlar.Default.Save();
        }

        private void btnNewEntry_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new NewEntry();
            if (dialog.ShowDialog() == true)
            {
                if (!string.IsNullOrEmpty(dialog.ResponseText))
                {
                    XmlDocument projectXml = new XmlDocument();

                    if (!File.Exists("Presets.xml"))
                    {
                        projectXml.LoadXml("<presets />");
                        projectXml.Save("Presets.xml");
                    }

                    projectXml.Load("Presets.xml");

                    projectXml = XmlLibrary.AddNewPreset(projectXml, dialog.ResponseText, null);

                    projectXml.Save("Presets.xml");
                }

                settings.LoadPresets();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            
        }

        private void btnDeleteEntry_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Seçili kriter silinecek!", "Uyarı", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                var selected = cmbPresets.SelectedItem.ToString();

                XmlDocument projectXml = new XmlDocument();
                projectXml.Load("Presets.xml");

                projectXml = XmlLibrary.DeletePreset(projectXml, selected);

                projectXml.Save("Presets.xml");

                settings.LoadPresets();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            txt1.Text = ayarlar.Default.level100max;
            txt2.Text = ayarlar.Default.rpm100max;
            txt3.Text = ayarlar.Default.rpm100min;
            txt4.Text = ayarlar.Default.rpm50max;
            txt5.Text = ayarlar.Default.rpm50min;
            txt6.Text = ayarlar.Default.level50max;
            txt7.Text = ayarlar.Default.watt100max;
            txt8.Text = ayarlar.Default.watt100min;
            txt9.Text = ayarlar.Default.watt50max;
            txt10.Text = ayarlar.Default.watt50min;
            txt11.Text = ayarlar.Default.comPort;
        }
    }
}
