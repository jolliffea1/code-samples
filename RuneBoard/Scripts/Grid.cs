using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Grid : MonoBehaviour
{
    private Camera mainCamera;
    private Renderer myRenderer;

    [SerializeField] TextMeshProUGUI labelText;

    public int x;
    public int z;
    public List<GameObject> occupants;
    public Vector3 singleOccupantLocation;
    public List<Vector3> multiOccupantLocations;

    public GameObject spaceMarker;
    public Vector3 spaceMarkerLocation;
    public string terrain = Constants.TERRAIN_NORMAL;

    private Ray ray;
    private RaycastHit rayHit;

    private Ray terrainRay;
    private RaycastHit terrainRayHit;

    private Ray hoverRay;
    private RaycastHit hoverRayHit;

    private Color baseColor;

    private bool clicked;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        myRenderer = GetComponent<Renderer>();
        baseColor = myRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        hoverRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(hoverRay, out hoverRayHit, 1000f) && !clicked)
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (hoverRayHit.transform == transform)
            {
                HoverSpace();
            }
            else
            {
                DeselectSpace();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out rayHit, 1000f))
            {
                if (rayHit.transform == transform)
                {
                    HighlightSpace();
                }
                else
                {
                    DeselectSpace();
                    clicked = false;
                }
            }
            else
            {
                DeselectSpace();
                clicked = false;
            }
        }

        else if (Input.GetMouseButtonDown(1))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            terrainRay = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(terrainRay, out terrainRayHit, 1000f))
            {
                if (terrainRayHit.transform == transform)
                {
                    // if a unit is selected, turn it into a terrain token
                    if (GameManager.clickedUnit != null)
                    {
                        // delete existing marker (if it exists)
                        Destroy(spaceMarker);

                        // set the new marker
                        spaceMarker = GameManager.clickedUnit;

                        // remove from the previous space
                        Grid markerPreviousSpace = GameManager.clickedUnit.transform.parent.gameObject.GetComponent<Grid>();
                        markerPreviousSpace.occupants.Remove(spaceMarker);
                        markerPreviousSpace.distributeOccupants();

                        // move to this space
                        spaceMarker.transform.SetParent(this.gameObject.transform);
                        spaceMarker.transform.position = spaceMarkerLocation;
                        spaceMarker.GetComponent<UnitDisplay>().setAsMarker();


                        distributeOccupants();

                        GameManager.clickedSpace = null;
                    }

                    // otherwise, change the color of the space
                    else
                    {
                        if (Constants.TERRAIN_NORMAL == terrain)
                        {
                            terrain = Constants.TERRAIN_DIFFICULT;
                            setColorByTerrain();
                        }
                        else if (Constants.TERRAIN_DIFFICULT == terrain)
                        {
                            terrain = Constants.TERRAIN_PERMANENT;
                            setColorByTerrain();
                        }
                        else if (Constants.TERRAIN_PERMANENT == terrain)
                        {
                            terrain = Constants.TERRAIN_NORMAL;
                            setColorByTerrain();
                        }
                    }

                }
            }
        }
    }

    public void initLabel(int boardSize)
    {
        string column = "";
        switch (this.x + 1)
        {
            case 2:
                column = "B";
                break;
            case 3:
                column = "C";
                break;
            case 4:
                column = "D";
                break;
            case 5:
                column = "E";
                break;
            case 6:
                column = "F";
                break;
            default:
                column = "A";
                break;
        }

        labelText.text = column + (boardSize - this.z);
    }

    public void setSpaceMarker(Sprite sprite, GameObject baseObject)
    {
        spaceMarker = Instantiate(baseObject);
        spaceMarker.GetComponent<UnitDisplay>().setSprite(sprite);
        spaceMarker.transform.SetParent(this.gameObject.transform);
        spaceMarker.transform.position = spaceMarkerLocation;
        spaceMarker.GetComponent<UnitDisplay>().setAsMarker();
    }

    private void HighlightSpace()
    {
        if (clicked)
        {
            DeselectSpace();
        }
        else
        {
            myRenderer.material.color = Color.yellow;
            clicked = true;
            GameManager.clickedSpace = this.gameObject;
        }
    }

    private void HoverSpace()
    {
        if (Constants.TERRAIN_NORMAL == terrain)
        {
            myRenderer.material.color = Color.gray;
        }
    }

    private void DeselectSpace()
    {
        myRenderer.material.color = baseColor;
        if (GameManager.clickedSpace == this.gameObject)
        {
            GameManager.clickedSpace = null;
        }
        clicked = false;
    }

    public void distributeOccupants()
    {
        for (int index = 0; index < occupants.Count; index++)
        {
            Vector3 newLocation = singleOccupantLocation;
            occupants[index].transform.SetParent(this.gameObject.transform);
            if (occupants.Count > 1 && occupants.Count <= multiOccupantLocations.Count)
            {
                newLocation = multiOccupantLocations[index];
            }

            Vector3 boxSize = occupants[index].GetComponent<Collider>().bounds.size;

            occupants[index].transform.position = new Vector3(newLocation.x, newLocation.y, newLocation.z);
        }
    }

    public void newRoundStart()
    {
        if (occupants.Count > 0)
        {
            foreach (GameObject unit in occupants)
            {
                unit.GetComponent<UnitDisplay>().rollStamina();
            }
        }
    }

    public void setColorByTerrain()
    {
        if (Constants.TERRAIN_NORMAL == terrain)
        {
            baseColor = Color.white;
        }
        else if (Constants.TERRAIN_DIFFICULT == terrain)
        {
            baseColor = Color.red;
        }
        else if (Constants.TERRAIN_PERMANENT == terrain)
        {
            baseColor = Color.black;
        }

        GetComponent<Renderer>().material.color = baseColor;
    }
}
