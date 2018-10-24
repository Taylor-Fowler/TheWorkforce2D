public class User
{
    private static int ID_Counter;

    private static int GenerateID => ID_Counter++;

    public int ID { get; protected set; }
}