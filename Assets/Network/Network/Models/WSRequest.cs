using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class WSRequest : Jsonable
{
    public int requestId;
    public string model;
    public string method;
    public Dictionary<string, object> parameters;
    public WSRequest() { }

    public WSRequest(string model, string method, Dictionary<string, object> parameters)
    {
        this.model = model;
        this.method = method;
        this.parameters = parameters;
    }
    public static WSRequest Simple(string model, string method)
    {
        return new WSRequest(model, method, null);
    }
}
