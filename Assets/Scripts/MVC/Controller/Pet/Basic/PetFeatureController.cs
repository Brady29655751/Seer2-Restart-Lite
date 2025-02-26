using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeatureController : Module
{
    [SerializeField] private PetFeatureModel featureModel;
    [SerializeField] private PetFeatureView featureView;
    
    public event Action<Buff> onRemoveBuffEvent;
    
    public override void Init() {
        featureView.SetOnRemoveCallback(buff => onRemoveBuffEvent?.Invoke(buff));
    }

    public void SetMode(PetBagMode mode) {
        featureView.SetMode(mode);
    }

    public void SetPet(Pet pet) {
        featureModel.SetPet(pet);
        featureView.SetPet(pet);
    }
    
    
    
    
    
}
