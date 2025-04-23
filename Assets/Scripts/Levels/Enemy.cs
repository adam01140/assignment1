using System;
using UnityEngine;

[Serializable]
public class Enemy
{
    public string name;
    public int sprite;
    public int hp;
    public int speed;
    public int damage;

    public override string ToString()
    {
        return $"Enemy: {name} (Sprite: {sprite}, HP: {hp}, Speed: {speed}, Damage: {damage})";
    }
} 
