public interface IObservableEventWinner {
    void SubscribeEventWinner(IObserverEventWinner observer);
    void UnSubscribeEventWinner(IObserverEventWinner observer);
}