using System;
using UnityEngine;
using TMPro;

////////////////////////////////////////////////////////////////////////
// GENERAL PURPOSE TASKS
////////////////////////////////////////////////////////////////////////

// Simple action task
public class ActionTask : Task {

    public Action _action { get; private set; }

    public ActionTask(Action action)
    {
        _action = action;
    }

    protected override void Init()
    {
        _action();
        SetStatus(TaskStatus.Success);
    }

}

public class OnGoingTask : Task
{
	private static readonly System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1);
	private readonly double _duration; //how long does this wait for
	public Action _action { get; private set; }

	private double _startTime; //when did we start waiting

	private static double GetTimestamp()
    {
        return (System.DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

	public OnGoingTask(Action action, double duration)
    {
        this._duration = duration;
		this._action = action;
    }

    protected override void Init()
    {
		_action();
        _startTime = GetTimestamp();
    }

    internal override void Update()
    {
        var now = GetTimestamp(); //use var for a) less typing, b) if it changes from float, to int, to double, etc.
        var durationElapsed = (now - _startTime) > _duration;

		_action();

        if (durationElapsed)
        {
            SetStatus(TaskStatus.Success);
        }
    }

}

public class WaitTask : Task
{
    // Get the timestamp in floating point milliseconds from the Unix epoch   
    private static readonly System.DateTime UnixEpoch = new System.DateTime(1970, 1, 1);

    private static double GetTimestamp()
    {
        return (System.DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
    }

    private readonly double _duration; //how long does this wait for
    private double _startTime; //when did we start waiting

    public WaitTask(double duration)
    {
        this._duration = duration;
    }

    protected override void Init()
    {
        _startTime = GetTimestamp();
    }

    internal override void Update()
    {
        var now = GetTimestamp(); //use var for a) less typing, b) if it changes from float, to int, to double, etc.
        var durationElapsed = (now - _startTime) > _duration;

        if (durationElapsed)
        {
            SetStatus(TaskStatus.Success);
        }
    }
}

// A base class for tasks that track time. Use it to make things like
// Wait, ScaleUpOverTime, etc. tasks
public abstract class TimedTask : Task
{
    public float Duration { get; private set; }
    public float StartTime { get; private set; }

    protected TimedTask(float duration)
    {
        Debug.Assert(duration > 0, "Cannot create a timed task with duration less than 0");
        Duration = duration;
    }

    protected override void Init()
    {
        StartTime = Time.time;
    }

    internal override void Update()
    {
        var now = Time.time;
        var elapsed = now - StartTime;
        var t = Mathf.Clamp01(elapsed / Duration);
        OnTick(t);
        if (t >= 1)
        {
            OnElapsed();
        }
    }

    // t is the normalized time for the task. E.g. if half the task's duration has elapsed then t == 0.5
    // This is where subclasses will do most of their work
    protected virtual void OnTick(float t) {}

    // Default to being successful if we get to the end of the duration
    protected virtual void OnElapsed()
    {
        SetStatus(TaskStatus.Success);
    }

}


// A VERY simple wait task
public class Wait : TimedTask
{
    public Wait(float duration) : base(duration) {}
}


////////////////////////////////////////////////////////////////////////
// GAME OBJECT TASKS
////////////////////////////////////////////////////////////////////////

// Base classes for tasks that operate on a game object.
// Since C# doesn't allow multiple inheritance we'll make two versions - one timed and one untimed
public abstract class GOTask : Task
{
    protected readonly GameObject gameObject;

    protected GOTask(GameObject gameObject)
    {
        this.gameObject = gameObject;
    }
}


public abstract class TimedGOTask : TimedTask
{
    protected readonly GameObject gameObject;

    protected TimedGOTask(GameObject gameObject, float duration) : base(duration)
    {
        this.gameObject = gameObject;
    }
}

// A task to teleport a gameobject
public class SetPos : GOTask
{
    private readonly Vector3 _pos;

    public SetPos(GameObject gameObject, Vector3 pos) : base(gameObject)
    {
        _pos = pos;
    }

    protected override void Init()
    {
        gameObject.transform.position = _pos;
        SetStatus(TaskStatus.Success);
    }
}


// A task to lerp a gameobject's position
public class LerpPos : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public LerpPos(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.position = Vector3.Lerp(Start, End, t);
    }
}

public class LerpRotation : TimedGOTask
{
    private Quaternion Start;
    private Quaternion End;

    public LerpRotation(GameObject gameObject, Quaternion _start, Quaternion _end, float duration) : base(gameObject, duration)
    {
        Start = _start;
        End = _end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.rotation = Quaternion.Lerp(Start, End, t);
    }
}


// A task to lerp a gameobject's scale
public class Scale : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public Scale(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        gameObject.transform.localScale = Vector3.Lerp(Start, End, t);
    }
}

public class LerpTextMeshProColor : TimedTask
{
    private Color startColor;
    private Color endColor;
    private TextMeshPro textMesh;
    private Easing.FunctionType easingType;

    public LerpTextMeshProColor(TextMeshPro _textMesh, Color _start, Color _end, Easing.FunctionType _easingType, float duration) : base(duration)
    {
        startColor = _start;
        endColor = _end;
        textMesh = _textMesh;
        easingType = _easingType;
    }

    protected override void OnTick(float t)
    {
        Easing.Function easingFunction = Easing.GetFunctionWithTypeEnum(easingType);
        textMesh.color = Color.Lerp(startColor, endColor, easingFunction(t));
    }
}

public class LerpBurnProgress : TimedTask
{
    private Renderer Renderer;
    private float Start;
    private float End;
    private Easing.FunctionType EasingType;

    public LerpBurnProgress(Renderer _renderer, float _start, float _end, Easing.FunctionType _easingType, float duration) : base(duration)
    {
        Renderer = _renderer;
        Start = _start;
        End = _end;
        EasingType = _easingType;
    }

    protected override void OnTick(float t)
    {
        Easing.Function easingFunction = Easing.GetFunctionWithTypeEnum(EasingType);
        float _emission = Mathf.Lerp(Start, End, easingFunction(t));
        Renderer.material.SetFloat("_Progress", _emission);
    }
}

public class PostBlinds : Task
{
    private PokerPlayerRedux Player;
    private int BetAmount;
    private bool isTip;

    public PostBlinds(PokerPlayerRedux _player, int _betAmount, bool _isTip)
    {
        Player = _player;
        BetAmount = _betAmount;
        isTip = _isTip;
    }

    protected override void Init()
    {
        Player.Bet(BetAmount, isTip);
        Player.currentBet = BetAmount;
        if (Player.chipCount == 0)
        {
            Player.playerIsAllIn = true;
            Player.moneyCommitted += BetAmount;
        }
        SetStatus(TaskStatus.Success);
    }
}

public class LerpBetChips : TimedTask
{
    private System.Collections.Generic.List<Chip> Chips;
    private System.Collections.Generic.List<Vector3> Starts;
    private System.Collections.Generic.List<Vector3> Ends;

    public LerpBetChips(System.Collections.Generic.List<Chip> _chips, System.Collections.Generic.List<Vector3> _starts, System.Collections.Generic.List<Vector3> _ends, float duration) : base(duration)
    {
        Chips = _chips;
        Starts = _starts;
        Ends = _ends;
    }

    protected override void Init()
    {
        Services.Dealer.consolidatingChips = true;
        base.Init();
    }

    protected override void OnTick(float t)
    {
        for (int i = 0; i < Chips.Count; i++)
        {
            Chips[i].gameObject.transform.position = Vector3.Lerp(Starts[i], Ends[i], t);
        }
    }

    //protected override void OnSuccess()
    //{
    //    Services.PokerRules.ConsolidateStack(Chips);
    //    for (int i = 0; i < Services.Dealer.chipsInPot.Count; i++)
    //    {
    //        Services.Dealer.chipsInPot[i].gameObject.transform.position = Services.Dealer.chipPositionInPot[i];
    //    }
    //    Services.Dealer.consolidatingChips = false;
    //}
}

public class ConsolidateChips : Task
{
    private System.Collections.Generic.List<Chip> Chips;

    public ConsolidateChips(System.Collections.Generic.List<Chip> _chips)
    {
        Chips = _chips;
    }

    protected override void Init()
    {
        Services.ChipManager.ConsolidateStack(Chips);
        for (int i = 0; i < Services.Dealer.chipsInPot.Count; i++)
        {
            Chips[i].gameObject.transform.position = Services.ChipManager.chipPositionInPot[i];
        }
        Services.Dealer.consolidatingChips = false;
        SetStatus(TaskStatus.Success);
    }
}

public class SetObjectActive : GOTask
{
    GameObject GameObject;
    bool SFX;

    public SetObjectActive(GameObject _gameObject, bool _sfx) : base(_gameObject)
    {
        GameObject = _gameObject;
        SFX = _sfx;
    }

    protected override void Init()
    {
		if (SFX) Services.SoundManager.GenerateSourceAndPlay(Services.SoundManager.poofNoise, 1);
        GameObject.SetActive(true);


        //instantiate juice at transrom. 
        SetStatus(TaskStatus.Success);
    }
}

public class SetGameState : Task
{
    GameState GameState;
    public SetGameState(GameState _gameState)
    {
        GameState = _gameState;
    }

    protected override void Init()
    {
        Table.gameState = GameState;
        SetStatus(TaskStatus.Success);
    }
}

public class PlayPlayerLine : Task
{
    PokerPlayerRedux Player;
    AudioClip Clip;

    public PlayPlayerLine(PokerPlayerRedux _player, AudioClip _clip)
    {
        Player = _player;
        Clip = _clip;
    }

    protected override void Init()
    {
        Services.SoundManager.GetSourceAndPlay(Player.playerAudioSource, Clip);
        SetStatus(TaskStatus.Success);
    }
}

public class TurnOnTutorial : Task
{
    protected override void Init()
    {
        Services.TextManager.inTutorial = true;
        SetStatus(TaskStatus.Success);
    }
}

public class TurnOnChipsFromTutorial : Task
{
    protected override void Init()
    {
        foreach(GameObject o in Services.Dealer.chipsToBring)
        {
            o.SetActive(true);
        }
        SetStatus(TaskStatus.Success);
    }
}

public class InitializeAnimations : Task
{
    protected override void Init()
    {
        Services.AnimationScript.AnimationInit();
        SetStatus(TaskStatus.Success);
    }
}

public class RollCredits : TimedGOTask
{
    public Vector3 Start { get; private set; }
    public Vector3 End { get; private set; }

    public RollCredits(GameObject gameObject, Vector3 start, Vector3 end, float duration) : base(gameObject, duration)
    {
        Start = start;
        End = end;
    }

    protected override void OnTick(float t)
    {
        Services.Dealer.credits.SetActive(true);
        gameObject.transform.position = Vector3.Lerp(Start, End, t);
    }
}



