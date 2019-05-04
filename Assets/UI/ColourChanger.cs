using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MaskableGraphic))]
public class ColourChanger : MonoBehaviour
{
    [SerializeField] private MaskableGraphic _graphic;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color[] _enterColors;

    private void Awake()
    {
        if(_graphic == null)
        {
            _graphic = GetComponent<MaskableGraphic>();
        }
        _defaultColor = _graphic.color;
    }

    public void Change(int index)
    {
        if(index >= _enterColors.Length)
        {
            Debug.LogError("Fatal Error in ColourChanger, index is out of bounds");
            return;
        }
        _graphic.color = _enterColors[index];
    }

    public void Reset()
    {
        _graphic.color = _defaultColor;
    }
}
