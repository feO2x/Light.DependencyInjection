using System;
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

        private ImmutableAvlNode(ImmutableAvlNode<TKey, TValue> previousNode, ImmutableList<HashEntry<TKey, TValue>> duplicates)
        {
            Duplicates = duplicates;
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

        private ImmutableAvlNode(HashEntry<TKey, TValue> hashEntry, ImmutableAvlNode<TKey, TValue> leftChild, ImmutableAvlNode<TKey, TValue> rightChild)
        {
            var balance = leftChild.Height - rightChild.Height;

            if (balance == -2)
            {
                if (rightChild.IsLeftHeavy)
                    rightChild = rightChild.RotateRight();

                // Rotate left
                HashEntry = rightChild.HashEntry;
                LeftChild = new ImmutableAvlNode<TKey, TValue>(hashEntry, leftChild, rightChild.LeftChild);
                RightChild = rightChild.RightChild;
            }
            else if (balance == 2)
            {
                if (leftChild.IsRightHeavy)
                    leftChild = leftChild.RotateLeft();

                // Rotate right
                HashEntry = leftChild.HashEntry;
                RightChild = new ImmutableAvlNode<TKey, TValue>(hashEntry, leftChild.RightChild, rightChild);
                LeftChild = leftChild.LeftChild;
            }
            else
            {
                HashEntry = hashEntry;
                LeftChild = leftChild;
                RightChild = rightChild;
            }

            Height = 1 + Math.Max(LeftChild.Height, RightChild.Height);
            Duplicates = ImmutableList<HashEntry<TKey, TValue>>.Empty;
            NodeCount = LeftChild.NodeCount + RightChild.NodeCount + 1;
            EntryCount = LeftChild.EntryCount + RightChild.EntryCount + 1;
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
            return new ImmutableAvlNode<TKey, TValue>(LeftChild.HashEntry,
                                                      LeftChild.LeftChild,
                                                      new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild.RightChild, RightChild));
        }

        private ImmutableAvlNode<TKey, TValue> RotateLeft()
        {
            return new ImmutableAvlNode<TKey, TValue>(RightChild.HashEntry,
                                                      new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild, RightChild.LeftChild),
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
                return new ImmutableAvlNode<TKey, TValue>(newEntry, Empty, Empty);

            if (newEntry.HashCode < HashEntry.HashCode)
                return new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild.Add(newEntry), RightChild);
            if (newEntry.HashCode > HashEntry.HashCode)
                return new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild, RightChild.Add(newEntry));

            EnsureEntryDoesNotExist(newEntry);
            return new ImmutableAvlNode<TKey, TValue>(this, Duplicates.Add(newEntry));
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
                return new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild.Replace(entry), RightChild);
            if (entry.HashCode > HashEntry.HashCode)
                return new ImmutableAvlNode<TKey, TValue>(HashEntry, LeftChild, RightChild.Replace(entry));

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

        public void TraverseInOrder(Action<ImmutableAvlNode<TKey, TValue>> nodeAction)
        {
            if (IsEmpty)
                return;

            LeftChild.TraverseInOrder(nodeAction);
            nodeAction(this);
            RightChild.TraverseInOrder(nodeAction);
        }
    }
}