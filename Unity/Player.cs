using UnityEngine;

[System.Serializable]
public class Player
{
    public string player_id;
    public float waste_quants;
    public int rat_count;

    public Player(string id, float waste, int rats)
    {
        player_id = id;
        waste_quants = waste;
        rat_count = rats;
    }

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}