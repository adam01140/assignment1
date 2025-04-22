using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public string name;
    public int waves;
    public List<Spawn> spawns;

    public override string ToString()
    {
        return $"Level: {name} (Waves: {waves}, Spawns: {spawns.Count})";
    }
}

[Serializable]
public class Spawn
{
    public string enemy;
    public string count;
    public string hp;
    public string speed;
    public string damage;
    public string delay = "2";
    public List<int> sequence;
    public string location = "random";

    public override string ToString()
    {
        return $"Spawn: {enemy} (Count: {count}, HP: {hp}, Location: {location})";
    }

    // Helper method to get default sequence if none is specified
    public List<int> GetSequence()
    {
        if (sequence == null || sequence.Count == 0)
        {
            return new List<int> { 1 };
        }
        return sequence;
    }
} 