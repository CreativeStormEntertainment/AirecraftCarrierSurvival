using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class TreeNode<T> where T : new()
{
    [SerializeField]
    public List<TreeNode<T>> Parents = new List<TreeNode<T>>();
    [SerializeField]
    public List<TreeNode<T>> Children = new List<TreeNode<T>>();
    [SerializeField]
    public int id;
    [SerializeField]
    public T Data;

    public TreeNode() 
    {
        Data = new T();
    }

    public TreeNode(int id, T Data)
    {
        this.id = id;
        this.Data = Data;
    }

    public void AddChild(TreeNode<T> child)
    {
        Children.Add(child);
        child.Parents.Add(this);
    }

    public void RemoveFromTree()
    {
        foreach (var child in Children)
        {
            child.Parents.Remove(this);
        }
        foreach (var parent in Parents)
        {
            parent.Children.Remove(this);
        }
        Children.Clear();
        Parents.Clear();
    }

    public void RemoveNode(TreeNode<T> node)
    {
        Parents.Remove(node);
        Children.Remove(node);

        node.Parents.Remove(this);
        node.Children.Remove(this);
    }

    public void GetAllNodes(HashSet<TreeNode<T>> nodes)
    {
        nodes.Add(this);

        foreach (var child in Children)
        {
            if (nodes.Add(child))
            {
                child.GetAllNodes(nodes);
            }
        }

        foreach (var parent in Parents)
        {
            if (nodes.Add(parent))
            {
                parent.GetAllNodes(nodes);
            }
        }
    }

    public void GetAllConnections(Dictionary<TreeNode<T>, HashSet<TreeNode<T>>> connections)
    {
        HashSet<TreeNode<T>> myConnections;
        if (!connections.TryGetValue(this, out myConnections))
        {
            myConnections = new HashSet<TreeNode<T>>();
            connections[this] = myConnections;
        }

        foreach (var child in Children)
        {
            HashSet<TreeNode<T>> childConnections;
            if (!connections.TryGetValue(child, out childConnections)) 
            {
                childConnections = new HashSet<TreeNode<T>>();
                connections[child] = childConnections;

                child.GetAllConnections(connections);
            }
            if (!childConnections.Contains(this))
            {
                myConnections.Add(child);
            }
        }

        foreach (var parent in Parents)
        {
            HashSet<TreeNode<T>> parentConnections;
            if (!connections.TryGetValue(parent, out parentConnections))
            {
                parentConnections = new HashSet<TreeNode<T>>();
                connections[parent] = parentConnections;

                parent.GetAllConnections(connections);
            }
            if (!myConnections.Contains(parent))
            {
                parentConnections.Add(this);
            }
        }
    }

    public TreeNode<T> FindNode(int id)
    {
        if (id == this.id)
        {
            return this;
        }
        return FindNode(id, new HashSet<TreeNode<T>>());
    }

    public TreeNode<T> Duplicate(Func<T,T> duplicateData)
    {
        var connections = new Dictionary<TreeNode<T>, HashSet<TreeNode<T>>>();
        GetAllConnections(connections);

        var oldNewNodes = new Dictionary<TreeNode<T>, TreeNode<T>>();

        foreach (var pair in connections)
        {
            var node = GetNode(pair.Key, oldNewNodes, duplicateData);
            foreach (var child in pair.Value)
            {
                node.AddChild(GetNode(child, oldNewNodes, duplicateData));
            }
        }
        return oldNewNodes[this];
    }

    TreeNode<T> GetNode(TreeNode<T> oldNode, Dictionary<TreeNode<T>, TreeNode<T>> oldNewNodes, Func<T, T> duplicateData)
    {
        TreeNode<T> result;
        if (!oldNewNodes.TryGetValue(oldNode, out result))
        {
            result = new TreeNode<T>();
            result.id = oldNode.id;
            result.Data = duplicateData(oldNode.Data);
            oldNewNodes[oldNode] = result;
        }
        return result;
    }

    TreeNode<T> FindNode(int id, HashSet<TreeNode<T>> nodes)
    {
        foreach (var child in Children)
        {
            if (nodes.Add(child))
            {
                if (id == child.id)
                {
                    return child;
                }
                var node = child.FindNode(id, nodes);
                if (node != null)
                {
                    return node;
                }
            }
        }

        foreach (var parent in Parents)
        {
            if (nodes.Add(parent))
            {
                if (id == parent.id)
                {
                    return parent;
                }
                var node = parent.FindNode(id, nodes);
                if (node != null)
                {
                    return node;
                }
            }
        }

        return null;
    }
}
