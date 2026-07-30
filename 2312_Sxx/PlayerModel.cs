using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public string Name { get; private set; }
    public int Level { get; private set; }
    public PlayerModel(string name,int StartLevel) 
    {
        Name = name;
        Level = StartLevel;
    }
    public void LevelUp() 
    {
        Level++;
    }
}
