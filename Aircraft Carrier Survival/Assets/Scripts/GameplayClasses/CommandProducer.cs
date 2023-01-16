
public class CommandProducer : ResourceProducer
{
    protected override void OnProgressed()
    {
        CurrentAmount += ProduceCount;
    }
}
