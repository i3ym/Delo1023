using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BuildingChooser : MonoBehaviour
{
    [SerializeField]
    Button house;

    void Awake()
    {
        Game.buildingChooser = gameObject;

        house.onClick.AddListener(new UnityAction(() => Game.world.StartBuilding<BuildingHouse>()));
}
}