namespace APlusOrFail
{
    public delegate void EventHandler<in TSender>(TSender sender);
    public delegate void EventHandler<in TSender, in TEventArgs>(TSender sender, TEventArgs eventArgs);
}
