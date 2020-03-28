using System;
using System.Collections.Generic;
using System.Linq;

namespace CustomObjects
{
    class EventsAction
    {
        protected List<Action> _actions = new List<Action>();
        public List<Action> actions {
            get { return _actions; }
        }

        public EventsAction()
        {

        }

        public EventsAction(List<Action> initialsActions)
        {

        }

        public void Execute()
        {
            foreach (var action in _actions)
            {
                action();
            }
        }

        public void Add(Action action)
        {
            if (action == null) throw new ArgumentException("Action cannot be null.");
            _actions.Add(action);
        }

        public void Remove(Action action)
        {
            if (action == null) throw new ArgumentException("Action cannot be null.");

            _actions.Remove(action);
        }

        public void Clean()
        {
            _actions = new List<Action>();
        }

        public EventsAction Concat(List<Action> actionsToAdd)
        {
            actionsToAdd?.Concat(_actions);
            return this;
        }

        // Pending finish.
        public static EventsAction operator- (EventsAction e1, EventsAction e2)
        {
            var eventAction = new EventsAction();
            return eventAction;
        }
        

        public static EventsAction operator+ (EventsAction e1, EventsAction e2)
        {
            var eventAction = new EventsAction();
            return eventAction
                .Concat(e1?.actions)
                .Concat(e2?.actions);
        }
    }
}
