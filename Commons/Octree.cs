using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Octree
{
    private Vector3 rootCenter;
    private float rootRadius;
    private int maxNodesInPartition;
    private int maxDepth;

    public Octree(Vector3 center, float radius, int maxNodesInPartition, int maxDepth = 32)
    {
        rootCenter = center;
        rootRadius = radius;
        this.maxNodesInPartition = maxNodesInPartition;
        this.maxDepth = maxDepth;

        octreeRoot = new OctreeNode();
        octreeRoot.nodePosition = center;
        octreeRoot.nodeRadius = radius;
        octreeRoot.maxNodesInPartition = maxNodesInPartition;
        octreeRoot.currentDepth = maxDepth;
        octreeRoot.maxDepth = maxDepth;
    }

    private OctreeNode octreeRoot = new OctreeNode();

    public GameObject GetObjectAtPartition(Vector3 objPos)
    {
        return octreeRoot.GetObjectAtPartition(objPos);
    }

    public bool InsertPosition(Vector3 position, GameObject obj = null)
    {
        return octreeRoot.InsertPosition(position, obj);
    }

    public bool RemovePosition(Vector3 position)
    {
        return octreeRoot.RemovePosition(position);
    }

    /// <summary>
    /// Clears the entire octree and rebuilds with fresh parameters
    /// </summary>
    public void Clear()
    {
        octreeRoot = new OctreeNode();
        octreeRoot.nodePosition = rootCenter;
        octreeRoot.nodeRadius = rootRadius;
        octreeRoot.maxNodesInPartition = maxNodesInPartition;
        octreeRoot.currentDepth = maxDepth;
        octreeRoot.maxDepth = maxDepth;
    }

    /// <summary>
    /// Gets all GameObjects within a radius of the given position
    /// </summary>
    public List<GameObject> GetObjectsInRadius(Vector3 position, float radius)
    {
        List<GameObject> result = new List<GameObject>();
        octreeRoot.GetObjectsInRadius(position, radius, result);
        return result;
    }

    /// <summary>
    /// Gets all positions within a radius of the given position
    /// </summary>
    public List<Vector3> GetPositionsInRadius(Vector3 position, float radius)
    {
        List<Vector3> result = new List<Vector3>();
        octreeRoot.GetPositionsInRadius(position, radius, result);
        return result;
    }

    public void DrawGizmos()
    {
        octreeRoot.DrawGizmos();
    }

    public class OctreeNode
    {
        public Vector3 nodePosition;
        public float nodeRadius;
        public OctreeNode[] children = new OctreeNode[8];
        public int maxNodesInPartition;
        public int currentNodesInPartition = 0;
        public int currentDepth;
        public int maxDepth;
        public List<Vector3> positions = new List<Vector3>();
        public List<GameObject> objects = new List<GameObject>();

        private bool hasBeenDivided = false;

        public bool InsertPosition(Vector3 position, GameObject obj = null)
        {
            if (currentDepth == 0)
                return false;

            if (positions.Contains(position))
                return true;

            if (currentNodesInPartition < maxNodesInPartition && !hasBeenDivided)
            {
                currentNodesInPartition++;
                positions.Add(position);
                if (obj != null)
                    objects.Add(obj);
                return true;
            }

            if (!hasBeenDivided)
            {
                SubDivide();
                PassDownObjects();
                currentNodesInPartition = 0;
                hasBeenDivided = true;
                return true;
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(position))
                    {
                        if (children[i].InsertPosition(position))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool RemovePosition(Vector3 position)
        {
            if (!hasBeenDivided)
            {
                if (positions.Contains(position))
                {
                    currentNodesInPartition--;
                    positions.Remove(position);
                    return true;
                }
            }
            else
            {
                int activeChildCount = 8;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].GetNodesInPartition() == 0)
                        activeChildCount--;
                    if (children[i].IsInsideBoundingBox(position))
                    {
                        if (children[i].RemovePosition(position))
                        {
                            return true;
                        }
                    }
                }
                if (activeChildCount == 0)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = null;
                    }
                    hasBeenDivided = false;
                }
            }

            return false;
        }

        public void DrawGizmos()
        {
            if (currentDepth == maxDepth)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < positions.Count; i++)
                {
                    Gizmos.DrawWireSphere(positions[i], .8f);
                }
                Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 2);
            }
            else
            {
                switch (currentDepth)
                {
                    case 3:
                        Gizmos.color = Color.magenta;
                        break;
                    case 2:
                        Gizmos.color = Color.cyan;
                        break;
                    case 1:
                        Gizmos.color = Color.green;
                        break;
                    case 0:
                        Gizmos.color = Color.red;
                        break;
                }
                for (int i = 0; i < positions.Count; i++)
                {
                    Gizmos.DrawWireSphere(positions[i], .8f);
                }
                Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 1.9f);
            }
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                    children[i].DrawGizmos();
            }
        }

        private void SubDivide(int subDivideIndex = -1)
        {
            float newRadius = nodeRadius / 2f;

            children[0] = new OctreeNode();
            children[0].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

            children[1] = new OctreeNode();
            children[1].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

            children[2] = new OctreeNode();
            children[2].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

            children[3] = new OctreeNode();
            children[3].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

            children[4] = new OctreeNode();
            children[4].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

            children[5] = new OctreeNode();
            children[5].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

            children[6] = new OctreeNode();
            children[6].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

            children[7] = new OctreeNode();
            children[7].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

            for (int i = 0; i < 8; i++)
            {
                children[i].nodeRadius = nodeRadius / 2;
                children[i].maxNodesInPartition = maxNodesInPartition;
                children[i].currentDepth = currentDepth - 1;
                children[i].maxDepth = maxDepth;
            }
        }

        private bool IsInsideBoundingBox(Vector3 position)
        {
            return (position.y <= (nodePosition + Vector3.up * nodeRadius).y) && (position.y >= (nodePosition - Vector3.up * nodeRadius).y) &&
            (position.x <= (nodePosition + Vector3.right * nodeRadius).x) && (position.x >= (nodePosition - Vector3.right * nodeRadius).x) &&
            (position.z <= (nodePosition + Vector3.forward * nodeRadius).z) && (position.z >= (nodePosition - Vector3.forward * nodeRadius).z);
        }

        private void PassDownObjects()
        {
            for (int i = 0; i < children.Length; i++)
            {
                for (int j = positions.Count - 1; j >= 0; j--)
                {
                    if (children[i].IsInsideBoundingBox(positions[j]))
                    {
                        GameObject obj = (j < objects.Count) ? objects[j] : null;
                        if (children[i].InsertPosition(positions[j], obj))
                        {
                            positions.RemoveAt(j);
                            if (j < objects.Count)
                                objects.RemoveAt(j);
                        }
                    }
                }
            }
        }

        private int GetNodesInPartition() => positions.Count;

        /// <summary>
        /// Checks if a sphere overlaps with this node's bounding box
        /// </summary>
        private bool IsSphereTouchingBoundingBox(Vector3 sphereCenter, float sphereRadius)
        {
            float dx = Mathf.Max(Mathf.Abs(sphereCenter.x - nodePosition.x) - nodeRadius, 0);
            float dy = Mathf.Max(Mathf.Abs(sphereCenter.y - nodePosition.y) - nodeRadius, 0);
            float dz = Mathf.Max(Mathf.Abs(sphereCenter.z - nodePosition.z) - nodeRadius, 0);

            float distanceSquared = dx * dx + dy * dy + dz * dz;
            return distanceSquared <= (sphereRadius * sphereRadius);
        }

        /// <summary>
        /// Gets all GameObjects within a radius of the given position
        /// </summary>
        public void GetObjectsInRadius(Vector3 position, float radius, List<GameObject> result)
        {
            if (!IsSphereTouchingBoundingBox(position, radius))
                return;

            // Check objects in this node
            for (int i = 0; i < objects.Count; i++)
            {
                if (Vector3.SqrMagnitude(objects[i].transform.position - position) <= radius * radius)
                {
                    result.Add(objects[i]);
                }
            }

            // Check children
            if (hasBeenDivided)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null)
                        children[i].GetObjectsInRadius(position, radius, result);
                }
            }
        }

        /// <summary>
        /// Gets all positions within a radius of the given position
        /// </summary>
        public void GetPositionsInRadius(Vector3 position, float radius, List<Vector3> result)
        {
            if (!IsSphereTouchingBoundingBox(position, radius))
                return;

            // Check positions in this node
            for (int i = 0; i < positions.Count; i++)
            {
                if (Vector3.SqrMagnitude(positions[i] - position) <= radius * radius)
                {
                    result.Add(positions[i]);
                }
            }

            // Check children
            if (hasBeenDivided)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i] != null)
                        children[i].GetPositionsInRadius(position, radius, result);
                }
            }
        }

        public GameObject GetObjectAtPartition(Vector3 objPos)
        {
            if (children.Length == 0)
            {
                if (IsInsideBoundingBox(objPos))
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (objects[i].transform.position == objPos)
                            return objects[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(objPos))
                        return children[i].GetObjectAtPartition(objPos);
                }
            }
            return null;
        }

        public List<GameObject> GetObjectsAtPartition(Vector3 partPos)
        {
            if (children.Length == 0)
            {
                if (IsInsideBoundingBox(partPos))
                {
                    return objects;
                }
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(partPos))
                        return children[i].GetObjectsAtPartition(partPos);
                }
            }
            return null;
        }
    }
}

public class OctreeGeneric<T>
{
    public OctreeGeneric(Vector3 center, float radius, int maxNodesInPartition, int maxDepth = 32)
    {
        octreeRoot.nodePosition = center;
        octreeRoot.nodeRadius = radius;
        octreeRoot.maxNodesInPartition = maxNodesInPartition;
        octreeRoot.currentDepth = maxDepth;
        octreeRoot.maxDepth = maxDepth;
    }

    private OctreeNodeGeneric<T> octreeRoot = new OctreeNodeGeneric<T>();

    public T GetObjectAtPartition(Vector3 pos)
    {
        return octreeRoot.GetObjectAtPartition(pos);
    }

    public List<T> GetObjectsAtPartition(Vector3 pos)
    {
        return octreeRoot.GetObjectsAtPartition(pos);
    }

    public bool Insert(Vector3 pos, T obj)
    {
        return octreeRoot.Insert(pos, obj);
    }

    public bool Remove(T obj)
    {
        return octreeRoot.Remove(obj);
    }

    public bool Remove(Vector3 pos)
    {
        return octreeRoot.Remove(pos);
    }

    public bool Remove(Vector3 pos, T obj)
    {
        return octreeRoot.Remove(pos, obj);
    }

    public void DrawGizmos()
    {
        octreeRoot.DrawGizmos();
    }

    public class OctreeNodeGeneric<T>
    {
        public Vector3 nodePosition;
        public float nodeRadius;
        public OctreeNodeGeneric<T>[] children = new OctreeNodeGeneric<T>[8];
        public int maxNodesInPartition;
        public int currentNodesInPartition = 0;
        public int currentDepth;
        public int maxDepth;
        public Dictionary<Vector3, T> objects = new Dictionary<Vector3, T>();

        private bool hasBeenDivided = false;

        public bool Insert(Vector3 pos, T obj)
        {
            if (currentDepth == 0)
                return false;

            if (objects.Contains(new KeyValuePair<Vector3, T>(pos, obj)))
                return true;

            if (currentNodesInPartition < maxNodesInPartition && !hasBeenDivided)
            {
                currentNodesInPartition++;
                objects.Add(pos, obj);
                return true;
            }

            if (!hasBeenDivided)
            {
                SubDivide();
                PassDownObjects();
                currentNodesInPartition = 0;
                hasBeenDivided = true;
                return true;
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(pos))
                    {
                        if (children[i].Insert(pos, obj))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool Remove(T obj)
        {
            if (!hasBeenDivided)
            {
                if (objects.ContainsValue(obj))
                {
                    currentNodesInPartition--;
                    KeyValuePair<Vector3, T> pair = objects.Where((x, y) => y.Equals(obj)).First();
                    objects.Remove(pair.Key);
                    return true;
                }
            }
            else
            {
                int activeChildCount = 8;
                KeyValuePair<Vector3, T> pair = objects.Where((x, y) => y.Equals(obj)).First();
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].GetNodesInPartition() == 0)
                        activeChildCount--;
                    if (children[i].IsInsideBoundingBox(pair.Key))
                    {
                        if (children[i].Remove(obj))
                        {
                            return true;
                        }
                    }
                }
                if (activeChildCount == 0)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = null;
                    }
                    hasBeenDivided = false;
                }
            }

            return false;
        }

        public bool Remove(Vector3 pos)
        {
            if (!hasBeenDivided)
            {
                if (objects.ContainsKey(pos))
                {
                    currentNodesInPartition--;
                    objects.Remove(pos);
                    return true;
                }
            }
            else
            {
                int activeChildCount = 8;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].GetNodesInPartition() == 0)
                        activeChildCount--;
                    if (children[i].IsInsideBoundingBox(pos))
                    {
                        if (children[i].Remove(pos))
                        {
                            return true;
                        }
                    }
                }
                if (activeChildCount == 0)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = null;
                    }
                    hasBeenDivided = false;
                }
            }

            return false;
        }

        public bool Remove(Vector3 pos, T obj)
        {
            if (!hasBeenDivided)
            {
                if (objects.ContainsValue(obj))
                {
                    currentNodesInPartition--;
                    objects.Remove(pos);
                    return true;
                }
            }
            else
            {
                int activeChildCount = 8;
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].GetNodesInPartition() == 0)
                        activeChildCount--;
                    if (children[i].IsInsideBoundingBox(pos))
                    {
                        if (children[i].Remove(pos, obj))
                        {
                            return true;
                        }
                    }
                }
                if (activeChildCount == 0)
                {
                    for (int i = 0; i < children.Length; i++)
                    {
                        children[i] = null;
                    }
                    hasBeenDivided = false;
                }
            }

            return false;
        }

        public void DrawGizmos()
        {
            if (currentDepth == maxDepth)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 2);
            }
            else
            {
                switch (currentDepth)
                {
                    case 3:
                        Gizmos.color = Color.magenta;
                        break;
                    case 2:
                        Gizmos.color = Color.cyan;
                        break;
                    case 1:
                        Gizmos.color = Color.green;
                        break;
                    case 0:
                        Gizmos.color = Color.red;
                        break;
                }
                Gizmos.DrawWireCube(nodePosition, Vector3.one * nodeRadius * 1.9f);
            }
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null)
                    children[i].DrawGizmos();
            }
        }

        private void SubDivide(int subDivideIndex = -1)
        {
            float newRadius = nodeRadius / 2f;

            children[0] = new OctreeNodeGeneric<T>();
            children[0].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

            children[1] = new OctreeNodeGeneric<T>();
            children[1].nodePosition = nodePosition + Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

            children[2] = new OctreeNodeGeneric<T>();
            children[2].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

            children[3] = new OctreeNodeGeneric<T>();
            children[3].nodePosition = nodePosition + Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

            children[4] = new OctreeNodeGeneric<T>();
            children[4].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius + Vector3.forward * newRadius;

            children[5] = new OctreeNodeGeneric<T>();
            children[5].nodePosition = nodePosition - Vector3.up * newRadius + Vector3.right * newRadius - Vector3.forward * newRadius;

            children[6] = new OctreeNodeGeneric<T>();
            children[6].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius + Vector3.forward * newRadius;

            children[7] = new OctreeNodeGeneric<T>();
            children[7].nodePosition = nodePosition - Vector3.up * newRadius - Vector3.right * newRadius - Vector3.forward * newRadius;

            for (int i = 0; i < 8; i++)
            {
                children[i].nodeRadius = nodeRadius / 2;
                children[i].maxNodesInPartition = maxNodesInPartition;
                children[i].currentDepth = currentDepth - 1;
                children[i].maxDepth = maxDepth;
            }
        }

        private bool IsInsideBoundingBox(Vector3 position)
        {
            return (position.y <= (nodePosition + Vector3.up * nodeRadius).y) && (position.y >= (nodePosition - Vector3.up * nodeRadius).y) &&
            (position.x <= (nodePosition + Vector3.right * nodeRadius).x) && (position.x >= (nodePosition - Vector3.right * nodeRadius).x) &&
            (position.z <= (nodePosition + Vector3.forward * nodeRadius).z) && (position.z >= (nodePosition - Vector3.forward * nodeRadius).z);
        }

        private void PassDownObjects()
        {
            for (int i = 0; i < children.Length; i++)
            {
                List<KeyValuePair<Vector3, T>> tempList = objects.ToList();
                for (int j = tempList.Count - 1; j >= 0; j--)
                {
                    if (children[i].IsInsideBoundingBox(tempList[j].Key))
                    {
                        if (children[i].Insert(tempList[j].Key, tempList[j].Value))
                            tempList.Remove(tempList[j]);
                    }
                }
                objects = tempList.ToDictionary(x => x.Key, y => y.Value);
            }
        }

        private int GetNodesInPartition() => objects.Count;

        public T GetObjectAtPartition(Vector3 partitionPosition)
        {
            if (children.Length == 0)
            {
                if (IsInsideBoundingBox(partitionPosition))
                {
                    foreach (KeyValuePair<Vector3, T> pair in objects)
                    {
                        if (pair.Key == partitionPosition)
                            return pair.Value;
                    }
                }
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(partitionPosition))
                        return children[i].GetObjectAtPartition(partitionPosition);
                }
            }
            return default(T);
        }

        public List<T> GetObjectsAtPartition(Vector3 partitionPosition)
        {
            if (children.Length == 0)
            {
                if (IsInsideBoundingBox(partitionPosition))
                {
                    return objects.Values.ToList();
                }
            }
            else
            {
                for (int i = 0; i < children.Length; i++)
                {
                    if (children[i].IsInsideBoundingBox(partitionPosition))
                        return children[i].GetObjectsAtPartition(partitionPosition);
                }
            }
            return null;
        }
    }
}