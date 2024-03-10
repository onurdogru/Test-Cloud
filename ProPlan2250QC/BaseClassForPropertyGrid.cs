using System.ComponentModel;
using System.Reflection;

namespace ProPlan2250QC
{
    public class BaseClassForPropertyGrid : INotifyPropertyChanged
    {
        #region Private Methods

        protected void InitializeWithDefaultValues()
        {
            // Tüm property'leri [DefaultValue(XXX)] şeklindeki attribute'lerdeki değerlerde initialize eder: 
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this))
            {
                DefaultValueAttribute myAttribute = (DefaultValueAttribute)property.Attributes[typeof(DefaultValueAttribute)];

                if (myAttribute != null)
                {
                    property.SetValue(this, myAttribute.Value);
                }
            }
        }
        #endregion

        #region Public Methods

        public void SetBrowsableProperty(string strPropertyName, bool bIsBrowsable)
        {
            // Get the Descriptor's Properties
            PropertyDescriptor theDescriptor = TypeDescriptor.GetProperties(this.GetType())[strPropertyName];

            // Get the Descriptor's "Browsable" Attribute
            BrowsableAttribute theDescriptorBrowsableAttribute = (BrowsableAttribute)theDescriptor.Attributes[typeof(BrowsableAttribute)];
            FieldInfo isBrowsable = theDescriptorBrowsableAttribute.GetType().GetField("browsable", BindingFlags.NonPublic | BindingFlags.Instance);

            // Set the Descriptor's "Browsable" Attribute
            isBrowsable.SetValue(theDescriptorBrowsableAttribute, bIsBrowsable);
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

}
