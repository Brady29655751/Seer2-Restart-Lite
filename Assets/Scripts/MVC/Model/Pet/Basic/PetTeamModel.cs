using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetTeamModel : SelectModel<IKeyValuePair<string, Pet[]>>
{
    [SerializeField] private IInputField inputField;

    public string teamName => inputField?.inputString;
}
