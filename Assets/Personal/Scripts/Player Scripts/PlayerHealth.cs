using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    Image healthBar;
    Image damageImage;
    Image overshieldBar;
    Text gameOverText;
    [SerializeField] Canvas gameOverCanvas;
    float flashSpeed = 5f;
    Color flashColor = new Color(1f, 0f, 0f, 0.1f);
    private int maxHealth;
    private float health;
    private float overshield;
    private bool damaged;
    private ImpactReceiver impactReceiver;
    PlayerMover playerMover;
    PlayerValues playerValues;
    AudioSource audioSource;
    AudioClip hitSound;
    private float overshieldMax;
    private UnityAction<string> pauseListener;

    private void Awake()
    {
        pauseListener = new UnityAction<string>(ToggleGameOverCanvas);
    }

    private void OnEnable()
    {
        EventManager.StartListening("pause", pauseListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening("pause", pauseListener);
    }

    void Start()
    {
        impactReceiver = gameObject.GetComponent<ImpactReceiver>();
        playerMover = gameObject.GetComponent<PlayerMover>();
        audioSource = gameObject.GetComponent<AudioSource>();
        playerValues = playerMover.playerValues;

        healthBar = playerValues.healthValues.HealthBar;
        damageImage = playerValues.healthValues.DamageImage;
        gameOverCanvas = playerValues.healthValues.GameOverCanvas;
        flashSpeed = playerValues.healthValues.FlashSpeed;
        flashColor = playerValues.healthValues.FlashColor;
        maxHealth = playerValues.healthValues.MaxHealth;
        hitSound = playerValues.healthValues.HitSound;
        overshieldBar = playerValues.healthValues.OvershieldBar;
        overshieldMax = 0;

        health = maxHealth;
        healthBar.type = Image.Type.Filled;
        healthBar.fillMethod = Image.FillMethod.Horizontal;
        //healthBar.fillAmount = 1f;
        overshieldBar.type = Image.Type.Filled;
        overshieldBar.fillMethod = Image.FillMethod.Horizontal;

        //added for testing purposes
        healthBar.fillAmount = health / maxHealth;

        overshieldBar.fillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(overshield > 0)
        {
            overshield -= 2*Time.deltaTime;
            overshieldBar.fillAmount = overshield / overshieldMax;
        }
        if (damaged)
        {
            damageImage.color = flashColor;
        }
        else
        {
            damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
        }
        damaged = false;
    }

    public void TakeDamage(int damage, Vector3 direction, float force)
    {
        if (playerMover.isVulnerable())
        {
            if (overshield > damage)
            {
                overshield -= damage;
                overshieldBar.fillAmount = overshield / overshieldMax;
                return;
            }
            if (overshield > 0 && overshield < damage)
            {
                damage -= (int)overshield;
                overshield = 0;
                overshieldBar.fillAmount = 0f;
            }
            health -= damage;
            damaged = true;
            healthBar.fillAmount = health / maxHealth;
            audioSource.PlayOneShot(hitSound, Mathf.Clamp(damage / 4f, 0f, 1f));
            if (health <= 0)
            {
                GameOver();
            }
            impactReceiver.AddImpact(direction.normalized, force);

            //moveDamageIndicator(direction);
        }
    }

    public void GainOvershield (float overshieldgain)
    {
        overshieldMax = overshieldgain;
        overshield = overshieldgain;
        overshieldBar.fillAmount = 1f;
    }

    public void GainHealth(int healthgain)
    {
        if ((healthgain + health) <= maxHealth && health > 0)
        {
            health += healthgain;
        }
        else if ((health + healthgain) > maxHealth)
        {
            health = maxHealth;
        }
        healthBar.fillAmount = health / maxHealth;
    }

    private void GameOver()
    {
        gameOverCanvas.gameObject.SetActive(true);
        playerMover.Die();
        playerMover.MouseLook.SetCursorLock(false);
    }

    private void ToggleGameOverCanvas(string none)
    {
        if (gameOverCanvas.gameObject.activeSelf) gameOverCanvas.gameObject.SetActive(false);
        else if (health <= 0) gameOverCanvas.gameObject.SetActive(true);
    }

    //to be implemented
    private void MoveDamageIndicator(Vector3 direction)
    {

    }
}
