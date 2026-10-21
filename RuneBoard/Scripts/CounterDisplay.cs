using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CounterDisplay : MonoBehaviour
{
    private int index;
    private Counter counter;

    [SerializeField] TMP_InputField nameField;
    [SerializeField] TextMeshProUGUI valueText;

    void Start()
    {
        valueText.text = counter.value.ToString();
        nameField.text = counter.name;
    }

    public int getIndex()
    {
        return index;
    }
    public void setIndex(int value)
    {
        index = value;
    }

    public void setCounter(Counter value)
    {
        counter = value;
    }

    public Counter getCounter()
    {
        return counter;
    }

    public void changeName(string value)
    {
        counter.name = value;
    }

    public void changeValue(int modifier)
    {
        counter.value += modifier;
        valueText.text = counter.value.ToString();
    }

    public void deleteCounter()
    {
        GameManager.deleteCounter(index);
    }
}
