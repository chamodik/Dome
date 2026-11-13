
using UdonSharp;
using Markcreator.ImpressiveButtons;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ToggleClassicInteract : UdonSharpBehaviour
{
    public bool classicInteractEnabled = false;
    public ImpressiveButton[] buttons;

    void Start()
    {
        _SetClassicInteractForAll();
    }

    public void _OnPress()
    {
        classicInteractEnabled = !classicInteractEnabled;
        _SetClassicInteractForAll();
    }

    private void _SetClassicInteractForAll()
    {
        foreach (ImpressiveButton button in buttons) if (button && button != null) button._SetClassicInteract(classicInteractEnabled);
    }
}
