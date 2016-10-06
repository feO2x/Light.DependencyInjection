using System;
using System.Collections.Generic;
using System.Diagnostics;
using Light.GuardClauses;

namespace Light.DependencyInjection.Multithreading
{
    public sealed class ImmutableAvlNode<TKey, TValue> where TKey : IEquatable<TKey>
    {
        public static readonly ImmutableAvlNode<TKey, TValue> Empty = new ImmutableAvlNode<TKey, TValue>();
        public readonly ImmutableList<HashEntry<TKey, TValue>> Duplicates;
        public readonly int EntryCount;
        public readonly HashEntry<TKey, TValue> HashEntry;
        public readonly int Height;
        public readonly bool IsEmpty;
        public readonly ImmutableAvlNode<TKey, TValue> LeftChild;
        public readonly int NodeCount;
        public readonly ImmutableAvlNode<TKey, TValue> RightChild;

        private ImmutableAvlNode(ImmutableAvlNode<TKey, TValue> previousNode, ImmutableList<HashEntry<TKey, TValue>> newDuplicates)
        {
            Duplicates = newDuplicates;
            HashEntry = previousNode.HashEntry;
            LeftChild = previousNode.LeftChild;
            RightChild = previousNode.RightChild;
            Height = previousNode.Height;
            EntryCount = previousNode.EntryCount;
            NodeCount = previousNode.NodeCount;
        }

        private ImmutableAvlNode(ImmutableAvlNode<TKey, TValue> previousNode, HashEntry<TKey, TValue> replacedEntry)
        {
            HashEntry = replacedEntry;
            LeftChild = previousNode.LeftChild;
            RightChild = previousNode.RightChild;
            Duplicates = previousNode.Duplicates;
            Height = previousNode.Height;
            EntryCount = previousNode.EntryCount;
            NodeCount = previousNode.NodeCount;
        }

        private ImmutableAvlNode(ImmutableAvlNode<TKey, TValue> previousNode,
                                 ImmutableAvlNode<TKey, TValue> leftChild,
                                 ImmutableAvlNode<TKey, TValue> rightChild)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            HashEntry = previousNode.HashEntry;
            Duplicates = previousNode.Duplicates;
            NodeCount = 1 + LeftChild.NodeCount + RightChild.NodeCount;
            EntryCount = 1 + LeftChild.EntryCount + RightChild.EntryCount + Duplicates.Count;
            Height = 1 + Math.Max(LeftChild.Height, RightChild.Height);
        }

        private ImmutableAvlNode(HashEntry<TKey, TValue> newEntry)
        {
            HashEntry = newEntry;
            EntryCount = 1;
            NodeCount = 1;
            Height = 1;
            LeftChild = Empty;
            RightChild = Empty;
            Duplicates = ImmutableList<HashEntry<TKey, TValue>>.Empty;
        }

        private ImmutableAvlNode()
        {
            Height = 0;
            NodeCount = 0;
            EntryCount = 0;
            IsEmpty = true;
            Duplicates = ImmutableList<HashEntry<TKey, TValue>>.Empty;
        }

        private bool IsLeftHeavy => LeftChild.Height > RightChild.Height;

        private bool IsRightHeavy => RightChild.Height > LeftChild.Height;

        private ImmutableAvlNode<TKey, TValue> RotateRight()
        {
            return new ImmutableAvlNode<TKey, TValue>(LeftChild,
                                                      LeftChild.LeftChild,
                                                      new ImmutableAvlNode<TKey, TValue>(this, LeftChild.RightChild, RightChild));
        }

        private ImmutableAvlNode<TKey, TValue> RotateLeft()
        {
            return new ImmutableAvlNode<TKey, TValue>(RightChild,
                                                      new ImmutableAvlNode<TKey, TValue>(this, LeftChild, RightChild.LeftChild),
                                                      RightChild.RightChild);
        }

        public bool TryFind(int hashCode, TKey key, out TValue value)
        {
            var currentNode = this;
            while (true)
            {
                // If this node is empty, there can't be any value
                if (currentNode.IsEmpty)
                {
                    value = default(TValue);
                    return false;
                }

                // Check if this node fits
                if (hashCode == currentNode.HashEntry.HashCode)
                {
                    if (key.Equals(currentNode.HashEntry.Key) == false)
                        return currentNode.Duplicates.TryFind(key, out value);

                    value = currentNode.HashEntry.Value;
                    return true;
                }

                // Update currentNode for next loop run
                currentNode = hashCode < currentNode.HashEntry.HashCode ? currentNode.LeftChild : currentNode.RightChild;
            }
        }

        public bool TryFind(int hashCode, TKey key)
        {
            var currentNode = this;
            while (true)
            {
                if (currentNode.IsEmpty)
                    return false;

                if (hashCode == currentNode.HashEntry.HashCode)
                    return key.Equals(currentNode.HashEntry.Key) || currentNode.Duplicates.ContainsEntryWithKey(key);

                currentNode = hashCode < currentNode.HashEntry.HashCode ? currentNode.LeftChild : currentNode.RightChild;
            }
        }

        public ImmutableAvlNode<TKey, TValue> Add(HashEntry<TKey, TValue> newEntry)
        {
            if (IsEmpty)
                return new ImmutableAvlNode<TKey, TValue>(newEntry);

            if (newEntry.HashCode < HashEntry.HashCode)
                return AddToLeftBranch(newEntry);
            if (newEntry.HashCode > HashEntry.HashCode)
                return AddToRightBranch(newEntry);

            EnsureEntryDoesNotExist(newEntry);
            return new ImmutableAvlNode<TKey, TValue>(this, Duplicates.Add(newEntry));
        }

        private ImmutableAvlNode<TKey, TValue> AddToLeftBranch(HashEntry<TKey, TValue> newEntry)
        {
            var newLeftChild = LeftChild.Add(newEntry);

            var balance = newLeftChild.Height - RightChild.Height;
            if (balance < 2)
                return new ImmutableAvlNode<TKey, TValue>(this, newLeftChild, RightChild);

            if (newLeftChild.IsRightHeavy)
                newLeftChild = newLeftChild.RotateLeft();

            // A rotate right is necessary to avoid imbalance
            var newRightChild = new ImmutableAvlNode<TKey, TValue>(this, newLeftChild.RightChild, RightChild);
            var newParentNode = new ImmutableAvlNode<TKey, TValue>(newLeftChild, newLeftChild.LeftChild, newRightChild);
            return newParentNode;
        }

        private ImmutableAvlNode<TKey, TValue> AddToRightBranch(HashEntry<TKey, TValue> newEntry)
        {
            var newRightChild = RightChild.Add(newEntry);

            var balance = LeftChild.Height - newRightChild.Height;
            if (balance > -2)
                return new ImmutableAvlNode<TKey, TValue>(this, LeftChild, newRightChild);

            if (newRightChild.IsLeftHeavy)
                newRightChild = newRightChild.RotateRight();

            // A rotate left is necessary to avoid imbalance
            var newLeftChild = new ImmutableAvlNode<TKey, TValue>(this, LeftChild, newRightChild.LeftChild);
            var newParentNode = new ImmutableAvlNode<TKey, TValue>(newRightChild, newLeftChild, newRightChild.RightChild);
            return newParentNode;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureEntryDoesNotExist(HashEntry<TKey, TValue> newEntry)
        {
            if (newEntry.Key.Equals(HashEntry.Key) || Duplicates.ContainsEntryWithKey(newEntry.Key))
                throw new ArgumentException($"The entry {newEntry} already exists in this AVL node.", nameof(newEntry));
        }

        public ImmutableAvlNode<TKey, TValue> Replace(HashEntry<TKey, TValue> entry)
        {
            if (entry.HashCode < HashEntry.HashCode)
                return new ImmutableAvlNode<TKey, TValue>(this, LeftChild.Replace(entry), RightChild);
            if (entry.HashCode > HashEntry.HashCode)
                return new ImmutableAvlNode<TKey, TValue>(this, LeftChild, RightChild.Replace(entry));

            if (HashEntry.Key.Equals(entry.Key))
                return new ImmutableAvlNode<TKey, TValue>(this, entry);

            var targetIndex = Duplicates.IndexOfEntry(entry.Key);
            EnsureEntryDoesExist(targetIndex, entry);
            return new ImmutableAvlNode<TKey, TValue>(this, Duplicates.ReplaceAt(targetIndex, entry));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void EnsureEntryDoesExist(int targetIndex, HashEntry<TKey, TValue> entry)
        {
            if (targetIndex != -1)
                return;

            throw new ArgumentException($"The entry with hashcode {entry.HashCode} and key {entry.Key} does not exist in the AVL node and thus cannot be replaced.");
        }

        public IEnumerable<ImmutableAvlNode<TKey, TValue>> TraverseInOrder()
        {
            if (IsEmpty)
                yield break;

            foreach (var leftChild in LeftChild.TraverseInOrder())
            {
                yield return leftChild;
            }
            yield return this;
            foreach (var rightChild in RightChild.TraverseInOrder())
            {
                yield return rightChild;
            }
        }

        public void TraverseInOrder(Action<ImmutableAvlNode<TKey, TValue>> nodeAction)
        {
            if (IsEmpty)
                return;

            LeftChild.TraverseInOrder(nodeAction);
            nodeAction(this);
            RightChild.TraverseInOrder(nodeAction);
        }

        public override string ToString()
        {
            if (IsEmpty)
                return "Empty AVL node";

            return $"AVL node with hash {HashEntry.HashCode}";
        }
    }
}