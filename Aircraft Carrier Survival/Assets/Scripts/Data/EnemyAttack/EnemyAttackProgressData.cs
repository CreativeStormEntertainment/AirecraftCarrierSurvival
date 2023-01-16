using UnityEngine.Playables;

public class EnemyAttackProgressData
{
    public PlayableDirector Animation;
    public bool HasAA;

    public EnemyAttackProgressData(PlayableDirector animation, bool hasAA)
    {
        Animation = animation;
        HasAA = hasAA;
    }
}
