using UnityEngine;
using UnityEngine.UI;

public class CaptainSceneBuffCard : MonoBehaviour
{
    [SerializeField]
    private EIslandBuff islandBuff = default;
    [SerializeField]
    private Button button = null;
    [SerializeField]
    private GameObject toHide = null;
    [SerializeField]
    private string param = "";
    [SerializeField]
    private Text orderName = null;
    [SerializeField]
    private Text desc = null;
    [SerializeField]
    private Text effect = null;

    private void Start()
    {
        button.onClick.AddListener(ChooseBuff);
        var locMan = LocalizationManager.Instance;
        var name = islandBuff.ToString().Replace(" ", "");
        orderName.text = locMan.GetText(name + "Title");
        if (string.IsNullOrEmpty(param))
        {
            desc.text = locMan.GetText(name + "Desc");
            effect.text = locMan.GetText(name + "Effect");
        }
        else
        {
            desc.text = locMan.GetText(name + "Desc", param.ToString());
            effect.text = locMan.GetText(name + "Effect", param.ToString());
        }
    }

    private void ChooseBuff()
    {
        CreateAdmiralMenu.Instance.SetChosenOrder(islandBuff);
        toHide.SetActive(false);
    }
}
