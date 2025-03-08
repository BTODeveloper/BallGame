using Match3.Gameplay;

public class BallRuntimeDataWrapper
{
    public BallRuntimeDataWrapper(BallGroupConfiguration ballGroupConfiguration, BallAppearanceData appearanceData , GameplayManager gameplayManager )
    {
        this.BallGroupConfiguration = ballGroupConfiguration;
        this.BallAppearanceData = appearanceData;
        this.GameplayManager = gameplayManager;
    }
    
    public BallGroupConfiguration BallGroupConfiguration { get; private set; }
    public BallAppearanceData BallAppearanceData{ get; private set; }
    public GameplayManager GameplayManager{ get; private set; }
}