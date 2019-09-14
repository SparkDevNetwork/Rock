using Newtonsoft.Json;
using System;
using System.Resources;

/// <summary>
/// Simple utility class to deal with globalized and localized string array
/// Uses Json for simplicity cannot be a static class but you do not have to
/// instantiate it to get the value
/// Usage:
///   private static String[,] questionData
///       {
///            get {
///                if (m_questionData is null)
///                {
///                    m_questionData = StringArrayResource.GetResourceStringArray("questionData", DiscServiceStrings.ResourceManager);
///                }
///                return m_questionData;
///            }
//        }
/// </summary>
///

namespace Rock.Utility.Globalizaton
{
    class StringArrayResource
    {
        public String[,] StringArray;
        // name: name of the string array stored as json
        // manager: resource string manager. 
        public static String[,] GetResourceStringArray(String name, ResourceManager manager)
        {
            String resStr = manager.GetString(name);
            StringArrayResource qao = JsonConvert.DeserializeObject<StringArrayResource>(resStr);
            return qao.StringArray;
        }
    }
}

/*
 * Rather than manually creating the array in resx you can create a simple console app
 * to get this formatted correctly:
 * class StringArrayResource
    {
        public String[,] StringArray;
        // name: name of the string array stored as json
        // manager: resource string manager. 
        //public static String[,] GetResourceStringArray(String name, ResourceManager manager)
        //{
        //    String resStr = manager.GetString(name);
        //    StringArrayResource qao = JsonConvert.DeserializeObject<StringArrayResource>(resStr);
        //    return qao.StringArray;
        //}
    }
    class Program
    {
        static void Main(string[] args)
        {
            StringArrayResource qa = new StringArrayResource();

            qa.StringArray = new string[,] { { "value1", "value2" }, { "value3", "value4" } };

            string outstr = JsonConvert.SerializeObject(qa);

            Console.WriteLine(outstr);
        }
    }

     */
