public interface IObservableEventDead {
    void SubscribeEventDead(IObserverEventDead observer);
    void UnSubscribeEventDead(IObserverEventDead observer);
}