using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    private void Start()
    {
        GameManager.instance.Message += UpdateMessage;
    }

    public void UpdateMessage(int currentTurnStep)
    {
        TextMeshProUGUI myText = GetComponent<TextMeshProUGUI>();
        myText.text = Constants.ALL_TURN_STEPS[currentTurnStep];
    }
}
