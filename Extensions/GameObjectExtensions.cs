using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DT {
  public static class GameObjectExtensions {
    public static string FullName(this GameObject g) {
      string name = g.name;
      while (g.transform.parent != null) {
        g = g.transform.parent.gameObject;
        name = g.name + "/" + name;
      }
      return name;
    }
    
    public static bool IsInLayerMask(this GameObject g, LayerMask mask) {
      return ((mask.value & (1 << g.layer)) > 0);
    }
    
    public static T GetCachedComponent<T>(this GameObject g, Dictionary<Type, MonoBehaviour> cache, bool searchChildren = false) where T : class {
			Type type = typeof(T);
      
			if (!cache.ContainsKey(type)) {
        Queue<GameObject> gameObjectQueue = new Queue<GameObject>();
        gameObjectQueue.Enqueue(g);
        
        MonoBehaviour component = null;
        while (gameObjectQueue.Count > 0) {
          GameObject current = gameObjectQueue.Dequeue();
				  component = current.GetComponent<T>() as MonoBehaviour;
          
          if (component != null) {
            break;
          }
          
          if (searchChildren) {
            foreach (Transform childTransform in current.transform) {
              gameObjectQueue.Enqueue(childTransform.gameObject);
            }
          }
        }
				
				if (component == null) {
					Debug.LogError("Failed to get component for type: " + type.Name);
					return default(T);
				}
				
				cache[type] = component;
			}
			
			return cache[type] as T;
    }
    
    public static T GetRequiredComponent<T>(this GameObject g) {
      T component = g.GetComponent<T>();
      if (component == null) {
        Debug.LogError("MissingRequiredComponent: Component " + typeof(T).Name + " missing in " + g.FullName());
      }
      return component;
    }
    
    public static T GetRequiredComponentInChildren<T>(this GameObject g) {
      T component = g.GetOnlyComponentInChildren<T>();
      if (component == null) {
        Debug.LogError("GetRequiredComponentInChildren: Component " + typeof(T).Name + " missing in " + g.FullName());
      }
      return component;
    }
    
    public static T GetOnlyComponentInChildren<T>(this GameObject g) {
      T[] components = g.GetComponentsInChildren<T>();
      if (components.Length <= 0) {
        return default(T);
      } else {
        if (components.Length > 1) {
          Debug.LogError("GetOnlyComponentInChildren: Found more than 1 component (" + typeof(T).Name + ") in children for gameObject: " + g.FullName());
        }
        return components[0];
      }
    }
    
    public static T GetRequiredComponentInParent<T>(this GameObject g) {
      T component = g.GetComponentInParent<T>();
      if (component == null) {
        Debug.LogError("GetRequiredComponentInParent: Component " + typeof(T).Name + " missing in " + g.FullName());
      }
      return component;
    }
    
    public static GameObject[] FindChildGameObjectsWithTag(this GameObject g, string tag) {
      List<GameObject> taggedChildGameObjects = new List<GameObject>();
      g.FindChildGameObjectsWithTagHelper(tag, taggedChildGameObjects);
      return taggedChildGameObjects.ToArray();
    }
    
    public static void FindChildGameObjectsWithTagHelper(this GameObject g, string tag, List<GameObject> objects) {
      foreach (Transform childTransform in g.transform) {
        GameObject child = childTransform.gameObject;
        if (child.CompareTag(tag)) {
          objects.Add(child.gameObject);
        }
        child.FindChildGameObjectsWithTagHelper(tag, objects);
      }
    }
  }
}
