using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class JuiceSandbox: MonoBehaviour {


    public Transform myTransform;
    public GameObject juiceTarget;
  
    public float lifeTime = 1f;

    protected virtual void Awake()
    {
        myTransform = juiceTarget.transform;
        myTransform.SetParent(juiceTarget.transform);
    }

    protected virtual void Start()
    {
        Death();
    }

    protected virtual void Death()
    {
        Destroy(this, lifeTime);
    }
}

public class BasicSound: JuiceSandbox{
    public AudioClip awakeSound;
    public AudioClip deathSound;

    BasicSound(GameObject _juiceTarget, AudioClip _awakeSound, AudioClip _deathSound = null, float _lifeTime = 0)
    {
        juiceTarget = _juiceTarget;
        awakeSound = _awakeSound;
        deathSound = _deathSound;
        lifeTime = _lifeTime;
    }

	protected override void Awake()
	{
        base.Awake();
        Services.SoundManager.GenerateSourceAndPlay(awakeSound, 1f);
        lifeTime = awakeSound.length;
	}

	protected override void Death()
	{
        Services.SoundManager.GenerateSourceAndPlay(deathSound, 1f);
        base.Death();
	}

}

public class Orbiter : JuiceSandbox {
    public float speed = 100f;
    public float radius = 5;
    public Vector3 centerPoint;
    float currentAngle = 0f;
    public float noiseMagnitude = 1;
    public float noiseSpeed = 1;

    Orbiter(GameObject _juiceTarget, float _speed = 100f, float _radius = 5f, float _noiseMagnitude = 1f, float _noiseSpeed = 1f)
    {
        juiceTarget = _juiceTarget;
        speed = _speed;
        radius = _radius;
        noiseMagnitude = _noiseMagnitude;
        noiseSpeed = _noiseSpeed;
    }

    protected override void Start()
    {
        base.Start();
        centerPoint = transform.parent.position;
    }

    // Update is called once per frame
    void Update()
    {
        currentAngle += speed * Time.deltaTime; //represents movement through angle from origin
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        Vector3 noiseOffset = (noiseMagnitude * new Vector3(Mathf.PerlinNoise(100, Time.time * noiseSpeed),
            Mathf.PerlinNoise(200, Time.time * noiseSpeed),
            Mathf.PerlinNoise(300, Time.time * noiseSpeed)))
            - (noiseMagnitude / 2 * Vector3.one);
        transform.position = centerPoint + new Vector3(Mathf.Cos(angleInRadians), 0, Mathf.Sin(angleInRadians)) + noiseOffset;
    }
}

public class CardBurning : JuiceSandbox
{

}

public class CardAndChipGlow : JuiceSandbox
{

}

public class CardWhirlwind : JuiceSandbox
{

}


public class PlayerPoof: JuiceSandbox{
    
}

