using FMODUnity;

public class StartGameSound : StudioEventEmitter
{
    public static StartGameSound Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
