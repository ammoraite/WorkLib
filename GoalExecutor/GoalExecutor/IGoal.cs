namespace AmmoraiteLib;
public interface IGoal
{
    Delegate Action { get; set; }
    object[] Objectparametr { get; set; }
    public int Priority { get; set; }

}

