using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialCanvasManager : MonoBehaviour
{
    private UnityAction<string> listener;

    float timer;
    [SerializeField] TextMeshProUGUI tutText;
    [SerializeField] TextMeshProUGUI UIText;
    bool displaying;
    float minDisplayTime = 1;
    bool tutorialConditionMet = false;
    bool secondaryConditionMet = false;
    enum TutorialCondition : int { move, jump, dash, hit, ui, groundPound, end}
    TutorialCondition currTutorialCondition;
    [SerializeField] SpawnManager spawnManager;
    GameObject tether;
    [SerializeField] PlayerMover pm;

    private UnityAction<string> pauseListener;

    private void Awake()
    {
        pauseListener = new UnityAction<string>(ToggleTutorialText);
    }

    private void OnEnable()
    {
        EventManager.StartListening("pause", pauseListener);
    }

    private void OnDisable()
    {
        EventManager.StopListening("pause", pauseListener);
    }
    //EventManager.TriggerEvent("newWave", (waveCount+1).ToString());




    // Start is called before the first frame update
    void Start()
    {
        displaying = true;
        tutText.gameObject.SetActive(true);
        currTutorialCondition = TutorialCondition.move;
        ActivateCanvas("Use WASD to move");
    }

    // Update is called once per frame
    void Update()
    {
        if (displaying)
        {
            timer += Time.deltaTime;
        }

        switch (currTutorialCondition)
        {
            case TutorialCondition.move:
                if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")){   // should change to GetButton and configure buttons
                    tutorialConditionMet = true;
                }
                break;
            case TutorialCondition.jump:
                if (Input.GetButton("Jump"))
                { 
                    tutorialConditionMet = true;
                }
                break;
            case TutorialCondition.dash:
                if (pm.currentState.GetType() == typeof(DashState))
                {
                    tutorialConditionMet = true;
                }
                break;
            case TutorialCondition.hit:
                // if enemy is dead done
                if (spawnManager.CurrNumOfEnemiesAlive() == 0)
                {
                    tutorialConditionMet = true;
                }
                else if (timer > 30)
                {
                    ActivateCanvas("To hit an enemy, hold down LEFT MOUSE to charge, aim at enemy, then release when wheel is full and green."
                    + "\n Make sure you aren't too far away from the enemy"
                    + "\n For practice, try walking directly in front of the enemy.");
                    // display help information
                }
                break;
            case TutorialCondition.ui:
                // after all other steps are completed
                if (timer < 3) 
                {
                    ActivateCanvas("Good job!");
                }
                else if (timer < 10)  
                {
                    tutText.gameObject.SetActive(false);
                    ActivateCanvasWithUIText("The green bar below represents your health.");
                }
                else if (timer < 17)
                {
                    ActivateCanvasWithUIText("The blue bar below represents your stamina");
                }
                else
                {
                    tutorialConditionMet = true;
                }
                break;
            case TutorialCondition.groundPound:
                if (pm.currentState.GetType() == typeof(GroundPoundState))
                {
                    secondaryConditionMet = true;
                }
                else if (spawnManager.CurrNumOfEnemiesAlive() == 0 && !secondaryConditionMet)
                {
                    Debug.Log("Should spawn a new enemy");
                    spawnManager.SpawnEnemy(SpawnManager.EnemyType.Cylinder, tether); // use same tether as before
                }

                else if (spawnManager.CurrNumOfEnemiesAlive() == 0 && secondaryConditionMet)
                {
                    tutorialConditionMet = true;
                }
                break;
            default:
                break;
        }

        if (tutorialConditionMet && timer > minDisplayTime)
        {
            ProgressTutorial();
        }

    }

    void ProgressTutorial()
    {
        timer = 0f;
        currTutorialCondition++;
        tutText.gameObject.SetActive(false);
        UIText.gameObject.SetActive(false);
        displaying = false;
        tutorialConditionMet = false;
        switch (currTutorialCondition)
        {
            case TutorialCondition.move:
                ActivateCanvas("Use WASD to move");
                break;
            case TutorialCondition.jump:
                ActivateCanvas("Press SPACEBAR to jump");
                break;
            case TutorialCondition.dash:
                ActivateCanvas("Press SHIFT while on the ground to dash");
                break;
            case TutorialCondition.hit:
                //spawn enemy 
                tether = spawnManager.FindSpawner();
                spawnManager.SpawnEnemy(SpawnManager.EnemyType.Cylinder, tether);
                ActivateCanvas("To hit an enemy, hold down LEFT MOUSE to charge, aim at enemy, then release when wheel is full and green."
                    + "\n Make sure you aren't too far away from the enemy");
                break;
            case TutorialCondition.ui:
                break; // text is handled in switch in update()
            case TutorialCondition.groundPound:
                spawnManager.SpawnEnemy(SpawnManager.EnemyType.Cylinder, tether); // use same tether as before
                ActivateCanvas("Stamina is used to DASH and GROUND POUND.\n"
                        + "Use the GROUND POUND to kill that enemy by pressing LEFT CTRL while in the air close to it");
                break;
            case TutorialCondition.end:
                ActivateCanvas("Great job! \n"
                    + "This concludes the tutorial. To get back to the main menu, press ESC and then MAIN MENU");
                break;
            default:
                break;
        }
    }

    void ActivateCanvas(string text)
    {
        tutText.text = text;
        tutText.gameObject.SetActive(true);
        displaying = true;
    }

    void ActivateCanvasWithUIText(string text)
    {
        UIText.text = text;
        UIText.gameObject.SetActive(true);
        displaying = true;
        
    }

    void ToggleTutorialText(string none)
    {
        if (tutText.IsActive()) tutText.gameObject.SetActive(false);
        else if (UIText.IsActive()) UIText.gameObject.SetActive(false);
        else if (!tutText.IsActive()) tutText.gameObject.SetActive(true);
        else if (!UIText.IsActive()) UIText.gameObject.SetActive(true);
    }
}
