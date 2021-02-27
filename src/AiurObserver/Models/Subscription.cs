﻿using System;
using System.Collections.Generic;

namespace AiurObserver.Models
{
    public class Subscription<T> : IDisposable
    {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        internal Subscription(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observers.Contains(_observer))
            {
                _observers.Remove(_observer);
            }
        }
    }
}
