using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitDisplay : MonoBehaviour
{
    public Unit unit;
    public int currentHealth;
    public List<int> staminaRolls;

    [SerializeField] GameObject sprite;
    [SerializeField] TextMeshProUGUI staminaText;
    [SerializeField] TextMeshProUGUI healthText;

    private Camera mainCamera;

    private Ray ray;
    private RaycastHit rayHit;

    private string tokenGroup = "A";
    private Color baseColor = Constants.TOKEN_COLOR_A;
    private bool clicked = false;

    private bool isMarker = false;

    void Awake()
    {
        mainCamera = Camera.main;
        setSprite(unit.sprite);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (currentHealth == 0)
        {
            currentHealth = unit.maxHealth;
        }

        healthText.text = currentHealth.ToString();

        // start the unit with blank dice
        if (staminaRolls.Count == 0)
        {
            staminaRolls = new List<int>();
            for (int i = 0; i < unit.stamina; i++)
            {
                staminaRolls.Add(0);
            }
        }

        setStaminaText();
    }

    private void Update()
    {
        healthText.text = currentHealth.ToString();

        if (Input.GetMouseButtonDown(0))
        {
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //  return;
            //}

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            bool rayDidHit = Physics.Raycast(ray, out rayHit, 1000f);
            if (!rayDidHit || (rayDidHit && rayHit.transform != transform))
            {
                DeselectUnit();
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject() || isMarker)
            {
                return;
            }

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            bool rayDidHit = Physics.Raycast(ray, out rayHit, 1000f);
            if (rayDidHit && rayHit.transform == transform)
            {
                if (tokenGroup == "A")
                {
                    tokenGroup = "B";
                    baseColor = Constants.TOKEN_COLOR_B;
                }
                else if (tokenGroup == "B")
                {
                    tokenGroup = "C";
                    baseColor = Constants.TOKEN_COLOR_C;
                }
                else
                {
                    tokenGroup = "A";
                    baseColor = Constants.TOKEN_COLOR_A;
                }

                changeSpriteColor(baseColor);
            }
        }
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (isMarker)
            {
                Destroy(gameObject);
                return;
            }

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out rayHit, 1000f))
            {
                if (rayHit.transform == transform)
                {
                    HighlightUnit();
                }
                else
                {
                    DeselectUnit();
                }
            }
            else
            {
                DeselectUnit();
            }
        }
    }

    public void changeHealth(int change)
    {
        currentHealth += change;
        if (currentHealth == 0)
        {
            Grid currentSpace = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject.GetComponent<Grid>() : null;
            if (currentSpace != null)
            {
                currentSpace.occupants.Remove(gameObject);
            }
            Destroy(gameObject);
        }
    }

    public void rollStamina()
    {
        System.Random rand = new System.Random();
        staminaRolls = new List<int>();
        for (int i = 0; i < unit.stamina; i++)
        {
            staminaRolls.Add(rand.Next(1, 7));
        }

        setStaminaText();
    }

    public void setStaminaText()
    {
        string rollString = "";
        for (int i = 0; i < staminaRolls.Count; i++)
        {
            rollString += staminaRolls[i].ToString();
        }
        staminaText.text = rollString;
    }

    private void HighlightUnit()
    {
        if (clicked)
        {
            DeselectUnit();
        }
        else
        {
            changeSpriteColor(Color.cyan);
            GameManager.clickedUnit = this.gameObject;
            clicked = true;
        }
    }

    private void DeselectUnit()
    {
        Grid currentSpace = gameObject.transform.parent != null ? gameObject.transform.parent.gameObject.GetComponent<Grid>() : null;
        Grid destination = GameManager.clickedSpace == null ? null : GameManager.clickedSpace.GetComponent<Grid>();

        if (destination != null && isValidMove(currentSpace, destination))
        {
            // move to the destination space
            this.gameObject.transform.position = destination.singleOccupantLocation;
            destination.occupants.Add(this.gameObject);
            destination.distributeOccupants();

            // remove from the previous space
            currentSpace.occupants.Remove(this.gameObject);
            currentSpace.distributeOccupants();
            GameManager.clickedSpace = null;
        }

        changeSpriteColor(baseColor);
        if (GameManager.clickedUnit == this.gameObject)
        {
            GameManager.clickedUnit = null;
        }
        clicked = false;
    }

    private bool isValidMove(Grid currentSpace, Grid destination)
    {
        if (currentSpace == null)
        {
            return true;
        }

        if (destination == null || (destination.x == currentSpace.x && destination.z == currentSpace.z))
        {
            return false;
        }

        if (GameManager.clickedUnit != this.gameObject)
        {
            return false;
        }

        if (destination.occupants.Count >= 2)
        {
            return false;
        }

        return true;
    }

    public Color getBaseColor()
    {
        return baseColor;
    }

    public void setBaseColor(Color color)
    {
        baseColor = color;
        changeSpriteColor(baseColor);
    } 

    public void changeSpriteColor(Color color)
    {
        sprite.GetComponent<SpriteRenderer>().color = color;
    }

    public void setSprite(Sprite newSprite)
    {
        sprite.GetComponent<SpriteRenderer>().sprite = newSprite;
    }

    public Sprite getSprite()
    {
        return sprite.GetComponent<SpriteRenderer>().sprite;
    }

    public void setAsMarker()
    {
        isMarker = true;
        DeselectUnit();
        setBaseColor(Color.green);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform currentItem = gameObject.transform.GetChild(i);
            if (currentItem.name == "Canvas")
            {
                currentItem.gameObject.SetActive(false);
                break;
            }
        }
        float proportion = 0.6f;
        Vector3 localScale = gameObject.transform.localScale;
        gameObject.transform.localScale = new Vector3(localScale.x * proportion, localScale.y * proportion, localScale.z * proportion);
    }
}
