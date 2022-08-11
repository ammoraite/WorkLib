namespace AmmoraiteLib;
public interface IGoal
{
    Task task { get; set; }
    object[] Objectparametr { get; set; }
    public int Priority { get; set; }

}

