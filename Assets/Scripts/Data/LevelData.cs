using System;
[Serializable]
public class LevelData
{
    public int levelId;
    public ScoreTargets scoreTargets;
    public string[] birds;
    public BlockData[] blocks;
    public PigData[] pigs;
}

[Serializable]
public class ScoreTargets
{
    public int oneStar;
    public int twoStars;
    public int threeStars;
}

[Serializable]
public class BlockData
{
    public string type;
    public float x;
    public float y;
    public float rotation;
    public float width;
    public float height;
    public int score;
    public float health;
}

[Serializable]
public class PigData
{
    public float x;
    public float y;
    public float health;
    public int killScore;
    public int damageScore;
    public PigType type = PigType.Brkonja;
}
