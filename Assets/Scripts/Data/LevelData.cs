using System;
[Serializable]
public class LevelData
{
    public int levelId;
    public string[] birds;
    public BlockData[] blocks;
    public PigData[] pigs;
}

[Serializable]
public class BlockData
{
    public string type;
    public float x;
    public float y;
}

[Serializable]
public class PigData
{
    public float x;
    public float y;
}