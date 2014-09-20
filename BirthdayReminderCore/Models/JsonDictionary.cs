using System;
using System.Collections.Generic;

namespace BirthdayReminder.Models
{
    public class JsonDictionary : Dictionary<String,String>
    {
        public JsonDictionary(Object jsonObj)
        {
            String formattedJson = jsonObj.ToString().Replace("{", "").Replace("}", "").Replace("'","").Replace(@"""","");
            formattedJson = formattedJson.Replace("data:", "").Replace("[", "").Replace("]", "");
            String []jsonArray = formattedJson.Split(new char[] { ',' });
            foreach (String keyValueString in jsonArray)
            {
                int separatorIndex = keyValueString.IndexOf(':');
                this.Add(keyValueString.Substring(0,separatorIndex), keyValueString.Substring(separatorIndex+1));
            }
        }
    }
}
