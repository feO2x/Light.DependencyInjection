using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    public struct AvlTreeEnumerator<TRegistration> : IEnumerator<ImmutableAvlNode<TRegistration>>
    {
        private Stack _stack;
        private readonly ImmutableAvlNode<TRegistration> _rootNode;
        private ImmutableAvlNode<TRegistration> _currentNode;
        private State _state;

        public AvlTreeEnumerator(ImmutableAvlNode<TRegistration> rootNode)
        {
            rootNode.MustNotBeNull(nameof(rootNode));

            _rootNode = rootNode;
            _stack = new Stack(rootNode.Height); // TODO: maybe we can reduce the capacity here to log(height)
            _currentNode = null;
            _state = State.AtStart;
        }


        public bool MoveNext()
        {
            if (_state == State.Completed)
                return false;

            if (_state == State.AtStart)
            {
                if (_rootNode.IsEmpty)
                {
                    _state = State.Completed;
                    return false;
                }
                _currentNode = _rootNode;
                _state = State.AccessingChildNode;
            }

            // ReSharper disable PossibleNullReferenceException
            while (true)
            {
                // Check if the current node is a child that was not returned before
                if (_state == State.AccessingChildNode)
                {
                    // Check if there is is another left child
                    if (_currentNode.LeftChild.IsEmpty == false)

                    {
                        // If yes, then push this node to the stack and start the next iteration
                        // with the left child as the current node
                        _stack.Push(_currentNode);
                        _currentNode = _currentNode.LeftChild;
                        continue;
                    }
                    // If not, then return this node
                    _state = State.ReturnedCurrentNode;
                    return true;
                }

                // Check if the current node was already returned last call
                if (_state == State.ReturnedCurrentNode)
                {
                    // Check if there is right child that has to be returned
                    if (_currentNode.RightChild.IsEmpty == false)
                    {
                        _currentNode = _currentNode.RightChild;
                        _state = State.AccessingChildNode;
                        continue;
                    }

                    // If not, then check if there is a parent left on the stack
                    if (_stack.Count > 0)
                    {
                        _currentNode = _stack.Pop();
                        return true;
                    }

                    // Else there is no node left to return
                    _state = State.Completed;
                    _currentNode = null;
                    return false;
                }
            }
            // ReSharper restore PossibleNullReferenceException
        }

        public void Reset()
        {
            _currentNode = null;
            _state = State.AtStart;
        }

        public ImmutableAvlNode<TRegistration> Current => _currentNode;

        object IEnumerator.Current => _currentNode;

        public void Dispose() { }

        private enum State
        {
            AtStart,
            AccessingChildNode,
            ReturnedCurrentNode,
            Completed
        }

        private struct Stack
        {
            private readonly ImmutableAvlNode<TRegistration>[] _array;
            private int _count;

            public Stack(int capacity)
            {
                _array = new ImmutableAvlNode<TRegistration>[capacity];
                _count = 0;
            }

            public void Push(ImmutableAvlNode<TRegistration> node)
            {
                _array[_count++] = node;
            }

            public ImmutableAvlNode<TRegistration> Pop()
            {
                var node = _array[--_count];
                _array[_count] = null;
                return node;
            }

            public int Count => _count;
        }
    }
}