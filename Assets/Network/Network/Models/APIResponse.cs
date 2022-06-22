using Newtonsoft.Json;

[System.Serializable]
public class APIResponse : Jsonable
{
    public int code;
    public string error;
    public object _data;
    public const int NETWORK_ERROR = -1;
    public static APIResponse Error(string error, int code = 500)
    {
        var response = new APIResponse();
        response.error = error;
        response.code = code;
        return response;
    }
    public bool IsNetworkError()
    {
        return code == NETWORK_ERROR;
    }
    public bool IsSuccess()
    {
        return code >= 200 && code < 300;
    }
    public bool IsFail()
    {
        return !IsSuccess();
    }
    public T GetData<T>()
    {
        return Jsonable.FromJSON<T>(JsonConvert.SerializeObject(_data));
    }
}