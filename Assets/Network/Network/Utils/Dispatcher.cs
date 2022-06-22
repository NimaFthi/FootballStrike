using System.Collections.Generic;


public class Dispatcher
{
    private List<System.Action> actions = new List<System.Action>();
    public Dispatcher()
    {

    }
    public void Add(System.Action a)
    {
        actions.Add(a);
    }
    public void Dispatch()
    {
        while (actions.Count != 0)
        {
            actions[0].Invoke();
            actions.RemoveAt(0);
        }
    }
}
