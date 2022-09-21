#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    [CustomPropertyDrawer(typeof(GearChangeComponent))]
    public partial class GearChangeComponentDrawer : SoundComponentDrawer
    {
        public override bool OnNUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!base.OnNUI(position, property, label))
            {
                return false;
            }

            drawer.Field("baseVolume");
            drawer.Field("basePitch");
            drawer.Field("randomVolumeRange");
            drawer.Field("randomPitchRange");
            DrawClipsSection(new List<string> {"Clunk",}, 1, 10);

            drawer.EndProperty();
            return true;
        }
    }
}

#endif
