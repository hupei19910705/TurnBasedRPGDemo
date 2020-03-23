using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class MyCoroutine
    {
        public static IEnumerator Sleep(float duration)
        {
            while(duration > 0f)
            {
                duration -= Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator _element;
        private Stack<IEnumerator> _stack = new Stack<IEnumerator>();
        public bool Finished { get { return _element == null && _stack.Count == 0; } }

        public MyCoroutine(IEnumerator enumerator)
        {
            _element = enumerator;
        }

        public void Execute()
        {
            if (Finished)
                return;
            _Execute();
        }

        private void _Execute()
        {
            if (_element == null)
                return;
            if(!_element.MoveNext())
            {
                if (_stack.Count == 0)
                {
                    _element = null;
                    return;
                }
                else
                    _element = _stack.Pop();
            }
            else
            {
                var current = _element.Current as IEnumerator;
                if (current != null)
                {
                    _stack.Push(_element);
                    _element = current;
                    _Execute();
                }
            }
        }
    }

    public class ParallelCoroutines
    {
        private List<MyCoroutine> coroutines = new List<MyCoroutine>();
        private bool _running = false;
        public bool Finished { get { return coroutines == null || coroutines.Count == 0; } }
        public bool Running { get { return _running; } }

        public IEnumerator Execute()
        {
            if (_running)
            {
                Debug.LogError("can not execute while parallel coroutines is already running");
                yield break;
            }

            _running = true;
            while (_running)
            {
                if (Finished)
                    yield return null;
                else
                {
                    for (int i = coroutines.Count - 1; i >= 0; i--)
                        coroutines[i].Execute();
                    coroutines.RemoveAll(c => c.Finished);
                    yield return null;
                }
            }
        }

        public void Add(IEnumerator element)
        {
            if (coroutines == null)
                coroutines = new List<MyCoroutine>();
            coroutines.Add(new MyCoroutine(element));
        }

        public void Stop()
        {
            _running = false;
        }

        public void Clear()
        {
            coroutines.Clear();
        }
    }

    public class SerialCoroutines
    {
        private Queue<IEnumerator> coroutines = new Queue<IEnumerator>();
        private bool _running = false;
        public bool Finished { get { return coroutines == null || coroutines.Count == 0; } }
        public bool Empty { get { return coroutines != null && coroutines.Count == 0; } }

        public IEnumerator Execute()
        {
            if (_running)
            {
                Debug.LogError("can not execute while serial coroutines is already running");
                yield break;
            }

            _running = true;
            while (_running)
            {
                if (Finished)
                    yield return null;
                else
                {
                    var cor = coroutines.Dequeue();
                    yield return cor;
                }
            }
        }

        public void Add(IEnumerator element)
        {
            if (coroutines == null)
                coroutines = new Queue<IEnumerator>();
            coroutines.Enqueue(element);
        }

        public void Stop()
        {
            _running = false;
            coroutines.Clear();
        }
    }
}