using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Windows.Forms;
using System.Xml;

/// <summary>
/// PortableSettingsProvider class ensures that application settings can be saved to the application folder itself
/// instead of the user profile. Used in the configuration of Application Settings.
/// Code of Portable Settings Provider from
/// [url]http://www.codeproject.com/KB/vb/CustomSettingsProvider.aspx[/url]  (C# version by gpgemini) 
/// </summary>


public class PortableSettingsProvider : SettingsProvider
{
    private const string SETTINGSROOT = "Settings";
    //XML Root Node
    
    public override void Initialize(string name, NameValueCollection col)
    {
        base.Initialize(this.ApplicationName, col);
    }
    
    public override string ApplicationName
    {
        get
        {
            if (Application.ProductName.Trim().Length > 0)
            {
                return Application.ProductName;
            }
            else
            {
                FileInfo fi = new FileInfo(Application.ExecutablePath);
                return fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length);
            }
        }
        set { }
        //Do nothing
    }
    
    public override string Name
    {
        get { return "PortableSettingsProvider"; }
    }
    
    public virtual string GetAppSettingsPath()
    {
        //Used to determine where to store the settings
        System.IO.FileInfo fi = new System.IO.FileInfo(Application.ExecutablePath);
        string directory = String.Format(@"{0}\Data", fi.DirectoryName);
        return directory;
    }
    
    public virtual string GetAppSettingsFilename()
    {
        //Used to determine the filename to store the settings
        return "UserSettings.settings";
    }
    
    public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propvals)
    {
        //Iterate through the settings to be stored
        //Only dirty settings are included in propvals, and only ones relevant to this provider
        foreach (SettingsPropertyValue propval in propvals)
        {
            SetValue(propval);
        }
        
        try
        {
            SettingsXML.Save(Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
        }
        catch (Exception)
        {
        }
        //Ignore if cant save, device been ejected
    }

    public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection props)
    {
        //Create new collection of values
        SettingsPropertyValueCollection values = new SettingsPropertyValueCollection();
        
        //Iterate through the settings to be retrieved
        foreach (SettingsProperty setting in props)
        {
            SettingsPropertyValue value = new SettingsPropertyValue(setting);
            value.IsDirty = false;
            value.SerializedValue = GetValue(setting);
            values.Add(value);
        }
        return values;
    }
    
    private XmlDocument _settingsXML = null;
    
    private XmlDocument SettingsXML
    {
        get
        {
            //If we dont hold an xml document, try opening one.  
            //If it doesnt exist then create a new one ready.
            if (_settingsXML == null)
            {
                _settingsXML = new XmlDocument();
                
                try
                {
                    _settingsXML.Load(Path.Combine(GetAppSettingsPath(), GetAppSettingsFilename()));
                }
                catch (Exception)
                {
                    //Create new document
                    XmlDeclaration dec = _settingsXML.CreateXmlDeclaration("1.0", "utf-8", string.Empty);
                    _settingsXML.AppendChild(dec);
                    
                    XmlNode nodeRoot = default(XmlNode);
                    
                    nodeRoot = _settingsXML.CreateNode(XmlNodeType.Element, SETTINGSROOT, "");
                    _settingsXML.AppendChild(nodeRoot);
                }
            }
            return _settingsXML;
        }
    }
    
    private string GetValue(SettingsProperty setting)
    {
        string ret = "";

        try
        {
            if (IsRoaming(setting))
            {
                ret = SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + setting.Name).InnerText;
            }
            else
            {
                ret =
                    SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + Environment.MachineName + "/" + setting.Name).
                        InnerText;
            }
        }
        catch (Exception)
        {
            if ((setting.DefaultValue != null))
            {
                ret = setting.DefaultValue.ToString();
            }
            else
            {
                ret = "";
            }
        }
        return ret;
    }


    private void SetValue(SettingsPropertyValue propVal)
    {
        XmlElement MachineNode = default(XmlElement);
        XmlElement SettingNode = default(XmlElement);
        
        //Determine if the setting is roaming.
        //If roaming then the value is stored as an element under the root
        //Otherwise it is stored under a machine name node 
        try
        {
            if (IsRoaming(propVal.Property))
            {
                SettingNode = (XmlElement)SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + propVal.Name);
            }
            else
            {
                SettingNode =
                    (XmlElement)
                    SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + Environment.MachineName + "/" + propVal.Name);
            }
        }
        catch (Exception)
        {
            SettingNode = null;
        }


        //Check to see if the node exists, if so then set its new value
        if ((SettingNode != null))
        {
            SettingNode.InnerText = Convert.ToString(propVal.SerializedValue);
        }
        else
        {
            if (IsRoaming(propVal.Property))
            {
                //Store the value as an element of the Settings Root Node
                SettingNode = SettingsXML.CreateElement(propVal.Name);
                SettingNode.InnerText = propVal.SerializedValue.ToString();
                SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(SettingNode);
            }
            else
            {
                //Its machine specific, store as an element of the machine name node,
                //creating a new machine name node if one doesnt exist.
                try
                {
                    MachineNode =
                        (XmlElement)SettingsXML.SelectSingleNode(SETTINGSROOT + "/" + Environment.MachineName);
                }
                catch (Exception)
                {
                    MachineNode = SettingsXML.CreateElement(Environment.MachineName);
                    SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(MachineNode);
                }

                if (MachineNode == null)
                {
                    MachineNode = SettingsXML.CreateElement(Environment.MachineName);
                    SettingsXML.SelectSingleNode(SETTINGSROOT).AppendChild(MachineNode);
                }
                
                SettingNode = SettingsXML.CreateElement(propVal.Name);
                SettingNode.InnerText = Convert.ToString(propVal.SerializedValue);
                MachineNode.AppendChild(SettingNode);
            }
        }
    }


    private bool IsRoaming(SettingsProperty prop)
    {
        //Determine if the setting is marked as Roaming
        foreach (DictionaryEntry d in prop.Attributes)
        {
            Attribute a = (Attribute)d.Value;
            if (a is System.Configuration.SettingsManageabilityAttribute)
            {
                return true;
            }
        }
        return false;
    }
}