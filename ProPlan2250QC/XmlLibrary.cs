using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ProPlan2250QC
{
    public static class XmlLibrary
    {
        static public XmlNode MakeXPath(XmlDocument doc, string xpath, bool forceNew)
        {
            return MakeXPath(doc, doc as XmlNode, xpath, forceNew);
        }

        static public XmlNode MakeXPath(XmlDocument doc, XmlNode parent, string xpath, bool forceNew)
        {
            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xpath.Trim('/').Split('/');
            string nextNodeInXPath = partsOfXPath.First();
            if (string.IsNullOrEmpty(nextNodeInXPath))
                return parent;

            // get or create the node from the name
            XmlNode node = parent.SelectSingleNode(nextNodeInXPath);
            if ((node == null) || (forceNew && partsOfXPath.Count() <= 1))
                node = parent.AppendChild(doc.CreateElement(nextNodeInXPath));

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = String.Join("/", partsOfXPath.Skip(1).ToArray());
            return MakeXPath(doc, node, rest, forceNew);
        }

        static public XmlDocument AddNewPreset(XmlDocument xmlDocument, string id, Settings settings)
        {
            XmlElement contentElement = (XmlElement)XmlLibrary.MakeXPath(xmlDocument, "/presets/preset", true);
            contentElement.SetAttribute("id", id);
            contentElement.SetAttribute(nameof(settings.SidebandWidth), settings == null ? "0" : settings.SidebandWidth.ToString());
            contentElement.SetAttribute(nameof(settings.DetectLow), settings == null ? "0" : settings.DetectLow.ToString());
            contentElement.SetAttribute(nameof(settings.DetectHigh), settings == null ? "0" : settings.DetectHigh.ToString());
            contentElement.SetAttribute(nameof(settings.ToleranceMinX), settings == null ? "0" : settings.ToleranceMinX.ToString());
            contentElement.SetAttribute(nameof(settings.ToleranceMaxX), settings == null ? "0" : settings.ToleranceMaxX.ToString());
            contentElement.SetAttribute(nameof(settings.ToleranceMinY), settings == null ? "0" : settings.ToleranceMinY.ToString());
            contentElement.SetAttribute(nameof(settings.ToleranceMaxY), settings == null ? "0" : settings.ToleranceMaxY.ToString());

            return xmlDocument;
        }


        static public XmlDocument ReplacePreset(XmlDocument xmlDocument, string id, Settings settings)
        {
            var allPresets = xmlDocument.SelectNodes("presets/preset").Cast<XmlElement>();

            if (allPresets != null && allPresets.Count() > 0)
            {
                foreach (var item in allPresets)
                {
                    item.SetAttribute("isSelected", "false");
                } 
            }

            // get or create the node from the name
            XmlElement contentElement = (XmlElement)xmlDocument.SelectNodes("presets/preset[@id='" + id + "']")?.Item(0);

            if (contentElement != null)
            {
                contentElement.SetAttribute("isSelected", "true");
                contentElement.SetAttribute(nameof(settings.SidebandWidth), settings.SidebandWidth.ToString());
                contentElement.SetAttribute(nameof(settings.DetectLow), settings.DetectLow.ToString());
                contentElement.SetAttribute(nameof(settings.DetectHigh), settings.DetectHigh.ToString());
                contentElement.SetAttribute(nameof(settings.ToleranceMinX), settings.ToleranceMinX.ToString());
                contentElement.SetAttribute(nameof(settings.ToleranceMaxX), settings.ToleranceMaxX.ToString());
                contentElement.SetAttribute(nameof(settings.ToleranceMinY), settings.ToleranceMinY.ToString());
                contentElement.SetAttribute(nameof(settings.ToleranceMaxY), settings.ToleranceMaxY.ToString());
            }

            return xmlDocument;
        }

        static public XmlDocument DeletePreset(XmlDocument xmlDocument, string id)
        {
            var presetsNode = xmlDocument.SelectNodes("presets").Item(0);

            // get or create the node from the name
            var contentElement = xmlDocument.SelectNodes("presets/preset[@id='" + id + "']")?.Item(0);

            presetsNode.RemoveChild(contentElement);

            return xmlDocument;
        }

        static public XmlDocument WriteSettings(XmlDocument xmlDocument, Settings settings)
        {
            // get or create the node from the name
            XmlElement contentElement = (XmlElement)xmlDocument.SelectNodes("application/settings")?.Item(0);

            if (contentElement == null)
            {
                contentElement = (XmlElement)XmlLibrary.MakeXPath(xmlDocument, "/application/settings", true); 
            }

            contentElement.SetAttribute(nameof(settings.DeviceIp), settings == null ? "" : settings.DeviceIp.ToString());
            contentElement.SetAttribute(nameof(settings.MeasurementDuration), settings == null ? "" : settings.MeasurementDuration.ToString());
            contentElement.SetAttribute(nameof(settings.PreDelayDuration), settings == null ? "" : settings.PreDelayDuration.ToString());
            contentElement.SetAttribute(nameof(settings.HighPassFrequency), settings == null ? "" : settings.HighPassFrequency.ToString());
            contentElement.SetAttribute(nameof(settings.IsAutoSaveOn), settings == null ? "" : settings.IsAutoSaveOn.ToString());
            contentElement.SetAttribute(nameof(settings.SelectedPrinter), settings == null ? "" : settings.SelectedPrinter.ToString());
            contentElement.SetAttribute(nameof(settings.DisplayType), settings == null ? "" : settings.DisplayType.ToString());

            return xmlDocument;
        }


    }
}
