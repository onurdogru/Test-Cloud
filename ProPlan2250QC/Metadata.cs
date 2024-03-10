using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace ProPlan2250QC
{
    [CategoryOrder("Ürün Bilgileri", 0)]
    [DisplayName("ÜRÜN VERİLERİ")]
    public class Metadata : BaseClassForPropertyGrid
    {
        #region Constructor

        public Metadata()
        {
            InitializeWithDefaultValues();
            UpdateDateTime();
        }
        #endregion

        #region Private Members

        #endregion

        #region Public Methods

        public void UpdateDateTime()
        {
            DateTimeUnformatted = System.DateTime.Now;
        }

        public void ResetAll(PropertyGrid propertyGridToBeReset)
        {
            foreach (PropertyItem pi in propertyGridToBeReset.Properties)
            {
                pi.PropertyDescriptor.ResetValue(propertyGridToBeReset.SelectedObject);
            }

            // Resetlenecek diğer şeyler (Metadata sınıfına özel)
            UpdateDateTime();

            propertyGridToBeReset.Update();
        }

        #endregion

        #region Public Properties Of PROPERTYVIEW

        [Category("Ürün Bilgileri")]
        [PropertyOrder(0)]
        [DisplayName("Ürün Seri No")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string Operator { get; set; }

        private DateTime _dateTimeUnformatted;
        [Browsable(false)]
        public DateTime DateTimeUnformatted
        {
            get { return _dateTimeUnformatted; }
            set
            {
                _dateTimeUnformatted = value;
                OnPropertyChanged(nameof(DateTime));
            }
        }

        private string _date;
        [Browsable(false)]
        public string Date
        {
            get { return DateTimeUnformatted.ToString("yyyy-MM-dd"); }
        }

        private string _time;
        [Browsable(false)]
        public string Time
        {
            get { return DateTimeUnformatted.ToString("HH:mm:ss"); }
        }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(1)]
        [DisplayName("Tarih / saat")]
        [Description("")]
        [Browsable(true)]
        public string DateTime
        {
            get
            {
                return Date + " / " + Time;
            }
        }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(2)]
        [DisplayName("Model")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string Model { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(3)]
        [DisplayName("Seri numarası")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string SerialNumber { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(4)]
        [DisplayName("Yükleme şartları")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string LoadingConditions { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(5)]
        [DisplayName("Montaj tipi")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string Mounting { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(6)]
        [DisplayName("Alpplas ürün bilgisi 1")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string AlpplasMetadata1 { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(7)]
        [DisplayName("Alpplas ürün bilgisi 2")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string AlpplasMetadata2 { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(8)]
        [DisplayName("Alpplas ürün bilgisi 3")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string AlpplasMetadata3 { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(9)]
        [DisplayName("Güç kaynağı")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string PowerSource { get; set; }

        [Category("Ürün Bilgileri")]
        [PropertyOrder(10)]
        [DisplayName("Aksam ve aksesuarlar")]
        [Description("")]
        [Browsable(true)]
        [DefaultValue("")]
        public string AttachmentsAndAccessories { get; set; }


        #endregion
    }
}
