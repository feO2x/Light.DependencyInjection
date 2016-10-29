using System;
using System.Diagnostics;
using Light.DependencyInjection.Registrations;
using Light.GuardClauses;

namespace Light.DependencyInjection.DataStructures
{
    /// <summary>
    ///     Represents a single node of an AVL tree with <see cref="TypeKey" /> as the key type. This data structure is immutable, thus if the
    ///     (sub-)tree is changed, new instances with the updated values will be returned.
    /// </summary>
    /// <typeparam name="TRegistration">The registration type that is stored as a value.</typeparam>
    public sealed class ImmutableAvlNode<TRegistration>
    {
        /// <summary>
        ///     Gets an empty immutable node.
        /// </summary>
        public static readonly ImmutableAvlNode<TRegistration> Empty = new ImmutableAvlNode<TRegistration>();

        /// <summary>
        ///     Gets the collection containing the entries having the same hash codes.
        /// </summary>
        public readonly ImmutableList<HashEntry<TypeKey, TRegistration>> Duplicates;

        /// <summary>
        ///     Gets the number of entries in the whole AVL (sub-)tree.
        /// </summary>
        public readonly int EntryCount;

        /// <summary>
        ///     Gets the hash entry of this node.
        /// </summary>
        public readonly HashEntry<TypeKey, TRegistration> HashEntry;

        /// <summary>
        ///     Gets the height of this whole AVL (sub-)tree.
        /// </summary>
        public readonly int Height;

        /// <summary>
        ///     Gets the value indicating whether this node is empty.
        /// </summary>
        public readonly bool IsEmpty;

        /// <summary>
        ///     Gets the left subtree.
        /// </summary>
        public readonly ImmutableAvlNode<TRegistration> LeftChild;

        /// <summary>
        ///     Gets the number of nodes in the whole AVL (sub-)tree.
        /// </summary>
        public readonly int NodeCount;

        /// <summary>
        ///     Gets the right subtree.
        /// </summary>
        public readonly ImmutableAvlNode<TRegistration> RightChild;

        private ImmutableAvlNode(ImmutableAvlNode<TRegistration> previousNode, ImmutableList<HashEntry<TypeKey, TRegistration>> newDuplicates)
        {
            Duplicates = newDuplicates;
            HashEntry = previousNode.HashEntry;
            LeftChild = previousNode.LeftChild;
            RightChild = previousNode.RightChild;
            Height = previousNode.Height;
            EntryCount = previousNode.EntryCount;
            NodeCount = previousNode.NodeCount;
        }

        private ImmutableAvlNode(ImmutableAvlNode<TRegistration> previousNode, HashEntry<TypeKey, TRegistration> replacedEntry)
        {
            HashEntry = replacedEntry;
            LeftChild = previousNode.LeftChild;
            RightChild = previousNode.RightChild;
            Duplicates = previousNode.Duplicates;
            Height = previousNode.Height;
            EntryCount = previousNode.EntryCount;
            NodeCount = previousNode.NodeCount;
        }

        private ImmutableAvlNode(ImmutableAvlNode<TRegistration> previousNode,
                                 ImmutableAvlNode<TRegistration> leftChild,
                                 ImmutableAvlNode<TRegistration> rightChild)
        {
            LeftChild = leftChild;
            RightChild = rightChild;
            HashEntry = previousNode.HashEntry;
            Duplicates = previousNode.Duplicates;
            NodeCount = 1 + LeftChild.NodeCount + RightChild.NodeCount;
            EntryCount = 1 + LeftChild.EntryCount + RightChild.EntryCount + Duplicates.Count;
            Height = 1 + Math.Max(LeftChild.Height, RightChild.Height);
        }

        private ImmutableAvlNode(HashEntry<TypeKey, TRegistration> newEntry)
        {
            HashEntry = newEntry;
            EntryCount = 1;
            NodeCount = 1;
            Height = 1;
            LeftChild = Empty;
            RightChild = Empty;
            Duplicates = ImmutableList<HashEntry<TypeKey, TRegistration>>.Empty;
        }

        private ImmutableAvlNode()
        {
            Height = 0;
            NodeCount = 0;
            EntryCount = 0;
            IsEmpty = true;
            Duplicates = ImmutableList<HashEntry<TypeKey, TRegistration>>.Empty;
        }

        private bool IsLeftHeavy => LeftChild.Height > RightChild.Height;

        private bool IsRightHeavy => RightChild.Height > LeftChild.Height;

        private ImmutableAvlNode<TRegistration> RotateRight()
        {
            return new ImmutableAvlNode<TRegistration>(LeftChild,
                                                       LeftChild.LeftChild,
                                                       new ImmutableAvlNode<TRegistration>(this, LeftChild.RightChild, RightChild));
        }

        private ImmutableAvlNode<TRegistration> RotateLeft()
        {
            return new ImmutableAvlNode<TRegistration>(RightChild,
                                                       new ImmutableAvlNode<TRegistration>(this, LeftChild, RightChild.LeftChild),
                                                       RightChild.RightChild);
        }

        /// <summary>
        ///     Tries to find the registration with the specified hash code and type key in the AVL (sub-)tree.
        /// </summary>
        /// <param name="hashCode">The hash code of the target node.</param>
        /// <param name="key">The key that uniquely identifies the value.</param>
        /// <param name="value">The value to be retrieved.</param>
        /// <returns>True if the value could be retrieved, else false.</returns>
        public bool TryFind(int hashCode, TypeKey key, out TRegistration value)
        {
            var currentNode = this;
            while (true)
            {
                // If this node is empty, there can't be any value
                if (currentNode.IsEmpty)
                {
                    value = default(TRegistration);
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

        /// <summary>
        ///     Checks if a registration with the specified hash code and type key exists in the AVL (sub-)tree.
        /// </summary>
        /// <param name="hashCode">The hash code of the target node.</param>
        /// <param name="typeKey">The key that uniquely identifies the value.</param>
        /// <returns>True if the registration is present, else false.</returns>
        public bool TryFind(int hashCode, TypeKey typeKey)
        {
            var currentNode = this;
            while (true)
            {
                if (currentNode.IsEmpty)
                    return false;

                if (hashCode == currentNode.HashEntry.HashCode)
                    return typeKey.Equals(currentNode.HashEntry.Key) || currentNode.Duplicates.ContainsEntryWithKey(typeKey);

                currentNode = hashCode < currentNode.HashEntry.HashCode ? currentNode.LeftChild : currentNode.RightChild;
            }
        }

        /// <summary>
        ///     Adds the specified hash entry in a new immutable AVL tree.
        /// </summary>
        /// <param name="newEntry">The entry to be added.</param>
        /// <returns>The new tree containing the new entry.</returns>
        /// <exception cref="ArgumentException">Thrown when the specified entry already exists in the tree.</exception>
        public ImmutableAvlNode<TRegistration> Add(HashEntry<TypeKey, TRegistration> newEntry)
        {
            if (IsEmpty)
                return new ImmutableAvlNode<TRegistration>(newEntry);

            if (newEntry.HashCode < HashEntry.HashCode)
                return AddToLeftBranch(newEntry);
            if (newEntry.HashCode > HashEntry.HashCode)
                return AddToRightBranch(newEntry);

            EnsureEntryDoesNotExist(newEntry);
            return new ImmutableAvlNode<TRegistration>(this, Duplicates.Add(newEntry));
        }

        private ImmutableAvlNode<TRegistration> AddToLeftBranch(HashEntry<TypeKey, TRegistration> newEntry)
        {
            var newLeftChild = LeftChild.Add(newEntry);

            var balance = newLeftChild.Height - RightChild.Height;
            if (balance < 2)
                return new ImmutableAvlNode<TRegistration>(this, newLeftChild, RightChild);

            if (newLeftChild.IsRightHeavy)
                newLeftChild = newLeftChild.RotateLeft();

            // A rotate right is necessary to avoid imbalance
            var newRightChild = new ImmutableAvlNode<TRegistration>(this, newLeftChild.RightChild, RightChild);
            var newParentNode = new ImmutableAvlNode<TRegistration>(newLeftChild, newLeftChild.LeftChild, newRightChild);
            return newParentNode;
        }

        private ImmutableAvlNode<TRegistration> AddToRightBranch(HashEntry<TypeKey, TRegistration> newEntry)
        {
            var newRightChild = RightChild.Add(newEntry);

            var balance = LeftChild.Height - newRightChild.Height;
            if (balance > -2)
                return new ImmutableAvlNode<TRegistration>(this, LeftChild, newRightChild);

            if (newRightChild.IsLeftHeavy)
                newRightChild = newRightChild.RotateRight();

            // A rotate left is necessary to avoid imbalance
            var newLeftChild = new ImmutableAvlNode<TRegistration>(this, LeftChild, newRightChild.LeftChild);
            var newParentNode = new ImmutableAvlNode<TRegistration>(newRightChild, newLeftChild, newRightChild.RightChild);
            return newParentNode;
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private void EnsureEntryDoesNotExist(HashEntry<TypeKey, TRegistration> newEntry)
        {
            if (newEntry.Key.Equals(HashEntry.Key) || Duplicates.ContainsEntryWithKey(newEntry.Key))
                throw new ArgumentException($"The entry {newEntry} already exists in this AVL node.", nameof(newEntry));
        }

        /// <summary>
        ///     Returns a new immutable AVL tree where an existing hash entry is replaced with the specified one, having the same hash code and key.
        /// </summary>
        /// <param name="entry">The entry that will replace the existing one.</param>
        /// <returns>The new AVL tree containing the specified entry.</returns>
        /// <exception cref="ArgumentException">Thrown when no hash entry with the hash code and key of <paramref name="entry" /> exists.</exception>
        public ImmutableAvlNode<TRegistration> Replace(HashEntry<TypeKey, TRegistration> entry)
        {
            if (entry.HashCode < HashEntry.HashCode)
                return new ImmutableAvlNode<TRegistration>(this, LeftChild.Replace(entry), RightChild);
            if (entry.HashCode > HashEntry.HashCode)
                return new ImmutableAvlNode<TRegistration>(this, LeftChild, RightChild.Replace(entry));

            if (HashEntry.Key.Equals(entry.Key))
                return new ImmutableAvlNode<TRegistration>(this, entry);

            var targetIndex = Duplicates.IndexOfEntry(entry.Key);
            EnsureEntryDoesExist(targetIndex, entry);
            return new ImmutableAvlNode<TRegistration>(this, Duplicates.ReplaceAt(targetIndex, entry));
        }

        [Conditional(Check.CompileAssertionsSymbol)]
        private static void EnsureEntryDoesExist(int targetIndex, HashEntry<TypeKey, TRegistration> entry)
        {
            if (targetIndex != -1)
                return;

            throw new ArgumentException($"The entry with hashcode {entry.HashCode} and key {entry.Key} does not exist in the AVL node and thus cannot be replaced.");
        }

        /// <summary>
        ///     Gets an enumerator for this AVL tree that iterates in sorted hash code order.
        /// </summary>
        public AvlTreeEnumerator<TRegistration> GetEnumerator()
        {
            return new AvlTreeEnumerator<TRegistration>(this);
        }

        /// <summary>
        ///     Gets the string representation of this AVL node.
        /// </summary>
        public override string ToString()
        {
            return IsEmpty ? "Empty AVL node" : $"AVL node with hash {HashEntry.HashCode}";
        }
    }
}