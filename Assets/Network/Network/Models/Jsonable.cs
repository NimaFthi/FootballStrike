using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class Jsonable
{
    public virtual string ToJSON()
    {
        return JsonConvert.SerializeObject(this);
    }
    public virtual Dictionary<string, object> ToDictionary()
    {
        return FromJSON<Dictionary<string, object>>(this.ToJSON());
    }
    public static T FromJSON<T>(string js)
    {
        return JsonConvert.DeserializeObject<T>(js);
    }
    public T GetCopy<T>()
    {
        return FromJSON<T>(this.ToJSON());
    }
    public static string JsonEncode(object d){
        return JsonConvert.SerializeObject(d);
    }
}
