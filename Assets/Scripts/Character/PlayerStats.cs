using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    public GameObject player;
    //public GameObject gameOverPanel;

    private scr_CharacterController characterController;

    //public Image INFWarningImage;

    //public HealthBar healthBar;
    //public InfectionBar infectionBar;

    public float playerHealth = 100f;
    public float playerInfection = 0f;

    public float healthDecreaseRate = 1f;
    public float infectionRate = 0.2f;

    public float playerInfectionThreshold = 100f;

    public void Initialize(scr_CharacterController CharacterController)
    {
        characterController = CharacterController;
    }

    private void Start()
    {
        //healthBar.SetHealth(playerHealth);
        //infectionBar.SetInfection(playerInfection);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
        //gameOverPanel.SetActive(false);
    }

    private void Update()
    {
        if (playerInfection >= playerInfectionThreshold)
        {
            PlayerHealthDecrease();
        }

        //healthBar.SetHealth(playerHealth);
        //infectionBar.SetInfection(playerInfection);

        isPlayerDead();

        //Debug.Log("HP = " + playerHealth + " = " + Mathf.FloorToInt(playerHealth).ToString() + "\nPlayer Infection = " + playerInfection + " = " + Mathf.FloorToInt(playerInfection).ToString());

        InfectionIncrease();
    }

    private void isPlayerDead()
    {
        if (playerHealth <= 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
            //gameOverPanel.SetActive(true);

            characterController.GetKilled();
        }
    }

    private void InfectionIncrease()
    {
        if (playerInfection >= playerInfectionThreshold)
        {
            playerInfection = playerInfectionThreshold;
            //INFWarningImage.enabled = true;
        }

        if (playerInfection < playerInfectionThreshold)
        {
            playerInfection += Time.deltaTime * infectionRate;
            //INFText.text = Mathf.FloorToInt(playerInfection).ToString();
            //INFWarningImage.enabled = false;
        }
    }

    private void PlayerHealthDecrease()
    {
        if (playerHealth > 0)
        {
            playerHealth -= Time.deltaTime * healthDecreaseRate;
        }
        if (playerHealth < 0)
        {
            playerHealth = 0;
        }
    }

    public void Health()
    {
        playerHealth -= 5;
    }

    public void Infection()
    {
        playerInfection -= 5;
    }
}
