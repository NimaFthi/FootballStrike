class WSRequestWithCallback
{
    public WSRequest request;
    public System.Action<WSResponse> callback;
    public WSRequestWithCallback(WSRequest request, System.Action<WSResponse> callback)
    {
        this.request = request;
        this.callback = callback;
    }
}