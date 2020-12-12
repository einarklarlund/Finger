using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Zenject;
using Signals;

public class DreamSwitcher : Interactable
{
    [SerializeField]
    private bool _switchOnSelection = false;

    [SerializeField]
    private bool _clearProfiles = false;
    
    [SerializeField]
    private bool _switchToEnding = false;

    [SerializeField]
    private Ending _ending = default(Ending);

    [SerializeField]
    private SceneAsset _dreamScene = null;

    private SignalBus _signalBus;

    [Inject]
    public void Construct(SignalBus signalBus)
    {
        _signalBus = signalBus;
    }

    public void SwitchDream()
    {
        if(!_switchToEnding)
        {
            _signalBus.Fire(new DreamTransitionSignal() 
                { 
                    clearProfiles = _clearProfiles,
                    dreamScene = _dreamScene,
                });
        }
        else
        {
            _signalBus.Fire(new DreamEndingSignal() 
                { 
                    endingScene = _dreamScene,
                    ending = _ending,
                });
        }
    }

    
    public override void OnInteraction()
    {
        SwitchDream();
    }
    
    public override void OnSelect()
    {
        if(_switchOnSelection)
        {
            SwitchDream();
        }
    }

    public override void OnUnselect()
    {

    }
}
