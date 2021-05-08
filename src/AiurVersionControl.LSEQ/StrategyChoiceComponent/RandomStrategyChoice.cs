using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using AiurVersionControl.LSEQ.BaseComponents;
using AiurVersionControl.LSEQ.Data;
using AiurVersionControl.LSEQ.LogootEngine;
using AiurVersionControl.LSEQ.StrategiesComponents;

namespace AiurVersionControl.LSEQ.StrategyChoiceComponent
{
    public class RandomStrategyChoice : IStrategyChoice
    {
        private readonly Dictionary<Positions, FakeListNode> _spectrum = new();

        private int _date = 0;

        private BitArray _strategies;

        private static readonly Random _r = new Random();
        
        IBase _base;

        private IIdProviderStrategy _strategy1;
        private IIdProviderStrategy _strategy2;

        public RandomStrategyChoice(IBase b, IIdProviderStrategy strategy1, IIdProviderStrategy strategy2)
        {
            _base = b;
            _strategy1 = strategy1;
            _strategy2 = strategy2;
            _strategies = new BitArray(0);
        }
        
        public IEnumerable<Positions> GenerateIdentifiers(Positions p, Positions q, int n, Replica rep)
        {
            // #1 count interval between p and q, until itz enough
            BigInteger interval = BigInteger.Zero;
            int index = 0;
            while(new BigInteger(n) > interval)
            {
                // #1 a: obtain index value
                ++index;
                // #1 b: obtain interval value
                interval = _base.Interval(p.D, q.D, index);
            }

            // #2 if not already setted value in strategies
            // random a full 64 bits of strategies, bitsize.size limitation
            if (index >= _strategies.Count)
            {
                int sizeBefore = _strategies.Count;
                _strategies.Set(_strategies.Count);
            }
        }

        public void Add(Positions prev, Positions id, Positions next)
        {
            if (!_spectrum.ContainsKey(prev))
            {
                var prevfln = new FakeListNode(null,_date,id);
                _spectrum.Add(prev, prevfln);
            }
            else
            {
                _spectrum[prev].Next = id;
            }

            if (!_spectrum.ContainsKey(prev))
            {
                var prevfln = new FakeListNode(id, _date, null);
                _spectrum.Add(next, prevfln);
            }
            else
            {
                _spectrum[next].Prev = id;
            }

            var fln = new FakeListNode(prev, _date, next);

            _spectrum.Add(id, fln);
        }

        public void Del(Positions id)
        {
            var fln = _spectrum[id];
            if (fln.Prev != null)
            {
                _spectrum[fln.Prev].Next = fln.Next;
            }
            if (fln.Next != null)
            {
                _spectrum[fln.Next].Prev = fln.Prev;
            }
        }

        public void IncDate()
        {
            ++_date;
        }

        public Dictionary<Positions, FakeListNode> GetSpectrum()
        {
            return _spectrum;
        }
    }
}