using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ObjectDuplicate {
    [MenuItem("Edit/DummyDuplicate %d", false, -1)]

    static void CreateEmptyObject() {
        foreach(var obj in Selection.objects) {
            var path = AssetDatabase.GetAssetPath(obj);
            if (path == string.Empty) {
                var gameObject = obj as GameObject;
                var copy = GameObject.Instantiate(gameObject, gameObject.transform.parent);
                copy.name = obj.name;
                copy.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex());
                Undo.RegisterCreatedObjectUndo(copy, "duplicate");
            } else {
                var newPath = AssetDatabase.GenerateUniqueAssetPath(path);
                AssetDatabase.CopyAsset(path, newPath);
            }
        }
    }
}