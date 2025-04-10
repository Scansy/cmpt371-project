// filepath: c:\Users\andre\OneDrive\Desktop\Documents\2-SFU\SFU-Work\Spring2025\CMPT371\Project\cmpt371-project\Assets\Scripts\Shared\SerializableVector2.cs
using System;

[Serializable]
public struct SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator SerializableVector2(UnityEngine.Vector2 vector)
    {
        return new SerializableVector2(vector.x, vector.y);
    }

    public static implicit operator UnityEngine.Vector2(SerializableVector2 vector)
    {
        return new UnityEngine.Vector2(vector.x, vector.y);
    }
}