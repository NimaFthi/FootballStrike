using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class WSResponse : Jsonable
{
    public int requestId;
    public string model;
    public string method;
    public int code;
    public string error;
    public object _data;
    public bool IsSuccess()
    {
        return code >= 200 && code < 300;
    }
    public bool IsFailed()
    {
        return !IsSuccess();
    }
    public string GetDataString()
    {
        return JsonConvert.SerializeObject(_data);
    }
    public T GetData<T>()
    {
        return Jsonable.FromJSON<T>(JsonConvert.SerializeObject(_data));
    }
}
