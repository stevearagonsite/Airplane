public interface IObservableEventDefeated {
    void SubscribeEventWinner(IObserverEventDefeated observer);
    void UnSubscribeEventWinner(IObserverEventDefeated observer);
}