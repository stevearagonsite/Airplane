using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Memento
{
    public class Caretaker<T>
    {
        private List<T> _savedArticles = new List<T>();
        private int _maxArticles = 10;
        /// <summary>
        /// Initials properties for CareTaker
        /// </summary>
        /// <param name="maxArticles">How many articles to save ?</param>
        public Caretaker(int maxArticles = 10)
        {
            _maxArticles = maxArticles;
        }

        public void Add(T m)
        {
            if (Count >= _maxArticles)
            {
                _savedArticles.RemoveAt(0);
                _savedArticles.Add(m);
            }
            else
            {
                _savedArticles.Add(m);
            }
        }

        public T Get(int i)
        {
            return _savedArticles[i];
        }

        public int Count
        {
            get { return _savedArticles.Count; }
        }
    }

    public class Memento<T>
    {
        public T article { get; protected set; }

        public Memento(T article)
        {
            this.article = article;
        }
    }

    public class Originator<T>
    {
        public T article { get; protected set; }

        public void Set(T article)
        {
            this.article = article;
        }

        public Memento<T> StoreInMemento()
        {
            return new Memento<T>(article);
        }

        public T RestoreFromMemento(Memento<T> memento)
        {
            article = memento.article;
            return article;
        }
    }
}