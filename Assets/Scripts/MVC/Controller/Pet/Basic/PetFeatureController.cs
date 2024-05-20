using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PetFeatureController : Module
{
    [SerializeField] private PetFeatureModel featureModel;
    [SerializeField] private PetFeatureView featureView;
    
    
    
    public void SetPet(Pet pet) {
        featureModel.SetPet(pet);
        featureView.SetPet(pet);
    }
    
    
    
    
    
}
