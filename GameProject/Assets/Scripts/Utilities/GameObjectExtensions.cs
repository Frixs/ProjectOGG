using UnityEngine;

/// <summary>
/// Extensions above <see cref="GameObject"/>
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Get the top-most parent of specific layer
    /// </summary>
    /// <param name="go">The starting game object</param>
    /// <param name="layerMask">The layer</param>
    /// <returns>The top-most gameobject with the layer or null for no results</returns>
    public static GameObject GetTopParent(this GameObject go, LayerMask layerMask)
    {
        GameObject parent, topmost = null;

        // Get the first parent
        parent = go.transform.parent.gameObject;

        // Go through the parents until there is no parent...
        while (parent != null)
        {
            // Check for a new top most...
            if (parent?.layer == layerMask)
                topmost = parent;

            // Get a new parent
            parent = parent.transform.parent?.gameObject;
        }

        return topmost;
    }
}
