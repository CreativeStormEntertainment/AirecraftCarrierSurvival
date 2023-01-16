using System.Collections.Generic;
using UnityEngine;

public class BindHighlightElement : MonoBehaviour
{
   [SerializeField] private List<GameObject> _elements;

   public void ActivateHighlight()
   {
      foreach (var element in _elements)
      {
         element.SetActive(true);
      }
   }

   public void DeactivateHighlight()
   {
      foreach (var element in _elements)
      {
         element.SetActive(false);
      }
   }
}
