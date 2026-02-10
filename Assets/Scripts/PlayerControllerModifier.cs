[System.Serializable]
public struct StatsModifiers
{
    public float speedMult;
    public float accelerationMult;
    public float deaccelerationMult;
    public float gravityMult;
    public float jumpForceMult;
    public int extraJumps;
    public static StatsModifiers Null => new StatsModifiers
    {
        speedMult = 1.0f,
        accelerationMult = 1.0f,
        deaccelerationMult = 1.0f,
        gravityMult = 1.0f,
        jumpForceMult = 1.0f,
        extraJumps = 0,
    };
}

public interface IPlayerStatModifier
{
    /// <summary>
    /// Sets the stats modifer for the player
    /// </summary>
    /// <param name="modifierID">Unique ID of the class/object that is modifying the stat to help keep track for reseting</param>
    /// <param name="modifiers"></param>
    public void ModifyStats(int modifierID, StatsModifiers modifiers);

    /// <summary>
    /// Resets the player modifier to default values
    /// </summary>
    /// <param name="modifierID"></param>
    public void ResetStats(int modifierID);
}
public partial class PlayerController : IPlayerStatModifier
{
    StatsModifiers _currentModifiers = StatsModifiers.Null;
    int _currentModifierID = 0;

    public void ModifyStats(int modifierID, StatsModifiers modifiers)
    {
        _currentModifierID = modifierID;
        _currentModifiers = modifiers;
    }

    public void ResetStats(int modifierID)
    {
        if (modifierID == _currentModifierID)
        {
            _currentModifiers = StatsModifiers.Null;
            _currentModifierID = 0;
        }
    }
}
