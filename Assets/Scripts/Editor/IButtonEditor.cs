using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/*
[CustomEditor(typeof(IButton), true)]
public class IButtonEditor : Editor {
    
    public override void OnInspectorGUI()
    {
        IButton ibutton = (IButton)target;
        ibutton.playSoundWhenHover = EditorGUILayout.Toggle("Play Sound When Hover", ibutton.playSoundWhenHover);
        ibutton.playSoundWhenClick = EditorGUILayout.Toggle("Play Sound When Click", ibutton.playSoundWhenClick);
        ibutton.sound = (AudioClip)EditorGUILayout.ObjectField("Sound", ibutton.sound, typeof(AudioClip), true);
        DrawDefaultInspector();
    }
    
}
*/