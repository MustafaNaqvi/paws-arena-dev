using System.Collections.Generic;

namespace BoomDaoWrapper
{
    public class WorldDataEntry
    {
        public string PrincipalId;
        public Dictionary<string, string> Data = new ();

        public string GetProperty(string _key)
        {
            return Data.ContainsKey(_key) ? Data[_key] : default;
        }
    }
}