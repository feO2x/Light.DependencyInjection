using System;
using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents an <see cref="IEnumerator{T}" /> that iterates through <see cref="ImmutableAvlNode{TRegistration}" /> trees in sorted hash code order.
    /// </summary>
    /// <typeparam name="TRegistration">The type of the registration stored in the AVL tree.</typeparam>
    public struct AvlTreeEnumerator<TRegistration> : IEnumerator<ImmutableAvlNode<TRegistration>>
    {
        private Stack _stack;
        private readonly ImmutableAvlNode<TRegistration> _rootNode;
        private ImmutableAvlNode<TRegistration> _currentNode;
        private State _state;

        /// <summary>
        ///     Initializes a new instance of <see cref="AvlTreeEnumerator{TRegistration}" />.
        /// </summary>
        /// <param name="rootNode">The AVL tree that will be iterated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="rootNode" /> is null.</exception>
        public AvlTreeEnumerator(ImmutableAvlNode<TRegistration> rootNode)
        {
            rootNode.MustNotBeNull(nameof(rootNode));

            _rootNode = rootNode;
            _stack = new Stack(rootNode.Height); // TODO: maybe we can reduce the capacity here to log(height)?
            _currentNode = null;
            _state = State.AtStart;
        }

        /// <summary>
        ///     Advances the enumerator to the next node of the AVL tree.
        /// </summary>
        /// <returns>true if the enumerator was successfully advanced to the next node; false if the enumerator has passed the end of the AVL tree.</returns>
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
                    // Check if there is a right child that has to be returned
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

        /// <summary>
        ///     Sets the enumerator to its initial position, which is before the first element in the AVL tree.
        /// </summary>
        public void Reset()
        {
            _currentNode = null;
            _state = State.AtStart;
        }

        /// <summary>
        ///     Gets the current node of the AVL tree.
        /// </summary>
        public ImmutableAvlNode<TRegistration> Current => _currentNode;

        object IEnumerator.Current => _currentNode;

        /// <summary>
        ///     Does nothing because this enumerator references no resources that must be diposed.
        /// </summary>
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