using UnityEngine;
using GRstory.Interaction;
using GRstory.UISystem;

public class SaveComputer : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        UIManager.Instance.ActiveUI<SaveUI>();
    }
}
