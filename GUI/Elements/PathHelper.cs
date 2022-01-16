using GUI.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace GUI.Elements
{
    public static class PathHelper
    {
        static private string Folder()
        {
            // + URF, 21.09.2018, переместил базу данных в папку с приложением для сохранения при обновлении
            //return new FileInfo(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath).Directory.FullName;
            return String.Format(@"{0}\Data", AppDomain.CurrentDomain.BaseDirectory);
            // - URF, 21.09.2018
        }

        static public string DataBasePath()
        {
            return Folder() + "\\" + UserSettings.Default.DataBaseName;
        }

        static public string LogPath()
        {
            return Folder() + "\\" +  UserSettings.Default.LogFileName;
        }
    }
}
